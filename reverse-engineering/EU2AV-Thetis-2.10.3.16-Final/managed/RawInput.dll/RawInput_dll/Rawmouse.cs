using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;

namespace RawInput_dll;

[StructLayout(LayoutKind.Explicit)]
internal struct Rawmouse
{
	[FieldOffset(0)]
	public ushort usFlags;

	[FieldOffset(4)]
	public uint ulButtons;

	[FieldOffset(4)]
	public ushort usButtonFlags;

	[FieldOffset(6)]
	public ushort usButtonData;

	[FieldOffset(8)]
	public uint ulRawButtons;

	[FieldOffset(12)]
	public int lLastX;

	[FieldOffset(16)]
	public int lLastY;

	[FieldOffset(20)]
	public uint ulExtraInformation;
}
public sealed class RawMouse
{
	public delegate void DeviceEventHandler(object sender, RawInputEventArg e);

	public readonly Dictionary<IntPtr, MouseEvent> _deviceList = new Dictionary<IntPtr, MouseEvent>();

	private readonly object _padLock = new object();

	private static InputData _rawBuffer;

	public int NumberOfMice { get; private set; }

	public event DeviceEventHandler MouseMoved;

	public RawMouse(IntPtr hwnd, bool captureOnlyInForeground)
	{
		RawInputDevice[] array = new RawInputDevice[1];
		array[0].UsagePage = HidUsagePage.GENERIC;
		array[0].Usage = HidUsage.Mouse;
		array[0].Flags = (RawInputDeviceFlags)(((!captureOnlyInForeground) ? 256 : 0) | 0x2000);
		array[0].Target = hwnd;
		if (!Win32.RegisterRawInputDevices(array, (uint)array.Length, (uint)Marshal.SizeOf(array[0])))
		{
			throw new ApplicationException("Failed to register raw mouse input device(s).");
		}
	}

	public void EnumerateDevices(string id)
	{
		lock (_padLock)
		{
			_deviceList.Clear();
			int num = 0;
			MouseEvent mouseEvent = new MouseEvent
			{
				DeviceName = "Global Mouse",
				DeviceHandle = IntPtr.Zero,
				DeviceType = Win32.GetDeviceType(1u),
				Name = "Fake Mouse",
				Source = num++.ToString(CultureInfo.InvariantCulture),
				ID = id
			};
			_deviceList.Add(mouseEvent.DeviceHandle, mouseEvent);
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
					if (rawinputdevicelist.dwType == 0)
					{
						string deviceDescription = Win32.GetDeviceDescription(device);
						MouseEvent value = new MouseEvent
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
				NumberOfMice = num2;
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
		if (size != Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, out _rawBuffer, ref size, Marshal.SizeOf(typeof(Rawinputheader))) || _rawBuffer.header.dwType != 0)
		{
			return;
		}
		int lLastX = _rawBuffer.data.mouse.lLastX;
		int lLastY = _rawBuffer.data.mouse.lLastY;
		uint ulButtons = _rawBuffer.data.mouse.ulButtons;
		ushort usButtonFlags = _rawBuffer.data.mouse.usButtonFlags;
		short buttonData = (short)_rawBuffer.data.mouse.usButtonData;
		if (_deviceList.ContainsKey(_rawBuffer.header.hDevice))
		{
			MouseEvent mouseEvent;
			lock (_padLock)
			{
				mouseEvent = _deviceList[_rawBuffer.header.hDevice];
			}
			mouseEvent.lastX = lLastX;
			mouseEvent.lastY = lLastY;
			mouseEvent.buttons = ulButtons;
			mouseEvent.buttonFlags = usButtonFlags;
			mouseEvent.buttonData = buttonData;
			if (MouseMoved != null && (usButtonFlags & 0x400) == 1024)
			{
				MouseMoved(this, new RawInputEventArg(mouseEvent));
			}
		}
	}
}
