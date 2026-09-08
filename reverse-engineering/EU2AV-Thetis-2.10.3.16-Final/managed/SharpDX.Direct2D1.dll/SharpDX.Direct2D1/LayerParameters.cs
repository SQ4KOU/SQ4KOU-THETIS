using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

public struct LayerParameters
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public RawRectangleF ContentBounds;

		public IntPtr GeometricMask;

		public AntialiasMode MaskAntialiasMode;

		public RawMatrix3x2 MaskTransform;

		public float Opacity;

		public IntPtr OpacityBrush;

		public LayerOptions LayerOptions;
	}

	public RawRectangleF ContentBounds;

	public Geometry GeometricMask;

	public AntialiasMode MaskAntialiasMode;

	public RawMatrix3x2 MaskTransform;

	public float Opacity;

	public Brush OpacityBrush;

	public LayerOptions LayerOptions;

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		ContentBounds = @ref.ContentBounds;
		if (@ref.GeometricMask != IntPtr.Zero)
		{
			GeometricMask = new Geometry(@ref.GeometricMask);
		}
		else
		{
			GeometricMask = null;
		}
		MaskAntialiasMode = @ref.MaskAntialiasMode;
		MaskTransform = @ref.MaskTransform;
		Opacity = @ref.Opacity;
		if (@ref.OpacityBrush != IntPtr.Zero)
		{
			OpacityBrush = new Brush(@ref.OpacityBrush);
		}
		else
		{
			OpacityBrush = null;
		}
		LayerOptions = @ref.LayerOptions;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.ContentBounds = ContentBounds;
		@ref.GeometricMask = CppObject.ToCallbackPtr<Geometry>(GeometricMask);
		@ref.MaskAntialiasMode = MaskAntialiasMode;
		@ref.MaskTransform = MaskTransform;
		@ref.Opacity = Opacity;
		@ref.OpacityBrush = CppObject.ToCallbackPtr<Brush>(OpacityBrush);
		@ref.LayerOptions = LayerOptions;
	}
}
