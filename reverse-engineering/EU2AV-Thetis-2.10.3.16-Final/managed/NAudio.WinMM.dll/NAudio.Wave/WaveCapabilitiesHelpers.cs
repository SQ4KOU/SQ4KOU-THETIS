using System;
using Microsoft.Win32;

namespace NAudio.Wave;

public static class WaveCapabilitiesHelpers
{
	public static readonly Guid MicrosoftDefaultManufacturerId = new Guid("d5a47fa8-6d98-11d1-a21a-00a0c9223196");

	public static readonly Guid DefaultWaveOutGuid = new Guid("E36DC310-6D9A-11D1-A21A-00A0C9223196");

	public static readonly Guid DefaultWaveInGuid = new Guid("E36DC311-6D9A-11D1-A21A-00A0C9223196");

	public static string GetNameFromGuid(Guid guid)
	{
		string result = null;
		RegistryKey val = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Control\\MediaCategories");
		try
		{
			RegistryKey val2 = val.OpenSubKey(guid.ToString("B"));
			try
			{
				if (val2 != null)
				{
					result = val2.GetValue("Name") as string;
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		return result;
	}
}
