from pathlib import Path
import importlib.util

HERE = Path(__file__).resolve().parent
ORIGINAL = HERE / 'integrate-ui.py'

spec = importlib.util.spec_from_file_location('sq4kou_rade_integrate_ui', ORIGINAL)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)


def _code_mask(text):
    """Return a byte mask marking C# lexical code positions.

    Braces and method-looking text inside // comments, /* */ comments,
    regular strings, verbatim strings and char literals must never affect
    member extraction. Newlines stay outside the mask but indexes are kept 1:1.
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


def safe_brace_end(text, open_pos):
    mask = _code_mask(text)
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
            end = safe_brace_end(text, open_pos)
        except RuntimeError:
            continue
        out.append((m.group('name'), m.start(), end, text[m.start():end]))
        seen_starts.add(m.start())
    return out


# Patch only lexical/member-boundary detection. All selection rules, protected
# RedPitaya/PTT/EOO policy and validation remain exactly in integrate-ui.py.
mod.brace_end = safe_brace_end
mod.members_with_bodies = safe_members_with_bodies

if __name__ == '__main__':
    mod.main()
