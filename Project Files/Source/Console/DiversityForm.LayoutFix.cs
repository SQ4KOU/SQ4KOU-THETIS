using System;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis
{
    public partial class DiversityForm
    {
        private bool _sq4kouDiversityLayoutFixInstalled;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            RestoreCompleteDiversityLayout();

            if (!_sq4kouDiversityLayoutFixInstalled)
            {
                _sq4kouDiversityLayoutFixInstalled = true;
                SizeChanged += DiversityLayoutFix_SizeChanged;
            }
        }

        private void DiversityLayoutFix_SizeChanged(object sender, EventArgs e)
        {
            RestoreCompleteDiversityLayout();
        }

        private void RestoreCompleteDiversityLayout()
        {
            // Keep the complete stock Thetis Diversity UI in its original 313 px left column.
            // PA3GHM extends the form to the right; it must not resize or cover the stock controls.
            if (panelDivControls != null)
            {
                panelDivControls.Visible = true;
                panelDivControls.Location = new Point(4, 22);
                panelDivControls.Size = new Size(305, 262);
                panelDivControls.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                panelDivControls.BringToFront();
            }

            if (picRadar != null)
            {
                picRadar.Visible = true;
                picRadar.Location = new Point(4, 289);
                picRadar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
                picRadar.Size = new Size(305, Math.Max(305, ClientSize.Height - 293));
            }

            // In the stock form this control is at x=247. Its original Top|Right anchor moves
            // it underneath the PA3GHM panel when the form is widened from 313 to 720 px.
            if (chkAlwaysOnTop != null)
            {
                chkAlwaysOnTop.Visible = true;
                chkAlwaysOnTop.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                chkAlwaysOnTop.Location = new Point(247, 5);
                chkAlwaysOnTop.BringToFront();
            }

            if (grpPA3GHMNative != null)
            {
                grpPA3GHMNative.Location = new Point(320, 6);
                grpPA3GHMNative.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                grpPA3GHMNative.Size = new Size(
                    Math.Max(392, ClientSize.Width - 328),
                    Math.Max(582, ClientSize.Height - 12));
                grpPA3GHMNative.BringToFront();
            }
        }
    }
}
