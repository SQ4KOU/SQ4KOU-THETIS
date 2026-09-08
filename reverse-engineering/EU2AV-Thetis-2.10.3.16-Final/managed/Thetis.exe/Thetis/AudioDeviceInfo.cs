using System.Globalization;

namespace Thetis;

public sealed class AudioDeviceInfo
{
	public int Id { get; private set; }

	public string Name { get; private set; }

	public AudioDeviceDriver Driver { get; private set; }

	public int DeviceId => Id;

	public string DisplayName
	{
		get
		{
			string text = ((!string.IsNullOrWhiteSpace(Name)) ? Name : ("Device " + Id.ToString(CultureInfo.InvariantCulture)));
			if (Driver == AudioDeviceDriver.WASAPI)
			{
				return "WASAPI: " + text;
			}
			if (Driver == AudioDeviceDriver.MME)
			{
				return "MME: " + text;
			}
			return text;
		}
	}

	public AudioDeviceInfo(int id, string name)
	{
		Id = id;
		Name = name ?? string.Empty;
		Driver = AudioDeviceDriver.MME;
	}

	public AudioDeviceInfo(int id, string name, AudioDeviceDriver driver)
	{
		Id = id;
		Name = name ?? string.Empty;
		Driver = driver;
	}

	public override string ToString()
	{
		return DisplayName;
	}
}
