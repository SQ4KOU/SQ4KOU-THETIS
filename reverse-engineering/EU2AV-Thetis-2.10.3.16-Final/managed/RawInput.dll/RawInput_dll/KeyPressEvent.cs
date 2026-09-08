using System;

namespace RawInput_dll;

public class KeyPressEvent
{
	public string DeviceName;

	public string DeviceType;

	public IntPtr DeviceHandle;

	public string Name;

	private string _source;

	public int VKey;

	public string VKeyName;

	public uint Message;

	public bool KeyPressState;

	public string ID;

	public string Source
	{
		get
		{
			return _source;
		}
		set
		{
			_source = $"Keyboard_{value.PadLeft(2, '0')}";
		}
	}

	public override string ToString()
	{
		return string.Format("Device\n DeviceName: {0}\n DeviceType: {1}\n DeviceHandle: {2}\n Name: {3}\n", DeviceName, DeviceType, DeviceHandle.ToInt64().ToString("X"), Name);
	}
}
