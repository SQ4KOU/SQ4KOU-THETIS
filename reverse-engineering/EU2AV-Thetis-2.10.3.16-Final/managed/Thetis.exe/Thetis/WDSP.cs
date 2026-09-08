using System.Runtime.InteropServices;

namespace Thetis;

internal class WDSP
{
	public enum MeterType
	{
		SIGNAL_STRENGTH,
		AVG_SIGNAL_STRENGTH,
		ADC_REAL,
		ADC_IMAG,
		AGC_GAIN,
		MIC,
		PWR,
		ALC,
		EQ,
		LEVELER,
		COMP,
		CPDR,
		ALC_G,
		LVL_G,
		MIC_PK,
		ALC_PK,
		EQ_PK,
		LEVELER_PK,
		COMP_PK,
		CPDR_PK,
		CFC_PK,
		CFC_G,
		AGC_PK,
		AGC_AV,
		CFC_AV,
		METERTYPE_LAST
	}

	public enum rxaMeterType
	{
		RXA_S_PK,
		RXA_S_AV,
		RXA_ADC_PK,
		RXA_ADC_AV,
		RXA_AGC_GAIN,
		RXA_AGC_PK,
		RXA_AGC_AV,
		RXA_METERTYPE_LAST
	}

	public enum txaMeterType
	{
		TXA_MIC_PK,
		TXA_MIC_AV,
		TXA_EQ_PK,
		TXA_EQ_AV,
		TXA_LVLR_PK,
		TXA_LVLR_AV,
		TXA_LVLR_GAIN,
		TXA_CFC_PK,
		TXA_CFC_AV,
		TXA_CFC_GAIN,
		TXA_COMP_PK,
		TXA_COMP_AV,
		TXA_ALC_PK,
		TXA_ALC_AV,
		TXA_ALC_GAIN,
		TXA_OUT_PK,
		TXA_OUT_AV,
		TXA_METERTYPE_LAST
	}

	private static double alcgain = 3.0;

	public static double ALCGain
	{
		get
		{
			return alcgain;
		}
		set
		{
			alcgain = value;
		}
	}

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void OpenChannel(int channel, int in_size, int dsp_size, int input_samplerate, int dsp_rate, int output_samplerate, int type, int state, double tdelayup, double tslewup, double tdelaydown, double tslewdown, int bfo);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void CloseChannel(int channel);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetInputBuffsize(int channel, int in_size);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDSPBuffsize(int channel, int dsp_size);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetInputSamplerate(int channel, int rate);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDSPSamplerate(int channel, int rate);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetOutputSamplerate(int channel, int rate);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetAllRates(int channel, int in_rate, int dsp_rate, int out_rate);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int SetChannelState(int channel, int state, int dmode);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetChannelTDelayUp(int channel, double time);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetChannelTSlewUp(int channel, double time);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetChannelTDelayDown(int channel, double time);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetChannelTSlewDown(int channel, double time);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAuSlewTime(int channel, double time);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAMode(int channel, DSPMode mode);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAMode(int channel, DSPMode mode);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void fexchange0(int channel, double* Cin, double* Cout, int* error);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void fexchange2(int channel, float* Iin, float* Qin, float* Iout, float* Qout, int* error);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAAGCMode(int channel, AGCMode mode);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAAGCFixed(int channel, double fixed_agc);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void GetRXAAGCTop(int channel, double* max_agc);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAAGCTop(int channel, double max_agc);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAAGCAttack(int channel, int attack);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAAGCDecay(int channel, int decay);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAAGCHang(int channel, int hang);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAAGCSlope(int channel, int slope);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void GetRXAAGCHangThreshold(int channel, int* hangthreshold);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAAGCHangThreshold(int channel, int hangthreshold);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void GetRXAAGCThresh(int channel, double* thresh, double size, double rate);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAAGCThresh(int channel, double thresh, double size, double rate);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void GetRXAAGCHangLevel(int channel, double* hanglevel);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAAGCHangLevel(int channel, double hanglevel);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAALCDecay(int channel, int decay);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAALCMaxGain(int channel, double maxgain);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAAMDSBMode(int channel, int sbmode);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAAMDFadeLevel(int channel, int fadelevel);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAAMSQRun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAAMSQThreshold(int channel, double threshold);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAAMSQMaxTail(int channel, double tail);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAAMCarrierLevel(int channel, double carrier);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAANFRun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAANFVals(int channel, int taps, int delay, double gain, double leak);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAANFTaps(int channel, int taps);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAANFDelay(int channel, int delay);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAANFGain(int channel, double gain);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAANFLeakage(int channel, double leakage);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAANFPosition(int channel, int position);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAANRRun(int channel, int run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAANRVals(int channel, int taps, int delay, double gain, double leak);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAANRTaps(int channel, int taps);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAANRDelay(int channel, int delay);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAANRGain(int channel, double gain);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAANRLeakage(int channel, double leakage);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAANRPosition(int channel, int position);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXABandpassFreqs(int channel, double low, double high);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXABandpassWindow(int channel, int wintype);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXABandpassFreqs(int channel, double low, double high);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXABandpassWindow(int channel, int wintype);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXACBLRun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXACBLPosition(int channel, int position);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXACFIRRun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXACompressorRun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXACompressorGain(int channel, double gain);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAosctrlRun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAEMNRRun(int channel, int run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXARNNRRun(int channel, int run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXARNNRPosition(int channel, int position);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void RNNRloadModel(string file_path);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXARNNRUseDefaultGain(int channel, int use_default_gain);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASBNRRun(int channel, int run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASBNRPosition(int channel, int position);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASBNRreductionAmount(int channel, float amount);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASBNRsmoothingFactor(int channel, float factor);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASBNRwhiteningFactor(int channel, float factor);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASBNRnoiseRescale(int channel, float factor);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASBNRpostFilterThreshold(int channel, float threshold);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASBNRnoiseScalingType(int channel, int noise_scaling_type);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAEMNRPosition(int channel, int position);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAEMNRgainMethod(int channel, int method);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAEMNRnpeMethod(int channel, int method);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAEMNRaeRun(int channel, int run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAEMNRpost2Run(int channel, int run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAEMNRpost2Nlevel(int channel, double nlevel);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAEMNRpost2Factor(int channel, double factor);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAEMNRpost2Rate(int channel, double tc);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAEMNRpost2Taper(int channel, int taper);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAEMNRtrainZetaThresh(int channel, double thresh);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAEMNRtrainT2(int channel, double t2);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAEQRun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAEQRun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void SetRXAGrphEQ(int channel, int* ptr);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void SetTXAGrphEQ(int channel, int* ptr);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void SetRXAGrphEQ10(int channel, int* ptr);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void SetTXAGrphEQ10(int channel, int* ptr);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAFMDeviation(int channel, double deviation);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAFMSQRun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAFMSQThreshold(int channel, double threshold);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAFMLimRun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAFMLimGain(int channel, double gaindB);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAFMAFFilter(int channel, double low, double high);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAFMAFFilter(int channel, double low, double high);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAFMDeviation(int channel, double deviation);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAFMEmphPosition(int channel, bool position);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXACTCSSRun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXACTCSSFreq(int channel, double freq_hz);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXACTCSSRun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXACTCSSFreq(int channel, double freq_hz);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXALevelerTop(int channel, double maxgain);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXALevelerDecay(int channel, int decay);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXALevelerSt(int channel, bool state);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern double GetRXAMeter(int channel, rxaMeterType meter);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern double GetTXAMeter(int channel, txaMeterType meter);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAPanelRun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAPanelSelect(int channel, int select);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAPanelGain1(int channel, double gain);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAPanelPan(int channel, double pan);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAPanelBinaural(int channel, bool bin);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPanelRun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPanelGain1(int channel, double gain);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAShiftFreq(int channel, double freq);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASpectrum(int channel, int flag, int disp, int ss, int LO);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void TXAGetSpecF1(int channel, float* results);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void RXAGetaSipF(int channel, float* results, int size);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void RXAGetaSipF1(int channel, float* results, int size);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void TXASetSipPosition(int channel, int pos);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void TXASetSipMode(int channel, int mode);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void TXASetSipDisplay(int channel, int disp);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void TXAGetaSipF(int channel, float* results, int size);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void TXAGetaSipF1(int channel, float* results, int size);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void* create_resampleFV(int in_rate, int out_rate);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void xresampleFV(float* input, float* output, int numsamps, int* outsamps, void* ptr);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void destroy_resampleFV(void* ptr);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int WDSPwisdom(string directory);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAPreGenRun(int channel, int run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAPreGenMode(int channel, int mode);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAPreGenToneMag(int channel, double mag);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAPreGenToneFreq(int channel, double freq);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAPreGenNoiseMag(int channel, double mag);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAPreGenSweepMag(int channel, double mag);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAPreGenSweepFreq(int channel, double freq1, double freq2);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAPreGenSweepRate(int channel, double rate);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPreGenRun(int channel, int run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPreGenMode(int channel, int mode);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPreGenToneMag(int channel, double mag);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPreGenToneFreq(int channel, double freq);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPreGenNoiseMag(int channel, double mag);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPreGenSweepMag(int channel, double mag);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPreGenSweepFreq(int channel, double freq1, double freq2);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPreGenSweepRate(int channel, double rate);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPreGenSawtoothMag(int channel, double mag);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPreGenSawtoothFreq(int channel, double freq);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPreGenTriangleMag(int channel, double mag);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPreGenTriangleFreq(int channel, double freq);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPreGenPulseMag(int channel, double mag);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPreGenPulseFreq(int channel, double freq);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPreGenPulseDutyCycle(int channel, double dc);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPreGenPulseToneFreq(int channel, double freq);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPreGenPulseTransition(int channel, double transtime);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenRun(int channel, int run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenMode(int channel, int mode);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenToneFreq(int channel, double freq);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenToneMag(int channel, double mag);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenTTMag(int channel, double mag1, double mag2);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenTTFreq(int channel, double freq1, double freq2);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenSweepMag(int channel, double mag);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenSweepFreq(int channel, double freq1, double freq2);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenSweepRate(int channel, double rate);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenPulseMag(int channel, double mag);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenPulseFreq(int channel, double freq);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenPulseDutyCycle(int channel, double dc);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenPulseToneFreq(int channel, double freq);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenPulseTransition(int channel, double transtime);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenPulseIQout(int channel, int IQout);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenTTPulseMag(int channel, double mag1, double mag2);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenTTPulseFreq(int channel, double freq1);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenTTPulseDutyCycle(int channel, double dc);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenTTPulseToneFreq(int channel, double freq1, double freq2);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenTTPulseTransition(int channel, double transtime);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPostGenTTPulseIQout(int channel, int IQout);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int GetWDSPVersion();

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void create_divEXT(int id, int run, int nr, int size);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void destroy_divEXT(int id);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEXTDIVRun(int id, int run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEXTDIVNr(int id, int nr);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEXTDIVOutput(int id, int output);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void SetEXTDIVRotate(int id, int nr, double* Irotate, double* Qrotate);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void create_eerEXT(int id, int run, int size, int rate, double mgain, double pgain, bool rundelays, double mdelay, double pdelay, int amiq);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void destroy_eerEXT(int id);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void xeerEXTF(int id, float* inI, float* inQ, float* outI, float* outQ, float* outM);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEERRun(int id, bool run);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEERAMIQ(int id, bool amiq);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEERMgain(int id, double gain);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEERPgain(int id, double gain);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEERRunDelays(int id, bool run);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEERMdelay(int id, double delay);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEERPdelay(int id, double delay);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEERSize(int id, int size);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEERSamplerate(int id, int rate);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASPCWRun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASPCWFreq(int channel, double freq);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASPCWBandwidth(int channel, double bw);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASPCWGain(int channel, double gain);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASPCWSelection(int channel, int selection);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAmpeakRun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAmpeakFilFreq(int channel, int fil, double freq);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAmpeakFilBw(int channel, int fil, double bw);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXAmpeakFilGain(int channel, int fil, double gain);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASNBARun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASNBAk1(int channel, double k1);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASNBAk2(int channel, double k2);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int RXANBPAddNotch(int channel, int notch, double fcenter, double fwidth, bool active);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern int RXANBPGetNotch(int channel, int notch, double* fcenter, double* fwidth, int* active);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int RXANBPDeleteNotch(int channel, int notch);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int RXANBPEditNotch(int channel, int notch, double fcenter, double fwidth, bool active);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void RXANBPGetNumNotches(int channel, int* nnotches);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void RXANBPSetTuneFrequency(int channel, double tunefreq);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void RXANBPSetShiftFrequency(int channel, double shift);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void RXANBPSetNotchesRun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void RXANBPSetFreqs(int channel, double flow, double fhigh);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void RXANBPSetWindow(int channel, int wintype);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void RXANBPGetMinNotchWidth(int channel, double* minwidth);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void RXANBPSetAutoIncrease(int channel, bool autoincr);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASNBAOutputBandwidth(int channel, double flow, double fhigh);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void RXASetMP(int channel, bool mp);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void TXASetMP(int channel, bool mp);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void RXASetNC(int channel, int nc);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void TXASetNC(int channel, int nc);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXACFCOMPRun(int channel, int run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void SetTXACFCOMPprofile(int channel, int nfreqs, double* F, double* G, double* E, double* Qg, double* Qe);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXACFCOMPPosition(int channel, int pos);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXACFCOMPPrecomp(int channel, double precomp);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXACFCOMPPeqRun(int channel, int run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXACFCOMPPrePeq(int channel, double prepeq);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPHROTRun(int channel, int run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPHROTCorner(int channel, double corner);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPHROTNstages(int channel, int nstages);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPHROTReverse(int channel, int reverse);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPHROTAutoMode(int channel, int autoMode);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetTXAPHROTAutoReset(int channel);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void GetTXAPHROTAsymmetry(int channel, double* in_pos, double* in_neg, double* in_ratio, double* out_pos, double* out_neg, double* out_ratio, double* current_fc, double* auto_step);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void SetTXAEQProfile(int channel, int nfreqs, double* F, double* G, double* Q);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void SetRXAEQProfile(int channel, int nfreqs, double* F, double* G, double* Q);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void GetTXACFCOMPDisplayCompression(int channel, double* comp_values, int* ready);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASSQLThreshold(int channel, double threshold);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASSQLRun(int channel, bool run);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASSQLTauMute(int channel, double tau_mute);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetRXASSQLTauUnMute(int channel, double tau_unmute);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void create_bfcu(int id, int min_size, int max_size, double rate, double corner, int points);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void getFilterCorners(int id, int* lower_index, int* upper_index);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void getFilterCurve(int id, int size, int w_type, int index_low, int index_high, double* segment);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void destroy_bfcu(int id);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void save_impulse_cache(string file);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void read_impulse_cache(string file);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void use_impulse_cache(int use);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void init_impulse_cache(int use);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void destroy_impulse_cache();

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetupDetectMaxBin(int run, int disp, int ss, int LO, double rate, double fLow, double fHigh, double tau, int frame_rate);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern double GetDetectMaxBin(int disp);

	public static int id(uint thread, uint subrx)
	{
		return (2 * thread + subrx) switch
		{
			0u => 0, 
			1u => 1, 
			2u => cmaster.CMsubrcvr * cmaster.CMrcvr, 
			3u => cmaster.CMsubrcvr * cmaster.CMrcvr, 
			4u => 2, 
			5u => 3, 
			_ => -1, 
		};
	}

	public static float CalculateRXMeter(uint thread, uint subrx, MeterType MT)
	{
		int channel = id(thread, subrx);
		return (float)(MT switch
		{
			MeterType.SIGNAL_STRENGTH => GetRXAMeter(channel, rxaMeterType.RXA_S_PK), 
			MeterType.AVG_SIGNAL_STRENGTH => GetRXAMeter(channel, rxaMeterType.RXA_S_AV), 
			MeterType.ADC_REAL => GetRXAMeter(channel, rxaMeterType.RXA_ADC_PK), 
			MeterType.ADC_IMAG => GetRXAMeter(channel, rxaMeterType.RXA_ADC_AV), 
			MeterType.AGC_GAIN => GetRXAMeter(channel, rxaMeterType.RXA_AGC_GAIN), 
			MeterType.AGC_PK => GetRXAMeter(channel, rxaMeterType.RXA_AGC_PK), 
			MeterType.AGC_AV => GetRXAMeter(channel, rxaMeterType.RXA_AGC_AV), 
			_ => -400.0, 
		});
	}

	public static float CalculateTXMeter(uint thread, MeterType MT)
	{
		int channel = cmaster.CMsubrcvr * cmaster.CMrcvr;
		return 0f - (float)(MT switch
		{
			MeterType.MIC => GetTXAMeter(channel, txaMeterType.TXA_MIC_AV), 
			MeterType.PWR => GetTXAMeter(channel, txaMeterType.TXA_OUT_PK), 
			MeterType.ALC => GetTXAMeter(channel, txaMeterType.TXA_ALC_AV), 
			MeterType.EQ => GetTXAMeter(channel, txaMeterType.TXA_EQ_AV), 
			MeterType.LEVELER => GetTXAMeter(channel, txaMeterType.TXA_LVLR_AV), 
			MeterType.COMP => GetTXAMeter(channel, txaMeterType.TXA_COMP_AV), 
			MeterType.CPDR => GetTXAMeter(channel, txaMeterType.TXA_COMP_AV), 
			MeterType.ALC_G => GetTXAMeter(channel, txaMeterType.TXA_ALC_GAIN) + alcgain, 
			MeterType.LVL_G => GetTXAMeter(channel, txaMeterType.TXA_LVLR_GAIN), 
			MeterType.MIC_PK => GetTXAMeter(channel, txaMeterType.TXA_MIC_PK), 
			MeterType.ALC_PK => GetTXAMeter(channel, txaMeterType.TXA_ALC_PK), 
			MeterType.EQ_PK => GetTXAMeter(channel, txaMeterType.TXA_EQ_PK), 
			MeterType.LEVELER_PK => GetTXAMeter(channel, txaMeterType.TXA_LVLR_PK), 
			MeterType.COMP_PK => GetTXAMeter(channel, txaMeterType.TXA_COMP_PK), 
			MeterType.CPDR_PK => GetTXAMeter(channel, txaMeterType.TXA_COMP_PK), 
			MeterType.CFC_PK => GetTXAMeter(channel, txaMeterType.TXA_CFC_PK), 
			MeterType.CFC_G => GetTXAMeter(channel, txaMeterType.TXA_CFC_GAIN), 
			MeterType.CFC_AV => GetTXAMeter(channel, txaMeterType.TXA_CFC_AV), 
			_ => -400.0, 
		});
	}
}
