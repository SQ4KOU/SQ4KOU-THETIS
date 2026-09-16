from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[2]
DESIGNER = ROOT / 'Project Files' / 'Source' / 'Console' / 'setup.designer.cs'
MARKER = '// SV1EIA RADE Setup -> DSP -> RADE'


def require(cond, msg):
    if not cond:
        raise RuntimeError(msg)


def main():
    text = DESIGNER.read_text(encoding='utf-8-sig')
    require(MARKER in text, 'RADE designer marker missing')

    marker_pos = text.index(MARKER)
    before = text[:marker_pos]
    after = text[marker_pos:]

    # SV1EIA has a General/Log tab that does not exist in the Red Pitaya base.
    # The selective RADE port can bring two logging checkboxes plus their
    # parent-use lines while the parent TabPage itself is not constructed.
    # First application removes both invalid lines. Re-applying the transform
    # must accept the already-clean state; a partial one-line state is rejected.
    general_log_initialized = bool(re.search(r'this\.tpGeneralLog\s*=\s*new\b', text))
    removed = []
    bad_lines = (
        'this.tpGeneralLog.Controls.Add(this.chkReporterLogEnable);',
        'this.tpGeneralLog.Controls.Add(this.chkRadaeLogEnable);',
    )
    if not general_log_initialized:
        present = [token for token in bad_lines if token in after]
        require(len(present) in (0, 2),
                f'inconsistent tpGeneralLog child-add state: present={len(present)}')
        if present:
            lines = after.splitlines(keepends=True)
            kept = []
            for line in lines:
                if any(token in line for token in bad_lines):
                    removed.append(line.strip())
                    continue
                kept.append(line)
            after = ''.join(kept)
            text = before + after
            require(len(removed) == 2,
                    f'expected two invalid tpGeneralLog child-add lines, removed={len(removed)}')

    # Startup-safety gate: every field used as a call/property receiver inside the
    # injected RADE block must have been constructed either in the original target
    # InitializeComponent or inside the injected block itself.
    marker_pos = text.index(MARKER)
    end = text.find('this.ResumeLayout(false);', marker_pos)
    if end < 0:
        end = len(text)
    pre = text[:marker_pos]
    block = text[marker_pos:end]

    initialized = set(re.findall(r'this\.([A-Za-z_]\w*)\s*=\s*new\b', pre + block))
    receivers = []
    for line_no, line in enumerate(block.splitlines(), 1):
        for field in re.findall(r'this\.([A-Za-z_]\w*)\.', line):
            if field not in initialized:
                receivers.append((line_no, field, line.strip()))

    # A receiver that is never constructed is a deterministic startup NRE.
    # Do not allow another MSI to be published with one.
    if receivers:
        detail = '\n'.join(f'{n}: {f}: {line}' for n, f, line in receivers[:20])
        raise RuntimeError('uninitialized RADE designer receiver(s):\n' + detail)

    # If tpGeneralLog is absent, neither invalid child-add may survive. If the
    # parent exists, those child-adds are valid and must not be deleted merely
    # because this safety pass is re-run.
    if not general_log_initialized:
        require('this.tpGeneralLog.Controls.Add(this.chkReporterLogEnable);' not in block,
                'invalid tpGeneralLog Reporter add survived')
        require('this.tpGeneralLog.Controls.Add(this.chkRadaeLogEnable);' not in block,
                'invalid tpGeneralLog RADE log add survived')
    require('this.tpDSPRADE = new System.Windows.Forms.TabPage();' in block,
            'RADE tab construction missing')

    DESIGNER.write_text(text, encoding='utf-8')
    state = 'initialized' if general_log_initialized else 'absent/clean'
    print('Removed invalid null-container lines: ' + (', '.join(removed) if removed else 'not needed'))
    print(f'tpGeneralLog state={state}')
    print(f'RADE designer initialized receivers={len(initialized)}')
    print('RADE_SETUP_INITIALIZECOMPONENT_NULL_GATE=PASS')


if __name__ == '__main__':
    main()
