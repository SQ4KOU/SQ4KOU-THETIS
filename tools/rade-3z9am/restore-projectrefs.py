from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[2]
CONSOLE = ROOT / 'Project Files' / 'Source' / 'Console'
CSPROJ = CONSOLE / 'Thetis.csproj'
BASE = 'ab0071751b0fbe006ddad243361a37cfd59bed46'
REL = 'Project Files/Source/Console/Thetis.csproj'
INCLUDES = (
    r'..\Midi2Cat\Midi2Cat.csproj',
    r'..\RawInput\RawInput.csproj',
)
RADE_SOURCES = ('RadeNative.cs', 'RadeIntegration.cs')


def require(cond, msg):
    if not cond:
        raise RuntimeError(msg)


def block_rx(include):
    return re.compile(
        r'(?ms)^[ \t]*<ProjectReference\s+Include="' + re.escape(include) +
        r'"\s*>.*?</ProjectReference>[ \t]*\r?\n?')


def ensure_rade_compile_items(text):
    for name in RADE_SOURCES:
        require((CONSOLE / name).is_file(), 'RADE source file missing: ' + name)

    missing = [name for name in RADE_SOURCES
               if re.search(r'<Compile\s+Include="' + re.escape(name) + r'"\s*/>', text) is None]
    if not missing:
        return text

    anchor = re.search(r'(?m)^(?P<indent>[ \t]*)<Compile\s+Include="cmaster\.cs"\s*/>', text)
    require(anchor is not None, 'Primary Compile anchor cmaster.cs missing')
    indent = anchor.group('indent') or '    '
    payload = ''.join(indent + '<Compile Include="' + name + '" />\n' for name in missing)
    return text[:anchor.start()] + payload + text[anchor.start():]


def main():
    current = CSPROJ.read_text(encoding='utf-8-sig')
    base_bytes = subprocess.check_output(['git', 'show', f'{BASE}:{REL}'])
    base = base_bytes.decode('utf-8-sig')

    # RADE does not own these references. Restore them exactly from native 3Z9AM.
    for include in INCLUDES:
        rx = block_rx(include)
        bm = rx.search(base)
        require(bm is not None, 'Base ProjectReference missing: ' + include)
        require(len(rx.findall(base)) == 1, 'Base ProjectReference not unique: ' + include)
        require(len(rx.findall(current)) == 1, 'Integrated ProjectReference not unique: ' + include)
        current, n = rx.subn(lambda _m, b=bm.group(0): b, current, count=1)
        require(n == 1, 'Could not restore ProjectReference: ' + include)

    # A clean 3Z9AM tree does not contain these Compile items yet. The imported
    # RADE files must be compiled explicitly; place them in the existing primary
    # Compile ItemGroup immediately before cmaster.cs.
    current = ensure_rade_compile_items(current)

    CSPROJ.write_text(current, encoding='utf-8')
    final = CSPROJ.read_text(encoding='utf-8-sig')

    for include in INCLUDES:
        rx = block_rx(include)
        bm = rx.search(base)
        fm = rx.search(final)
        require(bm is not None and fm is not None, 'ProjectReference verification missing: ' + include)
        require(fm.group(0).replace('\r\n', '\n') == bm.group(0).replace('\r\n', '\n'),
                'ProjectReference differs from 3Z9AM base: ' + include)

    for name in RADE_SOURCES:
        needle = '<Compile Include="' + name + '" />'
        require(final.count(needle) == 1, f'Expected one Compile item for {name}, got {final.count(needle)}')
    require('FreeDVReporter\\FreeDVReporterManager.cs' in final, 'FreeDVReporter Compile item lost')

    print('THETIS_3Z9AM_PROJECTREFS_RESTORED_FROM_BASE=PASS')
    print('THETIS_3Z9AM_RADE_PROJECT_ITEMS_PRESERVED=PASS')


if __name__ == '__main__':
    main()
