from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[2]
DESIGNER = ROOT / 'Project Files' / 'Source' / 'Console' / 'setup.designer.cs'


def require(cond, msg):
    if not cond:
        raise RuntimeError(msg)


def main():
    text = DESIGNER.read_text(encoding='utf-8-sig')
    start = text.find('private void InitializeComponent()')
    require(start >= 0, 'InitializeComponent missing')
    end = text.find('\n        }', start)
    require(end >= 0, 'InitializeComponent end missing')
    init = text[start:end]

    constructed = set(re.findall(r'this\.([A-Za-z_]\w*)\s*=\s*new\s+', init))
    removed = []

    # RedPitaya/SV1EIA owns a richer General->Log page than 3Z9AM.  The selective
    # RADE transplant may copy Controls.Add() statements for unrelated log-page
    # children (for example btnLogClear) without copying their construction.
    # They are not part of the RADE/Reporter surface and would make the final
    # runtime-null gate fail.  Remove only child-add statements whose child is
    # demonstrably not constructed in this exact 3Z9AM InitializeComponent.
    rx = re.compile(
        r'(?m)^(?P<indent>\s*)this\.tpGeneralLog\.Controls\.Add\(this\.(?P<child>[A-Za-z_]\w*)\);\s*\r?\n?')

    def repl(m):
        child = m.group('child')
        if child not in constructed:
            removed.append(child)
            return ''
        return m.group(0)

    new_init = rx.sub(repl, init)
    text = text[:start] + new_init + text[end:]
    DESIGNER.write_text(text, encoding='utf-8')

    final = DESIGNER.read_text(encoding='utf-8-sig')
    fstart = final.find('private void InitializeComponent()')
    fend = final.find('\n        }', fstart)
    finit = final[fstart:fend]
    final_constructed = set(re.findall(r'this\.([A-Za-z_]\w*)\s*=\s*new\s+', finit))
    dangling = [
        c for c in re.findall(r'this\.tpGeneralLog\.Controls\.Add\(this\.([A-Za-z_]\w*)\)', finit)
        if c not in final_constructed
    ]
    require(not dangling, 'tpGeneralLog still has unconstructed child(ren): ' + ','.join(sorted(set(dangling))))

    # RADE/Reporter controls must survive; this cleaner may remove only foreign
    # General->Log children.
    for token in ('chkReporterLogEnable', 'chkRadaeLogEnable'):
        if ('this.tpGeneralLog.Controls.Add(this.' + token + ');') in init:
            require(('this.tpGeneralLog.Controls.Add(this.' + token + ');') in finit,
                    'RADE/Reporter child was incorrectly pruned: ' + token)

    print('THETIS_3Z9AM_TPGENERALLOG_ORPHANS_REMOVED=' + (','.join(sorted(set(removed))) if removed else 'none'))
    print('THETIS_3Z9AM_TPGENERALLOG_CONSTRUCTED_CHILD_GATE=PASS')


if __name__ == '__main__':
    main()
