using System;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis
{
    // Settings surface ported from SDR-VST3, kept independent of its Vortice/.NET 10 renderer.
    public sealed class frm3DPanadapter : Form
    {
        private bool _initializing;
        private ToolTip _tips;
        private CheckBoxTS _waterfallSync;
        private CheckBoxTS _sideWalls;
        private NumericUpDownTS _perspective;
        private NumericUpDownTS _depth;
        private NumericUpDownTS _ridge;
        private NumericUpDownTS _haze;
        private NumericUpDownTS _lines;
        private NumericUpDownTS _speed;
        private NumericUpDownTS _floorLift;
        private ColorButton _lineColor;
        private CheckBoxTS _fillEnable;
        private ColorButton _fillColor;
        private TrackBarTS _fillOpacity;
        private ComboBoxTS _colorMap;
        private ButtonTS _reset;

        public frm3DPanadapter()
        {
            _initializing = true;
            InitializeComponent();
            Common.RestoreForm(this, "3DPanadapter", false);
            Common.ForceFormOnScreen(this);
            if (_colorMap.SelectedIndex < 0) _colorMap.SelectedIndex = 0;
            _initializing = false;
            PushAllSettings();
        }

        private LabelTS AddLabel(string text, int y)
        {
            LabelTS l = new LabelTS();
            l.Text = text;
            l.Image = null;
            l.Location = new Point(12, y + 3);
            l.Size = new Size(100, 18);
            Controls.Add(l);
            return l;
        }

        private void InitializeComponent()
        {
            _tips = new ToolTip();
            Text = "3D Panadapter Settings - SQ4KOU";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(266, 390);
            Name = "frm3DPanadapter";

            int y = 12;

            _waterfallSync = new CheckBoxTS();
            _waterfallSync.Name = "chk3DWaterfallSync";
            _waterfallSync.Text = "Waterfall Sync";
            _waterfallSync.Checked = true;
            _waterfallSync.Image = null;
            _waterfallSync.Location = new Point(12, y);
            _waterfallSync.Size = new Size(112, 20);
            _tips.SetToolTip(_waterfallSync, "Use the current RX waterfall palette and levels for the 3D surface.");
            Controls.Add(_waterfallSync);

            _sideWalls = new CheckBoxTS();
            _sideWalls.Name = "chk3DSideWalls";
            _sideWalls.Text = "Side Walls";
            _sideWalls.Checked = true;
            _sideWalls.Image = null;
            _sideWalls.Location = new Point(136, y);
            _sideWalls.Size = new Size(100, 20);
            Controls.Add(_sideWalls);
            y += 34;

            AddLabel("Perspective:", y);
            _perspective = new NumericUpDownTS();
            _perspective.Name = "ud3DXOffset";
            _perspective.DecimalPlaces = 2;
            _perspective.Increment = 0.05m;
            _perspective.Minimum = 0.10m;
            _perspective.Maximum = 1.00m;
            _perspective.Value = 0.60m;
            _perspective.Location = new Point(126, y);
            _perspective.Size = new Size(70, 20);
            Controls.Add(_perspective);
            y += 25;

            AddLabel("Depth:", y);
            _depth = new NumericUpDownTS();
            _depth.Name = "ud3DYOffset";
            _depth.DecimalPlaces = 2;
            _depth.Increment = 0.05m;
            _depth.Minimum = 0m;
            _depth.Maximum = 1m;
            _depth.Value = 0.58m;
            _depth.Location = new Point(126, y);
            _depth.Size = new Size(70, 20);
            Controls.Add(_depth);
            y += 25;

            AddLabel("Ridge Height:", y);
            _ridge = new NumericUpDownTS();
            _ridge.Name = "ud3DRidgeHeight";
            _ridge.DecimalPlaces = 2;
            _ridge.Increment = 0.02m;
            _ridge.Minimum = 0.10m;
            _ridge.Maximum = 1m;
            _ridge.Value = 0.46m;
            _ridge.Location = new Point(126, y);
            _ridge.Size = new Size(70, 20);
            Controls.Add(_ridge);
            y += 25;

            AddLabel("Haze:", y);
            _haze = new NumericUpDownTS();
            _haze.Name = "ud3DHaze";
            _haze.DecimalPlaces = 2;
            _haze.Increment = 0.02m;
            _haze.Minimum = 0m;
            _haze.Maximum = 1m;
            _haze.Value = 0.16m;
            _haze.Location = new Point(126, y);
            _haze.Size = new Size(70, 20);
            Controls.Add(_haze);
            y += 25;

            AddLabel("Depth Lines:", y);
            _lines = new NumericUpDownTS();
            _lines.Name = "ud3DLineCount";
            _lines.Minimum = 2;
            _lines.Maximum = Display.Max3DHistoryLines;
            _lines.Value = 35;
            _lines.Location = new Point(126, y);
            _lines.Size = new Size(70, 20);
            Controls.Add(_lines);
            y += 25;

            AddLabel("Speed FPS:", y);
            _speed = new NumericUpDownTS();
            _speed.Name = "ud3DSpeed";
            _speed.Minimum = 1;
            _speed.Maximum = 60;
            _speed.Value = 25;
            _speed.Location = new Point(126, y);
            _speed.Size = new Size(70, 20);
            Controls.Add(_speed);
            y += 25;

            AddLabel("Floor Lift:", y);
            _floorLift = new NumericUpDownTS();
            _floorLift.Name = "ud3DZCurve";
            _floorLift.DecimalPlaces = 2;
            _floorLift.Increment = 0.05m;
            _floorLift.Minimum = 0.05m;
            _floorLift.Maximum = 1m;
            _floorLift.Value = 0.90m;
            _floorLift.Location = new Point(126, y);
            _floorLift.Size = new Size(70, 20);
            Controls.Add(_floorLift);
            y += 27;

            AddLabel("Ridge Color:", y);
            _lineColor = new ColorButton();
            _lineColor.Name = "clrbtn3DLineColor";
            _lineColor.Automatic = "Automatic";
            _lineColor.MoreColors = "More Colors...";
            _lineColor.Color = Color.Aquamarine;
            _lineColor.Image = null;
            _lineColor.Selectable = true;
            _lineColor.Location = new Point(126, y);
            _lineColor.Size = new Size(52, 22);
            Controls.Add(_lineColor);
            y += 28;

            _fillEnable = new CheckBoxTS();
            _fillEnable.Name = "chk3DFillColorEnable";
            _fillEnable.Text = "Front Fill";
            _fillEnable.Image = null;
            _fillEnable.Location = new Point(12, y);
            _fillEnable.Size = new Size(94, 20);
            Controls.Add(_fillEnable);

            _fillColor = new ColorButton();
            _fillColor.Name = "clrbtn3DFillColor";
            _fillColor.Automatic = "Automatic";
            _fillColor.MoreColors = "More Colors...";
            _fillColor.Color = Color.Aquamarine;
            _fillColor.Image = null;
            _fillColor.Selectable = true;
            _fillColor.Location = new Point(126, y);
            _fillColor.Size = new Size(52, 22);
            Controls.Add(_fillColor);

            _fillOpacity = new TrackBarTS();
            _fillOpacity.Name = "tb3DFillOpacity";
            _fillOpacity.Minimum = 0;
            _fillOpacity.Maximum = 100;
            _fillOpacity.Value = 55;
            _fillOpacity.TickStyle = TickStyle.None;
            _fillOpacity.AutoSize = false;
            _fillOpacity.Location = new Point(182, y);
            _fillOpacity.Size = new Size(68, 22);
            Controls.Add(_fillOpacity);
            y += 31;

            AddLabel("Colormap:", y);
            _colorMap = new ComboBoxTS();
            _colorMap.Name = "combo3DColorMap";
            _colorMap.DropDownStyle = ComboBoxStyle.DropDownList;
            _colorMap.Items.AddRange(new object[] { "Classic", "Turbo", "Viridis", "Inferno" });
            _colorMap.SelectedIndex = 0;
            _colorMap.Location = new Point(126, y);
            _colorMap.Size = new Size(112, 21);
            Controls.Add(_colorMap);
            y += 34;

            _reset = new ButtonTS();
            _reset.Text = "Reset Defaults";
            _reset.Image = null;
            _reset.Location = new Point(12, y);
            _reset.Size = new Size(226, 27);
            Controls.Add(_reset);

            _waterfallSync.CheckedChanged += delegate { ApplyIfReady(); };
            _sideWalls.CheckedChanged += delegate { ApplyIfReady(); };
            _perspective.ValueChanged += delegate { ApplyIfReady(); };
            _depth.ValueChanged += delegate { ApplyIfReady(); };
            _ridge.ValueChanged += delegate { ApplyIfReady(); };
            _haze.ValueChanged += delegate { ApplyIfReady(); };
            _lines.ValueChanged += delegate { ApplyIfReady(); };
            _speed.ValueChanged += delegate { ApplyIfReady(); };
            _floorLift.ValueChanged += delegate { ApplyIfReady(); };
            _lineColor.Changed += delegate { ApplyIfReady(); };
            _fillEnable.CheckedChanged += delegate { ApplyIfReady(); };
            _fillColor.Changed += delegate { ApplyIfReady(); };
            _fillOpacity.Scroll += delegate { ApplyIfReady(); };
            _colorMap.SelectedIndexChanged += delegate { ApplyIfReady(); };
            _reset.Click += Reset_Click;
            FormClosing += Form_FormClosing;
        }

        private void ApplyIfReady()
        {
            if (!_initializing) PushAllSettings();
        }

        private void PushAllSettings()
        {
            Display.Pan3DWaterfallSync = _waterfallSync.Checked;
            Display.Pan3DSideWalls = _sideWalls.Checked;
            Display.Pan3DPerspective = (float)_perspective.Value;
            Display.Pan3DDepth = (float)_depth.Value;
            Display.Pan3DRidgeHeight = (float)_ridge.Value;
            Display.Pan3DDepthFade = (float)_haze.Value;
            Display.Pan3DLineCount = (int)_lines.Value;
            Display.Pan3DSpeed = (int)_speed.Value;
            Display.Pan3DZCurve = (float)_floorLift.Value;
            Display.Pan3DLineColor = _lineColor.Color;
            Display.Pan3DFillColorEnabled = _fillEnable.Checked;
            Display.Pan3DFillColor = _fillColor.Color;
            Display.Pan3DFillAlpha = _fillOpacity.Value / 100f;
            Display.Pan3DColorMap = Math.Max(0, _colorMap.SelectedIndex);
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            _initializing = true;
            _waterfallSync.Checked = true;
            _sideWalls.Checked = true;
            _perspective.Value = 0.60m;
            _depth.Value = 0.58m;
            _ridge.Value = 0.46m;
            _haze.Value = 0.16m;
            _lines.Value = 35;
            _speed.Value = 25;
            _floorLift.Value = 0.90m;
            _lineColor.Color = Color.Aquamarine;
            _fillEnable.Checked = false;
            _fillColor.Color = Color.Aquamarine;
            _fillOpacity.Value = 55;
            _colorMap.SelectedIndex = 0;
            _initializing = false;
            PushAllSettings();
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            Common.SaveForm(this, "3DPanadapter");
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Hide();
                e.Cancel = true;
            }
        }
    }
}
