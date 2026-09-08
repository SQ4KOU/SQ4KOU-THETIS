using System.Runtime.InteropServices;

namespace RawInput_dll;

[StructLayout(LayoutKind.Explicit)]
public struct DeviceInfo
{
	[FieldOffset(0)]
	public int Size;

	[FieldOffset(4)]
	public int Type;

	[FieldOffset(8)]
	public DeviceInfoMouse MouseInfo;

	[FieldOffset(8)]
	public DeviceInfoKeyboard KeyboardInfo;

	[FieldOffset(8)]
	public DeviceInfoHid HIDInfo;

	public override string ToString()
	{
		return $"DeviceInfo\n Size: {Size}\n Type: {Type}\n";
	}
}
