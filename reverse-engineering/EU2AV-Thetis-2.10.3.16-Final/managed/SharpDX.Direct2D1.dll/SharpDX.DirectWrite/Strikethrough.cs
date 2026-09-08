using System;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;

namespace SharpDX.DirectWrite;

public struct Strikethrough
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public float Width;

		public float Thickness;

		public float Offset;

		public ReadingDirection ReadingDirection;

		public FlowDirection FlowDirection;

		public IntPtr LocaleName;

		public MeasuringMode MeasuringMode;
	}

	public float Width;

	public float Thickness;

	public float Offset;

	public ReadingDirection ReadingDirection;

	public FlowDirection FlowDirection;

	public string LocaleName;

	public MeasuringMode MeasuringMode;

	internal void __MarshalFree(ref __Native @ref)
	{
		Marshal.FreeHGlobal(@ref.LocaleName);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Width = @ref.Width;
		Thickness = @ref.Thickness;
		Offset = @ref.Offset;
		ReadingDirection = @ref.ReadingDirection;
		FlowDirection = @ref.FlowDirection;
		LocaleName = Marshal.PtrToStringUni(@ref.LocaleName);
		MeasuringMode = @ref.MeasuringMode;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.Width = Width;
		@ref.Thickness = Thickness;
		@ref.Offset = Offset;
		@ref.ReadingDirection = ReadingDirection;
		@ref.FlowDirection = FlowDirection;
		@ref.LocaleName = Marshal.StringToHGlobalUni(LocaleName);
		@ref.MeasuringMode = MeasuringMode;
	}
}
