from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[2]
DESIGNER = ROOT / 'Project Files' / 'Source' / 'Console' / 'setup.designer.cs'


def require(cond, msg):
    if not cond:
        raise RuntimeError(msg)


def main():
    text = DESIGNER.read_text(encoding='utf-8-sig')
    require('private void InitializeComponent()' in text, 'InitializeComponent missing')

    # Global construction scan is intentional. WinForms construction statements
    # all live in InitializeComponent, while this avoids guessing that method's
    # closing brace in the very large generated designer.
    constructed = set(re.findall(r'this\.([A-Za-z_]\w*)\s*=\s*new\s+', text))
    removed = []

    line_rx = re.compile(
        r'^(?P<indent>[ \t]*)this\.tpGeneralLog\.Controls\.Add\(this\.(?P<child>[A-Za-z_]\w*)\);[ \t]*$')
    out = []
    for line in text.splitlines(keepends=True):
        bare = line.rstrip('\r\n')
        m = line_rx.match(bare)
        if m and m.group('child') not in constructed:
            removed.append(m.group('child'))
            continue
        out.append(line)

    final = ''.join(out)
    DESIGNER.write_text(final, encoding='utf-8')

    final = DESIGNER.read_text(encoding='utf-8-sig')
    final_constructed = set(re.findall(r'this\.([A-Za-z_]\w*)\s*=\s*new\s+', final))
    attached = re.findall(
        r'this\.tpGeneralLog\.Controls\.Add\(this\.([A-Za-z_]\w*)\)', final)
    dangling = [c for c in attached if c not in final_constructed]
    require(not dangling,
            'tpGeneralLog still has unconstructed child(ren): ' + ','.join(sorted(set(dangling))))

    # The actual RADE/Reporter logging controls are required to survive when
    # present in the imported surface.
    for token in ('chkReporterLogEnable', 'chkRadaeLogEnable'):
        add = 'this.tpGeneralLog.Controls.Add(this.' + token + ');'
        if add in text:
            require(add in final, 'RADE/Reporter child was incorrectly pruned: ' + token)
            require(token in final_constructed,
                    'RADE/Reporter child lacks construction: ' + token)

    # In 3Z9AM these are foreign General->Log widgets; if they were imported only
    # as dangling child-adds they must be gone. This assertion makes the exact
    # regression visible in CI.
    for token in ('btnLogClear', 'txtLogViewer', 'udLogMaxLines'):
        if token not in constructed:
            require(('this.tpGeneralLog.Controls.Add(this.' + token + ');') not in final,
                    'Foreign unconstructed log child survived: ' + token)

    print('THETIS_3Z9AM_TPGENERALLOG_ORPHANS_REMOVED=' +
          (','.join(sorted(set(removed))) if removed else 'none'))
    print('THETIS_3Z9AM_TPGENERALLOG_CONSTRUCTED_CHILD_GATE=PASS')


if __name__ == '__main__':
    main()
