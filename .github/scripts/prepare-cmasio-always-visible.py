from pathlib import Path
import re

path = Path("Project Files/Source/Console/setup.cs")
text = path.read_text(encoding="utf-8-sig")

if "SQ4KOU_CMASIO_ALWAYS_VISIBLE" not in text:
    pattern = re.compile(
        r"(?P<indent>[ \t]*)#if !DEBUG\s*\r?\n"
        r"(?P=indent)if\s*\(portaudio_issue\s*\|\|\s*\(!cmasio_config_flag\s*&&\s*!ignore\)\s*\)\s*\r?\n"
        r"(?P=indent)\{\s*\r?\n"
        r"(?P=indent)[ \t]+tcAudio\.TabPages\.Remove\(tpCMAsio\);\s*\r?\n"
        r"(?P=indent)[ \t]+return;\s*\r?\n"
        r"(?P=indent)\}\s*\r?\n"
        r"(?P=indent)#endif",
        re.MULTILINE,
    )
    matches = list(pattern.finditer(text))
    if len(matches) != 1:
        raise RuntimeError(f"cmASIO release visibility gate: expected exactly one match, found {len(matches)}")

    indent = matches[0].group("indent")
    replacement = (
        f"{indent}// SQ4KOU_CMASIO_ALWAYS_VISIBLE: Setup > Audio > cmASIO is always visible.\n"
        f"{indent}// Legacy visibility inputs are deliberately ignored for hiding; engine state is unchanged.\n"
        f"{indent}if (ignore || portaudio_issue || cmasio_config_flag)\n"
        f"{indent}{{\n"
        f"{indent}    // Visibility only: no cmASIO activation or audio-path change here.\n"
        f"{indent}}}\n"
        f"{indent}if (!tcAudio.TabPages.Contains(tpCMAsio))\n"
        f"{indent}    tcAudio.TabPages.Add(tpCMAsio);"
    )
    text = pattern.sub(replacement, text, count=1)
    path.write_text(text, encoding="utf-8-sig", newline="")

text = path.read_text(encoding="utf-8-sig")
if "SQ4KOU_CMASIO_ALWAYS_VISIBLE" not in text:
    raise RuntimeError("cmASIO always-visible marker missing")
if "tcAudio.TabPages.Remove(tpCMAsio);" in text:
    raise RuntimeError("cmASIO hide call still present")
if "tcAudio.TabPages.Add(tpCMAsio);" not in text:
    raise RuntimeError("cmASIO always-visible add call missing")

print("CMASIO_ALWAYS_VISIBLE_PREPATCH=PASS")
