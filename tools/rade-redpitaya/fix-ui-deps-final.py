from pathlib import Path
import importlib.util
import re

HERE = Path(__file__).resolve().parent
BASE_PATH = HERE / 'fix-ui-deps.py'

spec = importlib.util.spec_from_file_location('sq4kou_rade_fix_ui_deps', BASE_PATH)
base = importlib.util.module_from_spec(spec)
spec.loader.exec_module(base)
mod = base.mod


def require(cond, msg):
    if not cond:
        raise RuntimeError(msg)


def patch_setup_public_properties():
    """Close the exact Setup properties referenced by SV1EIA Console mirrors."""
    names = (
        'RADAE',
        'RADAEReporter',
        'RADAEReporting',
        'RADAERX2',
        'RADAEReportingRX2',
        'RADAEVersionRX1',
        'RADAEVersionRX2',
    )
    base.transplant(
        'Project Files/Source/Console/setup.cs',
        'Setup',
        properties=names,
    )
    text = mod.read_text(mod.CONSOLE / 'setup.cs')
    for name in names:
        require(base.target_has_member(text, mod.PROPERTY_RE, name),
                'Setup property missing after transplant: ' + name)
    print('SV1EIA_SETUP_PUBLIC_PROPERTIES=PASS')


def normalize_reporter_compile_items():
    """Put FreeDVReporter Compile items in the existing primary Compile ItemGroup.

    integrate-ui-original.py historically appended a new ItemGroup at the very
    end of this old-style csproj.  MSBuild for this project does not reliably
    feed those late Compile items into CoreCompile.  Remove any prior copies
    and insert the exact four source items immediately before cmaster.cs, which
    is inside the established source Compile ItemGroup.
    """
    path = mod.CONSOLE / 'Thetis.csproj'
    text = mod.read_text(path)

    items = (
        ('FreeDVReporter\\FreeDVReporterClient.cs', False),
        ('FreeDVReporter\\FreeDVReporterForm.cs', True),
        ('FreeDVReporter\\FreeDVReporterManager.cs', False),
        ('FreeDVReporter\\Maidenhead.cs', False),
    )

    for include, is_form in items:
        esc = re.escape(include)
        # Remove either self-closing or expanded Compile form, wherever an
        # earlier integration pass put it.
        text = re.sub(
            r'(?m)^\s*<Compile\s+Include="' + esc + r'"\s*/>\s*\r?\n?',
            '', text)
        text = re.sub(
            r'(?ms)^\s*<Compile\s+Include="' + esc + r'"\s*>.*?</Compile>\s*\r?\n?',
            '', text)

    # Clean only truly empty ItemGroups left by the old appended block.
    text = re.sub(r'(?ms)^\s*<ItemGroup>\s*</ItemGroup>\s*\r?\n?', '', text)

    lines = [
        '    <Compile Include="FreeDVReporter\\FreeDVReporterClient.cs" />',
        '    <Compile Include="FreeDVReporter\\FreeDVReporterForm.cs">',
        '      <SubType>Form</SubType>',
        '    </Compile>',
        '    <Compile Include="FreeDVReporter\\FreeDVReporterManager.cs" />',
        '    <Compile Include="FreeDVReporter\\Maidenhead.cs" />',
    ]
    payload = '\n'.join(lines) + '\n'
    anchor = '    <Compile Include="cmaster.cs" />'
    require(anchor in text, 'Thetis.csproj cmaster.cs primary Compile anchor missing')
    text = text.replace(anchor, payload + anchor, 1)
    mod.write_text(path, text)

    final = mod.read_text(path)
    cmaster_pos = final.find(anchor)
    require(cmaster_pos >= 0, 'cmaster.cs anchor lost')
    for include, _ in items:
        needle = '<Compile Include="' + include + '"'
        require(final.count(needle) == 1,
                f'expected exactly one Compile item for {include}, got {final.count(needle)}')
        require(final.find(needle) < cmaster_pos,
                'Reporter Compile item is not in primary source group: ' + include)
        src = mod.CONSOLE / include.replace('\\', '/')
        require(src.is_file(), 'Reporter source missing: ' + include)
        source = mod.read_text(src)
        require('namespace Thetis.FreeDVReporter' in source,
                'Unexpected reporter namespace: ' + include)

    print('FREEDV_REPORTER_PRIMARY_COMPILE_GROUP=PASS')


def validate_protected_arbiter():
    ri = mod.read_text(mod.CONSOLE / 'RadeIntegration.cs')
    for token in (
        'SetRadaeTxSilenceHold(1)',
        'RadaeNotifyEndOfOver()',
        'GetRadaeEooFlushed()',
    ):
        require(token in ri, 'SQ4KOU EOO arbiter damaged: ' + token)
    print('SQ4KOU_REDPITAYA_PTT_EOO_STILL=PASS')


def main():
    patch_setup_public_properties()
    normalize_reporter_compile_items()
    validate_protected_arbiter()
    print('FINAL_SV1EIA_RADE_COMPILE_CLOSURE=PASS')


if __name__ == '__main__':
    main()
