namespace RawInput_dll;

public struct DeviceInfoMouse
{
	public uint Id;

	public uint NumberOfButtons;

	public uint SampleRate;

	public bool HasHorizontalWheel;

	public override string ToString()
	{
		return $"MouseInfo\n Id: {Id}\n NumberOfButtons: {NumberOfButtons}\n SampleRate: {SampleRate}\n HorizontalWheel: {HasHorizontalWheel}\n";
	}
}
