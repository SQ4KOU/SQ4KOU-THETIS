using System;
using System.Windows.Forms;

namespace Thetis
{
    public partial class Console
    {
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            EnsureWaterfallIDMenuVisible();
        }

        private void EnsureWaterfallIDMenuVisible()
        {
            if (menuStrip1 == null) return;

            if (_waterfallIdMenuItem == null)
            {
                _waterfallIdMenuItem = new ToolStripMenuItem("Waterfall ID");
                _waterfallIdMenuItem.Name = "waterfallIdToolStripMenuItem";
                _waterfallIdMenuItem.ToolTipText = "Transmit text as an on-air waterfall image";
                _waterfallIdMenuItem.Click += WaterfallIdMenuItem_Click;
            }

            if (_waterfallIdMenuItem.Owner != menuStrip1)
            {
                ToolStripItem existing = menuStrip1.Items["waterfallIdToolStripMenuItem"];
                if (existing != null)
                    menuStrip1.Items.Remove(existing);

                int insertIndex = menuStrip1.Items.IndexOf(cWXToolStripMenuItem);
                if (insertIndex < 0)
                    insertIndex = 0;
                else
                    insertIndex++;

                menuStrip1.Items.Insert(insertIndex, _waterfallIdMenuItem);
            }

            _waterfallIdMenuItem.Visible = true;
            _waterfallIdMenuItem.Enabled = true;
        }
    }
}
