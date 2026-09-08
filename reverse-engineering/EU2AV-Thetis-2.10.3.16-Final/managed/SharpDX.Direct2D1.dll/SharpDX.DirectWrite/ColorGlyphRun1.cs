using System;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

public struct ColorGlyphRun1
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public GlyphRun.__Native GlyphRun;

		public IntPtr GlyphRunDescription;

		public float BaselineOriginX;

		public float BaselineOriginY;

		public RawColor4 RunColor;

		public short PaletteIndex;

		public GlyphImageFormatS GlyphImageFormat;

		public MeasuringMode MeasuringMode;
	}

	public GlyphRun GlyphRun;

	public IntPtr GlyphRunDescription;

	public float BaselineOriginX;

	public float BaselineOriginY;

	public RawColor4 RunColor;

	public short PaletteIndex;

	public GlyphImageFormatS GlyphImageFormat;

	public MeasuringMode MeasuringMode;

	internal void __MarshalFree(ref __Native @ref)
	{
		GlyphRun.__MarshalFree(ref @ref.GlyphRun);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		GlyphRun.__MarshalFrom(ref @ref.GlyphRun);
		GlyphRunDescription = @ref.GlyphRunDescription;
		BaselineOriginX = @ref.BaselineOriginX;
		BaselineOriginY = @ref.BaselineOriginY;
		RunColor = @ref.RunColor;
		PaletteIndex = @ref.PaletteIndex;
		GlyphImageFormat = @ref.GlyphImageFormat;
		MeasuringMode = @ref.MeasuringMode;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		GlyphRun.__MarshalTo(ref @ref.GlyphRun);
		@ref.GlyphRunDescription = GlyphRunDescription;
		@ref.BaselineOriginX = BaselineOriginX;
		@ref.BaselineOriginY = BaselineOriginY;
		@ref.RunColor = RunColor;
		@ref.PaletteIndex = PaletteIndex;
		@ref.GlyphImageFormat = GlyphImageFormat;
		@ref.MeasuringMode = MeasuringMode;
	}
}
