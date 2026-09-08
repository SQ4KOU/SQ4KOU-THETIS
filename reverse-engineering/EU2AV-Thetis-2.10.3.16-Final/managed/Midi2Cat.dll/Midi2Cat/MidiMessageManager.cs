using System;
using System.Collections.Generic;
using System.Reflection;
using Midi2Cat.Data;
using Midi2Cat.IO;

namespace Midi2Cat;

public class MidiMessageManager
{
	private Midi2CatDatabase DB;

	private object midi_handler_lock = new object();

	private Dictionary<int, ControllerBinding> bindings = new Dictionary<int, ControllerBinding>();

	private object midi2CatCommands;

	public string DbFile { get; set; }

	public MidiMessageManager(object Midi2CatCommands, string DbFile)
	{
		midi2CatCommands = Midi2CatCommands;
		this.DbFile = DbFile;
	}

	public void Open()
	{
		if (bindings.Count > 0)
		{
			Close();
		}
		MidiDevices midiDevices = new MidiDevices();
		DB = new Midi2CatDatabase(DbFile);
		int num = 0;
		foreach (string inDevice in midiDevices.InDevices)
		{
			List<ControllerMapping> mappings = DB.GetMappings(inDevice, MappingFilter.Active);
			if (mappings.Count > 0)
			{
				InitDevice(inDevice, mappings, num);
			}
			num++;
		}
	}

	public void Close()
	{
		foreach (ControllerBinding value in bindings.Values)
		{
			value.Device.CloseMidiIn();
			value.Device.CloseMidiOut();
		}
		bindings.Clear();
	}

	public MidiDevice PL1Device()
	{
		if (bindings.Count == 0)
		{
			return null;
		}
		int key = 0;
		using (Dictionary<int, ControllerBinding>.ValueCollection.Enumerator enumerator = bindings.Values.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				ControllerBinding current = enumerator.Current;
				if (current.DeviceName == "CMD PL-1")
				{
					key = current.DeviceIndex;
				}
			}
		}
		return bindings[key].Device;
	}

	public int PL1Index()
	{
		if (bindings.Count == 0)
		{
			return -1;
		}
		int result = -1;
		foreach (ControllerBinding value in bindings.Values)
		{
			if (value.DeviceName == "CMD PL-1")
			{
				result = value.DeviceIndex;
				break;
			}
		}
		return result;
	}

	public void SendUpdateToMidi(CatCmd cmd, double pct)
	{
		int num = PL1Index();
		if (num < 0)
		{
			return;
		}
		ControllerMapping reverseMapping = DB.GetReverseMapping(bindings[num].DeviceName, cmd);
		MidiDevice device = bindings[num].Device;
		if (reverseMapping != null)
		{
			int midiControlId = reverseMapping.MidiControlId;
			int n = Convert.ToInt32(16.0 * pct);
			if (midiControlId == 73)
			{
				device.SetPL1KnobLight(n, 10);
			}
			else
			{
				device.SetPL1KnobLight(n, midiControlId);
			}
		}
	}

	public void PL1InitialButtonLights(MidiDevice device)
	{
		device.SendMsg(1, 1, 144, 34, "902201");
		device.SendMsg(1, 1, 144, 35, "902301");
		device.SendMsg(1, 1, 144, 38, "902601");
		device.SendMsg(1, 1, 144, 39, "902701");
		device.SendMsg(1, 0, 144, 36, "902400");
		device.SendMsg(1, 0, 144, 37, "902500");
		device.SendMsg(1, 1, 144, 32, "902001");
		device.SendMsg(1, 1, 144, 33, "902101");
		device.SendMsg(1, 1, 144, 27, "901B01");
		device.SendMsg(1, 1, 144, 16, "901001");
		device.SendMsg(1, 1, 144, 17, "901101");
		device.SendMsg(1, 1, 144, 18, "901201");
		device.SendMsg(1, 1, 144, 19, "901301");
		device.SendMsg(1, 1, 144, 20, "901401");
		device.SendMsg(1, 1, 144, 21, "901501");
		device.SendMsg(1, 1, 144, 22, "901601");
		device.SendMsg(1, 1, 144, 23, "901701");
		device.SendMsg(1, 1, 144, 24, "901801");
		device.SendMsg(0, 0, 0, 0, "901900");
	}

	public void MicroInitialButtonLights(MidiDevice device)
	{
		device.SendMsg(0, 0, 0, 0, "901701");
		device.SendMsg(0, 0, 0, 0, "901801");
		device.SendMsg(0, 0, 0, 0, "902701");
		device.SendMsg(0, 0, 0, 0, "902801");
	}

	private void InitDevice(string deviceName, List<ControllerMapping> mappings, int Idx)
	{
		MidiDevice midiDevice = new MidiDevice();
		midiDevice.OpenMidiIn(Idx, deviceName);
		Dictionary<int, MidMessageHandler> cmdBindings = BindMappingHandlers(mappings);
		ControllerBinding value = new ControllerBinding
		{
			DeviceName = deviceName,
			DeviceIndex = Idx,
			Device = midiDevice,
			CmdBindings = cmdBindings
		};
		bindings.Add(Idx, value);
		midiDevice.onMidiDebugMessage += onMidiDebugMsg;
		midiDevice.onMidiInput += OnMidiInput;
		if (deviceName == "CMD PL-1")
		{
			PL1InitialButtonLights(midiDevice);
		}
		if (deviceName == "CMD Micro")
		{
			MicroInitialButtonLights(midiDevice);
		}
	}

	private Dictionary<int, MidMessageHandler> BindMappingHandlers(List<ControllerMapping> mappings)
	{
		Dictionary<int, MidMessageHandler> dictionary = new Dictionary<int, MidMessageHandler>();
		foreach (ControllerMapping mapping in mappings)
		{
			try
			{
				string name = mapping.CatCmdId.ToString();
				MethodInfo method = midi2CatCommands.GetType().GetMethod(name);
				if (!mapping.CatCmd.IsToggled)
				{
					ProcessMidiMessageHandler cmdHandler = (ProcessMidiMessageHandler)Delegate.CreateDelegate(typeof(ProcessMidiMessageHandler), midi2CatCommands, method);
					MidMessageHandler value = new MidMessageHandler
					{
						CmdHandler = cmdHandler,
						MidiOutCmdDown = mapping.MidiOutCmdDown,
						MidiOutCmdUp = mapping.MidiOutCmdUp,
						MidiOutCmdSetValue = mapping.MidiOutCmdSetValue
					};
					dictionary.Add(mapping.MidiControlId, value);
				}
				else
				{
					ProcessMidiMessageToggleHandler toggleCmdHandler = (ProcessMidiMessageToggleHandler)Delegate.CreateDelegate(typeof(ProcessMidiMessageToggleHandler), midi2CatCommands, method);
					MidMessageHandler value2 = new MidMessageHandler
					{
						ToggleCmdHandler = toggleCmdHandler,
						MidiOutCmdDown = mapping.MidiOutCmdDown,
						MidiOutCmdUp = mapping.MidiOutCmdUp,
						MidiOutCmdSetValue = mapping.MidiOutCmdSetValue
					};
					dictionary.Add(mapping.MidiControlId, value2);
				}
			}
			catch
			{
			}
		}
		return dictionary;
	}

	private void onMidiDebugMsg(int Device, Direction direction, Status status, string msg1, string msg2)
	{
	}

	private void OnMidiInput(MidiDevice Device, int DeviceIdx, int ControlId, int Data, int Status, int Voice, int Channel)
	{
		Device.latestControlID = ControlId;
		try
		{
			if (!bindings.TryGetValue(DeviceIdx, out var value))
			{
				return;
			}
			CmdState cmdState = CmdState.NoChange;
			if (!value.CmdBindings.TryGetValue(ControlId, out var value2) || value2 == null)
			{
				return;
			}
			lock (midi_handler_lock)
			{
				if (value2.CmdHandler != null)
				{
					value2.CmdHandler(Data, Device);
					cmdState = ((Data > 0) ? CmdState.On : CmdState.Off);
				}
				else if (value2.ToggleCmdHandler != null)
				{
					cmdState = value2.ToggleCmdHandler(Data, Device);
				}
				if (cmdState == CmdState.On && value2.MidiOutCmdDown != null)
				{
					Device.SendMsg(Channel, 127, Status, ControlId, value2.MidiOutCmdDown);
				}
				if (cmdState == CmdState.Off && value2.MidiOutCmdUp != null)
				{
					Device.SendMsg(Channel, 0, Status, ControlId, value2.MidiOutCmdUp);
				}
				if (value2.MidiOutCmdSetValue != null)
				{
					Device.SendMsg(Channel, Data, Status, ControlId, value2.MidiOutCmdSetValue);
				}
			}
		}
		catch
		{
		}
	}
}
