from pathlib import Path
import difflib
import re
import shutil
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[2]
VENDOR = ROOT / 'Project Files' / 'lib' / 'Thetis-RADE-vendor'
CONSOLE = ROOT / 'Project Files' / 'Source' / 'Console'
BASE = '3759d096067b7574550b963c1e7a22003da2ab00'
PIN = '408f2b5232ff0a2aec9b538a40d4cb1b02627b17'

# Only SV1EIA changes belonging to RADE / FreeDV Reporter are transplanted.
# Red Pitaya PTT/EOO arbitration remains owned by RadeIntegration.cs.
KEEP_RE = re.compile(r'(?i)(radae|\brade\b|freedv|reporter|qsy)')
EOO_DENY_RE = re.compile(r'(?i)(eoo|endofove?r|mox|ptt|pollptt|un-?key|keying)')

MERGE_FILES = [
    'Project Files/Source/Console/setup.cs',
    'Project Files/Source/Console/setup.designer.cs',
    'Project Files/Source/Console/cmaster.cs',
    'Project Files/Source/Console/common.cs',
    'Project Files/Source/Console/console.cs',
    'Project Files/Source/Console/Thetis.csproj',
]


def require(cond, msg):
    if not cond:
        raise RuntimeError(msg)


def read_text(path):
    return Path(path).read_text(encoding='utf-8-sig')


def git_show(ref, path):
    cp = subprocess.run(
        ['git', 'show', f'{ref}:{path}'], cwd=ROOT,
        stdout=subprocess.PIPE, stderr=subprocess.PIPE, check=False)
    require(cp.returncode == 0, f'git show failed for {ref}:{path}: {cp.stderr.decode(errors="replace")}')
    return cp.stdout.decode('utf-8-sig')


def vendor_text(path):
    p = VENDOR / path
    require(p.exists(), 'vendor file missing: ' + path)
    return read_text(p)


def selected_reference(base_text, ref_text, path):
    """Return BASE plus only RADE/FreeDV-related BASE->SV1EIA edits.

    SequenceMatcher opcodes are grouped when separated by <=6 unchanged lines;
    this keeps WinForms property blocks structurally intact.  For console.cs,
    SV1EIA's own PTT/MOX/EOO hunks are intentionally excluded because the
    Red Pitaya branch already has a verified EOO-safe arbiter.
    """
    a = base_text.splitlines(keepends=True)
    b = ref_text.splitlines(keepends=True)
    sm = difflib.SequenceMatcher(None, a, b, autojunk=False)
    ops = sm.get_opcodes()

    groups = []
    cur = []
    for op in ops:
        tag, i1, i2, j1, j2 = op
        if tag == 'equal' and (i2 - i1) > 6:
            if cur:
                groups.append(cur)
                cur = []
            groups.append([op])
        else:
            cur.append(op)
    if cur:
        groups.append(cur)

    out = []
    selected = 0
    for group in groups:
        changed = [x for x in group if x[0] != 'equal']
        if not changed:
            for _, i1, i2, _, _ in group:
                out.extend(a[i1:i2])
            continue

        changed_text = ''.join(
            ''.join(a[i1:i2]) + ''.join(b[j1:j2])
            for tag, i1, i2, j1, j2 in changed if tag != 'equal')
        use_ref = bool(KEEP_RE.search(changed_text))
        if path.endswith('/console.cs') and EOO_DENY_RE.search(changed_text):
            use_ref = False

        for tag, i1, i2, j1, j2 in group:
            if tag == 'equal':
                out.extend(a[i1:i2])
            elif use_ref:
                out.extend(b[j1:j2])
                selected += 1
            else:
                out.extend(a[i1:i2])

    return ''.join(out), selected


def merge_three_way(path, base_text, selective_ref):
    target_path = ROOT / path
    require(target_path.exists(), 'target file missing: ' + path)
    target_text = read_text(target_path)

    with tempfile.TemporaryDirectory() as td:
        td = Path(td)
        ours = td / 'ours'
        base = td / 'base'
        theirs = td / 'theirs'
        ours.write_text(target_text, encoding='utf-8')
        base.write_text(base_text, encoding='utf-8')
        theirs.write_text(selective_ref, encoding='utf-8')
        cp = subprocess.run(
            ['git', 'merge-file', '-p', str(ours), str(base), str(theirs)],
            cwd=ROOT, stdout=subprocess.PIPE, stderr=subprocess.PIPE, check=False)
        require(cp.returncode == 0,
                f'3-way RADE UI merge conflict in {path}: {cp.stderr.decode(errors="replace")}')
        merged = cp.stdout.decode('utf-8')
        require('<<<<<<<' not in merged and '>>>>>>>' not in merged,
                'merge markers remain in ' + path)
        target_path.write_text(merged, encoding='utf-8')


def merge_managed_files():
    total = 0
    for path in MERGE_FILES:
        base = git_show(BASE, path)
        ref = vendor_text(path)
        selective, count = selected_reference(base, ref, path)
        require(count > 0, 'no RADE/FreeDV changes selected for ' + path)
        merge_three_way(path, base, selective)
        print(f'SV1EIA selective merge: {path}: {count} change blocks')
        total += count
    require(total > 20, f'unexpectedly small RADE UI transplant: {total} blocks')


def copy_reporter_sources():
    src = VENDOR / 'Project Files' / 'Source' / 'Console' / 'FreeDVReporter'
    dst = CONSOLE / 'FreeDVReporter'
    require(src.is_dir(), 'SV1EIA FreeDVReporter source directory missing')
    if dst.exists():
        shutil.rmtree(dst)
    shutil.copytree(src, dst)
    n = sum(1 for p in dst.rglob('*') if p.is_file())
    require(n >= 2, 'FreeDVReporter copy is incomplete')
    print(f'FreeDVReporter source copy: {n} files')


def validate_setup_surface():
    setup = read_text(CONSOLE / 'setup.cs')
    designer = read_text(CONSOLE / 'setup.designer.cs')
    cmaster = read_text(CONSOLE / 'cmaster.cs')
    project = read_text(CONSOLE / 'Thetis.csproj')

    must_designer = [
        'tpDSPRADE',
        'chkRX1RadeControl',
        'chkRX2RadeControl',
        'chkRADAEReporter',
        'chkRADAEReporting',
        'chkRadaeMicRNNoise',
        'chkRadaeMicAGC',
        'chkRadaeMicEQ',
    ]
    for token in must_designer:
        require(token in designer, 'missing SV1EIA RADE Setup control: ' + token)

    must_setup = [
        'chkRX1RadeControl_CheckedChanged',
        'chkRX2RadeControl_CheckedChanged',
        'SetRadaeProtocolV2',
        'chkRADAEReporter_CheckedChanged',
    ]
    for token in must_setup:
        require(token in setup, 'missing SV1EIA RADE Setup event/wiring: ' + token)

    for token in ['SetRadaeRxEnabled', 'SetRadaeProtocolV2', 'SetRadaeRxDialScale']:
        require(token in cmaster, 'missing RADE cmaster interop: ' + token)

    require('FreeDVReporter' in project, 'FreeDVReporter is not included in Thetis.csproj')

    # Protect the Red Pitaya-specific EOO arbiter.  The UI import must never
    # replace or bypass these hooks.
    ri = read_text(CONSOLE / 'RadeIntegration.cs')
    for token in [
        'SetRadaeTxSilenceHold(1)', 'RadaeNotifyEndOfOver()',
        'GetRadaeEooFlushed()', 'RadeEooMarginMs = 300', 'RadeEooTimeoutMs = 3000'
    ]:
        require(token in ri, 'Red Pitaya EOO arbiter damaged: ' + token)

    print('SV1EIA_RADE_SETUP_SURFACE=PASS')
    print('REDPITAYA_EOO_PRESERVED=PASS')


def main():
    require(VENDOR.exists(), 'RADE vendor submodule missing')
    vsha = subprocess.check_output(['git', '-C', str(VENDOR), 'rev-parse', 'HEAD'], text=True).strip()
    require(vsha == PIN, 'wrong RADE vendor pin: ' + vsha)
    subprocess.run(['git', 'cat-file', '-e', BASE + '^{commit}'], cwd=ROOT, check=True)

    copy_reporter_sources()
    merge_managed_files()
    validate_setup_surface()
    print('SV1EIA_RADE_SETUP_1TO1_SELECTIVE=PASS')


if __name__ == '__main__':
    main()
