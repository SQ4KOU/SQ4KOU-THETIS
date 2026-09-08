using System.Collections.ObjectModel;

namespace Midi2Cat.IO;

public class MidiDevices
{
	public int DeviceInCount => WinMM.MidiInGetNumDevs();

	public int DeviceOutCount => WinMM.MidiOutGetNumDevs();

	public Collection<string> InDevices
	{
		get
		{
			Collection<string> collection = new Collection<string>();
			int deviceInCount = DeviceInCount;
			for (int i = 0; i < deviceInCount; i++)
			{
				collection.Add(MidiInGetName(i));
			}
			return collection;
		}
	}

	public Collection<string> OutDevices
	{
		get
		{
			Collection<string> collection = new Collection<string>();
			int deviceOutCount = DeviceOutCount;
			for (int i = 0; i < deviceOutCount; i++)
			{
				collection.Add(MidiOutGetName(i));
			}
			return collection;
		}
	}

	public static string MidiInGetName(int index)
	{
		WinMM.MIDIINCAPS caps = default(WinMM.MIDIINCAPS);
		if (WinMM.MidiInGetDevCaps(index, ref caps, 44) == 0)
		{
			return caps.szPname;
		}
		return "";
	}

	public static string MidiOutGetName(int index)
	{
		WinMM.MIDIOUTCAPS caps = default(WinMM.MIDIOUTCAPS);
		if (WinMM.MidiOutGetDevCaps(index, ref caps, 52) == 0)
		{
			return caps.szPname;
		}
		return "";
	}
}
