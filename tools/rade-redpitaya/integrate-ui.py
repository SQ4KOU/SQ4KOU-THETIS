from pathlib import Path
import importlib.util
import re

HERE = Path(__file__).resolve().parent
ORIGINAL = HERE / 'integrate-ui-original.py'

spec = importlib.util.spec_from_file_location('sq4kou_rade_integrate_ui', ORIGINAL)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)


def _code_mask(text):
    """Return a byte mask marking C# lexical code positions.

    Braces and method-looking text inside // comments, /* */ comments,
    regular strings, verbatim strings and char literals must never affect
    member extraction. Indexes are preserved 1:1 with the source text.
    """
    mask = bytearray(len(text))
    i = 0
    state = 'code'
    while i < len(text):
        ch = text[i]
        nxt = text[i + 1] if i + 1 < len(text) else ''

        if state == 'code':
            if ch == '/' and nxt == '/':
                state = 'line_comment'
                i += 2
                continue
            if ch == '/' and nxt == '*':
                state = 'block_comment'
                i += 2
                continue
            if ch == '@' and nxt == '"':
                state = 'verbatim_string'
                i += 2
                continue
            if ch == '$' and nxt == '"':
                state = 'string'
                i += 2
                continue
            if ch == '$' and nxt == '@' and i + 2 < len(text) and text[i + 2] == '"':
                state = 'verbatim_string'
                i += 3
                continue
            if ch == '@' and nxt == '$' and i + 2 < len(text) and text[i + 2] == '"':
                state = 'verbatim_string'
                i += 3
                continue
            if ch == '"':
                state = 'string'
                i += 1
                continue
            if ch == "'":
                state = 'char'
                i += 1
                continue
            mask[i] = 1
            i += 1
            continue

        if state == 'line_comment':
            if ch == '\n':
                state = 'code'
            i += 1
            continue

        if state == 'block_comment':
            if ch == '*' and nxt == '/':
                state = 'code'
                i += 2
            else:
                i += 1
            continue

        if state == 'string':
            if ch == '\\':
                i += 2
            elif ch == '"':
                state = 'code'
                i += 1
            else:
                i += 1
            continue

        if state == 'verbatim_string':
            if ch == '"' and nxt == '"':
                i += 2
            elif ch == '"':
                state = 'code'
                i += 1
            else:
                i += 1
            continue

        if state == 'char':
            if ch == '\\':
                i += 2
            elif ch == "'":
                state = 'code'
                i += 1
            else:
                i += 1
            continue

    return mask


def _brace_end_with_mask(text, open_pos, mask):
    if open_pos < 0 or open_pos >= len(text) or text[open_pos] != '{' or not mask[open_pos]:
        raise RuntimeError('invalid C# opening brace')
    depth = 0
    for i in range(open_pos, len(text)):
        if not mask[i]:
            continue
        ch = text[i]
        if ch == '{':
            depth += 1
        elif ch == '}':
            depth -= 1
            if depth == 0:
                return i + 1
    raise RuntimeError('unbalanced C# braces')


def safe_brace_end(text, open_pos):
    return _brace_end_with_mask(text, open_pos, _code_mask(text))


def safe_members_with_bodies(text, regex):
    mask = _code_mask(text)
    out = []
    seen_starts = set()
    for m in regex.finditer(text):
        if m.start() in seen_starts or not mask[m.start()]:
            continue
        open_pos = -1
        for i in range(m.start(), m.end()):
            if text[i] == '{' and mask[i]:
                open_pos = i
                break
        if open_pos < 0:
            continue
        try:
            end = _brace_end_with_mask(text, open_pos, mask)
        except RuntimeError:
            continue
        out.append((m.group('name'), m.start(), end, text[m.start():end]))
        seen_starts.add(m.start())
    return out


_CLASS_RE = re.compile(
    r'(?m)^[ \t]*(?:(?:public|internal|protected|private|sealed|abstract|static|partial|new|unsafe)\s+)*'
    r'(?P<kw>class)\s+(?P<name>[A-Za-z_]\w*)\b')

_PRIMARY_CLASS_NAMES = {'setup', 'cmaster', 'common', 'console'}


def _code_brace_depth_before(text, pos, mask):
    depth = 0
    for i in range(pos):
        if not mask[i]:
            continue
        if text[i] == '{':
            depth += 1
        elif text[i] == '}':
            depth -= 1
            if depth < 0:
                raise RuntimeError('unbalanced C# braces before class')
    return depth


def safe_class_insert_pos(text):
    """Return the real closing-brace position of the intended host class.

    The original integrator guessed the class end from the final lines of the
    file. That is unsafe when a source has trailing regions, comments or
    helper types. Here every top-level class is located lexically and closed
    with its own matching brace. For the known integration targets, the host
    class is selected explicitly by its native class name; helper classes in
    the same file are never used as an insertion target.
    """
    mask = _code_mask(text)
    candidates = []

    for m in _CLASS_RE.finditer(text):
        kw = m.start('kw')
        if kw >= len(mask) or not mask[kw]:
            continue

        open_pos = -1
        i = m.end()
        while i < len(text):
            if not mask[i]:
                i += 1
                continue
            ch = text[i]
            if ch == '{':
                open_pos = i
                break
            if ch in ';}':
                break
            i += 1
        if open_pos < 0:
            continue

        try:
            end = _brace_end_with_mask(text, open_pos, mask)
            depth = _code_brace_depth_before(text, open_pos, mask)
        except RuntimeError:
            continue
        candidates.append((depth, m.group('name'), open_pos, end - 1))

    if not candidates:
        raise RuntimeError('cannot find lexical top-level C# class')

    min_depth = min(c[0] for c in candidates)
    top = [c for c in candidates if c[0] == min_depth]

    preferred = [c for c in top if c[1].lower() in _PRIMARY_CLASS_NAMES]
    if len(preferred) == 1:
        top = preferred
    elif len(top) != 1:
        names = ', '.join(c[1] for c in top)
        raise RuntimeError('ambiguous top-level C# classes: ' + names)

    _, _, _, close_pos = top[0]
    return close_pos


_DELEGATE_RE = re.compile(
    r'(?m)^[ \t]*(?:(?:public|internal|protected|private|static|new|unsafe)\s+)*'
    r'delegate\s+[A-Za-z_][\w<>,\.\[\]\? ]*\s+(?P<name>[A-Za-z_]\w*)\s*\([^;{}]*\)\s*;')


def _delegate_declarations(text):
    mask = _code_mask(text)
    out = []
    for m in _DELEGATE_RE.finditer(text):
        if m.start() < len(mask) and mask[m.start()]:
            out.append((m.group('name'), m.group(0)))
    return out


def safe_patch_console_named_members():
    """Run the original RADE member port, then copy exact RADE delegate types.

    The original selector covers methods, properties and fields but not C#
    delegate declarations. RADE exposes members typed with those delegates,
    so copying a field without its type produces CS0246. Copy declarations
    verbatim from the pinned SV1EIA source; never synthesize signatures.
    """
    _original_patch_console_named_members()

    rel = 'Project Files/Source/Console/console.cs'
    ref = mod.vendor_text(rel)
    path = mod.CONSOLE / 'console.cs'
    target = mod.read_text(path)

    ref_delegates = {
        name: block for name, block in _delegate_declarations(ref)
        if mod.KEEP_RE.search(name) and not mod.EOO_DENY_RE.search(name)
    }
    mod.require('RadaeEnabledChanged' in ref_delegates,
                'SV1EIA RadaeEnabledChanged delegate declaration not found')

    target_names = {name for name, _ in _delegate_declarations(target)}
    missing = [block for name, block in ref_delegates.items() if name not in target_names]
    if missing:
        target = mod.insert_class_members(target, missing)
        mod.write_text(path, target)

    final_names = {name for name, _ in _delegate_declarations(target)}
    mod.require('RadaeEnabledChanged' in final_names,
                'RadaeEnabledChanged delegate was not transplanted')
    print('SV1EIA Console RADE delegates ported: ' +
          ', '.join(sorted(name for name in ref_delegates if name not in target_names)))


# Patch lexical/member-boundary and real class-boundary detection. Extend the
# original Console port only for exact delegate declarations missing from its
# selector. All selection rules and protected RedPitaya/PTT/EOO policy remain.
mod.brace_end = safe_brace_end
mod.members_with_bodies = safe_members_with_bodies
mod.class_insert_pos = safe_class_insert_pos
_original_patch_console_named_members = mod.patch_console_named_members
mod.patch_console_named_members = safe_patch_console_named_members

if __name__ == '__main__':
    mod.main()
