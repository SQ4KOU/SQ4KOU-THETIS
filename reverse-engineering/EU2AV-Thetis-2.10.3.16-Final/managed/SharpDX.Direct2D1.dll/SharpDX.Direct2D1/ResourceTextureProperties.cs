using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

public class ResourceTextureProperties
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public IntPtr ExtentsPointer;

		public int Dimensions;

		public BufferPrecision BufferPrecision;

		public ChannelDepth ChannelDepth;

		public Filter Filter;

		public IntPtr ExtendModesPointer;
	}

	internal IntPtr ExtentsPointer;

	public int Dimensions;

	public BufferPrecision BufferPrecision;

	public ChannelDepth ChannelDepth;

	public Filter Filter;

	internal IntPtr ExtendModesPointer;

	public int[] Extents { get; set; }

	public ExtendMode[] ExtendModes { get; set; }

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		ExtentsPointer = @ref.ExtentsPointer;
		Dimensions = @ref.Dimensions;
		BufferPrecision = @ref.BufferPrecision;
		ChannelDepth = @ref.ChannelDepth;
		Filter = @ref.Filter;
		ExtendModesPointer = @ref.ExtendModesPointer;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.ExtentsPointer = ExtentsPointer;
		@ref.Dimensions = Dimensions;
		@ref.BufferPrecision = BufferPrecision;
		@ref.ChannelDepth = ChannelDepth;
		@ref.Filter = Filter;
		@ref.ExtendModesPointer = ExtendModesPointer;
	}
}
