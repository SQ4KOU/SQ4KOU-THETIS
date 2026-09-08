using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Midi2Cat.IO;

public static class WinMM
{
	public struct MidiHeader
	{
		public IntPtr data;

		public int bufferLength;

		public int bytesRecorded;

		public int user;

		public int flags;

		public IntPtr lpNext;

		public int reserved;

		public int offset;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public int[] dwReserved;
	}

	public delegate int MidiInCallback(int hMidiIn, int wMsg, int dwInstance, int dwParam1, int dwParam2);

	public struct MIDIINCAPS
	{
		public short wMid;

		public short wPid;

		public int vDriverVersion;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string szPname;

		public int dwSupport;
	}

	public struct MIDIOUTCAPS
	{
		public short wMid;

		public short wPid;

		public int vDriverVersion;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string szPname;

		public short wTechnology;

		public short wVoices;

		public short wNotes;

		public short wChannelMask;

		public int dwSupport;
	}

	[Flags]
	public enum MidiInOpenFlags
	{
		Null = 0,
		Window = 0x10000,
		Task = 0x20000,
		Function = Window | Task,
		MidiIoStatus = 0x20
	}

	[Flags]
	public enum MidiOutOpenFlags
	{
		Null = 0,
		Function = 1,
		Thread = 2,
		Window = Function | Thread,
		Event = 4
	}

	public const int MAXPNAMELEN = 32;

	[DllImport("winmm.dll", CharSet = CharSet.Ansi, EntryPoint = "midiInGetNumDevs")]
	public static extern int MidiInGetNumDevs();

	[DllImport("winmm.dll", CharSet = CharSet.Ansi, EntryPoint = "midiInGetDevCaps")]
	public static extern int MidiInGetDevCaps(int uDeviceID, ref MIDIINCAPS caps, int cbMidiInCaps);

	[DllImport("winmm.dll", CharSet = CharSet.Ansi, EntryPoint = "midiInOpen")]
	public static extern int MidiInOpen(out IntPtr lphMidiIn, uint uDeviceID, MidiInCallback dwCallback, IntPtr dwInstance, MidiInOpenFlags dwFlags);

	[DllImport("winmm.dll", CharSet = CharSet.Ansi, EntryPoint = "midiInClose")]
	public static extern int MidiInClose(IntPtr hMidiIn);

	[DllImport("winmm.dll", CharSet = CharSet.Ansi, EntryPoint = "midiInReset")]
	public static extern int MidiInReset(IntPtr hMidiIn);

	[DllImport("winmm.dll", CharSet = CharSet.Ansi, EntryPoint = "midiInStart")]
	public static extern int MidiInStart(IntPtr hMidiIn);

	[DllImport("winmm.dll", CharSet = CharSet.Ansi, EntryPoint = "midiInStop")]
	public static extern int MidiInStop(IntPtr hMidiIn);

	[DllImport("winmm.dll", EntryPoint = "midiInAddBuffer")]
	public static extern int MidiInAddBuffer(IntPtr hMidiIn, IntPtr headerPtr, int cbMidiInHdr);

	[DllImport("winmm.dll", EntryPoint = "midiInPrepareHeader")]
	public static extern int MidiInPrepareHeader(IntPtr hMidiIn, IntPtr headerPtr, int cbMidiInHdr);

	[DllImport("winmm.dll", EntryPoint = "midiInUnprepareHeader")]
	public static extern int MidiInUnprepareHeader(int hMidiIn, IntPtr headerPtr, int cbMidiInHdr);

	[DllImport("winmm.dll", EntryPoint = "midiInGetErrorText")]
	public static extern int MidiInGetErrorText(int wError, StringBuilder lpText, int cchText);

	[DllImport("winmm.dll", EntryPoint = "midiOutGetNumDevs")]
	public static extern int MidiOutGetNumDevs();

	[DllImport("winmm.dll", EntryPoint = "midiOutGetDevCaps")]
	public static extern int MidiOutGetDevCaps(int uDeviceID, ref MIDIOUTCAPS caps, int cbMidiOutCaps);

	[DllImport("winmm.dll", EntryPoint = "midiOutOpen")]
	public static extern int MidiOutOpen(out IntPtr lphMidiOut, uint uDeviceID, IntPtr dwCallback, IntPtr dwInstance, MidiOutOpenFlags dwFlags);

	[DllImport("winmm.dll", EntryPoint = "midiOutClose")]
	public static extern int MidiOutClose(IntPtr hMidiOut);

	[DllImport("winmm.dll", EntryPoint = "midiOutShortMsg")]
	public static extern int MidiOutShortMessage(IntPtr hMidiOut, uint dwMsg);

	[DllImport("winmm.dll", EntryPoint = "midiOutLongMsg")]
	public static extern int MidiOutLongMessage(int handle, IntPtr headerPtr, int sizeOfMidiHeader);

	[DllImport("winmm.dll", EntryPoint = "midiOutPrepareHeader")]
	public static extern int MidiOutPrepareHeader(int handle, IntPtr headerPtr, int sizeOfMidiHeader);

	[DllImport("winmm.dll", EntryPoint = "midiOutUnprepareHeader")]
	public static extern int MidiOutUnprepareHeader(int handle, IntPtr headerPtr, int sizeOfMidiHeader);
}
