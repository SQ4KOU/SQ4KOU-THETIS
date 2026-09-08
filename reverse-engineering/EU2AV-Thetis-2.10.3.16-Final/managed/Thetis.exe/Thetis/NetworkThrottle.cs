using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Thetis;

internal static class NetworkThrottle
{
	public static bool GetNetworkThrottle(out int throttle, bool showErrors = true)
	{
		bool result = false;
		throttle = 0;
		RegistryKey registryKey = null;
		try
		{
			registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
		}
		catch
		{
			if (showErrors)
			{
				MessageBox.Show("Unable to open LocalMachine registry base key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			}
		}
		if (registryKey != null)
		{
			RegistryKey registryKey2 = null;
			try
			{
				registryKey2 = registryKey.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile", writable: false);
			}
			catch
			{
				if (showErrors)
				{
					MessageBox.Show("Unable to open SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile registry key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
				}
			}
			if (registryKey2 != null)
			{
				try
				{
					object value = registryKey2.GetValue("NetworkThrottlingIndex");
					if (value != null)
					{
						if (value is int)
						{
							throttle = (int)value;
							result = true;
						}
						else if (showErrors)
						{
							MessageBox.Show("Unsuitable value in NetworkThrottlingIndex key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
						}
					}
				}
				catch
				{
					if (showErrors)
					{
						MessageBox.Show("Unable to GetValue on NetworkThrottlingIndex registry entry.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
					}
				}
				registryKey2.Close();
			}
			registryKey.Close();
		}
		return result;
	}

	public static bool SetNetworkThrottle(int throttle)
	{
		bool result = false;
		if (Common.IsAdministrator())
		{
			RegistryKey registryKey = null;
			try
			{
				registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
			}
			catch
			{
				MessageBox.Show("Unable to open LocalMachine registry base key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			}
			if (registryKey != null)
			{
				RegistryKey registryKey2 = null;
				try
				{
					registryKey2 = registryKey.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile", writable: true);
				}
				catch
				{
					MessageBox.Show("Unable to open SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile registry key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
				}
				if (registryKey2 != null)
				{
					try
					{
						registryKey2.SetValue("NetworkThrottlingIndex", throttle, RegistryValueKind.DWord);
						result = true;
					}
					catch
					{
						MessageBox.Show("Unable to SetValue on NetworkThrottlingIndex registry entry.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
					}
					registryKey2.Close();
				}
				registryKey.Close();
			}
		}
		else
		{
			MessageBox.Show("You need to be an Administrator. Please run Thetis 'As Administrator'.", "No Administrator Rights", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
		}
		return result;
	}
}
