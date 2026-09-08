using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;
using SharpDX.Win32;

namespace SharpDX.Direct2D1;

[Guid("86b88e4d-afa4-4d7b-88e4-68a51c4a0aec")]
public class SvgDocument : Resource
{
	public Size2F ViewportSize
	{
		get
		{
			return GetViewportSize();
		}
		set
		{
			SetViewportSize(value);
		}
	}

	public SvgElement Root
	{
		get
		{
			GetRoot(out var root);
			return root;
		}
		set
		{
			SetRoot(value);
		}
	}

	public SvgElement FindElementById(string id)
	{
		TryFindElementById_(id, out var svgElement).CheckError();
		return svgElement;
	}

	public SvgDocument(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SvgDocument(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SvgDocument(nativePtr);
		}
		return null;
	}

	internal unsafe void SetViewportSize(Size2F viewportSize)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, Size2F, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, viewportSize)).CheckError();
	}

	internal unsafe Size2F GetViewportSize()
	{
		Size2F result = default(Size2F);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	internal unsafe void SetRoot(SvgElement root)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SvgElement>(root);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void GetRoot(out SvgElement root)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			root = new SvgElement(zero);
		}
		else
		{
			root = null;
		}
	}

	private unsafe Result TryFindElementById_(string id, out SvgElement svgElement)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (char* ptr = id)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr, &zero);
		}
		if (zero != IntPtr.Zero)
		{
			svgElement = new SvgElement(zero);
			return result;
		}
		svgElement = null;
		return result;
	}

	public unsafe void Serialize(IStream outputXmlStream, SvgElement subtree)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IStream>(outputXmlStream);
		zero2 = CppObject.ToCallbackPtr<SvgElement>(subtree);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2)).CheckError();
	}

	public unsafe void Deserialize(IStream inputXmlStream, out SvgElement subtree)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IStream>(inputXmlStream);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		if (zero2 != IntPtr.Zero)
		{
			subtree = new SvgElement(zero2);
		}
		else
		{
			subtree = null;
		}
		result.CheckError();
	}

	public unsafe void CreatePaint(SvgPaintType paintType, RawColor4? color, string id, out SvgPaint aintRef)
	{
		IntPtr zero = IntPtr.Zero;
		RawColor4 value = default(RawColor4);
		if (color.HasValue)
		{
			value = color.Value;
		}
		Result result;
		fixed (char* ptr = id)
		{
			void* nativePointer = _nativePointer;
			RawColor4* intPtr = ((!color.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(nativePointer, (int)paintType, intPtr, ptr, &zero);
		}
		if (zero != IntPtr.Zero)
		{
			aintRef = new SvgPaint(zero);
		}
		else
		{
			aintRef = null;
		}
		result.CheckError();
	}

	public unsafe void CreateStrokeDashArray(SvgLength[] dashes, int dashesCount, out SvgStrokeDashArray strokeDashArray)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (SvgLength* ptr = dashes)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, ptr2, dashesCount, &zero);
		}
		if (zero != IntPtr.Zero)
		{
			strokeDashArray = new SvgStrokeDashArray(zero);
		}
		else
		{
			strokeDashArray = null;
		}
		result.CheckError();
	}

	public unsafe void CreatePointCollection(RawVector2[] ointsRef, int pointsCount, out SvgPointCollection ointCollectionRef)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (RawVector2* ptr = ointsRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, ptr2, pointsCount, &zero);
		}
		if (zero != IntPtr.Zero)
		{
			ointCollectionRef = new SvgPointCollection(zero);
		}
		else
		{
			ointCollectionRef = null;
		}
		result.CheckError();
	}

	public unsafe void CreatePathData(float[] segmentData, int segmentDataCount, SvgPathCommand[] commands, int commandsCount, out SvgPathData athDataRef)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (SvgPathCommand* ptr = commands)
		{
			void* ptr2 = ptr;
			fixed (float* ptr3 = segmentData)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, ptr4, segmentDataCount, ptr2, commandsCount, &zero);
			}
		}
		if (zero != IntPtr.Zero)
		{
			athDataRef = new SvgPathData(zero);
		}
		else
		{
			athDataRef = null;
		}
		result.CheckError();
	}
}
