using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("4dc583bf-3a10-438a-8722-e9765224f1f1")]
public class SpriteBatch : Resource
{
	public int SpriteCount => GetSpriteCount();

	public SpriteBatch(DeviceContext3 context3)
		: this(IntPtr.Zero)
	{
		context3.CreateSpriteBatch(this);
	}

	public SpriteBatch(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SpriteBatch(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SpriteBatch(nativePtr);
		}
		return null;
	}

	public unsafe void AddSprites(int spriteCount, RawRectangleF[] destinationRectangles, RawRectangle[] sourceRectangles, RawColor4[] colors, RawMatrix3x2[] transforms, int destinationRectanglesStride, int sourceRectanglesStride, int colorsStride, int transformsStride)
	{
		Result result;
		fixed (RawMatrix3x2* ptr = transforms)
		{
			void* ptr2 = ptr;
			fixed (RawColor4* ptr3 = colors)
			{
				void* ptr4 = ptr3;
				fixed (RawRectangle* ptr5 = sourceRectangles)
				{
					void* ptr6 = ptr5;
					fixed (RawRectangleF* ptr7 = destinationRectangles)
					{
						void* ptr8 = ptr7;
						result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, void*, void*, int, int, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, spriteCount, ptr8, ptr6, ptr4, ptr2, destinationRectanglesStride, sourceRectanglesStride, colorsStride, transformsStride);
					}
				}
			}
		}
		result.CheckError();
	}

	public unsafe void SetSprites(int startIndex, int spriteCount, RawRectangleF[] destinationRectangles, RawRectangle[] sourceRectangles, RawColor4[] colors, RawMatrix3x2[] transforms, int destinationRectanglesStride, int sourceRectanglesStride, int colorsStride, int transformsStride)
	{
		Result result;
		fixed (RawMatrix3x2* ptr = transforms)
		{
			void* ptr2 = ptr;
			fixed (RawColor4* ptr3 = colors)
			{
				void* ptr4 = ptr3;
				fixed (RawRectangle* ptr5 = sourceRectangles)
				{
					void* ptr6 = ptr5;
					fixed (RawRectangleF* ptr7 = destinationRectangles)
					{
						void* ptr8 = ptr7;
						result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void*, int, int, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, startIndex, spriteCount, ptr8, ptr6, ptr4, ptr2, destinationRectanglesStride, sourceRectanglesStride, colorsStride, transformsStride);
					}
				}
			}
		}
		result.CheckError();
	}

	public unsafe void GetSprites(int startIndex, int spriteCount, RawRectangleF[] destinationRectangles, RawRectangle[] sourceRectangles, RawColor4[] colors, RawMatrix3x2[] transforms)
	{
		Result result;
		fixed (RawMatrix3x2* ptr = transforms)
		{
			void* ptr2 = ptr;
			fixed (RawColor4* ptr3 = colors)
			{
				void* ptr4 = ptr3;
				fixed (RawRectangle* ptr5 = sourceRectangles)
				{
					void* ptr6 = ptr5;
					fixed (RawRectangleF* ptr7 = destinationRectangles)
					{
						void* ptr8 = ptr7;
						result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, startIndex, spriteCount, ptr8, ptr6, ptr4, ptr2);
					}
				}
			}
		}
		result.CheckError();
	}

	internal unsafe int GetSpriteCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void Clear()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer);
	}
}
