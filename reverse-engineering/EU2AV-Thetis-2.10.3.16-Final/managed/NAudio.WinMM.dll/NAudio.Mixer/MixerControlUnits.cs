using System;

namespace NAudio.Mixer;

[Flags]
internal enum MixerControlUnits
{
	Custom = 0,
	Boolean = 0x10000,
	Signed = 0x20000,
	Unsigned = Boolean | Signed,
	Decibels = 0x40000,
	Percent = Boolean | Decibels,
	Mask = 0xFF0000
}
