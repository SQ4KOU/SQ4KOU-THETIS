using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Thetis
{
    public partial class Console
    {
        private ToolStripMenuItem _pan3DMenuItem;
        private ToolStripMenuItem _pan3DEnableMenuItem;
        private ToolStripMenuItem _pan3DSettingsMenuItem;
        private frm3DPanadapter _pan3DSettingsForm;
        private bool _pan3DStateRestored;

        private void Ensure3DPanadapterMenuVisible()
        {
            if (menuStrip1 == null) return;

            if (!_pan3DStateRestored)
            {
                Display.RestorePan3DPersisted();
                _pan3DStateRestored = true;
            }

            if (_pan3DMenuItem == null)
            {
                _pan3DMenuItem = new ToolStripMenuItem("3D Panadapter");
                _pan3DMenuItem.Name = "pan3DToolStripMenuItem";

                _pan3DEnableMenuItem = new ToolStripMenuItem("Enable 3D");
                _pan3DEnableMenuItem.Name = "pan3DEnableToolStripMenuItem";
                _pan3DEnableMenuItem.CheckOnClick = true;
                _pan3DEnableMenuItem.Click += Pan3DEnableMenuItem_Click;

                _pan3DSettingsMenuItem = new ToolStripMenuItem("Settings...");
                _pan3DSettingsMenuItem.Name = "pan3DSettingsToolStripMenuItem";
                _pan3DSettingsMenuItem.Click += Pan3DSettingsMenuItem_Click;

                _pan3DMenuItem.DropDownItems.Add(_pan3DEnableMenuItem);
                _pan3DMenuItem.DropDownItems.Add(new ToolStripSeparator());
                _pan3DMenuItem.DropDownItems.Add(_pan3DSettingsMenuItem);
            }

            _pan3DEnableMenuItem.Checked = Display.Pan3DEnabled;

            if (_pan3DMenuItem.Owner != menuStrip1)
            {
                ToolStripItem existing = menuStrip1.Items["pan3DToolStripMenuItem"];
                if (existing != null && !Object.ReferenceEquals(existing, _pan3DMenuItem))
                    menuStrip1.Items.Remove(existing);

                if (_pan3DMenuItem.Owner != null)
                    _pan3DMenuItem.Owner.Items.Remove(_pan3DMenuItem);

                int insertIndex = -1;
                ToolStripItem wf = menuStrip1.Items["waterfallIdToolStripMenuItem"];
                if (wf != null) insertIndex = menuStrip1.Items.IndexOf(wf) + 1;
                if (insertIndex < 0)
                {
                    insertIndex = menuStrip1.Items.IndexOf(cWXToolStripMenuItem);
                    if (insertIndex >= 0) insertIndex++;
                }
                if (insertIndex < 0 || insertIndex > menuStrip1.Items.Count)
                    insertIndex = menuStrip1.Items.Count;

                menuStrip1.Items.Insert(insertIndex, _pan3DMenuItem);
            }

            if (cWXToolStripMenuItem != null)
            {
                _pan3DMenuItem.ForeColor = cWXToolStripMenuItem.ForeColor;
                _pan3DMenuItem.BackColor = cWXToolStripMenuItem.BackColor;
                _pan3DMenuItem.Font = cWXToolStripMenuItem.Font;
                _pan3DMenuItem.TextAlign = cWXToolStripMenuItem.TextAlign;
                _pan3DMenuItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
            }

            _pan3DMenuItem.Visible = true;
            _pan3DMenuItem.Enabled = true;
        }

        private void Pan3DEnableMenuItem_Click(object sender, EventArgs e)
        {
            Display.Pan3DEnabled = _pan3DEnableMenuItem.Checked;
            try
            {
                DB.SaveVars("3DPanadapter", new List<string>
                {
                    "chk3DEnabled/" + Display.Pan3DEnabled.ToString()
                });
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
        }

        private void Pan3DSettingsMenuItem_Click(object sender, EventArgs e)
        {
            if (_pan3DSettingsForm == null || _pan3DSettingsForm.IsDisposed)
                _pan3DSettingsForm = new frm3DPanadapter();

            if (!_pan3DSettingsForm.Visible)
                _pan3DSettingsForm.Show(this);
            else
            {
                _pan3DSettingsForm.BringToFront();
                _pan3DSettingsForm.Activate();
            }
        }
    }
}
