using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Midi2Cat.Data;

namespace Midi2Cat.IO;

public class MidiDevice
{
	public enum MidiUniqueDevices
	{
		Default,
		BehringerCMDPL1,
		BehringerCMDMicro,
		DJStarlight,
		NumarkDJ2GO2Touch,
		BehringerGeneric
	}

	public static bool BuildIDFromControlIDAndChannel;

	public static bool IncludeStatusInControlID;

	public static bool Ignore14bitMessages;

	public static int VFOSelect;

	public int latestControlID;

	public int lastLED;

	private WinMM.MidiInCallback callback;

	private IntPtr midi_in_handle = (IntPtr)0;

	private IntPtr midi_out_handle = (IntPtr)0;

	private object in_lock_obj = new object();

	private object out_lock_obj = new object();

	public const int CALLBACK_FUNCTION = 196608;

	public const int MIM_OPEN = 961;

	public const int MIM_CLOSE = 962;

	public const int MIM_DATA = 963;

	public const int MIM_LONGDATA = 964;

	public const int MIM_ERROR = 965;

	public const int MIM_LONGERROR = 966;

	public const int MIM_MOREDATA = 967;

	private int DeviceIndex;

	private string DeviceName;

	private MidiUniqueDevices _uniqueDevice;

	public event MidiInputEventHandler onMidiInput;

	public event DebugMsgEventHandler onMidiDebugMessage;

	public string GetDeviceName()
	{
		return DeviceName;
	}

	private MidiUniqueDevices getUniqueDevice(string sDeviceName)
	{
		string text = sDeviceName.ToLower();
		if (text.Contains("djcontrol") && text.Contains("starlight"))
		{
			return MidiUniqueDevices.DJStarlight;
		}
		if (text.Contains("dj2go2") && text.Contains("touch") && text.Contains("midi"))
		{
			return MidiUniqueDevices.NumarkDJ2GO2Touch;
		}
		if (text.Contains("cmd") && text.Contains("pl-1"))
		{
			return MidiUniqueDevices.BehringerCMDPL1;
		}
		if (text.Contains("cmd") && text.Contains("micro"))
		{
			return MidiUniqueDevices.BehringerCMDMicro;
		}
		if (text.Contains("cmd"))
		{
			return MidiUniqueDevices.BehringerGeneric;
		}
		return MidiUniqueDevices.Default;
	}

	public bool OpenMidiIn(int deviceIndex, string deviceName)
	{
		DeviceIndex = deviceIndex;
		DeviceName = deviceName;
		_uniqueDevice = getUniqueDevice(deviceName);
		callback = InCallback;
		int num = WinMM.MidiInOpen(out midi_in_handle, (uint)DeviceIndex, callback, IntPtr.Zero, WinMM.MidiInOpenFlags.Function | WinMM.MidiInOpenFlags.MidiIoStatus);
		if (num != 0)
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			WinMM.MidiInGetErrorText(num, stringBuilder, 256);
			DebugMsg(Direction.In, Status.Error, "MidiInOpen Error: ", stringBuilder.ToString());
			return false;
		}
		num = WinMM.MidiInStart(midi_in_handle);
		if (num != 0)
		{
			StringBuilder stringBuilder2 = new StringBuilder(256);
			WinMM.MidiInGetErrorText(num, stringBuilder2, 256);
			DebugMsg(Direction.In, Status.Error, "MidiInStart Error:", stringBuilder2.ToString());
			return false;
		}
		if (OpenMidiOut())
		{
			SendMsg(15, 15, 0, 0);
		}
		DebugMsg(Direction.In, Status.Open);
		return true;
	}

	public bool OpenMidiOut()
	{
		int num = -1;
		MidiDevices midiDevices = new MidiDevices();
		int num2 = 0;
		foreach (string outDevice in midiDevices.OutDevices)
		{
			if (outDevice == DeviceName)
			{
				num = num2;
				break;
			}
			num2++;
		}
		if (num == -1)
		{
			DebugMsg(Direction.Out, Status.Error, "MidiOutOpen Error: Unable to find " + DeviceName + " output device.");
			return false;
		}
		int num3 = WinMM.MidiOutOpen(out midi_out_handle, (uint)num, IntPtr.Zero, IntPtr.Zero, WinMM.MidiOutOpenFlags.Null);
		if (num3 != 0)
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			WinMM.MidiInGetErrorText(num3, stringBuilder, 256);
			DebugMsg(Direction.Out, Status.Error, "MidiOutOpen Error:", stringBuilder.ToString());
			return false;
		}
		DebugMsg(Direction.Out, Status.Open);
		return true;
	}

	public void CloseMidiIn()
	{
		_ = midi_in_handle;
		WinMM.MidiInStop(midi_in_handle);
		WinMM.MidiInReset(midi_in_handle);
		WinMM.MidiInClose(midi_in_handle);
		midi_in_handle = (IntPtr)0;
		DebugMsg(Direction.Out, Status.Closed);
	}

	public void CloseMidiOut()
	{
		_ = midi_out_handle;
		WinMM.MidiOutClose(midi_out_handle);
		DebugMsg(Direction.Out, Status.Closed);
	}

	private void Reset()
	{
		CloseMidiIn();
		CloseMidiOut();
	}

	public static void DebugByte(byte[] b)
	{
		for (int i = 0; i < b.Length; i++)
		{
		}
	}

	private int InCallback(int hMidiIn, int wMsg, int dwInstance, int dwParam1, int dwParam2)
	{
		lock (in_lock_obj)
		{
			switch (wMsg)
			{
			case 963:
			{
				_ = (byte)dwParam1;
				byte controlId = (byte)(dwParam1 >> 8);
				byte data = (byte)(dwParam1 >> 16);
				byte b = (byte)(dwParam1 & 0xFF);
				byte b2 = (byte)((b & 0xF0) >> 4);
				byte channel = (byte)(b & 0xF);
				if (b2 == 8)
				{
					data = 0;
				}
				inDevice_ChannelMessageReceived(controlId, data, b, b2, channel);
				break;
			}
			case 961:
			case 962:
			case 964:
			case 965:
			case 966:
			case 967:
				break;
			}
		}
		return 0;
	}

	private static byte[] SwapBytes(byte[] b)
	{
		for (int i = 0; i < b.Length / 2; i++)
		{
			byte b2 = b[i];
			b[i] = b[b.Length - 1 - i];
			b[b.Length - 1 - i] = b2;
		}
		return b;
	}

	public static int SendMsg(int handle, ushort msg_id, byte protocol_id, ushort opcode, uint data1, uint data2)
	{
		byte[] array = new byte[16]
		{
			240, 125, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		};
		SwapBytes(BitConverter.GetBytes(msg_id)).CopyTo(array, 2);
		array[4] = protocol_id;
		SwapBytes(BitConverter.GetBytes(opcode)).CopyTo(array, 5);
		SwapBytes(BitConverter.GetBytes(data1)).CopyTo(array, 7);
		SwapBytes(BitConverter.GetBytes(data2)).CopyTo(array, 11);
		array[15] = 247;
		return SendLongMessage(handle, array);
	}

	public static int SendLongMessage(int handle, byte[] data)
	{
		int sizeOfMidiHeader = Marshal.SizeOf(typeof(WinMM.MidiHeader));
		WinMM.MidiHeader structure = new WinMM.MidiHeader
		{
			data = Marshal.AllocHGlobal(data.Length)
		};
		for (int i = 0; i < data.Length; i++)
		{
			Marshal.WriteByte(structure.data, i, data[i]);
		}
		structure.bufferLength = data.Length;
		structure.bytesRecorded = data.Length;
		structure.flags = 0;
		IntPtr intPtr;
		try
		{
			intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WinMM.MidiHeader)));
		}
		catch (Exception)
		{
			Marshal.FreeHGlobal(structure.data);
			throw;
		}
		try
		{
			Marshal.StructureToPtr(structure, intPtr, fDeleteOld: false);
		}
		catch (Exception)
		{
			Marshal.FreeHGlobal(structure.data);
			Marshal.FreeHGlobal(intPtr);
			throw;
		}
		int num = WinMM.MidiOutPrepareHeader(handle, intPtr, sizeOfMidiHeader);
		if (num == 0)
		{
			num = WinMM.MidiOutLongMessage(handle, intPtr, sizeOfMidiHeader);
		}
		if (num == 0)
		{
			num = WinMM.MidiOutUnprepareHeader(handle, intPtr, sizeOfMidiHeader);
		}
		Marshal.FreeHGlobal(structure.data);
		Marshal.FreeHGlobal(intPtr);
		return num;
	}

	public void inDevice_ChannelMessageReceived(int ControlId, int Data, int Status, int Event, int Channel)
	{
		if (onMidiInput == null)
		{
			return;
		}
		try
		{
			if (filterAndMap(ControlId, Status, Channel, Data, out var controlIDmapped, out var dataMapped))
			{
				ControlId = FixBehringerCtlID(controlIDmapped, Status);
				onMidiInput(this, DeviceIndex, ControlId, dataMapped, Status, Event, Channel);
			}
		}
		catch
		{
		}
	}

	private bool filterAndMap(int controlId, int status, int channel, int data, out int controlIDmapped, out int dataMapped)
	{
		controlIDmapped = controlId;
		dataMapped = data;
		controlId &= 0xFF;
		if (Ignore14bitMessages && controlId >= 32 && controlId <= 63)
		{
			return false;
		}
		switch (_uniqueDevice)
		{
		case MidiUniqueDevices.NumarkDJ2GO2Touch:
			if (controlId == 9 && (channel == 1 || channel == 2) && (status == 176 || status == 177))
			{
				dataMapped = 127 - (data & 0x7F);
			}
			controlIDmapped = ((controlId & 0xFF) << 8) | (channel & 0xFF);
			break;
		case MidiUniqueDevices.DJStarlight:
			if (controlId == 8 && (channel == 1 || channel == 2) && (status == 145 || status == 146))
			{
				return false;
			}
			if (controlId == 3 && channel == 1 && status == 145)
			{
				return false;
			}
			controlIDmapped = ((controlId & 0xFF) << 8) | (channel & 0xFF);
			break;
		default:
			if (BuildIDFromControlIDAndChannel)
			{
				if (IncludeStatusInControlID)
				{
					controlIDmapped = ((controlId & 0xFF) << 16) | ((channel & 0xFF) << 8) | (status & 0xFF);
				}
				else
				{
					controlIDmapped = ((controlId & 0xFF) << 8) | (channel & 0xFF);
				}
			}
			break;
		}
		return true;
	}

	public int FixBehringerCtlID(int ControlId, int Status)
	{
		if (DeviceName == "CMD PL-1")
		{
			if (Status == 224)
			{
				ControlId = 73;
			}
			if (ControlId == 31 && VFOSelect == 2)
			{
				ControlId = 77;
			}
		}
		if (DeviceName == "CMD Micro" && Status == 176)
		{
			if (ControlId == 16)
			{
				ControlId = 73;
			}
			if (ControlId == 18)
			{
				ControlId = 74;
			}
			if (ControlId == 34)
			{
				ControlId = 75;
			}
			if (ControlId == 32)
			{
				ControlId = 76;
			}
			if (ControlId == 17)
			{
				ControlId = 77;
			}
			if (ControlId == 33)
			{
				ControlId = 78;
			}
			if (ControlId == 48)
			{
				ControlId = 79;
			}
			if (ControlId == 49)
			{
				ControlId = 80;
			}
		}
		return ControlId;
	}

	private void DebugMsg(Direction direction, Status status, string msg1 = "", string msg2 = "")
	{
		if (onMidiDebugMessage != null)
		{
			try
			{
				onMidiDebugMessage(DeviceIndex, direction, status, msg1, msg2);
			}
			catch
			{
			}
		}
	}

	public void SendMsg(int Event, int Channel, int Data1, int Data2)
	{
		_ = midi_out_handle;
		byte b = (byte)(Event << 4);
		b |= (byte)Channel;
		uint dwMsg = BitConverter.ToUInt32(new byte[4]
		{
			b,
			(byte)Data1,
			(byte)Data2,
			0
		}, 0);
		_ = midi_out_handle;
		WinMM.MidiOutShortMessage(midi_out_handle, dwMsg);
	}

	public int UnmapControlID(int inControl, out int byteCount)
	{
		byteCount = 1;
		MidiUniqueDevices uniqueDevice = _uniqueDevice;
		if ((uint)(uniqueDevice - 3) <= 1u)
		{
			inControl = (inControl >> 8) & 0xFF;
			byteCount = 2;
		}
		else if (BuildIDFromControlIDAndChannel)
		{
			if (IncludeStatusInControlID)
			{
				inControl = (inControl >> 16) & 0xFF;
				byteCount = 3;
			}
			else
			{
				inControl = (inControl >> 8) & 0xFF;
				byteCount = 2;
			}
		}
		return inControl;
	}

	public ParsedMidiMessage ParseMsg(int inChannel, int inValue, int inStatus, int inControl, string inMsg)
	{
		inControl = UnmapControlID(inControl, out var _);
		string text = inMsg;
		ParsedMidiMessage parsedMidiMessage = new ParsedMidiMessage();
		if (text.Length != 6)
		{
			parsedMidiMessage.ErrMsg = string.Format("Msg:{0} {1}", inMsg, "Must be 6 characters long.");
			return parsedMidiMessage;
		}
		if (text.Contains("SS"))
		{
			if (text.Substring(0, 2) != "SS")
			{
				parsedMidiMessage.ErrMsg = string.Format("Msg:{0} {1}", inMsg, "SS must be the 1st and 2nd characters.");
				return parsedMidiMessage;
			}
			text = text.Replace("SS", inStatus.ToString("X2"));
		}
		if (text.Contains("YY"))
		{
			if (text.Substring(2, 2) != "YY")
			{
				parsedMidiMessage.ErrMsg = string.Format("Msg:{0} {1}", inMsg, "YY must be the 3rd & 4th characters.");
				return parsedMidiMessage;
			}
			text = text.Replace("YY", inControl.ToString("X2"));
		}
		if (text.Contains("VV"))
		{
			if (text.Substring(4, 2) != "VV")
			{
				parsedMidiMessage.ErrMsg = string.Format("Msg:{0} {1}", inMsg, "VV must be the 5th & 6th characters.");
				return parsedMidiMessage;
			}
			text = text.Replace("VV", inValue.ToString("X2"));
		}
		text = text.Replace("X", inChannel.ToString());
		string s = text.Substring(0, 1);
		string s2 = text.Substring(1, 1);
		string s3 = text.Substring(2, 2);
		string s4 = text.Substring(4, 2);
		try
		{
			parsedMidiMessage.Event = int.Parse(s, NumberStyles.HexNumber);
			parsedMidiMessage.Channel = int.Parse(s2, NumberStyles.HexNumber);
			parsedMidiMessage.Data1 = int.Parse(s3, NumberStyles.HexNumber);
			parsedMidiMessage.Data2 = int.Parse(s4, NumberStyles.HexNumber);
			parsedMidiMessage.Valid = true;
		}
		catch
		{
			parsedMidiMessage.ErrMsg = string.Format("Msg:{0} {1}", inMsg, "contains invalid hexideciaml characters.");
		}
		return parsedMidiMessage;
	}

	public void SendMsg(int inChannel, int inValue, int inStatus, int inControl, string inMessages)
	{
		char[] separator = new char[4] { ' ', ',', ';', ':' };
		string[] array = inMessages.Split(separator, StringSplitOptions.RemoveEmptyEntries);
		foreach (string inMsg in array)
		{
			ParsedMidiMessage parsedMidiMessage = ParseMsg(inChannel, inValue, inStatus, inControl, inMsg);
			if (parsedMidiMessage.Valid)
			{
				SendMsg(parsedMidiMessage.Event, parsedMidiMessage.Channel, parsedMidiMessage.Data1, parsedMidiMessage.Data2);
			}
		}
	}

	public string[] ValidateMidiMessages(string inMessages)
	{
		List<string> list = new List<string>();
		char[] separator = new char[4] { ' ', ',', ';', ':' };
		string[] array = inMessages.Split(separator, StringSplitOptions.RemoveEmptyEntries);
		foreach (string inMsg in array)
		{
			ParsedMidiMessage parsedMidiMessage = ParseMsg(0, 0, 0, 0, inMsg);
			if (!parsedMidiMessage.Valid)
			{
				list.Add(parsedMidiMessage.ErrMsg);
			}
		}
		return list.ToArray();
	}

	public void SetPL1ButtonLight(int n)
	{
		int inControl = latestControlID;
		string text = inControl.ToString("X2");
		string text2 = ((n != 0 && n != 1 && n != 2) ? "00" : n.ToString("X2"));
		string inMessages = "90" + text + text2;
		SendMsg(1, 1, 144, inControl, inMessages);
	}

	public void SetPL1ButtonLight(int n, int inCtlID)
	{
		string text = inCtlID.ToString("X2");
		string text2 = ((n != 0 && n != 1 && n != 2) ? "00" : n.ToString("X2"));
		string inMessages = "90" + text + text2;
		SendMsg(1, 1, 144, inCtlID, inMessages);
	}

	public void SetPL1KnobLight(int n, int inCtlID)
	{
		if (n != lastLED || inCtlID != 73)
		{
			lastLED = n;
			string text = inCtlID.ToString("X2");
			string text2 = n.ToString("X2");
			string inMessages = "B0" + text + text2;
			SendMsg(1, 0, 0, 0, inMessages);
			Thread.Sleep(1);
		}
	}
}
