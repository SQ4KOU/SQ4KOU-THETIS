using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("63ac0b32-ba44-450f-8806-7f4ca1ff2f1b")]
public class BlendTransform : ConcreteTransform
{
	public BlendDescription Description
	{
		get
		{
			GetDescription(out var description);
			return description;
		}
		set
		{
			SetDescription(ref value);
		}
	}

	public BlendTransform(EffectContext context, int numInputs, BlendDescription blendDescription)
		: base(IntPtr.Zero)
	{
		context.CreateBlendTransform(numInputs, ref blendDescription, this);
	}

	public BlendTransform(EffectContext context, int numInputs, ref BlendDescription blendDescription)
		: base(IntPtr.Zero)
	{
		context.CreateBlendTransform(numInputs, ref blendDescription, this);
	}

	public BlendTransform(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BlendTransform(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BlendTransform(nativePtr);
		}
		return null;
	}

	internal unsafe void SetDescription(ref BlendDescription description)
	{
		fixed (BlendDescription* ptr = &description)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	internal unsafe void GetDescription(out BlendDescription description)
	{
		description = default(BlendDescription);
		fixed (BlendDescription* ptr = &description)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
