namespace Midi2Cat.IO;

public delegate void MidiInputEventHandler(MidiDevice Device, int DeviceIdx, int ControlId, int Data, int Status, int Event, int Channel);
