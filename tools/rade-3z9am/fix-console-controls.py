from pathlib import Path
import importlib.util
import re

ROOT = Path(__file__).resolve().parents[2]
HELPER = ROOT / 'tools' / 'rade-redpitaya' / 'fix-ui-deps.py'
DESIGNER = ROOT / 'Project Files' / 'Source' / 'Console' / 'console.Designer.cs'
CONTROLS = (
    'chkRADE', 'chkREPR', 'chkVIS', 'cmbRadeVersionRX1',
    'chkRADERX2', 'chkVISRX2', 'cmbRadeVersionRX2',
)


def require(cond, msg):
    if not cond:
        raise RuntimeError(msg)


def main():
    require(HELPER.is_file(), 'RedPitaya RADE UI helper missing')
    spec = importlib.util.spec_from_file_location('sq4kou_rade_ui_deps', HELPER)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)

    # Deliberately invoke only the proven main-Console control transplant.
    # Do not run the helper's full main(): 3Z9AM keeps its own Setup/NRE policy.
    mod.patch_console_designer_controls()

    text = DESIGNER.read_text(encoding='utf-8-sig')
    for name in CONTROLS:
        require(re.search(r'\b' + re.escape(name) + r'\s*;', text) is not None,
                '3Z9AM Console designer field missing: ' + name)
        require(('this.' + name + ' = new ') in text,
                '3Z9AM Console designer construction missing: ' + name)

    print('THETIS_3Z9AM_RADE_CONSOLE_7_CONTROLS=PASS')


if __name__ == '__main__':
    main()
