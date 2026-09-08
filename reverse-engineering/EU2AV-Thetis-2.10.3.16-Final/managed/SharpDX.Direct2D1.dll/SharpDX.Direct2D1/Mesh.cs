using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906c2-12e2-11dc-9fed-001143a055f9")]
public class Mesh : Resource
{
	public Mesh(RenderTarget renderTarget)
		: base(IntPtr.Zero)
	{
		renderTarget.CreateMesh(this);
	}

	public Mesh(RenderTarget renderTarget, Triangle[] triangles)
		: this(renderTarget)
	{
		using TessellationSink tessellationSink = Open();
		tessellationSink.AddTriangles(triangles);
		tessellationSink.Close();
	}

	public TessellationSink Open()
	{
		Open_(out var tessellationSink);
		return tessellationSink;
	}

	public Mesh(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Mesh(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Mesh(nativePtr);
		}
		return null;
	}

	internal unsafe void Open_(out TessellationSink tessellationSink)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			tessellationSink = new TessellationSinkNative(zero);
		}
		else
		{
			tessellationSink = null;
		}
		result.CheckError();
	}
}
