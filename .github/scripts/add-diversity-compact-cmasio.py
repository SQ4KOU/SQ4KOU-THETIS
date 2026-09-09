from pathlib import Path
import re

ROOT = Path("Project Files/Source/Console")
DIV = ROOT / "DiversityForm.cs"
SETUP = ROOT / "setup.cs"


def read(path: Path) -> str:
    return path.read_text(encoding="utf-8-sig")


def write(path: Path, text: str) -> None:
    path.write_text(text, encoding="utf-8-sig", newline="")


def replace_once(text: str, old: str, new: str, label: str) -> str:
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f"{label}: expected exactly one anchor, found {count}")
    return text.replace(old, new, 1)


d = read(DIV)

if "SQ4KOU_DIVERSITY_COMPACT" not in d:
    if "using System.Collections.Generic;" not in d:
        d = replace_once(
            d,
            "using System.Collections;\n",
            "using System.Collections;\nusing System.Collections.Generic;\n",
            "add generic collections using",
        )

    field_anchor = "        private int _mouse_down_memory_index = -1;\n"
    fields = field_anchor + r'''

        // SQ4KOU_DIVERSITY_COMPACT
        // Compact UI only: DSP/diversity processing remains untouched.
        private bool _sq4kouCompactMode;
        private bool _sq4kouCompactInitialised;
        private Size _sq4kouExpandedClientSize;
        private Size _sq4kouExpandedMinimumSize;
        private Rectangle _sq4kouExpandedRadarBounds;
        private AnchorStyles _sq4kouExpandedRadarAnchor;
        private readonly Dictionary<Control, bool> _sq4kouExpandedVisibility = new Dictionary<Control, bool>();

        // Preserve the Phase/Gain values when a left double-click is used only to toggle UI.
        private bool _sq4kouSavedMouseStateValid;
        private decimal _sq4kouSavedR;
        private decimal _sq4kouSavedAngle;
        private decimal _sq4kouSavedR1;
        private decimal _sq4kouSavedR2;
        private decimal _sq4kouSavedAngle0;
        private decimal _sq4kouSavedFineNull;
        private double _sq4kouSavedLockedR;
        private double _sq4kouSavedLockedAngle;
'''
    d = replace_once(d, field_anchor, fields, "compact fields")

    ctor_anchor = """            if (console != null && !console.IsSetupFormNull)\n            {\n                DarkMode = console.SetupForm.DarkMode;\n            }\n        }\n\n        #region DARK-MODE\n"""
    ctor_repl = """            if (console != null && !console.IsSetupFormNull)\n            {\n                DarkMode = console.SetupForm.DarkMode;\n            }\n\n            // Start with the unobtrusive Phase/Gain radar only.\n            InitializeSQ4KOUCompactMode();\n        }\n\n        private void InitializeSQ4KOUCompactMode()\n        {\n            if (_sq4kouCompactInitialised || picRadar == null) return;\n\n            _sq4kouExpandedClientSize = this.ClientSize;\n            _sq4kouExpandedMinimumSize = this.MinimumSize;\n            _sq4kouExpandedRadarBounds = picRadar.Bounds;\n            _sq4kouExpandedRadarAnchor = picRadar.Anchor;\n            _sq4kouExpandedVisibility.Clear();\n\n            foreach (Control control in this.Controls)\n            {\n                if (control != picRadar)\n                    _sq4kouExpandedVisibility[control] = control.Visible;\n            }\n\n            _sq4kouCompactInitialised = true;\n            SetSQ4KOUCompactMode(true);\n        }\n\n        private void SetSQ4KOUCompactMode(bool compact)\n        {\n            if (!_sq4kouCompactInitialised || picRadar == null) return;\n            if (_sq4kouCompactMode == compact && compact) return;\n\n            this.SuspendLayout();\n            try\n            {\n                if (compact)\n                {\n                    foreach (KeyValuePair<Control, bool> item in _sq4kouExpandedVisibility)\n                    {\n                        if (item.Key != null && !item.Key.IsDisposed) item.Key.Visible = false;\n                    }\n\n                    // The original radar itself is 305x305. Keep it unscaled in compact mode.\n                    this.MinimumSize = Size.Empty;\n                    picRadar.Anchor = AnchorStyles.Top | AnchorStyles.Left;\n                    picRadar.Location = new Point(4, 4);\n                    picRadar.Size = new Size(305, 305);\n                    this.ClientSize = new Size(313, 313);\n                }\n                else\n                {\n                    // Restore exactly the full layout captured after PA3GHM panel sizing/RestoreForm.\n                    this.MinimumSize = Size.Empty;\n                    this.ClientSize = _sq4kouExpandedClientSize;\n                    picRadar.Anchor = AnchorStyles.Top | AnchorStyles.Left;\n                    picRadar.Bounds = _sq4kouExpandedRadarBounds;\n                    picRadar.Anchor = _sq4kouExpandedRadarAnchor;\n\n                    foreach (KeyValuePair<Control, bool> item in _sq4kouExpandedVisibility)\n                    {\n                        if (item.Key != null && !item.Key.IsDisposed) item.Key.Visible = item.Value;\n                    }\n\n                    this.MinimumSize = _sq4kouExpandedMinimumSize;\n                    EnsurePA3GHMNativePanelSize();\n                }\n\n                _sq4kouCompactMode = compact;\n                picRadar.Invalidate();\n            }\n            finally\n            {\n                this.ResumeLayout(true);\n            }\n        }\n\n        private void CaptureSQ4KOURadarState()\n        {\n            _sq4kouSavedR = udR.Value;\n            _sq4kouSavedAngle = udAngle.Value;\n            _sq4kouSavedR1 = udR1.Value;\n            _sq4kouSavedR2 = udR2.Value;\n            _sq4kouSavedAngle0 = udAngle0.Value;\n            _sq4kouSavedFineNull = udFineNull.Value;\n            _sq4kouSavedLockedR = locked_r;\n            _sq4kouSavedLockedAngle = locked_angle;\n            _sq4kouSavedMouseStateValid = true;\n        }\n\n        private void RestoreSQ4KOURadarState()\n        {\n            if (!_sq4kouSavedMouseStateValid) return;\n\n            bool oldInitialising = _initalising;\n            _initalising = true;\n            try\n            {\n                udR.Value = _sq4kouSavedR;\n                udAngle.Value = _sq4kouSavedAngle;\n                udR1.Value = _sq4kouSavedR1;\n                udR2.Value = _sq4kouSavedR2;\n                udAngle0.Value = _sq4kouSavedAngle0;\n                udFineNull.Value = _sq4kouSavedFineNull;\n                locked_r = _sq4kouSavedLockedR;\n                locked_angle = _sq4kouSavedLockedAngle;\n            }\n            finally\n            {\n                _initalising = oldInitialising;\n            }\n\n            _sq4kouSavedMouseStateValid = false;\n            UpdateDiversity();\n            picRadar.Invalidate();\n        }\n\n        private void ToggleSQ4KOUCompactMode()\n        {\n            SetSQ4KOUCompactMode(!_sq4kouCompactMode);\n        }\n\n        #region DARK-MODE\n"""
    d = replace_once(d, ctor_anchor, ctor_repl, "compact constructor/methods")

    mouse_down_anchor = """        private void picRadar_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)\n        {\n            if (_initalising) return;\n\n            updateHoverMemory(e.Location);\n"""
    mouse_down_repl = """        private void picRadar_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)\n        {\n            if (_initalising) return;\n\n            // Right click is a zero-side-effect compact/full toggle.\n            if (e.Button == MouseButtons.Right)\n            {\n                mouse_down = false;\n                ToggleSQ4KOUCompactMode();\n                return;\n            }\n\n            // Left double-click also toggles. Restore the Phase/Gain snapshot from the\n            // first click so using the UI toggle cannot leave a changed diversity setting.\n            if (e.Button == MouseButtons.Left && e.Clicks >= 2)\n            {\n                mouse_down = false;\n                RestoreSQ4KOURadarState();\n                ToggleSQ4KOUCompactMode();\n                return;\n            }\n\n            if (e.Button == MouseButtons.Left && e.Clicks == 1)\n                CaptureSQ4KOURadarState();\n\n            updateHoverMemory(e.Location);\n"""
    d = replace_once(d, mouse_down_anchor, mouse_down_repl, "radar mouse down toggle")

    mouse_up_anchor = """        private void picRadar_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)\n        {\n            if (_initalising) return;\n\n            updateHoverMemory(e.Location);\n"""
    mouse_up_repl = """        private void picRadar_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)\n        {\n            if (_initalising) return;\n\n            if (e.Button == MouseButtons.Right || (e.Button == MouseButtons.Left && e.Clicks >= 2))\n            {\n                mouse_down = false;\n                return;\n            }\n\n            updateHoverMemory(e.Location);\n"""
    d = replace_once(d, mouse_up_anchor, mouse_up_repl, "radar mouse up toggle guard")

    closing_anchor = """        private void DiversityForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)\n        {\n            txtMemoryDataHidden.Text = SerializeObjectToString<memorySettings[]>(_memories);\n\n            Common.SaveForm(this, \"DiversityForm\");\n        }\n"""
    closing_repl = """        private void DiversityForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)\n        {\n            txtMemoryDataHidden.Text = SerializeObjectToString<memorySettings[]>(_memories);\n\n            // Save the expanded layout, never the temporary 313x313 compact shell.\n            if (_sq4kouCompactInitialised && _sq4kouCompactMode)\n                SetSQ4KOUCompactMode(false);\n\n            Common.SaveForm(this, \"DiversityForm\");\n        }\n"""
    d = replace_once(d, closing_anchor, closing_repl, "save expanded Diversity layout")

    write(DIV, d)

s = read(SETUP)
if "SQ4KOU_CMASIO_ALWAYS_VISIBLE" not in s:
    pattern = re.compile(
        r"(?P<indent>[ \t]*)if\s*\(portaudio_issue\s*\|\|\s*\(!cmasio_config_flag\s*&&\s*!ignore\)\s*\)\s*\r?\n"
        r"(?P=indent)\{\s*\r?\n"
        r"(?P=indent)[ \t]+tcAudio\.TabPages\.Remove\(tpCMAsio\);\s*\r?\n"
        r"(?P=indent)\}",
        re.MULTILINE,
    )
    matches = list(pattern.finditer(s))
    if len(matches) != 1:
        raise RuntimeError(f"cmASIO visibility gate: expected exactly one match, found {len(matches)}")
    indent = matches[0].group("indent")
    repl = (
        f"{indent}// SQ4KOU_CMASIO_ALWAYS_VISIBLE: configuration tab is always available.\n"
        f"{indent}// This does not enable the cmASIO engine or change the selected audio path.\n"
        f"{indent}if (!tcAudio.TabPages.Contains(tpCMAsio))\n"
        f"{indent}    tcAudio.TabPages.Add(tpCMAsio);"
    )
    s = pattern.sub(repl, s, count=1)
    write(SETUP, s)

# Deterministic post-patch audit.
d = read(DIV)
s = read(SETUP)
required_div = [
    "SQ4KOU_DIVERSITY_COMPACT",
    "InitializeSQ4KOUCompactMode();",
    "SetSQ4KOUCompactMode(true);",
    "this.ClientSize = new Size(313, 313);",
    "if (e.Button == MouseButtons.Right)",
    "e.Clicks >= 2",
    "RestoreSQ4KOURadarState();",
    "EnsurePA3GHMNativePanelSize();",
]
for token in required_div:
    if token not in d:
        raise RuntimeError(f"Diversity compact audit missing: {token}")

if "SQ4KOU_CMASIO_ALWAYS_VISIBLE" not in s:
    raise RuntimeError("cmASIO always-visible audit marker missing")
if re.search(r"if\s*\(portaudio_issue\s*\|\|\s*\(!cmasio_config_flag\s*&&\s*!ignore\)\s*\)\s*\{\s*tcAudio\.TabPages\.Remove\(tpCMAsio\);", s, re.S):
    raise RuntimeError("Legacy cmASIO TabPages.Remove visibility gate still active")

print("DIVERSITY_COMPACT_CMASIO_PATCH=PASS")
