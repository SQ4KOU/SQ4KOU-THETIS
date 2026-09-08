using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("688d15c3-02b0-438d-b13a-d1b44c32c39a")]
public class ResourceTexture : ComObject
{
	public ResourceTexture(EffectContext context, Guid resourceId, ResourceTextureProperties resourceTextureProperties)
		: base(IntPtr.Zero)
	{
		CreateResourceTexture(context, resourceId, resourceTextureProperties, null, null, this);
	}

	public ResourceTexture(EffectContext context, Guid resourceId, ResourceTextureProperties resourceTextureProperties, byte[] data, int[] strides)
		: base(IntPtr.Zero)
	{
		CreateResourceTexture(context, resourceId, resourceTextureProperties, data, strides, this);
	}

	private unsafe static void CreateResourceTexture(EffectContext context, Guid resourceId, ResourceTextureProperties resourceTextureProperties, byte[] data, int[] strides, ResourceTexture outTexture)
	{
		ResourceTextureProperties.__Native @ref = default(ResourceTextureProperties.__Native);
		resourceTextureProperties.__MarshalTo(ref @ref);
		if (resourceTextureProperties.Extents == null || resourceTextureProperties.Extents.Length != resourceTextureProperties.Dimensions)
		{
			throw new ArgumentException("Extents array must be same size than dimensions", "resourceTextureProperties");
		}
		if (resourceTextureProperties.ExtendModes == null || resourceTextureProperties.ExtendModes.Length != resourceTextureProperties.Dimensions)
		{
			throw new ArgumentException("ExtendModes array must be same size than dimensions", "resourceTextureProperties");
		}
		fixed (int* extents = resourceTextureProperties.Extents)
		{
			void* ptr = extents;
			fixed (ExtendMode* extendModes = resourceTextureProperties.ExtendModes)
			{
				void* ptr2 = extendModes;
				@ref.ExtentsPointer = (IntPtr)ptr;
				@ref.ExtendModesPointer = (IntPtr)ptr2;
				context.CreateResourceTexture(resourceId, new IntPtr(&@ref), data, strides, (data != null) ? data.Length : 0, outTexture);
			}
		}
	}

	public ResourceTexture(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ResourceTexture(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ResourceTexture(nativePtr);
		}
		return null;
	}

	public unsafe void Update(int[] minimumExtents, int[] maximimumExtents, int[] strides, int dimensions, byte[] data, int dataCount)
	{
		Result result;
		fixed (byte* ptr = data)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = strides)
			{
				void* ptr4 = ptr3;
				fixed (int* ptr5 = maximimumExtents)
				{
					void* ptr6 = ptr5;
					fixed (int* ptr7 = minimumExtents)
					{
						void* ptr8 = ptr7;
						result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr8, ptr6, ptr4, dimensions, ptr2, dataCount);
					}
				}
			}
		}
		result.CheckError();
	}
}
