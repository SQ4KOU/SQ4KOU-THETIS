using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;

namespace RawInput_dll;

internal struct Rawkeyboard
{
	public ushort Makecode;

	public ushort Flags;

	private readonly ushort Reserved;

	public ushort VKey;

	public uint Message;

	public uint ExtraInformation;

	public override string ToString()
	{
		return string.Format("Rawkeyboard\n Makecode: {0}\n Makecode(hex) : {0:X}\n Flags: {1}\n Reserved: {2}\n VKeyName: {3}\n Message: {4}\n ExtraInformation {5}\n", Makecode, Flags, Reserved, VKey, Message, ExtraInformation);
	}
}
public sealed class RawKeyboard
{
	public delegate void DeviceEventHandler(object sender, RawInputEventArg e);

	public readonly Dictionary<IntPtr, KeyPressEvent> _deviceList = new Dictionary<IntPtr, KeyPressEvent>();

	private readonly object _padLock = new object();

	private static InputData _rawBuffer;

	public int NumberOfKeyboards { get; private set; }

	public event DeviceEventHandler KeyPressed;

	public RawKeyboard(IntPtr hwnd, bool captureOnlyInForeground)
	{
		RawInputDevice[] array = new RawInputDevice[1];
		array[0].UsagePage = HidUsagePage.GENERIC;
		array[0].Usage = HidUsage.Keyboard;
		array[0].Flags = (RawInputDeviceFlags)(((!captureOnlyInForeground) ? 256 : 0) | 0x2000);
		array[0].Target = hwnd;
		if (!Win32.RegisterRawInputDevices(array, (uint)array.Length, (uint)Marshal.SizeOf(array[0])))
		{
			throw new ApplicationException("Failed to register raw input device(s).");
		}
	}

	public void EnumerateDevices(string id)
	{
		lock (_padLock)
		{
			_deviceList.Clear();
			int num = 0;
			KeyPressEvent keyPressEvent = new KeyPressEvent
			{
				DeviceName = "Global Keyboard",
				DeviceHandle = IntPtr.Zero,
				DeviceType = Win32.GetDeviceType(1u),
				Name = "Fake Keyboard. Some keys (ZOOM, MUTE, VOLUMEUP, VOLUMEDOWN) are sent to rawinput with a handle of zero.",
				Source = num++.ToString(CultureInfo.InvariantCulture),
				ID = id
			};
			_deviceList.Add(keyPressEvent.DeviceHandle, keyPressEvent);
			int num2 = 0;
			uint numberDevices = 0u;
			int num3 = Marshal.SizeOf(typeof(Rawinputdevicelist));
			if (Win32.GetRawInputDeviceList(IntPtr.Zero, ref numberDevices, (uint)num3) == 0)
			{
				IntPtr intPtr = Marshal.AllocHGlobal((int)(num3 * numberDevices));
				Win32.GetRawInputDeviceList(intPtr, ref numberDevices, (uint)num3);
				for (int i = 0; i < numberDevices; i++)
				{
					uint size = 0u;
					Rawinputdevicelist rawinputdevicelist = (Rawinputdevicelist)Marshal.PtrToStructure(new IntPtr(intPtr.ToInt64() + num3 * i), typeof(Rawinputdevicelist));
					Win32.GetRawInputDeviceInfo(rawinputdevicelist.hDevice, RawInputDeviceInfo.RIDI_DEVICENAME, IntPtr.Zero, ref size);
					if (size == 0)
					{
						continue;
					}
					IntPtr intPtr2 = Marshal.AllocHGlobal((int)size);
					Win32.GetRawInputDeviceInfo(rawinputdevicelist.hDevice, RawInputDeviceInfo.RIDI_DEVICENAME, intPtr2, ref size);
					string device = Marshal.PtrToStringAnsi(intPtr2);
					if (rawinputdevicelist.dwType == 1 || rawinputdevicelist.dwType == 2)
					{
						string deviceDescription = Win32.GetDeviceDescription(device);
						KeyPressEvent value = new KeyPressEvent
						{
							DeviceName = Marshal.PtrToStringAnsi(intPtr2),
							DeviceHandle = rawinputdevicelist.hDevice,
							DeviceType = Win32.GetDeviceType(rawinputdevicelist.dwType),
							Name = deviceDescription,
							Source = num++.ToString(CultureInfo.InvariantCulture),
							ID = id
						};
						if (!_deviceList.ContainsKey(rawinputdevicelist.hDevice))
						{
							num2++;
							_deviceList.Add(rawinputdevicelist.hDevice, value);
						}
					}
					Marshal.FreeHGlobal(intPtr2);
				}
				Marshal.FreeHGlobal(intPtr);
				NumberOfKeyboards = num2;
				return;
			}
		}
		throw new Win32Exception(Marshal.GetLastWin32Error());
	}

	public void ProcessRawInput(IntPtr hdevice)
	{
		if (_deviceList.Count == 0)
		{
			return;
		}
		int size = 0;
		Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, IntPtr.Zero, ref size, Marshal.SizeOf(typeof(Rawinputheader)));
		if (size != Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, out _rawBuffer, ref size, Marshal.SizeOf(typeof(Rawinputheader))) || _rawBuffer.header.dwType != 1)
		{
			return;
		}
		int vKey = _rawBuffer.data.keyboard.VKey;
		int makecode = _rawBuffer.data.keyboard.Makecode;
		int flags = _rawBuffer.data.keyboard.Flags;
		if (vKey == 255)
		{
			return;
		}
		bool isE0BitSet = (flags & 2) != 0;
		if (_deviceList.ContainsKey(_rawBuffer.header.hDevice))
		{
			KeyPressEvent keyPressEvent;
			lock (_padLock)
			{
				keyPressEvent = _deviceList[_rawBuffer.header.hDevice];
			}
			bool flag = (flags & 1) != 0;
			keyPressEvent.KeyPressState = !flag;
			keyPressEvent.Message = _rawBuffer.data.keyboard.Message;
			keyPressEvent.VKeyName = KeyMapper.GetKeyName(VirtualKeyCorrection(vKey, isE0BitSet, makecode)).ToUpper();
			keyPressEvent.VKey = vKey;
			if (KeyPressed != null)
			{
				KeyPressed(this, new RawInputEventArg(keyPressEvent));
			}
		}
	}

	private static int VirtualKeyCorrection(int virtualKey, bool isE0BitSet, int makeCode)
	{
		int result = virtualKey;
		if (!(_rawBuffer.header.hDevice == IntPtr.Zero))
		{
			result = virtualKey switch
			{
				17 => isE0BitSet ? 163 : 162, 
				18 => isE0BitSet ? 165 : 164, 
				16 => (makeCode == 54) ? 161 : 160, 
				_ => virtualKey, 
			};
		}
		else if (_rawBuffer.data.keyboard.VKey == 17)
		{
			result = 251;
		}
		return result;
	}
}
