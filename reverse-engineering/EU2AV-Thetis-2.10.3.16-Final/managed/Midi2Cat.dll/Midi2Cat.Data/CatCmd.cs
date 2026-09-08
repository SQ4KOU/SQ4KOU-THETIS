namespace Midi2Cat.Data;

public enum CatCmd
{
	[CatCommand("Not Mapped", ControlType.Unknown)]
	None = 0,
	[CatCommand("VfoA To B", ControlType.Button)]
	VfoAtoB = 1,
	[CatCommand("VfoB To A", ControlType.Button)]
	VfoBtoA = 2,
	[CatCommand("Vfo Swap", ControlType.Button)]
	VfoSwap = 3,
	[CatCommand("Split On Off", ControlType.Button, true)]
	SplitOnOff = 4,
	[CatCommand("Zero Beat", ControlType.Button)]
	ZeroBeatPress = 5,
	[CatCommand("Rit On Off", ControlType.Button, true)]
	RitOnOff = 6,
	[CatCommand("Xit On Off", ControlType.Button, true)]
	XitOnOff = 7,
	[CatCommand("RIT Clear", ControlType.Button)]
	RIT_clear = 8,
	[CatCommand("XIT Clear", ControlType.Button)]
	XIT_clear = 9,
	[CatCommand("Multi Rx On Off", ControlType.Button, true)]
	MultiRxOnOff = 10,
	[CatCommand("Vfo Sync On Off", ControlType.Button, true)]
	VfoSyncOnOff = 11,
	[CatCommand("Toggle VFO Lock - A,both,off", ControlType.Button, true)]
	LockVFOOnOff = 12,
	[CatCommand("MOX On Off", ControlType.Button, true)]
	MOXOnOff = 13,
	[CatCommand("VOX On Off", ControlType.Button, true)]
	VOXOnOff = 14,
	[CatCommand("Mute On Off", ControlType.Button, true)]
	MuteOnOff = 15,
	[CatCommand("Rx1 Noise Blanker1 On Off", ControlType.Button, true)]
	Rx1NoiseBlanker1OnOff = 16,
	[CatCommand("Rx1 Noise Blanker2 On Off", ControlType.Button, true)]
	Rx1Noiseblanker2OnOff = 17,
	[CatCommand("Auto Notch On Off", ControlType.Button, true)]
	AutoNotchOnOff = 18,
	[CatCommand("Noise Reduction 1 On Off", ControlType.Button, true)]
	NoiseReductionOnOff = 19,
	[CatCommand("Noise Reduction 2 On Off", ControlType.Button, true)]
	NoiseReduction2OnOff = 20,
	[CatCommand("Noise Reduction 3 On Off", ControlType.Button, true)]
	NoiseReduction3OnOff = 113,
	[CatCommand("Noise Reduction 4 On Off", ControlType.Button, true)]
	NoiseReduction4OnOff = 114,
	[CatCommand("Noise Reduction 4 Amount", ControlType.Knob_or_Slider)]
	NoiseReduction4Amount = 115,
	[CatCommand("Binaural On Off", ControlType.Button, true)]
	BinauralOnOff = 21,
	[CatCommand("Rx1 Filter Wider", ControlType.Button)]
	Rx1FilterWider = 22,
	[CatCommand("Rx1 Filter Narrower", ControlType.Button)]
	Rx1FilterNarrower = 23,
	[CatCommand("Rx1 Mode Next", ControlType.Button)]
	Rx1ModeNext = 24,
	[CatCommand("Rx1 Mode Prev", ControlType.Button)]
	Rx1ModePrev = 25,
	[CatCommand("Tuning Step Up", ControlType.Button)]
	TuningStepUp = 26,
	[CatCommand("Tuning Step Down", ControlType.Button)]
	TuningStepDown = 27,
	[CatCommand("Band Up", ControlType.Button)]
	BandUp = 28,
	[CatCommand("Band Down", ControlType.Button)]
	BandDown = 29,
	[CatCommand("Start On Off", ControlType.Button, true)]
	StartOnOff = 30,
	[CatCommand("Tuner On Off", ControlType.Button, true)]
	TunerOnOff = 31,
	[CatCommand("Compander On Off", ControlType.Button, true)]
	CompanderOnOff = 32,
	[CatCommand("Stereo Diversity On Off", ControlType.Button, true)]
	StereoDiversityOnOff = 33,
	[CatCommand("DEXP On Off", ControlType.Button, true)]
	DEXPOnOff = 34,
	[CatCommand("RX2 On Off", ControlType.Button, true)]
	RX2OnOff = 35,
	[CatCommand("Rx2 Pre Amp On Off", ControlType.Button, true)]
	Rx2PreAmpOnOff = 36,
	[CatCommand("Rx2 Noise Blanker1 On Off", ControlType.Button, true)]
	Rx2NoiseBlanker1OnOff = 37,
	[CatCommand("Rx2 Noise Blanker2 On Off", ControlType.Button, true)]
	Rx2Noiseblanker2OnOff = 38,
	[CatCommand("Rx2 Band Up", ControlType.Button)]
	Rx2BandUp = 39,
	[CatCommand("Rx2 Band Down", ControlType.Button)]
	Rx2BandDown = 40,
	[CatCommand("RX EQ On Off", ControlType.Button, true)]
	RXEQOnOff = 41,
	[CatCommand("TX EQ On Off", ControlType.Button, true)]
	TXEQOnOff = 42,
	[CatCommand("Squelch On Off", ControlType.Button, true)]
	SquelchOnOff = 43,
	[CatCommand("Spectral Noise Blanker On Off", ControlType.Button, true)]
	SpectralNoiseBlankerOnOff = 44,
	[CatCommand("AGC Mode Up", ControlType.Button)]
	AGCModeUp = 45,
	[CatCommand("AGC Mode Down", ControlType.Button)]
	AGCModeDown = 46,
	[CatCommand("Rx2 Spectral Noise Blanker On Off", ControlType.Button, true)]
	SpectralNoiseBlankerRx2OnOff = 47,
	[CatCommand("Display Average", ControlType.Button)]
	DisplayAverage = 48,
	[CatCommand("Display Peak", ControlType.Button)]
	DisplayPeak = 49,
	[CatCommand("Display Tx Filter", ControlType.Button)]
	DisplayTxFilter = 50,
	[CatCommand("Display Mode Next", ControlType.Button)]
	DisplayModeNext = 51,
	[CatCommand("Display Mode Prev", ControlType.Button)]
	DisplayModePrev = 52,
	[CatCommand("Zoom Inc", ControlType.Button)]
	ZoomInc = 53,
	[CatCommand("Zoom Dec", ControlType.Button)]
	ZoomDec = 54,
	[CatCommand("Quick Mode Save", ControlType.Button)]
	QuickModeSave = 55,
	[CatCommand("Quick Mode Restore", ControlType.Button)]
	QuickModeRestore = 56,
	[CatCommand("CW XMacro 1", ControlType.Button)]
	CWXMacro1 = 57,
	[CatCommand("CWX Macro 2", ControlType.Button)]
	CWXMacro2 = 58,
	[CatCommand("CWX Macro 3", ControlType.Button)]
	CWXMacro3 = 59,
	[CatCommand("CWX Macro 4", ControlType.Button)]
	CWXMacro4 = 60,
	[CatCommand("CWX Macro 5", ControlType.Button)]
	CWXMacro5 = 61,
	[CatCommand("CWX Macro 6", ControlType.Button)]
	CWXMacro6 = 62,
	[CatCommand("CWX Macro 7", ControlType.Button)]
	CWXMacro7 = 63,
	[CatCommand("CWX Macro 8", ControlType.Button)]
	CWXMacro8 = 64,
	[CatCommand("CWX Macro 9", ControlType.Button)]
	CWXMacro9 = 65,
	[CatCommand("CWX Stop", ControlType.Button)]
	CWXStop = 66,
	[CatCommand("MON On Off", ControlType.Button, true)]
	MONOnOff = 67,
	[CatCommand("Pan Center", ControlType.Button)]
	PanCenter = 68,
	[CatCommand("VAC On Off", ControlType.Button, true)]
	VACOnOff = 69,
	[CatCommand("Rx1 IQ To VAC", ControlType.Button)]
	IQtoVAC = 70,
	[CatCommand("Rx2 IQ to VAC", ControlType.Button)]
	IQtoVACRX2 = 71,
	[CatCommand("VAC 2 On Off", ControlType.Button, true)]
	VAC2OnOff = 72,
	[CatCommand("Click Tune On Off", ControlType.Button, true)]
	CTunOnOff = 73,
	[CatCommand("ESC Form On Off", ControlType.Button, true)]
	ESCFormOnOff = 74,
	[CatCommand("RX2 Mute On Off", ControlType.Button, true)]
	MuteRX2OnOff = 75,
	[CatCommand("Tune On Off", ControlType.Button, true)]
	TunOnOff = 76,
	[CatCommand("Tuner Bypass", ControlType.Button, true)]
	TunerBypassOnOff = 77,
	[CatCommand("Band 160m", ControlType.Button)]
	Band160m = 78,
	[CatCommand("Band 80m", ControlType.Button)]
	Band80m = 79,
	[CatCommand("Band 60m", ControlType.Button)]
	Band60m = 80,
	[CatCommand("Band 40m", ControlType.Button)]
	Band40m = 81,
	[CatCommand("Band 30m", ControlType.Button)]
	Band30m = 82,
	[CatCommand("Band 20m", ControlType.Button)]
	Band20m = 83,
	[CatCommand("Band 17m", ControlType.Button)]
	Band17m = 84,
	[CatCommand("Band 15m", ControlType.Button)]
	Band15m = 85,
	[CatCommand("Band 12m", ControlType.Button)]
	Band12m = 86,
	[CatCommand("Band 10m", ControlType.Button)]
	Band10m = 87,
	[CatCommand("Band 6m", ControlType.Button)]
	Band6m = 88,
	[CatCommand("Band 2m", ControlType.Button)]
	Band2m = 89,
	[CatCommand("RX2 Band 160m", ControlType.Button)]
	Band160mRX2 = 90,
	[CatCommand("RX2 Band 80m", ControlType.Button)]
	Band80mRX2 = 91,
	[CatCommand("RX2 Band 60m", ControlType.Button)]
	Band60mRX2 = 92,
	[CatCommand("RX2 Band 40m", ControlType.Button)]
	Band40mRX2 = 93,
	[CatCommand("RX2 Band 30m", ControlType.Button)]
	Band30mRX2 = 94,
	[CatCommand("RX2 Band 20m", ControlType.Button)]
	Band20mRX2 = 95,
	[CatCommand("RX2 Band 17m", ControlType.Button)]
	Band17mRX2 = 96,
	[CatCommand("RX2 Band 15m", ControlType.Button)]
	Band15mRX2 = 97,
	[CatCommand("RX2 Band 12m", ControlType.Button)]
	Band12mRX2 = 98,
	[CatCommand("RX2 Band 10m", ControlType.Button)]
	Band10mRX2 = 99,
	[CatCommand("RX2 Band 6m", ControlType.Button)]
	Band6mRX2 = 500,
	[CatCommand("Band2mRX2", ControlType.Button)]
	Band2mRX2 = 501,
	[CatCommand("Mode SSB", ControlType.Button)]
	ModeSSB = 502,
	[CatCommand("Mode LSB", ControlType.Button)]
	ModeLSB = 503,
	[CatCommand("Mode USB", ControlType.Button)]
	ModeUSB = 504,
	[CatCommand("Mode DSB", ControlType.Button)]
	ModeDSB = 505,
	[CatCommand("Mode CW", ControlType.Button)]
	ModeCW = 506,
	[CatCommand("Mode CWL", ControlType.Button)]
	ModeCWL = 507,
	[CatCommand("Mode CWU", ControlType.Button)]
	ModeCWU = 508,
	[CatCommand("Mode FM", ControlType.Button)]
	ModeFM = 509,
	[CatCommand("Mode AM", ControlType.Button)]
	ModeAM = 510,
	[CatCommand("Mode DIGU", ControlType.Button)]
	ModeDIGU = 511,
	[CatCommand("Mode SPEC", ControlType.Button)]
	ModeSPEC = 512,
	[CatCommand("Mode DIGL", ControlType.Button)]
	ModeDIGL = 513,
	[CatCommand("Mode SAM", ControlType.Button)]
	ModeSAM = 514,
	[CatCommand("Mode DRM", ControlType.Button)]
	ModeDRM = 515,
	[CatCommand("Move VFOA Down 100Khz", ControlType.Button)]
	MoveVFOADown100Khz = 520,
	[CatCommand("Move VFOA Up 100Khz", ControlType.Button)]
	MoveVFOAUp100Khz = 521,
	[CatCommand("Change Freq Vfo A", ControlType.Wheel)]
	ChangeFreqVfoA = 101,
	[CatCommand("Change Freq Vfo B", ControlType.Wheel)]
	ChangeFreqVfoB = 102,
	[CatCommand("FilterBandwidth", ControlType.Wheel)]
	FilterBandwidth = 103,
	[CatCommand("RIT", ControlType.Wheel)]
	RIT_inc = 104,
	[CatCommand("XIT", ControlType.Wheel)]
	XIT_inc = 105,
	[CatCommand("Zoom", ControlType.Wheel)]
	ZoomSliderInc = 106,
	[CatCommand("Filter High", ControlType.Wheel)]
	FilterHigh = 107,
	[CatCommand("Filter Low", ControlType.Wheel)]
	FilterLow = 108,
	[CatCommand("Pan", ControlType.Wheel)]
	PanSliderInc = 109,
	[CatCommand("Multi Step Vfo A", ControlType.Wheel)]
	MultiStepVfoA = 110,
	[CatCommand("RIT", ControlType.Knob_or_Slider)]
	RIT = 201,
	[CatCommand("Manual or Semi Break-In", ControlType.Button, true)]
	CWBreakIn = 111,
	[CatCommand("Semi or QSK Break-In", ControlType.Button, true)]
	CWQSK = 112,
	[CatCommand("XIT", ControlType.Knob_or_Slider)]
	XIT = 202,
	[CatCommand("Filter Shift", ControlType.Knob_or_Slider)]
	FilterShift = 203,
	[CatCommand("Volume VfoA", ControlType.Knob_or_Slider)]
	VolumeVfoA = 204,
	[CatCommand("Volume VfoB", ControlType.Knob_or_Slider)]
	VolumeVfoB = 205,
	[CatCommand("Ratio Main Sub Rx", ControlType.Knob_or_Slider)]
	RatioMainSubRx = 206,
	[CatCommand("PreAmp Setting", ControlType.Knob_or_Slider)]
	PreAmpSettingsKnob = 207,
	[CatCommand("CW Speed", ControlType.Knob_or_Slider)]
	CWSpeed = 208,
	[CatCommand("AF Gain", ControlType.Knob_or_Slider)]
	SetAFGain = 209,
	[CatCommand("RX1 AGC Level", ControlType.Knob_or_Slider)]
	AGCLevel = 210,
	[CatCommand("DriveLevel", ControlType.Knob_or_Slider)]
	DriveLevel = 211,
	[CatCommand("MicGain", ControlType.Knob_or_Slider)]
	MicGain = 212,
	[CatCommand("DXLevel", ControlType.Knob_or_Slider)]
	DXLevel = 213,
	[CatCommand("CPDRLevel", ControlType.Knob_or_Slider)]
	CPDRLevel = 214,
	[CatCommand("VOXGain", ControlType.Knob_or_Slider)]
	VOXGain = 215,
	[CatCommand("DEXP Threshold", ControlType.Knob_or_Slider)]
	DEXPThreshold = 216,
	[CatCommand("Squelch", ControlType.Knob_or_Slider)]
	SquelchControl = 217,
	[CatCommand("RX2 AGC Level", ControlType.Knob_or_Slider)]
	RX2AGCLevel = 218,
	[CatCommand("TX AF Monitor", ControlType.Knob_or_Slider)]
	TXAFMonitor = 219,
	[CatCommand("AGC Mode", ControlType.Knob_or_Slider)]
	AGCModeKnob = 220,
	[CatCommand("Zoom", ControlType.Knob_or_Slider)]
	ZoomSliderFix = 221,
	[CatCommand("RX2 Volume", ControlType.Knob_or_Slider)]
	RX2Volume = 222,
	[CatCommand("Pan", ControlType.Knob_or_Slider)]
	PanSlider = 223,
	[CatCommand("VAC Gain RX", ControlType.Knob_or_Slider)]
	VACGainRX = 224,
	[CatCommand("VAC Gain TX", ControlType.Knob_or_Slider)]
	VACGainTX = 225,
	[CatCommand("VAC2 Gain RX", ControlType.Knob_or_Slider)]
	VAC2GainRX = 226,
	[CatCommand("VAC2 Gain TX", ControlType.Knob_or_Slider)]
	VAC2GainTX = 227,
	[CatCommand("Waterfall Low Limit", ControlType.Knob_or_Slider)]
	WaterfallLowLimit = 228,
	[CatCommand("Waterfall High Limit", ControlType.Knob_or_Slider)]
	WaterfallHighLimit = 229,
	[CatCommand("Stereo Balance RX2 (PAN) ", ControlType.Knob_or_Slider)]
	RX2Pan = 230,
	[CatCommand("Volume VfoA Incr", ControlType.Wheel)]
	VolumeVfoA_inc = 241,
	[CatCommand("Volume VfoB Incr", ControlType.Wheel)]
	VolumeVfoB_inc = 242,
	[CatCommand("RX1 AGC Level Incr", ControlType.Wheel)]
	AGCLevel_inc = 243,
	[CatCommand("RX2 AGC Level Incr", ControlType.Wheel)]
	RX2AGCLevel_inc = 244,
	[CatCommand("CW Speed Incr", ControlType.Wheel)]
	CWSpeed_inc = 245,
	[CatCommand("Audio Peak Filter On Off", ControlType.Button, true)]
	APF_OnOff = 246,
	[CatCommand("Audio Peak Filter Tune", ControlType.Knob_or_Slider)]
	APFFreq = 247,
	[CatCommand("Audio Peak Filter Bandwidth", ControlType.Knob_or_Slider)]
	APFBandwidth = 248,
	[CatCommand("Audio Peak Filter Gain", ControlType.Knob_or_Slider)]
	APFGain = 249,
	[CatCommand("Rx2 Noise Reduction1 On Off", ControlType.Button, true)]
	Rx2NoiseReductionOnOff = 250,
	[CatCommand("Rx2 Noise Reduction2 On Off", ControlType.Button, true)]
	Rx2NoiseReduction2OnOff = 251,
	[CatCommand("Rx2 Noise Reduction 3 On Off", ControlType.Button, true)]
	Rx2NoiseReduction3OnOff = 309,
	[CatCommand("Rx2 Noise Reduction 4 On Off", ControlType.Button, true)]
	Rx2NoiseReduction4OnOff = 310,
	[CatCommand("Rx2 Noise Reduction 4 Amount", ControlType.Knob_or_Slider)]
	Rx2NoiseReduction4Amount = 311,
	[CatCommand("Increase wheel rotation per VFO tune step", ControlType.Button)]
	MidiMessagesPerTuneStepUp = 252,
	[CatCommand("Decrease wheel rotation per VFO tune step", ControlType.Button)]
	MidiMessagesPerTuneStepDown = 253,
	[CatCommand("VFO Wheel Sensitivity High/Low Toggle", ControlType.Button, true)]
	MidiMessagesPerTuneStepToggle = 254,
	[CatCommand("Drive Level Increment", ControlType.Wheel)]
	DriveLevel_inc = 255,
	[CatCommand("Lock VFO A", ControlType.Button, true)]
	LockVFOAOnOff = 256,
	[CatCommand("Lock VFO B", ControlType.Button, true)]
	LockVFOBOnOff = 257,
	[CatCommand("Diversity Form Open", ControlType.Button, true)]
	DiversityFormOpen = 258,
	[CatCommand("Diversity Enable", ControlType.Button, true)]
	DiversityEnable = 259,
	[CatCommand("Diversity Phase", ControlType.Wheel)]
	DiversityPhase = 260,
	[CatCommand("Diversity Gain", ControlType.Wheel)]
	DiversityGain = 261,
	[CatCommand("Diversity RX Reference", ControlType.Button, true)]
	DiversityReference = 262,
	[CatCommand("Diversity RX Source", ControlType.Button, true)]
	DiversitySource = 263,
	[CatCommand("2Tone On Off", ControlType.Button, true)]
	TwoToneOnOff = 264,
	[CatCommand("PS-A On Off", ControlType.Button, true)]
	PSOnOff = 265,
	[CatCommand("Move VFOB Down 100Khz", ControlType.Button)]
	MoveVFOBDown100Khz = 266,
	[CatCommand("Move VFOB Up 100Khz", ControlType.Button)]
	MoveVFOBUp100Khz = 267,
	[CatCommand("RX2 Auto Notch On Off", ControlType.Button, true)]
	RX2AutoNotchOnOff = 268,
	[CatCommand("RX2 Mode Next", ControlType.Button)]
	Rx2ModeNext = 269,
	[CatCommand("RX2 Mode Prev", ControlType.Button)]
	Rx2ModePrev = 270,
	[CatCommand("RX2 Filter Wider", ControlType.Button)]
	Rx2FilterWider = 271,
	[CatCommand("RX2 Filter Narrower", ControlType.Button)]
	Rx2FilterNarrower = 272,
	[CatCommand("RX2 AGC Mode Up", ControlType.Button)]
	RX2AGCModeUp = 273,
	[CatCommand("RX2 AGC Mode Down", ControlType.Button)]
	RX2AGCModeDown = 274,
	[CatCommand("RX2 CTUN On Off", ControlType.Button, true)]
	RX2CTunOnOff = 275,
	[CatCommand("RX2 Mode SSB", ControlType.Button)]
	RX2ModeSSB = 276,
	[CatCommand("RX2 Mode LSB", ControlType.Button)]
	RX2ModeLSB = 277,
	[CatCommand("RX2 Mode USB", ControlType.Button)]
	RX2ModeUSB = 278,
	[CatCommand("RX2 Mode DSB", ControlType.Button)]
	RX2ModeDSB = 279,
	[CatCommand("RX2 Mode CW", ControlType.Button)]
	RX2ModeCW = 280,
	[CatCommand("RX2 Mode CWL", ControlType.Button)]
	RX2ModeCWL = 281,
	[CatCommand("RX2 Mode CWU", ControlType.Button)]
	RX2ModeCWU = 282,
	[CatCommand("RX2 Mode FM", ControlType.Button)]
	RX2ModeFM = 283,
	[CatCommand("RX2 Mode AM", ControlType.Button)]
	RX2ModeAM = 284,
	[CatCommand("RX2 Mode DIGU", ControlType.Button)]
	RX2ModeDIGU = 285,
	[CatCommand("RX2 Mode SPEC", ControlType.Button)]
	RX2ModeSPEC = 286,
	[CatCommand("RX2 Mode DIGL", ControlType.Button)]
	RX2ModeDIGL = 287,
	[CatCommand("RX2 Mode SAM", ControlType.Button)]
	RX2ModeSAM = 288,
	[CatCommand("RX2 Mode DRM", ControlType.Button)]
	RX2ModeDRM = 289,
	[CatCommand("Close Thetis", ControlType.Button)]
	CloseConsole = 290,
	[CatCommand("Toggle TX VFOA VFOB", ControlType.Button, true)]
	ToggleTX = 291,
	[CatCommand("RX2 AGC Mode", ControlType.Knob_or_Slider)]
	RX2AGCModeKnob = 292,
	[CatCommand("TUN Power Level", ControlType.Knob_or_Slider)]
	TUNPowerLevel = 293,
	[CatCommand("RX2 Squelch On Off", ControlType.Button, true)]
	RX2SquelchOnOff = 294,
	[CatCommand("RX2 Squelch Level", ControlType.Knob_or_Slider)]
	RX2SquelchControl = 295,
	[CatCommand("TX Filter high", ControlType.Wheel)]
	TXFilterHigh = 296,
	[CatCommand("TX Filter low", ControlType.Wheel)]
	TXFilterLow = 297,
	[CatCommand("External PA On Off", ControlType.Button, true)]
	ExternalPAOnOff = 298,
	[CatCommand("Zoom To Band Recall", ControlType.Button)]
	ZoomToBandRecall = 299,
	[CatCommand("Zoom To Band Store", ControlType.Button)]
	ZoomToBandStore = 300,
	[CatCommand("RX1 Auto AGC compensation", ControlType.Button, true)]
	RX1AutoAGC = 301,
	[CatCommand("RX2 Auto AGC compensation", ControlType.Button, true)]
	RX2AutoAGC = 302,
	[CatCommand("Swap VFO Wheels", ControlType.Button, true)]
	SwapVFOWheels = 303,
	[CatCommand("Quick Split On Off", ControlType.Button, true)]
	QuickSplitOnOff = 304,
	[CatCommand("Quick Split + VFO Split On Off", ControlType.Button, true)]
	QuickSplitOnOffandSplitOnOff = 305,
	[CatCommand("Quick Play Wave File", ControlType.Button, true)]
	QuickPlayOnOff = 306,
	[CatCommand("Quick Rec Wave File", ControlType.Button, true)]
	QuickRecOnOff = 307,
	[CatCommand("Audio Amp On Off", ControlType.Button, true)]
	AudioAmpOnOff = 308,
	[CatCommand("APF Type Double Pole", ControlType.Button)]
	APFType_doublepole = 312,
	[CatCommand("APF Type Matched", ControlType.Button)]
	APFType_matched = 313,
	[CatCommand("APF Type Gaussian", ControlType.Button)]
	APFType_gaussian = 314,
	[CatCommand("APF Type Bi-Quad", ControlType.Button)]
	APFType_biquad = 315,
	[CatCommand("Toggle Wheel to VFOA/VFOB ", ControlType.Button)]
	ToggleVFOWheel = 700
}
