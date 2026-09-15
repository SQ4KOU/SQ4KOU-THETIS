from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[2]
CSPROJ = ROOT / 'Project Files' / 'Source' / 'Console' / 'Thetis.csproj'
BASE = 'ab0071751b0fbe006ddad243361a37cfd59bed46'
REL = 'Project Files/Source/Console/Thetis.csproj'
INCLUDES = (
    r'..\Midi2Cat\Midi2Cat.csproj',
    r'..\RawInput\RawInput.csproj',
)


def require(cond, msg):
    if not cond:
        raise RuntimeError(msg)


def block_rx(include):
    return re.compile(
        r'(?ms)^[ \t]*<ProjectReference\s+Include="' + re.escape(include) +
        r'"\s*>.*?</ProjectReference>[ \t]*\r?\n?')


def main():
    current = CSPROJ.read_text(encoding='utf-8-sig')
    base_bytes = subprocess.check_output(['git', 'show', f'{BASE}:{REL}'])
    base = base_bytes.decode('utf-8-sig')

    for include in INCLUDES:
        rx = block_rx(include)
        bm = rx.search(base)
        require(bm is not None, 'Base ProjectReference missing: ' + include)
        require(len(rx.findall(base)) == 1, 'Base ProjectReference not unique: ' + include)
        require(len(rx.findall(current)) == 1, 'Integrated ProjectReference not unique: ' + include)
        base_block = bm.group(0)
        current, n = rx.subn(lambda _m, b=base_block: b, current, count=1)
        require(n == 1, 'Could not restore ProjectReference: ' + include)

    CSPROJ.write_text(current, encoding='utf-8')
    final = CSPROJ.read_text(encoding='utf-8-sig')

    for include in INCLUDES:
        rx = block_rx(include)
        bm = rx.search(base)
        fm = rx.search(final)
        require(bm is not None and fm is not None, 'ProjectReference verification missing: ' + include)
        require(fm.group(0).replace('\r\n', '\n') == bm.group(0).replace('\r\n', '\n'),
                'ProjectReference differs from 3Z9AM base: ' + include)

    # RADE project additions must survive the restoration.
    require('RadeNative.cs' in final, 'RadeNative Compile item lost')
    require('RadeIntegration.cs' in final, 'RadeIntegration Compile item lost')
    require('FreeDVReporter\\FreeDVReporterManager.cs' in final, 'FreeDVReporter Compile item lost')

    print('THETIS_3Z9AM_PROJECTREFS_RESTORED_FROM_BASE=PASS')
    print('THETIS_3Z9AM_RADE_PROJECT_ITEMS_PRESERVED=PASS')


if __name__ == '__main__':
    main()
