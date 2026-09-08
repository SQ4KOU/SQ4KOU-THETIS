namespace Thetis;

internal sealed class AsioDeviceInfo
{
	public string Name { get; private set; }

	public int InputChannelCount { get; private set; }

	public int OutputChannelCount { get; private set; }

	public int DeviceIndex { get; private set; }

	public AsioDeviceInfo(string name, int input_channel_count, int output_channel_count, int device_index)
	{
		Name = name;
		InputChannelCount = input_channel_count;
		OutputChannelCount = output_channel_count;
		DeviceIndex = device_index;
	}

	public override string ToString()
	{
		return Name;
	}
}
