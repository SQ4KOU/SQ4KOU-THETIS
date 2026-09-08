using System.Collections.Generic;
using Midi2Cat.IO;

namespace Midi2Cat.Data;

public class ControllerBinding
{
	public string DeviceName;

	public int DeviceIndex;

	public MidiDevice Device;

	public Dictionary<int, MidMessageHandler> CmdBindings;
}
