using System;
using System.Windows.Forms;

namespace Thetis
{
    public partial class Console
    {
        // SQ4KOU SDR-VST3 lifecycle adapter: keep the host's own Form lifecycle untouched.
        // One-shot Idle hook repairs/inserts the menu only after the main Console is visible.
        private static readonly bool _sq4kouWaterfallIdMenuHook = InstallWaterfallIdMenuHook();

        private static bool InstallWaterfallIdMenuHook()
        {
            Application.Idle += Sq4kouWaterfallIdMenuIdle;
            return true;
        }

        private static void Sq4kouWaterfallIdMenuIdle(object sender, EventArgs e)
        {
            foreach (Form form in Application.OpenForms)
            {
                Console console = form as Console;
                if (console == null || !console.Visible)
                    continue;

                console.EnsureWaterfallIDMenuVisible();
                Application.Idle -= Sq4kouWaterfallIdMenuIdle;
                break;
            }
        }

        private void EnsureWaterfallIDMenuVisible()
        {
            if (menuStrip1 == null) return;

            if (_waterfallIdMenuItem == null)
            {
                _waterfallIdMenuItem = new ToolStripMenuItem("Waterfall ID");
                _waterfallIdMenuItem.Name = "waterfallIdToolStripMenuItem";
            }

            // Always route the visible menu entry to the advanced test UI. The legacy handler
            // remains compiled as a fallback implementation but is deliberately detached here.
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

            // Match the native CWX menu item, including the normal-state text colour. This fixes
            // the previously invisible label which only became readable under the hover renderer.
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
