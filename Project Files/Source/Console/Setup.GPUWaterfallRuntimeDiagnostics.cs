using System;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis
{
    public partial class Setup
    {
        private bool _gpuRuntimeDiagnosticsInstalled;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            InstallGPUWaterfallRuntimeDiagnostics();
        }

        private void InstallGPUWaterfallRuntimeDiagnostics()
        {
            if (_gpuRuntimeDiagnosticsInstalled) return;
            _gpuRuntimeDiagnosticsInstalled = true;

            // The recovered control was deliberately hidden in the previous integration.
            // It must be visible so the operator can explicitly see/force the managed FFT path.
            if (chkGPUWaterfallFFT != null)
            {
                chkGPUWaterfallFFT.Visible = true;
                chkGPUWaterfallFFT.Checked = Display.GPUWaterfallPipelineEnabled;
                chkGPUWaterfallFFT.BringToFront();
            }

            // Reuse the existing two-second GPU status timer.  This handler is attached
            // after the legacy one, so the final label reports the ACTUAL managed FFT/
            // renderer state instead of only the requested GPU mode.
            if (_gpuStatusTimer != null)
                _gpuStatusTimer.Tick += GPUWaterfallRuntimeStatus_Tick;

            UpdateGPUWaterfallRuntimeStatusLabel();
        }

        private void GPUWaterfallRuntimeStatus_Tick(object sender, EventArgs e)
        {
            UpdateGPUWaterfallRuntimeStatusLabel();
        }

        private void UpdateGPUWaterfallRuntimeStatusLabel()
        {
            if (lblGPUInfo == null || lblGPUInfo.IsDisposed) return;

            string rx1 = Display.GPUWaterfallRuntimeStatusRX1;
            string rx2 = Display.GPUWaterfallRuntimeStatusRX2;
            lblGPUInfo.Text = "RX1: " + rx1 + "\nRX2: " + rx2;
            lblGPUInfo.ForeColor = (Display.GPUWaterfallActualActiveRX1 || Display.GPUWaterfallActualActiveRX2)
                ? Color.DarkGreen
                : Color.Firebrick;
        }
    }
}
