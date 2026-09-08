using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("0359dc30-95e6-4568-9055-27720d130e93")]
public class AnalysisTransform : ComObject
{
	public void ProcessAnalysisResults(DataStream analysisData)
	{
		ProcessAnalysisResults(analysisData.DataPointer, (int)analysisData.Length);
	}

	public unsafe void ProcessAnalysisResults<T>(T analysisData) where T : struct
	{
		fixed (T* ptr = &System.Runtime.CompilerServices.Unsafe.AsRef<T>(&analysisData))
		{
			ProcessAnalysisResults((IntPtr)ptr, Utilities.SizeOf<T>());
		}
	}

	public unsafe void ProcessAnalysisResults<T>(T[] analysisData) where T : struct
	{
		fixed (T* ptr = &analysisData[0])
		{
			ProcessAnalysisResults((IntPtr)ptr, Utilities.SizeOf<T>() * analysisData.Length);
		}
	}

	public AnalysisTransform(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator AnalysisTransform(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new AnalysisTransform(nativePtr);
		}
		return null;
	}

	internal unsafe void ProcessAnalysisResults(IntPtr analysisData, int analysisDataCount)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)analysisData, analysisDataCount)).CheckError();
	}
}
