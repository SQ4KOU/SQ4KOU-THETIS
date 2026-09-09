using System;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis
{
    public partial class DiversityForm
    {
        private bool _sq4kouDiversityLayoutFixInstalled;
        private bool _sq4kouCompactDiversity;
        private bool _sq4kouApplyingDiversityLayout;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (!_sq4kouDiversityLayoutFixInstalled)
            {
                _sq4kouDiversityLayoutFixInstalled = true;
                this.DoubleClick += DiversityLayout_DoubleClick;
                if (picRadar != null)
                    picRadar.DoubleClick += DiversityLayout_DoubleClick;
            }

            // Default: complete Thetis Diversity UI plus the PA3GHM pane.
            _sq4kouCompactDiversity = false;
            ApplyDiversityLayout();
        }

        private void DiversityLayout_DoubleClick(object sender, EventArgs e)
        {
            _sq4kouCompactDiversity = !_sq4kouCompactDiversity;
            ApplyDiversityLayout();
        }

        private void ApplyDiversityLayout()
        {
            if (_sq4kouApplyingDiversityLayout) return;
            _sq4kouApplyingDiversityLayout = true;

            try
            {
                if (_sq4kouCompactDiversity)
                {
                    // Compact mode: title bar + radar only.
                    this.MinimumSize = new Size(329, 352);
                    this.ClientSize = new Size(313, 313);

                    if (panelDivControls != null)
                        panelDivControls.Visible = false;

                    if (chkAlwaysOnTop != null)
                        chkAlwaysOnTop.Visible = false;

                    if (grpPA3GHMNative != null)
                        grpPA3GHMNative.Visible = false;

                    if (picRadar != null)
                    {
                        picRadar.Visible = true;
                        picRadar.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                        picRadar.Location = new Point(4, 4);
                        picRadar.Size = new Size(305, 305);
                        picRadar.BringToFront();
                    }
                }
                else
                {
                    // Full mode: original 313 px Thetis column + native PA3GHM pane.
                    this.MinimumSize = new Size(736, 637);
                    this.ClientSize = new Size(720, 598);

                    if (panelDivControls != null)
                    {
                        panelDivControls.Visible = true;
                        panelDivControls.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                        panelDivControls.Location = new Point(4, 22);
                        panelDivControls.Size = new Size(305, 262);
                        panelDivControls.BringToFront();
                    }

                    if (chkAlwaysOnTop != null)
                    {
                        chkAlwaysOnTop.Visible = true;
                        chkAlwaysOnTop.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                        chkAlwaysOnTop.Location = new Point(247, 5);
                        chkAlwaysOnTop.BringToFront();
                    }

                    if (picRadar != null)
                    {
                        picRadar.Visible = true;
                        picRadar.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                        picRadar.Location = new Point(4, 289);
                        picRadar.Size = new Size(305, 305);
                    }

                    if (grpPA3GHMNative != null)
                    {
                        grpPA3GHMNative.Visible = true;
                        grpPA3GHMNative.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                        grpPA3GHMNative.Location = new Point(320, 6);
                        grpPA3GHMNative.Size = new Size(
                            Math.Max(392, ClientSize.Width - 328),
                            Math.Max(582, ClientSize.Height - 12));
                        grpPA3GHMNative.BringToFront();
                    }
                }

                if (picRadar != null)
                    picRadar.Invalidate();
            }
            finally
            {
                _sq4kouApplyingDiversityLayout = false;
            }
        }
    }
}
