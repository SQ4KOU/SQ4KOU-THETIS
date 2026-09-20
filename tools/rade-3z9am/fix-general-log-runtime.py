from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[2]
PATH = ROOT / 'Project Files' / 'Source' / 'Console' / 'setup.designer.cs'

ESSENTIAL_RE = re.compile(r'(?i)(radae|\brade\b|reporter)')
ADD_RE = re.compile(
    r'(?m)^(?P<indent>[ \t]*)this\.tpGeneralLog\.Controls\.Add\(this\.(?P<name>[A-Za-z_]\w*)\);[ \t]*\r?\n?'
)


def require(cond, msg):
    if not cond:
        raise RuntimeError(msg)


def main():
    text = PATH.read_text(encoding='utf-8-sig')
    matches = list(ADD_RE.finditer(text))
    require(matches, 'tpGeneralLog has no child Controls.Add statements')

    children = []
    prune = set()
    for m in matches:
        name = m.group('name')
        children.append(name)
        ctor = re.search(r'this\.' + re.escape(name) + r'\s*=\s*new\s+', text)
        if ctor is None:
            if ESSENTIAL_RE.search(name):
                raise RuntimeError('RADE_RUNTIME_NULL_RISK: essential control is not constructed: ' + name)
            prune.add(name)

    if prune:
        def repl(m):
            return '' if m.group('name') in prune else m.group(0)
        text = ADD_RE.sub(repl, text)
        PATH.write_text(text, encoding='utf-8')

    final = PATH.read_text(encoding='utf-8-sig')
    remaining = [m.group('name') for m in ADD_RE.finditer(final)]
    require(remaining, 'tpGeneralLog has no remaining controls after runtime filtering')

    for name in sorted(set(remaining)):
        ctor = re.search(r'this\.' + re.escape(name) + r'\s*=\s*new\s+', final)
        require(ctor is not None, 'RUNTIME_NULL_RISK after filter: ' + name)
        use = re.search(r'this\.tpGeneralLog\.Controls\.Add\(this\.' + re.escape(name) + r'\)', final)
        require(use is not None, 'tpGeneralLog child use missing after filter: ' + name)
        require(ctor.start() < use.start(), 'RUNTIME_ORDER_RISK: child constructed after Controls.Add: ' + name)

    print('THETIS_3Z9AM_TPGENERALLOG_PRUNED=' + (','.join(sorted(prune)) if prune else 'none'))
    print('THETIS_3Z9AM_TPGENERALLOG_CHILDREN=' + ','.join(sorted(set(remaining))))
    print('THETIS_3Z9AM_GENERAL_LOG_RUNTIME_GATE=PASS')


if __name__ == '__main__':
    main()
