using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

public class EffectInputDescription
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public IntPtr Effect;

		public int InputIndex;

		public RawRectangleF InputRectangle;
	}

	public Effect Effect;

	public int InputIndex;

	public RawRectangleF InputRectangle;

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		if (@ref.Effect != IntPtr.Zero)
		{
			Effect = new Effect(@ref.Effect);
		}
		else
		{
			Effect = null;
		}
		InputIndex = @ref.InputIndex;
		InputRectangle = @ref.InputRectangle;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.Effect = CppObject.ToCallbackPtr<Effect>(Effect);
		@ref.InputIndex = InputIndex;
		@ref.InputRectangle = InputRectangle;
	}
}
