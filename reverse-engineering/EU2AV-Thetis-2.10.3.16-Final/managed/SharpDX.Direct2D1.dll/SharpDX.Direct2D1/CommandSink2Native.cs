using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("3bab440e-417e-47df-a2e2-bc0be6a00916")]
internal class CommandSink2Native : CommandSink1Native, CommandSink2, CommandSink1, CommandSink, IUnknown, ICallbackable, IDisposable
{
	public void DrawInk(Ink ink, Brush brush, InkStyle inkStyle)
	{
		DrawInk_(ink, brush, inkStyle);
	}

	public void DrawGradientMesh(GradientMesh gradientMesh)
	{
		DrawGradientMesh_(gradientMesh);
	}

	public void DrawGdiMetafile(GdiMetafile gdiMetafile, RawRectangleF? destinationRectangle, RawRectangleF? sourceRectangle)
	{
		DrawGdiMetafile_(gdiMetafile, destinationRectangle, sourceRectangle);
	}

	public CommandSink2Native(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator CommandSink2Native(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new CommandSink2Native(nativePtr);
		}
		return null;
	}

	internal unsafe void DrawInk_(Ink ink, Brush brush, InkStyle inkStyle)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Ink>(ink);
		zero2 = CppObject.ToCallbackPtr<Brush>(brush);
		zero3 = CppObject.ToCallbackPtr<InkStyle>(inkStyle);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)29 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2, (void*)zero3)).CheckError();
	}

	internal unsafe void DrawGradientMesh_(GradientMesh gradientMesh)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<GradientMesh>(gradientMesh);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)30 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void DrawGdiMetafile_(GdiMetafile gdiMetafile, RawRectangleF? destinationRectangle, RawRectangleF? sourceRectangle)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<GdiMetafile>(gdiMetafile);
		RawRectangleF value = default(RawRectangleF);
		if (destinationRectangle.HasValue)
		{
			value = destinationRectangle.Value;
		}
		RawRectangleF value2 = default(RawRectangleF);
		if (sourceRectangle.HasValue)
		{
			value2 = sourceRectangle.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		RawRectangleF* intPtr2 = ((!destinationRectangle.HasValue) ? null : (&value));
		RawRectangleF* intPtr3 = ((!sourceRectangle.HasValue) ? null : (&value2));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)31 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, intPtr3)).CheckError();
	}
}
