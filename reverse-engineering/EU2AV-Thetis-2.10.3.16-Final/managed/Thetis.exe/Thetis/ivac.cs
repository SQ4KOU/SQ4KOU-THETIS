using System.Runtime.InteropServices;

namespace Thetis;

internal class ivac
{
	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern int StartAudioIVAC(int id);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void StopAudioIVAC(int id);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACstereo(int id, int stereo);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACrun(int id, int run);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACiqType(int id, int type);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACvacRate(int id, int rate);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACvacSize(int id, int size);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVAChostAPIindex(int id, int index);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACinputDEVindex(int id, int index);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACoutputDEVindex(int id, int index);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACnumChannels(int id, int n);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACInLatency(int id, double lat, int reset);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACOutLatency(int id, double lat, int reset);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACPAInLatency(int id, double lat, int reset);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACPAOutLatency(int id, double lat, int reset);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACpreamp(int id, double preamp);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACbypass(int id, int enabled);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACRBReset(int id, int enabled);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACvox(int id, int enabled);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACrxscale(int id, double scale);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACcombine(int id, int combine);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACmon(int id, int mon);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACmonVol(int id, double vol);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACmox(int id, int mox);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void getIVACdiags(int id, int type, int* underflows, int* overflows, double* var, int* ringsize, int* nring);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void forceIVACvar(int id, int type, bool force, double fvar);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void resetIVACdiags(int id, int type);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACFeedbackGain(int id, int type, double feedback_gain);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACSlewTime(int id, int type, double slew_time);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACPropRingMin(int id, int type, int prop_min);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACPropRingMax(int id, int type, int prop_max);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACFFRingMin(int id, int type, int ff_ringmin);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACFFRingMax(int id, int type, int ff_ringmax);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACFFAlpha(int id, int type, double ff_alpha);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void GetIVACControlFlag(int id, int type, int* control_flag);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACinitialVars(int id, double INvar, double OUTvar);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACswapIQout(int id, int swap);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACExclusiveOut(int id, int exclusive_out);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetIVACExclusiveIn(int id, int exclusive_in);
}
