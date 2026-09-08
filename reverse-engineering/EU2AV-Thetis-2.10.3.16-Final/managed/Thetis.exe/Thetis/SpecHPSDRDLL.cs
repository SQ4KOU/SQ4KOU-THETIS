using System;
using System.Runtime.InteropServices;

namespace Thetis;

internal class SpecHPSDRDLL
{
	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetAnalyzer(int disp, int n_pixout, int n_fft, int typ, IntPtr flp, int sz, int bf_sz, int win_type, double pi, int ovrlp, int clp, double fscLin, double fscHin, int n_pix, int n_stch, int calset, double fmin, double fmax, int max_w);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void XCreateAnalyzer(int disp, ref int success, int m_size, int m_LO, int m_stitch, string app_data_path);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void ResetPixelBuffers(int disp);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void DestroyAnalyzer(int disp);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetPixelRef(int disp, double pixel_ref);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetPixels")]
	private unsafe static extern void GetPixelsNative(int disp, int pixout, float* pix, ref int flag, out double pixel_ref);

	public unsafe static void GetPixels(int disp, int pixout, float* pix, ref int flag)
	{
		GetPixelsNative(disp, pixout, pix, ref flag, out var _);
	}

	public unsafe static void GetPixels(int disp, int pixout, float* pix, ref int flag, out double pixel_ref)
	{
		GetPixelsNative(disp, pixout, pix, ref flag, out pixel_ref);
	}

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void Spectrum(int disp, int ss, int LO, float* pI, float* pQ);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetCalibration(int disp, int set, int points, IntPtr cal);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void SnapSpectrum(int disp, int ss, int LO, double* snap_buff);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void SnapSpectrumTimeout(int disp, int ss, int LO, double* snap_buff, uint timeout, ref int flag);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDisplayDetectorMode(int disp, int pixout, int mode);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDisplayAverageMode(int disp, int pixout, int mode);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDisplayNumAverage(int disp, int pixout, int num);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDisplayAvBackmult(int disp, int pixout, double mult);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDisplayNormOneHz(int disp, int pixout, bool norm);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern double GetDisplayENB(int disp);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetDisplaySampleRate(int disp, int rate);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void create_nobEXT(int id, int run, int mode, int buffsize, double samplerate, double tau, double hangtime, double advtime, double backtau, double threshold);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void destroy_nobEXT(int id);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void xnobEXTF(int id, float* I, float* Q);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEXTNOBBuffsize(int id, int size);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEXTNOBSamplerate(int id, int rate);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEXTNOBTau(int id, double tau);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEXTNOBHangtime(int id, double time);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEXTNOBAdvtime(int id, double time);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEXTNOBBacktau(int id, double tau);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEXTNOBThreshold(int id, double thresh);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEXTNOBMode(int id, int mode);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void create_anbEXT(int id, int run, int buffsize, double samplerate, double tau, double hangtime, double advtime, double backtau, double threshold);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void destroy_anbEXT(int id);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void xanbEXTF(int id, float* I, float* Q);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEXTANBBuffsize(int id, int size);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEXTANBSamplerate(int id, int rate);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEXTANBTau(int id, double tau);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEXTANBHangtime(int id, double time);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEXTANBAdvtime(int id, double time);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEXTANBBacktau(int id, double tau);

	[DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetEXTANBThreshold(int id, double thresh);
}
