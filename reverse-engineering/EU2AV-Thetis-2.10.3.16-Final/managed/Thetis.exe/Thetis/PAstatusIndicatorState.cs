using System;

namespace Thetis;

[Flags]
public enum PAstatusIndicatorState
{
	NotUsed = 0,
	OK = 1,
	PSUVoltage = 2,
	DrainCurrent = 4,
	ReversePower = 8,
	HeatsinkTemperature = 0x10,
	ForwardPower = 0x20,
	Resettable = 0x40
}
