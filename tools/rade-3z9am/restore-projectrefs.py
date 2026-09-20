from pathlib import Path
import re
import subprocess
import sys

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
REPORTER_SOURCES = (
    'FreeDVReporter\\FreeDVReporterClient.cs',
    'FreeDVReporter\\FreeDVReporterForm.cs',
    'FreeDVReporter\\FreeDVReporterManager.cs',
    'FreeDVReporter\\Maidenhead.cs',
)


def require(cond, msg):
    if not cond:
        raise RuntimeError(msg)


def block_rx(include):
    return re.compile(
        r'(?ms)^[ \t]*<ProjectReference\s+Include="' + re.escape(include) +
        r'"\s*>.*?</ProjectReference>[ \t]*\r?\n?')


def remove_compile_item(text, include):
    esc = re.escape(include)
    text = re.sub(r'(?m)^[ \t]*<Compile\s+Include="' + esc + r'"\s*/>[ \t]*\r?\n?', '', text)
    text = re.sub(r'(?ms)^[ \t]*<Compile\s+Include="' + esc + r'"\s*>.*?</Compile>[ \t]*\r?\n?', '', text)
    return text


def ensure_compile_items(text):
    for name in RADE_SOURCES:
        require((CONSOLE / name).is_file(), 'RADE source file missing: ' + name)
    for include in REPORTER_SOURCES:
        require((CONSOLE / include.replace('\\', '/')).is_file(), 'Reporter source file missing: ' + include)

    for include in RADE_SOURCES + REPORTER_SOURCES:
        text = remove_compile_item(text, include)

    anchor = re.search(r'(?m)^(?P<indent>[ \t]*)<Compile\s+Include="cmaster\.cs"\s*/>', text)
    require(anchor is not None, 'Primary Compile anchor cmaster.cs missing')
    indent = anchor.group('indent') or '    '
    lines = [
        indent + '<Compile Include="RadeNative.cs" />',
        indent + '<Compile Include="RadeIntegration.cs" />',
        indent + '<Compile Include="FreeDVReporter\\FreeDVReporterClient.cs" />',
        indent + '<Compile Include="FreeDVReporter\\FreeDVReporterForm.cs">',
        indent + '  <SubType>Form</SubType>',
        indent + '</Compile>',
        indent + '<Compile Include="FreeDVReporter\\FreeDVReporterManager.cs" />',
        indent + '<Compile Include="FreeDVReporter\\Maidenhead.cs" />',
    ]
    payload = '\n'.join(lines) + '\n'
    return text[:anchor.start()] + payload + text[anchor.start():]


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
        current, n = rx.subn(lambda _m, b=bm.group(0): b, current, count=1)
        require(n == 1, 'Could not restore ProjectReference: ' + include)

    current = ensure_compile_items(current)
    CSPROJ.write_text(current, encoding='utf-8')
    final = CSPROJ.read_text(encoding='utf-8-sig')

    for include in INCLUDES:
        rx = block_rx(include)
        bm = rx.search(base)
        fm = rx.search(final)
        require(bm is not None and fm is not None, 'ProjectReference verification missing: ' + include)
        require(fm.group(0).replace('\r\n', '\n') == bm.group(0).replace('\r\n', '\n'),
                'ProjectReference differs from 3Z9AM base: ' + include)

    for include in RADE_SOURCES + REPORTER_SOURCES:
        count = len(re.findall(r'<Compile\s+Include="' + re.escape(include) + r'"(?:\s*/>|\s*>)', final))
        require(count == 1, f'Expected one Compile item for {include}, got {count}')

    # Close only the seven main Console controls proven missing by the previous
    # 3Z9AM C# build. The helper deliberately does not touch Setup/GPU/PTT paths.
    control_fix = ROOT / 'tools' / 'rade-3z9am' / 'fix-console-controls.py'
    require(control_fix.is_file(), '3Z9AM Console control fixer missing')
    subprocess.check_call([sys.executable, str(control_fix)])

    print('THETIS_3Z9AM_PROJECTREFS_RESTORED_FROM_BASE=PASS')
    print('THETIS_3Z9AM_RADE_REPORTER_COMPILE_ITEMS=PASS')
    print('THETIS_3Z9AM_RADE_CONSOLE_DEPENDENCY_GATE=PASS')


if __name__ == '__main__':
    main()
