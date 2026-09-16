using System;
using System.Windows.Forms;

namespace Thetis
{
    public partial class Console
    {
        // The main Console class already owns the Form.OnShown override.
        // Keep the Waterfall ID menu repair isolated here; the build integration
        // calls this helper from the existing lifecycle path instead of declaring
        // a second OnShown override.
        private void EnsureWaterfallIDMenuVisible()
        {
            if (menuStrip1 == null) return;

            if (_waterfallIdMenuItem == null)
            {
                _waterfallIdMenuItem = new ToolStripMenuItem("Waterfall ID");
                _waterfallIdMenuItem.Name = "waterfallIdToolStripMenuItem";
            }

            _waterfallIdMenuItem.Click -= WaterfallIdMenuItem_Click;
            _waterfallIdMenuItem.Click -= WaterfallIdAdvancedMenuItem_Click;
            _waterfallIdMenuItem.Click += WaterfallIdAdvancedMenuItem_Click;
            _waterfallIdMenuItem.ToolTipText = "Transmit text or bitmap as an on-air waterfall image";

            if (_waterfallIdMenuItem.Owner != menuStrip1)
            {
                ToolStripItem existing = menuStrip1.Items["waterfallIdToolStripMenuItem"];
                if (existing != null && !object.ReferenceEquals(existing, _waterfallIdMenuItem))
                    menuStrip1.Items.Remove(existing);

                if (_waterfallIdMenuItem.Owner != null)
                    _waterfallIdMenuItem.Owner.Items.Remove(_waterfallIdMenuItem);

                int insertIndex = menuStrip1.Items.IndexOf(cWXToolStripMenuItem);
                if (insertIndex < 0)
                    insertIndex = 0;
                else
                    insertIndex++;

                menuStrip1.Items.Insert(insertIndex, _waterfallIdMenuItem);
            }

            if (cWXToolStripMenuItem != null)
            {
                _waterfallIdMenuItem.ForeColor = cWXToolStripMenuItem.ForeColor;
                _waterfallIdMenuItem.BackColor = cWXToolStripMenuItem.BackColor;
                _waterfallIdMenuItem.Font = cWXToolStripMenuItem.Font;
                _waterfallIdMenuItem.TextAlign = cWXToolStripMenuItem.TextAlign;
                _waterfallIdMenuItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
            }

            _waterfallIdMenuItem.Visible = true;
            _waterfallIdMenuItem.Enabled = true;
        }
    }
}
