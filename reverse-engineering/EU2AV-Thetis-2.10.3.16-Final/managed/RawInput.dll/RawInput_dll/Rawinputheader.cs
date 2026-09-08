using System;

namespace RawInput_dll;

public struct Rawinputheader
{
	public uint dwType;

	public uint dwSize;

	public IntPtr hDevice;

	public IntPtr wParam;

	public override string ToString()
	{
		return $"RawInputHeader\n dwType : {dwType}\n dwSize : {dwSize}\n hDevice : {hDevice}\n wParam : {wParam}";
	}
}
