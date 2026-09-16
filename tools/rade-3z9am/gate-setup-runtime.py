from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[2]
DESIGNER = ROOT / 'Project Files' / 'Source' / 'Console' / 'setup.designer.cs'


def require(cond, msg):
    if not cond:
        raise RuntimeError(msg)


def main():
    text = DESIGNER.read_text(encoding='utf-8-sig')
    require('private void InitializeComponent()' in text, 'Setup.InitializeComponent missing')

    # We deliberately use absolute source positions instead of attempting to
    # re-parse the huge generated WinForms method. All constructor assignments
    # and receiver uses below are unique enough to establish runtime ordering.
    constructors = {}
    for m in re.finditer(r'this\.([A-Za-z_]\w*)\s*=\s*new\s+', text):
        constructors.setdefault(m.group(1), m.start())

    # tpGeneralLog itself must exist before any property/method receiver use.
    tp_ctor = constructors.get('tpGeneralLog')
    require(tp_ctor is not None, 'RUNTIME_NULL_RISK: tpGeneralLog is never constructed')
    tp_uses = [m.start() for m in re.finditer(r'this\.tpGeneralLog\.', text)]
    require(tp_uses, 'tpGeneralLog has no runtime use')
    require(tp_ctor < min(tp_uses), 'RUNTIME_NULL_RISK: tpGeneralLog constructed after first use')

    # Every child actually attached to tpGeneralLog must be constructed before
    # the Controls.Add call. Foreign SV1EIA-only children have already been
    # pruned by prune-general-log-orphans.py.
    add_rx = re.compile(
        r'this\.tpGeneralLog\.Controls\.Add\(this\.([A-Za-z_]\w*)\)')
    attached = []
    for m in add_rx.finditer(text):
        child = m.group(1)
        attached.append(child)
        cpos = constructors.get(child)
        require(cpos is not None,
                'RUNTIME_NULL_RISK: tpGeneralLog child used without construction: ' + child)
        require(cpos < m.start(),
                'RUNTIME_NULL_RISK: tpGeneralLog child constructed after Controls.Add: ' + child)

    require(attached, 'tpGeneralLog has no attached controls')

    # These two are the RADE/Reporter controls that triggered the original
    # RedPitaya startup crash. In 3Z9AM they are valid and must remain attached,
    # provided their construction order is correct.
    for child in ('chkReporterLogEnable', 'chkRadaeLogEnable'):
        require(child in attached, 'Required RADE/Reporter log control not attached: ' + child)
        require(child in constructors, 'Required RADE/Reporter log control not constructed: ' + child)

    # Core RADE tab itself must be constructed before any receiver use.
    rade_ctor = constructors.get('tpDSPRADE')
    require(rade_ctor is not None, 'RUNTIME_NULL_RISK: tpDSPRADE is never constructed')
    rade_uses = [m.start() for m in re.finditer(r'this\.tpDSPRADE\.', text)]
    require(rade_uses, 'tpDSPRADE has no runtime use')
    require(rade_ctor < min(rade_uses), 'RUNTIME_NULL_RISK: tpDSPRADE constructed after first use')

    # Generic RADE/Reporter receiver gate: if an imported control is used as a
    # method/property receiver, it must have a constructor assignment before
    # that first use. This catches the class of startup NRE seen in the first
    # RedPitaya test without forbidding valid Controls.Add statements.
    receiver_rx = re.compile(
        r'this\.([A-Za-z_]\w*(?:rade|reporter)[A-Za-z0-9_]*)\.', re.I)
    checked = set()
    for m in receiver_rx.finditer(text):
        name = m.group(1)
        if name in checked:
            continue
        checked.add(name)
        cpos = constructors.get(name)
        require(cpos is not None,
                'RUNTIME_NULL_RISK: RADE/Reporter receiver lacks construction: ' + name)
        first_use = next(x.start() for x in receiver_rx.finditer(text) if x.group(1) == name)
        require(cpos < first_use,
                'RUNTIME_NULL_RISK: RADE/Reporter receiver constructed after first use: ' + name)

    print('THETIS_3Z9AM_TPGENERALLOG_CHILDREN=' + ','.join(sorted(attached)))
    print('THETIS_3Z9AM_RADE_REPORTER_RECEIVERS=' + str(len(checked)))
    print('THETIS_3Z9AM_SETUP_RUNTIME_NRE_GATE=PASS')


if __name__ == '__main__':
    main()
