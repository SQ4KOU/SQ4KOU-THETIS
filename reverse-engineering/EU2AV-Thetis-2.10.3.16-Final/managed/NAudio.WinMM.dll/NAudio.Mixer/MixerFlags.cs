using System;

namespace NAudio.Mixer;

[Flags]
public enum MixerFlags
{
	Handle = int.MinValue,
	Mixer = 0,
	MixerHandle = Handle,
	WaveOut = 0x10000000,
	WaveOutHandle = Handle | WaveOut,
	WaveIn = 0x20000000,
	WaveInHandle = Handle | WaveIn,
	MidiOut = WaveOut | WaveIn,
	MidiOutHandle = WaveOutHandle | WaveIn,
	MidiIn = 0x40000000,
	MidiInHandle = Handle | MidiIn,
	Aux = WaveOut | MidiIn,
	Value = 0,
	ListText = 1,
	QueryMask = 0xF,
	All = 0,
	OneById = ListText,
	OneByType = 2,
	GetLineInfoOfDestination = 0,
	GetLineInfoOfSource = ListText,
	GetLineInfoOfLineId = OneByType,
	GetLineInfoOfComponentType = 3,
	GetLineInfoOfTargetType = 4,
	GetLineInfoOfQueryMask = QueryMask
}
