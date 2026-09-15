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
    r'(?m)^[ \t]*(?:(?:public|internal|protected|private|sealed|abstract|static|partial|new)\s+)*'
    r'(?P<kw>class)\s+(?P<name>[A-Za-z_]\w*)\b')


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
    """Return the real closing-brace position of the single top-level class.

    The original integrator guessed the class end from the final lines of the
    file. That is unsafe when a source has trailing regions, comments, helper
    types or a layout different from the expected namespace/class suffix.
    Here the class body is located lexically and its own matching brace is
    used as the insertion boundary. Nested classes are ignored.
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
    if len(top) != 1:
        names = ', '.join(c[1] for c in top)
        raise RuntimeError('ambiguous top-level C# classes: ' + names)

    _, _, _, close_pos = top[0]
    return close_pos


# Patch only lexical/member-boundary and real class-boundary detection.
# All selection rules, protected RedPitaya/PTT/EOO policy and validation remain
# exactly in the original integrator.
mod.brace_end = safe_brace_end
mod.members_with_bodies = safe_members_with_bodies
mod.class_insert_pos = safe_class_insert_pos

if __name__ == '__main__':
    mod.main()
