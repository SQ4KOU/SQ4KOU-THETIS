using System;

namespace NAudio.Mixer;

[Flags]
internal enum MixerControlClass
{
	Custom = 0,
	Meter = 0x10000000,
	Switch = 0x20000000,
	Number = Meter | Switch,
	Slider = 0x40000000,
	Fader = Meter | Slider,
	Time = Switch | Slider,
	List = Number | Slider,
	Mask = List
}
