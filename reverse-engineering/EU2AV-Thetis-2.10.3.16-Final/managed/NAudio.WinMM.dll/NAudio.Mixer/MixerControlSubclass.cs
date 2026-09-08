using System;

namespace NAudio.Mixer;

[Flags]
internal enum MixerControlSubclass
{
	SwitchBoolean = 0,
	SwitchButton = 0x1000000,
	MeterPolled = 0,
	TimeMicrosecs = 0,
	TimeMillisecs = SwitchButton,
	ListSingle = 0,
	ListMultiple = SwitchButton,
	Mask = 0xF000000
}
