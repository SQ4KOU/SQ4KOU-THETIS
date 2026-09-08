using System;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("D37D7598-09BE-4222-A236-2081341CC1F2")]
public class FontFace3 : FontFace2
{
	public FontFaceReference FontFaceReference
	{
		get
		{
			GetFontFaceReference(out var fontFaceReference);
			return fontFaceReference;
		}
	}

	public Panose Panose
	{
		get
		{
			GetPanose(out var anoseRef);
			return anoseRef;
		}
	}

	public FontWeight Weight => GetWeight();

	public FontStretch Stretch => GetStretch();

	public FontStyle Style => GetStyle();

	public LocalizedStrings FamilyNames
	{
		get
		{
			GetFamilyNames(out var names);
			return names;
		}
	}

	public LocalizedStrings FaceNames
	{
		get
		{
			GetFaceNames(out var names);
			return names;
		}
	}

	public FontFace3(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontFace3(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontFace3(nativePtr);
		}
		return null;
	}

	internal unsafe void GetFontFaceReference(out FontFaceReference fontFaceReference)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)35 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			fontFaceReference = new FontFaceReference(zero);
		}
		else
		{
			fontFaceReference = null;
		}
		result.CheckError();
	}

	internal unsafe void GetPanose(out Panose anoseRef)
	{
		Panose.__Native @ref = default(Panose.__Native);
		anoseRef = default(Panose);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)36 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		anoseRef.__MarshalFrom(ref @ref);
	}

	internal unsafe FontWeight GetWeight()
	{
		return ((delegate* unmanaged[Stdcall]<void*, FontWeight>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)37 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe FontStretch GetStretch()
	{
		return ((delegate* unmanaged[Stdcall]<void*, FontStretch>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)38 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe FontStyle GetStyle()
	{
		return ((delegate* unmanaged[Stdcall]<void*, FontStyle>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)39 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetFamilyNames(out LocalizedStrings names)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)40 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			names = new LocalizedStrings(zero);
		}
		else
		{
			names = null;
		}
		result.CheckError();
	}

	internal unsafe void GetFaceNames(out LocalizedStrings names)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)41 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			names = new LocalizedStrings(zero);
		}
		else
		{
			names = null;
		}
		result.CheckError();
	}

	public unsafe void GetInformationalStrings(InformationalStringId informationalStringID, out LocalizedStrings informationalStrings, out RawBool exists)
	{
		IntPtr zero = IntPtr.Zero;
		exists = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &exists)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)42 * (nint)sizeof(void*))))(_nativePointer, (int)informationalStringID, &zero, ptr2);
		}
		if (zero != IntPtr.Zero)
		{
			informationalStrings = new LocalizedStrings(zero);
		}
		else
		{
			informationalStrings = null;
		}
		result.CheckError();
	}

	public unsafe RawBool HasCharacter(int unicodeValue)
	{
		return ((delegate* unmanaged[Stdcall]<void*, int, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)43 * (nint)sizeof(void*))))(_nativePointer, unicodeValue);
	}

	public unsafe void GetRecommendedRenderingMode(float fontEmSize, float dpiX, float dpiY, RawMatrix3x2? transform, RawBool isSideways, OutlineThreshold outlineThreshold, MeasuringMode measuringMode, RenderingParams renderingParams, out RenderingMode1 renderingMode, out GridFitMode gridFitMode)
	{
		IntPtr zero = IntPtr.Zero;
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (transform.HasValue)
		{
			value = transform.Value;
		}
		zero = CppObject.ToCallbackPtr<RenderingParams>(renderingParams);
		Result result;
		fixed (GridFitMode* ptr = &gridFitMode)
		{
			void* ptr2 = ptr;
			fixed (RenderingMode1* ptr3 = &renderingMode)
			{
				void* ptr4 = ptr3;
				void* nativePointer = _nativePointer;
				RawMatrix3x2* intPtr = ((!transform.HasValue) ? null : (&value));
				result = ((delegate* unmanaged[Stdcall]<void*, float, float, float, void*, RawBool, int, int, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)44 * (nint)sizeof(void*))))(nativePointer, fontEmSize, dpiX, dpiY, intPtr, isSideways, (int)outlineThreshold, (int)measuringMode, (void*)zero, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe RawBool IsCharacterLocal(int unicodeValue)
	{
		return ((delegate* unmanaged[Stdcall]<void*, int, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)45 * (nint)sizeof(void*))))(_nativePointer, unicodeValue);
	}

	public unsafe RawBool IsGlyphLocal(short glyphId)
	{
		return ((delegate* unmanaged[Stdcall]<void*, short, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)46 * (nint)sizeof(void*))))(_nativePointer, glyphId);
	}

	public unsafe void AreCharactersLocal(string characters, int characterCount, RawBool enqueueIfNotLocal, out RawBool isLocal)
	{
		isLocal = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &isLocal)
		{
			void* ptr2 = ptr;
			fixed (char* ptr3 = characters)
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, RawBool, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)47 * (nint)sizeof(void*))))(_nativePointer, ptr3, characterCount, enqueueIfNotLocal, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void AreGlyphsLocal(short[] glyphIndices, int glyphCount, RawBool enqueueIfNotLocal, out RawBool isLocal)
	{
		isLocal = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &isLocal)
		{
			void* ptr2 = ptr;
			fixed (short* ptr3 = glyphIndices)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, RawBool, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)48 * (nint)sizeof(void*))))(_nativePointer, ptr4, glyphCount, enqueueIfNotLocal, ptr2);
			}
		}
		result.CheckError();
	}
}
