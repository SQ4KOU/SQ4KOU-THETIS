using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.WIC;

[Guid("00000040-a8f2-4877-ba0a-fd2b6645fb94")]
public class Palette : ComObject
{
	public BitmapPaletteType TypeInfo
	{
		get
		{
			GetTypeInfo(out var ePaletteTypeRef);
			return ePaletteTypeRef;
		}
	}

	public int ColorCount
	{
		get
		{
			GetColorCount(out var countRef);
			return countRef;
		}
	}

	public RawBool IsBlackWhite
	{
		get
		{
			IsBlackWhite_(out var fIsBlackWhiteRef);
			return fIsBlackWhiteRef;
		}
	}

	public RawBool IsGrayscale
	{
		get
		{
			IsGrayscale_(out var fIsGrayscaleRef);
			return fIsGrayscaleRef;
		}
	}

	public Palette(ImagingFactory factory)
		: base(IntPtr.Zero)
	{
		factory.CreatePalette(this);
	}

	public unsafe void Initialize<T>(T[] colors) where T : struct
	{
		if (Utilities.SizeOf<T>() != 4)
		{
			throw new ArgumentException("Color type must be 4 bytes RGBA");
		}
		fixed (T* ptr = &colors[0])
		{
			void* ptr2 = ptr;
			Initialize((IntPtr)ptr2, colors.Length);
		}
	}

	public unsafe T[] GetColors<T>() where T : struct
	{
		//The blocks IL_0076 are reachable both inside and outside the pinned region starting at IL_005c. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		if (Utilities.SizeOf<T>() != 4)
		{
			throw new ArgumentException("Color type must be 4 bytes RGBA");
		}
		int colorCount = ColorCount;
		T[] array = new T[colorCount];
		fixed (T* ptr = &array[0])
		{
			void* ptr2 = ptr;
			GetColors(colorCount, (IntPtr)ptr2, out var actualColorsRef);
			if (actualColorsRef == 0)
			{
				return new T[0];
			}
			if (colorCount != actualColorsRef)
			{
				array = new T[actualColorsRef];
				fixed (T* ptr3 = &array[0])
				{
					void* ptr4 = ptr3;
					GetColors(actualColorsRef, (IntPtr)ptr4, out actualColorsRef);
					return array;
				}
			}
			return array;
		}
	}

	public Palette(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Palette(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Palette(nativePtr);
		}
		return null;
	}

	public unsafe void Initialize(BitmapPaletteType ePaletteType, RawBool fAddTransparentColor)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, RawBool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (int)ePaletteType, fAddTransparentColor)).CheckError();
	}

	internal unsafe void Initialize(IntPtr colorsRef, int count)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)colorsRef, count)).CheckError();
	}

	public unsafe void Initialize(BitmapSource surfaceRef, int count, RawBool fAddTransparentColor)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<BitmapSource>(surfaceRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, RawBool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, count, fAddTransparentColor)).CheckError();
	}

	public unsafe void Initialize(Palette paletteRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Palette>(paletteRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void GetTypeInfo(out BitmapPaletteType ePaletteTypeRef)
	{
		Result result;
		fixed (BitmapPaletteType* ptr = &ePaletteTypeRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetColorCount(out int countRef)
	{
		Result result;
		fixed (int* ptr = &countRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetColors(int count, IntPtr colorsRef, out int actualColorsRef)
	{
		Result result;
		fixed (int* ptr = &actualColorsRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, count, (void*)colorsRef, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void IsBlackWhite_(out RawBool fIsBlackWhiteRef)
	{
		fIsBlackWhiteRef = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &fIsBlackWhiteRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void IsGrayscale_(out RawBool fIsGrayscaleRef)
	{
		fIsGrayscaleRef = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &fIsGrayscaleRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void HasAlpha(out RawBool fHasAlphaRef)
	{
		fHasAlphaRef = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &fHasAlphaRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
