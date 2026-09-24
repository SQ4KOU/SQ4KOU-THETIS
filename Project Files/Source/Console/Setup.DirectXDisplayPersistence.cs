using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Thetis
{
    public partial class Setup
    {
        // DirectX/3D controls live on the main Setup form and are restored from the
        // Options table. Historically several GPU controls were intentionally
        // session-only, and the normal close-time SaveForm path is not reliable when
        // the application is terminated with Setup already hidden. Persist this small
        // group immediately and flush the database after every user-visible change.
        private bool _directXDisplayPersistenceHooksInstalled;
        private bool _directXDisplayPersisting;

        // Force CPU mode temporarily clears the legacy GPU checkboxes. Keep the user's
        // real GPU preferences separately so those forced UI changes never overwrite
        // the saved choices.
        private bool _directXGpuPreferencesCaptured;
        private bool _directXGpuMeshPreference;
        private bool _directXGpuComputePreference;
        private bool _directXGpuOverlayPreference;

        internal void InstallDirectXDisplayPersistenceHooks()
        {
            if (_directXDisplayPersistenceHooksInstalled || IsDisposed)
                return;

            _directXDisplayPersistenceHooksInstalled = true;
            CaptureDirectXGpuPreferences();

            HookDirectXCheckBox("chkDisplay3DPanadapter");
            HookDirectXCheckBox("chkShowFPS");
            HookDirectXCheckBox("chkAntiAlias");
            HookDirectXCheckBox("chkGpuMesh3D");
            HookDirectXCheckBox("chkGpuComputeShaders");
            HookDirectXCheckBox("chkGpuOverlay");
            HookDirectXCheckBox("chkVSyncDX");
            HookDirectXCheckBox("chkForceCPURendering");
            HookDirectXComboBox("comboDisplayThreadPriority");

            FormClosing += DirectXDisplayPersistence_FormClosing;
        }

        private T FindDirectXDisplayControl<T>(string name) where T : Control
        {
            Control[] found = Controls.Find(name, true);
            if (found == null)
                return null;

            for (int i = 0; i < found.Length; i++)
            {
                if (found[i] is T control)
                    return control;
            }

            return null;
        }

        private void HookDirectXCheckBox(string name)
        {
            CheckBox control = FindDirectXDisplayControl<CheckBox>(name);
            if (control != null)
                control.CheckedChanged += DirectXDisplaySettingChanged;
        }

        private void HookDirectXComboBox(string name)
        {
            ComboBox control = FindDirectXDisplayControl<ComboBox>(name);
            if (control != null)
                control.SelectedIndexChanged += DirectXDisplaySettingChanged;
        }

        private bool IsGpuPreferenceControl(Control control)
        {
            if (control == null)
                return false;

            return string.Equals(control.Name, "chkGpuMesh3D", StringComparison.Ordinal) ||
                   string.Equals(control.Name, "chkGpuComputeShaders", StringComparison.Ordinal) ||
                   string.Equals(control.Name, "chkGpuOverlay", StringComparison.Ordinal);
        }

        private bool IsForceCpuChecked()
        {
            CheckBox forceCpu = FindDirectXDisplayControl<CheckBox>("chkForceCPURendering");
            return forceCpu != null && forceCpu.Checked;
        }

        private void CaptureDirectXGpuPreferences()
        {
            CheckBox mesh = FindDirectXDisplayControl<CheckBox>("chkGpuMesh3D");
            CheckBox compute = FindDirectXDisplayControl<CheckBox>("chkGpuComputeShaders");
            CheckBox overlay = FindDirectXDisplayControl<CheckBox>("chkGpuOverlay");

            bool foundAny = false;
            if (mesh != null)
            {
                _directXGpuMeshPreference = mesh.Checked;
                foundAny = true;
            }

            if (compute != null)
            {
                _directXGpuComputePreference = compute.Checked;
                foundAny = true;
            }

            if (overlay != null)
            {
                _directXGpuOverlayPreference = overlay.Checked;
                foundAny = true;
            }

            if (foundAny)
                _directXGpuPreferencesCaptured = true;
        }

        private void DirectXDisplaySettingChanged(object sender, EventArgs e)
        {
            if (_directXDisplayPersisting || IsDisposed)
                return;

            Control changed = sender as Control;

            // When Force CPU is active the legacy handler clears GPU checkboxes only
            // for runtime gating. Do not treat those programmatic clears as new user
            // preferences. Otherwise refresh the preference snapshot immediately.
            if (!IsForceCpuChecked() && IsGpuPreferenceControl(changed))
                CaptureDirectXGpuPreferences();

            PersistDirectXDisplaySettingsNow();
        }

        private void DirectXDisplayPersistence_FormClosing(object sender, FormClosingEventArgs e)
        {
            PersistDirectXDisplaySettingsNow();
        }

        private static void StoreDirectXCheckBox(Dictionary<string, string> options, CheckBox control)
        {
            if (control != null && !string.IsNullOrEmpty(control.Name))
                options[control.Name] = control.Checked.ToString();
        }

        private static void StoreDirectXComboBox(Dictionary<string, string> options, ComboBox control)
        {
            if (control != null && !string.IsNullOrEmpty(control.Name))
                options[control.Name] = control.Text ?? string.Empty;
        }

        private void PersistDirectXDisplaySettingsNow()
        {
            if (_directXDisplayPersisting || IsDisposed || DB.ds == null)
                return;

            _directXDisplayPersisting = true;
            try
            {
                Dictionary<string, string> options = DB.GetVarsDictionary("Options");
                if (options == null)
                    options = new Dictionary<string, string>(StringComparer.Ordinal);

                StoreDirectXCheckBox(options, FindDirectXDisplayControl<CheckBox>("chkDisplay3DPanadapter"));
                StoreDirectXComboBox(options, FindDirectXDisplayControl<ComboBox>("comboDisplayThreadPriority"));
                StoreDirectXCheckBox(options, FindDirectXDisplayControl<CheckBox>("chkShowFPS"));
                StoreDirectXCheckBox(options, FindDirectXDisplayControl<CheckBox>("chkAntiAlias"));
                StoreDirectXCheckBox(options, FindDirectXDisplayControl<CheckBox>("chkVSyncDX"));
                StoreDirectXCheckBox(options, FindDirectXDisplayControl<CheckBox>("chkForceCPURendering"));

                if (!IsForceCpuChecked())
                {
                    CaptureDirectXGpuPreferences();
                    StoreDirectXCheckBox(options, FindDirectXDisplayControl<CheckBox>("chkGpuMesh3D"));
                    StoreDirectXCheckBox(options, FindDirectXDisplayControl<CheckBox>("chkGpuComputeShaders"));
                    StoreDirectXCheckBox(options, FindDirectXDisplayControl<CheckBox>("chkGpuOverlay"));
                }
                else if (_directXGpuPreferencesCaptured)
                {
                    // Keep the choices the user made before entering WARP/CPU mode.
                    options["chkGpuMesh3D"] = _directXGpuMeshPreference.ToString();
                    options["chkGpuComputeShaders"] = _directXGpuComputePreference.ToString();
                    options["chkGpuOverlay"] = _directXGpuOverlayPreference.ToString();
                }

                DB.SaveVarsDictionary("Options", ref options, true);
                DB.WriteDB();
            }
            catch (Exception ex)
            {
                Common.LogString("DirectX/3D settings persistence failed: " + ex.Message);
            }
            finally
            {
                _directXDisplayPersisting = false;
            }
        }
    }
}
