using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Thetis;

internal static class CMASIOConfig
{
	private const string _registry_path = "SOFTWARE\\OpenHPSDR\\Thetis-x64";

	private static RegistryKey openRegistryKey()
	{
		RegistryKey registryKey = null;
		RegistryView view = (Common.Is64Bit ? RegistryView.Registry64 : RegistryView.Registry32);
		try
		{
			registryKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view).OpenSubKey("SOFTWARE\\OpenHPSDR\\Thetis-x64", writable: true);
		}
		catch
		{
		}
		if (registryKey == null)
		{
			try
			{
				registryKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view).CreateSubKey("SOFTWARE\\OpenHPSDR\\Thetis-x64");
			}
			catch
			{
			}
		}
		return registryKey;
	}

	public static bool DoesRegistryValueExist(string valueName, RegistryKey key = null)
	{
		if (key == null)
		{
			key = openRegistryKey();
		}
		if (key != null)
		{
			return key.GetValue(valueName) != null;
		}
		return false;
	}

	public static string GetASIOdrivername(RegistryKey key = null)
	{
		if (key == null)
		{
			key = openRegistryKey();
		}
		if (key != null)
		{
			object value = key.GetValue("ASIOdrivername");
			if (value is string)
			{
				return (string)value;
			}
		}
		return null;
	}

	public static void SetASIOdrivername(string driverName, RegistryKey key = null)
	{
		if (key == null)
		{
			key = openRegistryKey();
		}
		key?.SetValue("ASIOdrivername", driverName, RegistryValueKind.String);
	}

	public static int GetASIOblocknum(RegistryKey key = null)
	{
		if (key == null)
		{
			key = openRegistryKey();
		}
		if (key != null)
		{
			object value = key.GetValue("ASIOblocknum");
			if (value is int)
			{
				return (int)value & 0xFFFF;
			}
		}
		return 5;
	}

	public static bool GetASIOlockmode(RegistryKey key = null)
	{
		if (key == null)
		{
			key = openRegistryKey();
		}
		if (key != null)
		{
			object value = key.GetValue("ASIOblocknum");
			if (value is int)
			{
				return ((int)value & 0xFFFF0000u) != 0;
			}
		}
		return false;
	}

	public static void SetASIOblocknum(int blockNum, bool lock_mode, RegistryKey key = null)
	{
		if (key == null)
		{
			key = openRegistryKey();
		}
		if (key != null)
		{
			if (lock_mode)
			{
				blockNum |= 0xF0000;
			}
			key.SetValue("ASIOblocknum", blockNum, RegistryValueKind.DWord);
		}
	}

	public static void SetASIObaseinchannel(int base_input_channel, RegistryKey key = null)
	{
		if (key == null)
		{
			key = openRegistryKey();
		}
		key?.SetValue("ASIObaseinchannel", base_input_channel, RegistryValueKind.DWord);
	}

	public static void SetASIObaseoutchannel(int base_output_channel, RegistryKey key = null)
	{
		if (key == null)
		{
			key = openRegistryKey();
		}
		key?.SetValue("ASIObaseoutchannel", base_output_channel, RegistryValueKind.DWord);
	}

	public static int GetASIObaseinchannel(RegistryKey key = null)
	{
		if (key == null)
		{
			key = openRegistryKey();
		}
		if (key != null)
		{
			object value = key.GetValue("ASIObaseinchannel");
			if (value is int)
			{
				return (int)value;
			}
		}
		return 0;
	}

	public static int GetASIObaseoutchannel(RegistryKey key = null)
	{
		if (key == null)
		{
			key = openRegistryKey();
		}
		if (key != null)
		{
			object value = key.GetValue("ASIObaseoutchannel");
			if (value is int)
			{
				return (int)value;
			}
		}
		return 0;
	}

	public static void SetASIOinputmode(int input_mode, RegistryKey key = null)
	{
		if (key == null)
		{
			key = openRegistryKey();
		}
		key?.SetValue("ASIOinputmode", input_mode, RegistryValueKind.DWord);
	}

	public static int GetASIOinputmode(RegistryKey key = null)
	{
		if (key == null)
		{
			key = openRegistryKey();
		}
		if (key != null)
		{
			object value = key.GetValue("ASIOinputmode");
			if (value is int)
			{
				return (int)value;
			}
		}
		return 2;
	}

	private static void deleteRegistryValue(string valueName, RegistryKey key = null)
	{
		if (key == null)
		{
			key = openRegistryKey();
		}
		if (key != null)
		{
			try
			{
				key.DeleteValue(valueName);
			}
			catch
			{
			}
		}
	}

	public unsafe static List<AsioDeviceInfo> GetASIODevices()
	{
		List<AsioDeviceInfo> list = new List<AsioDeviceInfo>();
		int num = PA19.PA_GetDeviceCount();
		PA19.PaStreamParameters paStreamParameters = default(PA19.PaStreamParameters);
		PA19.PaStreamParameters paStreamParameters2 = default(PA19.PaStreamParameters);
		for (int i = 0; i < num; i++)
		{
			PA19.PaDeviceInfo paDeviceInfo = PA19.PA_GetDeviceInfo(i);
			if (PA19.PA_GetHostApiInfo(paDeviceInfo.hostApi).name.Contains("asio", StringComparison.OrdinalIgnoreCase))
			{
				bool flag = paDeviceInfo.maxInputChannels > 0;
				bool flag2 = paDeviceInfo.maxOutputChannels > 0;
				paStreamParameters.device = i;
				paStreamParameters.channelCount = paDeviceInfo.maxOutputChannels;
				paStreamParameters.sampleFormat = 2u;
				paStreamParameters.suggestedLatency = paDeviceInfo.defaultLowOutputLatency;
				paStreamParameters.hostApiSpecificStreamInfo = null;
				paStreamParameters2.device = i;
				paStreamParameters2.channelCount = paDeviceInfo.maxInputChannels;
				paStreamParameters2.sampleFormat = 2u;
				paStreamParameters2.suggestedLatency = paDeviceInfo.defaultLowInputLatency;
				paStreamParameters2.hostApiSpecificStreamInfo = null;
				if (((PA19.PA_IsFormatSupported(&paStreamParameters2, &paStreamParameters, 48000.0) == 0) & flag & flag2) && !string.IsNullOrEmpty(paDeviceInfo.name))
				{
					AsioDeviceInfo item = new AsioDeviceInfo(paDeviceInfo.name.Left(32), paDeviceInfo.maxInputChannels, paDeviceInfo.maxOutputChannels, i);
					list.Add(item);
				}
			}
		}
		return list;
	}
}
