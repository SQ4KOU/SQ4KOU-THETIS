using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

public struct RawToneCurve
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public int CPoints;

		public RawToneCurvePoint APoints;
	}

	public int CPoints;

	internal RawToneCurvePoint[] _APoints;

	public RawToneCurvePoint[] APoints
	{
		get
		{
			return _APoints ?? (_APoints = new RawToneCurvePoint[1]);
		}
		private set
		{
			_APoints = value;
		}
	}

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal unsafe void __MarshalFrom(ref __Native @ref)
	{
		CPoints = @ref.CPoints;
		fixed (RawToneCurvePoint* ptr = &APoints[0])
		{
			void* ptr2 = ptr;
			fixed (RawToneCurvePoint* aPoints = &@ref.APoints)
			{
				void* ptr3 = aPoints;
				Utilities.CopyMemory((IntPtr)ptr2, (IntPtr)ptr3, sizeof(RawToneCurvePoint));
			}
		}
	}

	internal unsafe void __MarshalTo(ref __Native @ref)
	{
		@ref.CPoints = CPoints;
		fixed (RawToneCurvePoint* ptr = &APoints[0])
		{
			void* ptr2 = ptr;
			fixed (RawToneCurvePoint* aPoints = &@ref.APoints)
			{
				void* ptr3 = aPoints;
				Utilities.CopyMemory((IntPtr)ptr3, (IntPtr)ptr2, sizeof(RawToneCurvePoint));
			}
		}
	}
}
