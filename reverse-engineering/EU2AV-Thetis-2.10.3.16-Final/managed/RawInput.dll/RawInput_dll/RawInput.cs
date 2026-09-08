using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RawInput_dll;

public class RawInput : NativeWindow
{
	public delegate void DevicesEventHandler(object sender);

	private RawKeyboard _keyboardDriver;

	private RawMouse _mouseDriver;

	private string _id;

	private readonly IntPtr _devNotifyHandle;

	private static readonly Guid DeviceInterfaceHid = new Guid("4D1E55B2-F16F-11CF-88CB-001111000030");

	private PreMessageFilter _filter;

	public int NumberOfMice => _mouseDriver.NumberOfMice;

	public int NumberOfKeyboards => _keyboardDriver.NumberOfKeyboards;

	public bool IgnoreNextWheelEvent
	{
		get
		{
			if (_filter == null)
			{
				return false;
			}
			return _filter.IgnoreNextWheelEvent;
		}
		set
		{
			if (_filter != null)
			{
				_filter.IgnoreNextWheelEvent = value;
			}
		}
	}

	public event DevicesEventHandler DevicesChanged;

	public event RawKeyboard.DeviceEventHandler KeyPressed
	{
		add
		{
			_keyboardDriver.KeyPressed += value;
		}
		remove
		{
			_keyboardDriver.KeyPressed -= value;
		}
	}

	public event RawMouse.DeviceEventHandler MouseMoved
	{
		add
		{
			_mouseDriver.MouseMoved += value;
		}
		remove
		{
			_mouseDriver.MouseMoved -= value;
		}
	}

	public Dictionary<IntPtr, MouseEvent> MouseDevices()
	{
		return _mouseDriver._deviceList;
	}

	public void AddMessageFilter()
	{
		if (_filter == null)
		{
			_filter = new PreMessageFilter();
			Application.AddMessageFilter(_filter);
		}
	}

	public void RemoveMessageFilter()
	{
		if (_filter != null)
		{
			Application.RemoveMessageFilter(_filter);
			_filter = null;
		}
	}

	public RawInput(IntPtr parentHandle, bool captureOnlyInForegroundMouse, bool captureOnlyInForegroundKeyboard, string id = "")
	{
		_id = id;
		AssignHandle(parentHandle);
		_keyboardDriver = new RawKeyboard(parentHandle, captureOnlyInForegroundKeyboard);
		_keyboardDriver.EnumerateDevices(id);
		_mouseDriver = new RawMouse(parentHandle, captureOnlyInForegroundMouse);
		_mouseDriver.EnumerateDevices(id);
		_devNotifyHandle = RegisterForDeviceNotifications(parentHandle);
	}

	private static IntPtr RegisterForDeviceNotifications(IntPtr parent)
	{
		IntPtr intPtr = IntPtr.Zero;
		BroadcastDeviceInterface structure = default(BroadcastDeviceInterface);
		structure.DbccSize = Marshal.SizeOf(structure);
		structure.BroadcastDeviceType = BroadcastDeviceType.DBT_DEVTYP_DEVICEINTERFACE;
		structure.DbccClassguid = DeviceInterfaceHid;
		IntPtr intPtr2 = IntPtr.Zero;
		try
		{
			intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BroadcastDeviceInterface)));
			Marshal.StructureToPtr(structure, intPtr2, fDeleteOld: false);
			intPtr = Win32.RegisterDeviceNotification(parent, intPtr2, DeviceNotification.DEVICE_NOTIFY_WINDOW_HANDLE);
		}
		catch (Exception)
		{
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr2);
		}
		_ = intPtr == IntPtr.Zero;
		return intPtr;
	}

	protected override void WndProc(ref Message message)
	{
		switch (message.Msg)
		{
		case 255:
			_keyboardDriver.ProcessRawInput(message.LParam);
			_mouseDriver.ProcessRawInput(message.LParam);
			break;
		case 537:
			if (((int)message.WParam & 0x8000) == 32768)
			{
				_keyboardDriver.EnumerateDevices(_id);
				_mouseDriver.EnumerateDevices(_id);
				DevicesChanged?.Invoke(this);
			}
			if (((int)message.WParam & 0x8004) == 32772)
			{
				_keyboardDriver.EnumerateDevices(_id);
				_mouseDriver.EnumerateDevices(_id);
				DevicesChanged?.Invoke(this);
			}
			break;
		}
		base.WndProc(ref message);
	}

	~RawInput()
	{
		Win32.UnregisterDeviceNotification(_devNotifyHandle);
		RemoveMessageFilter();
	}
}
