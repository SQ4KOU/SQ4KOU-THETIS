using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

public struct OutputDescription1
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public char DeviceName;

		public char __DeviceName1;

		public char __DeviceName2;

		public char __DeviceName3;

		public char __DeviceName4;

		public char __DeviceName5;

		public char __DeviceName6;

		public char __DeviceName7;

		public char __DeviceName8;

		public char __DeviceName9;

		public char __DeviceName10;

		public char __DeviceName11;

		public char __DeviceName12;

		public char __DeviceName13;

		public char __DeviceName14;

		public char __DeviceName15;

		public char __DeviceName16;

		public char __DeviceName17;

		public char __DeviceName18;

		public char __DeviceName19;

		public char __DeviceName20;

		public char __DeviceName21;

		public char __DeviceName22;

		public char __DeviceName23;

		public char __DeviceName24;

		public char __DeviceName25;

		public char __DeviceName26;

		public char __DeviceName27;

		public char __DeviceName28;

		public char __DeviceName29;

		public char __DeviceName30;

		public char __DeviceName31;

		public RawRectangle DesktopCoordinates;

		public RawBool AttachedToDesktop;

		public DisplayModeRotation Rotation;

		public IntPtr Monitor;

		public int BitsPerColor;

		public ColorSpaceType ColorSpace;

		public float RedPrimary;

		public float __RedPrimary1;

		public float GreenPrimary;

		public float __GreenPrimary1;

		public float BluePrimary;

		public float __BluePrimary1;

		public float WhitePoint;

		public float __WhitePoint1;

		public float MinLuminance;

		public float MaxLuminance;

		public float MaxFullFrameLuminance;
	}

	public string DeviceName;

	public RawRectangle DesktopCoordinates;

	public RawBool AttachedToDesktop;

	public DisplayModeRotation Rotation;

	public IntPtr Monitor;

	public int BitsPerColor;

	public ColorSpaceType ColorSpace;

	internal float[] _RedPrimary;

	internal float[] _GreenPrimary;

	internal float[] _BluePrimary;

	internal float[] _WhitePoint;

	public float MinLuminance;

	public float MaxLuminance;

	public float MaxFullFrameLuminance;

	public float[] RedPrimary
	{
		get
		{
			return _RedPrimary ?? (_RedPrimary = new float[2]);
		}
		private set
		{
			_RedPrimary = value;
		}
	}

	public float[] GreenPrimary
	{
		get
		{
			return _GreenPrimary ?? (_GreenPrimary = new float[2]);
		}
		private set
		{
			_GreenPrimary = value;
		}
	}

	public float[] BluePrimary
	{
		get
		{
			return _BluePrimary ?? (_BluePrimary = new float[2]);
		}
		private set
		{
			_BluePrimary = value;
		}
	}

	public float[] WhitePoint
	{
		get
		{
			return _WhitePoint ?? (_WhitePoint = new float[2]);
		}
		private set
		{
			_WhitePoint = value;
		}
	}

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal unsafe void __MarshalFrom(ref __Native @ref)
	{
		fixed (char* deviceName = &@ref.DeviceName)
		{
			void* ptr = deviceName;
			DeviceName = Utilities.PtrToStringUni((IntPtr)ptr, 31);
		}
		DesktopCoordinates = @ref.DesktopCoordinates;
		AttachedToDesktop = @ref.AttachedToDesktop;
		Rotation = @ref.Rotation;
		Monitor = @ref.Monitor;
		BitsPerColor = @ref.BitsPerColor;
		ColorSpace = @ref.ColorSpace;
		fixed (float* ptr2 = &RedPrimary[0])
		{
			void* ptr3 = ptr2;
			fixed (float* redPrimary = &@ref.RedPrimary)
			{
				void* ptr4 = redPrimary;
				Utilities.CopyMemory((IntPtr)ptr3, (IntPtr)ptr4, 8);
			}
		}
		fixed (float* redPrimary = &GreenPrimary[0])
		{
			void* ptr5 = redPrimary;
			fixed (float* ptr2 = &@ref.GreenPrimary)
			{
				void* ptr6 = ptr2;
				Utilities.CopyMemory((IntPtr)ptr5, (IntPtr)ptr6, 8);
			}
		}
		fixed (float* ptr2 = &BluePrimary[0])
		{
			void* ptr7 = ptr2;
			fixed (float* redPrimary = &@ref.BluePrimary)
			{
				void* ptr8 = redPrimary;
				Utilities.CopyMemory((IntPtr)ptr7, (IntPtr)ptr8, 8);
			}
		}
		fixed (float* redPrimary = &WhitePoint[0])
		{
			void* ptr9 = redPrimary;
			fixed (float* ptr2 = &@ref.WhitePoint)
			{
				void* ptr10 = ptr2;
				Utilities.CopyMemory((IntPtr)ptr9, (IntPtr)ptr10, 8);
			}
		}
		MinLuminance = @ref.MinLuminance;
		MaxLuminance = @ref.MaxLuminance;
		MaxFullFrameLuminance = @ref.MaxFullFrameLuminance;
	}

	internal unsafe void __MarshalTo(ref __Native @ref)
	{
		fixed (char* deviceName = DeviceName)
		{
			fixed (char* deviceName2 = &@ref.DeviceName)
			{
				int num = Math.Min((DeviceName?.Length ?? 0) * 2, 62);
				Utilities.CopyMemory((IntPtr)deviceName2, (IntPtr)deviceName, num);
				deviceName2[num] = '\0';
			}
		}
		@ref.DesktopCoordinates = DesktopCoordinates;
		@ref.AttachedToDesktop = AttachedToDesktop;
		@ref.Rotation = Rotation;
		@ref.Monitor = Monitor;
		@ref.BitsPerColor = BitsPerColor;
		@ref.ColorSpace = ColorSpace;
		fixed (float* ptr = &RedPrimary[0])
		{
			void* ptr2 = ptr;
			fixed (float* redPrimary = &@ref.RedPrimary)
			{
				void* ptr3 = redPrimary;
				Utilities.CopyMemory((IntPtr)ptr3, (IntPtr)ptr2, 8);
			}
		}
		fixed (float* redPrimary = &GreenPrimary[0])
		{
			void* ptr4 = redPrimary;
			fixed (float* ptr = &@ref.GreenPrimary)
			{
				void* ptr5 = ptr;
				Utilities.CopyMemory((IntPtr)ptr5, (IntPtr)ptr4, 8);
			}
		}
		fixed (float* ptr = &BluePrimary[0])
		{
			void* ptr6 = ptr;
			fixed (float* redPrimary = &@ref.BluePrimary)
			{
				void* ptr7 = redPrimary;
				Utilities.CopyMemory((IntPtr)ptr7, (IntPtr)ptr6, 8);
			}
		}
		fixed (float* redPrimary = &WhitePoint[0])
		{
			void* ptr8 = redPrimary;
			fixed (float* ptr = &@ref.WhitePoint)
			{
				void* ptr9 = ptr;
				Utilities.CopyMemory((IntPtr)ptr9, (IntPtr)ptr8, 8);
			}
		}
		@ref.MinLuminance = MinLuminance;
		@ref.MaxLuminance = MaxLuminance;
		@ref.MaxFullFrameLuminance = MaxFullFrameLuminance;
	}
}
