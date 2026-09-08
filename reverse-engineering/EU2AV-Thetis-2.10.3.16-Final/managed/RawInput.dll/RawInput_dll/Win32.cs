using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace RawInput_dll;

public static class Win32
{
	public const int KEYBOARD_OVERRUN_MAKE_CODE = 255;

	public const int WM_APPCOMMAND = 793;

	private const int FAPPCOMMANDMASK = 61440;

	internal const int FAPPCOMMANDMOUSE = 32768;

	internal const int FAPPCOMMANDOEM = 4096;

	public const int WM_KEYDOWN = 256;

	public const int WM_KEYUP = 257;

	internal const int WM_SYSKEYDOWN = 260;

	internal const int WM_INPUT = 255;

	internal const int WM_USB_DEVICECHANGE = 537;

	internal const int DBT_DEVICEARRIVAL = 32768;

	internal const int DBT_DEVICEREMOVECOMPLETE = 32772;

	internal const int DBT_DEVNODES_CHANGED = 7;

	internal const int WM_MOUSEWHEEL = 522;

	internal const int VK_SHIFT = 16;

	internal const int RI_KEY_MAKE = 0;

	internal const int RI_KEY_BREAK = 1;

	internal const int RI_KEY_E0 = 2;

	internal const int RI_KEY_E1 = 4;

	internal const int VK_CONTROL = 17;

	internal const int VK_MENU = 18;

	internal const int VK_ZOOM = 251;

	internal const int VK_LSHIFT = 160;

	internal const int VK_RSHIFT = 161;

	internal const int VK_LCONTROL = 162;

	internal const int VK_RCONTROL = 163;

	internal const int VK_LMENU = 164;

	internal const int VK_RMENU = 165;

	internal const int SC_SHIFT_R = 54;

	internal const int SC_SHIFT_L = 42;

	internal const int RIM_INPUT = 0;

	public static int LoWord(int dwValue)
	{
		return dwValue & 0xFFFF;
	}

	public static int HiWord(long dwValue)
	{
		return (int)(dwValue >> 16) & -61441;
	}

	public static ushort LowWord(uint val)
	{
		return (ushort)val;
	}

	public static ushort HighWord(uint val)
	{
		return (ushort)(val >> 16);
	}

	public static uint BuildWParam(ushort low, ushort high)
	{
		return (uint)((high << 16) | low);
	}

	[DllImport("User32.dll", SetLastError = true)]
	internal static extern int GetRawInputData(IntPtr hRawInput, DataCommand command, out InputData buffer, [In][Out] ref int size, int cbSizeHeader);

	[DllImport("User32.dll", SetLastError = true)]
	internal static extern int GetRawInputData(IntPtr hRawInput, DataCommand command, [Out] IntPtr pData, [In][Out] ref int size, int sizeHeader);

	[DllImport("User32.dll", SetLastError = true)]
	internal static extern uint GetRawInputDeviceInfo(IntPtr hDevice, RawInputDeviceInfo command, IntPtr pData, ref uint size);

	[DllImport("user32.dll")]
	private static extern uint GetRawInputDeviceInfo(IntPtr hDevice, uint command, ref DeviceInfo data, ref uint dataSize);

	[DllImport("User32.dll", SetLastError = true)]
	internal static extern uint GetRawInputDeviceList(IntPtr pRawInputDeviceList, ref uint numberDevices, uint size);

	[DllImport("User32.dll", SetLastError = true)]
	internal static extern bool RegisterRawInputDevices(RawInputDevice[] pRawInputDevice, uint numberDevices, uint size);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr notificationFilter, DeviceNotification flags);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern bool UnregisterDeviceNotification(IntPtr handle);

	public static void DeviceAudit()
	{
		FileStream fileStream = new FileStream("DeviceAudit.txt", FileMode.Create, FileAccess.Write);
		StreamWriter streamWriter = new StreamWriter(fileStream);
		int num = 0;
		uint numberDevices = 0u;
		int num2 = Marshal.SizeOf(typeof(Rawinputdevicelist));
		if (GetRawInputDeviceList(IntPtr.Zero, ref numberDevices, (uint)num2) == 0)
		{
			IntPtr intPtr = Marshal.AllocHGlobal((int)(num2 * numberDevices));
			GetRawInputDeviceList(intPtr, ref numberDevices, (uint)num2);
			for (int i = 0; i < numberDevices; i++)
			{
				uint size = 0u;
				Rawinputdevicelist rawinputdevicelist = (Rawinputdevicelist)Marshal.PtrToStructure(new IntPtr(intPtr.ToInt64() + num2 * i), typeof(Rawinputdevicelist));
				GetRawInputDeviceInfo(rawinputdevicelist.hDevice, RawInputDeviceInfo.RIDI_DEVICENAME, IntPtr.Zero, ref size);
				if (size == 0)
				{
					streamWriter.WriteLine("pcbSize: " + size);
					streamWriter.WriteLine(Marshal.GetLastWin32Error());
					return;
				}
				uint dataSize = (uint)Marshal.SizeOf(typeof(DeviceInfo));
				DeviceInfo data = new DeviceInfo
				{
					Size = Marshal.SizeOf(typeof(DeviceInfo))
				};
				if (GetRawInputDeviceInfo(rawinputdevicelist.hDevice, 536870923u, ref data, ref dataSize) == 0)
				{
					streamWriter.WriteLine(Marshal.GetLastWin32Error());
					return;
				}
				IntPtr intPtr2 = Marshal.AllocHGlobal((int)size);
				GetRawInputDeviceInfo(rawinputdevicelist.hDevice, RawInputDeviceInfo.RIDI_DEVICENAME, intPtr2, ref size);
				string device = Marshal.PtrToStringAnsi(intPtr2);
				if (rawinputdevicelist.dwType == 1 || rawinputdevicelist.dwType == 2)
				{
					string deviceDescription = GetDeviceDescription(device);
					KeyPressEvent keyPressEvent = new KeyPressEvent
					{
						DeviceName = Marshal.PtrToStringAnsi(intPtr2),
						DeviceHandle = rawinputdevicelist.hDevice,
						DeviceType = GetDeviceType(rawinputdevicelist.dwType),
						Name = deviceDescription,
						Source = num++.ToString(CultureInfo.InvariantCulture)
					};
					streamWriter.WriteLine(keyPressEvent.ToString());
					streamWriter.WriteLine(data.ToString());
					streamWriter.WriteLine(data.KeyboardInfo.ToString());
					streamWriter.WriteLine(data.HIDInfo.ToString());
					streamWriter.WriteLine("=========================================================================================================");
				}
				Marshal.FreeHGlobal(intPtr2);
			}
			Marshal.FreeHGlobal(intPtr);
			streamWriter.Flush();
			streamWriter.Close();
			fileStream.Close();
			return;
		}
		throw new Win32Exception(Marshal.GetLastWin32Error());
	}

	public static string GetDeviceType(uint device)
	{
		return device switch
		{
			0u => "MOUSE", 
			1u => "KEYBOARD", 
			2u => "HID", 
			_ => "UNKNOWN", 
		};
	}

	public static string GetDeviceDescription(string device)
	{
		try
		{
			string text = RegistryAccess.GetDeviceKey(device).GetValue("DeviceDesc").ToString();
			return text.Substring(text.IndexOf(';') + 1);
		}
		catch (Exception)
		{
			return "Device is malformed unable to look up in the registry";
		}
	}
}
