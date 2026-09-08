using System;
using System.Runtime.InteropServices;
using SharpDX.Direct3D;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;

namespace SharpDX.Direct2D1;

[Guid("3d9f916b-27dc-4ad7-b4f1-64945340f563")]
public class EffectContext : ComObject
{
	public RawVector2 Dpi
	{
		get
		{
			RawVector2 result = default(RawVector2);
			GetDpi(out result.X, out result.Y);
			return result;
		}
	}

	public SharpDX.Direct3D.FeatureLevel GetMaximumSupportedFeatureLevel(SharpDX.Direct3D.FeatureLevel[] featureLevels)
	{
		return GetMaximumSupportedFeatureLevel(featureLevels, featureLevels.Length);
	}

	public void LoadPixelShader(Guid shaderId, byte[] shaderBytecode)
	{
		LoadPixelShader(shaderId, shaderBytecode, shaderBytecode.Length);
	}

	public void LoadVertexShader(Guid shaderId, byte[] shaderBytecode)
	{
		LoadVertexShader(shaderId, shaderBytecode, shaderBytecode.Length);
	}

	public void LoadComputeShader(Guid shaderId, byte[] shaderBytecode)
	{
		LoadComputeShader(shaderId, shaderBytecode, shaderBytecode.Length);
	}

	public unsafe bool CheckFeatureSupport(Feature feature)
	{
		switch (feature)
		{
		case Feature.Doubles:
		{
			FeatureDataDoubles featureDataDoubles = default(FeatureDataDoubles);
			if (CheckFeatureSupport(Feature.Doubles, new IntPtr(&featureDataDoubles), Utilities.SizeOf<FeatureDataDoubles>()).Failure)
			{
				return false;
			}
			return featureDataDoubles.DoublePrecisionFloatShaderOps;
		}
		case Feature.D3D10XHardwareOptions:
		{
			FeatureDataD3D10XHardwareOptions featureDataD3D10XHardwareOptions = default(FeatureDataD3D10XHardwareOptions);
			if (CheckFeatureSupport(Feature.D3D10XHardwareOptions, new IntPtr(&featureDataD3D10XHardwareOptions), Utilities.SizeOf<FeatureDataD3D10XHardwareOptions>()).Failure)
			{
				return false;
			}
			return featureDataD3D10XHardwareOptions.ComputeShadersPlusRawAndStructuredBuffersViaShader4X;
		}
		default:
			throw new SharpDXException("Unsupported Feature. Use specialized CheckXXX methods");
		}
	}

	public EffectContext(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator EffectContext(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new EffectContext(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDpi(out float dpiX, out float dpiY)
	{
		fixed (float* ptr = &dpiY)
		{
			void* ptr2 = ptr;
			fixed (float* ptr3 = &dpiX)
			{
				void* ptr4 = ptr3;
				((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2);
			}
		}
	}

	internal unsafe void CreateEffect(Guid effectId, Effect effect)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, &effectId, &zero);
		effect.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe SharpDX.Direct3D.FeatureLevel GetMaximumSupportedFeatureLevel(SharpDX.Direct3D.FeatureLevel[] featureLevels, int featureLevelsCount)
	{
		Result result;
		SharpDX.Direct3D.FeatureLevel result2 = default(SharpDX.Direct3D.FeatureLevel);
		fixed (SharpDX.Direct3D.FeatureLevel* ptr = featureLevels)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr2, featureLevelsCount, &result2);
		}
		result.CheckError();
		return result2;
	}

	public unsafe TransformNode CreateTransformNodeFromEffect(Effect effect)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Effect>(effect);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		TransformNode result2 = ((!(zero2 != IntPtr.Zero)) ? null : new TransformNodeNative(zero2));
		result.CheckError();
		return result2;
	}

	internal unsafe void CreateBlendTransform(int numInputs, ref BlendDescription blendDescription, BlendTransform transform)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (BlendDescription* ptr = &blendDescription)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, numInputs, ptr2, &zero);
		}
		transform.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateBorderTransform(ExtendMode extendModeX, ExtendMode extendModeY, BorderTransform transform)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (int)extendModeX, (int)extendModeY, &zero);
		transform.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateOffsetTransform(RawPoint offset, OffsetTransform transform)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, RawPoint, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, offset, &zero);
		transform.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateBoundsAdjustmentTransform(RawRectangle outputRectangle, BoundsAdjustmentTransform transform)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, &outputRectangle, &zero);
		transform.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void LoadPixelShader(Guid shaderId, byte[] shaderBuffer, int shaderBufferCount)
	{
		Result result;
		fixed (byte* ptr = shaderBuffer)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, &shaderId, ptr2, shaderBufferCount);
		}
		result.CheckError();
	}

	internal unsafe void LoadVertexShader(Guid resourceId, byte[] shaderBuffer, int shaderBufferCount)
	{
		Result result;
		fixed (byte* ptr = shaderBuffer)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, &resourceId, ptr2, shaderBufferCount);
		}
		result.CheckError();
	}

	internal unsafe void LoadComputeShader(Guid resourceId, byte[] shaderBuffer, int shaderBufferCount)
	{
		Result result;
		fixed (byte* ptr = shaderBuffer)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, &resourceId, ptr2, shaderBufferCount);
		}
		result.CheckError();
	}

	public unsafe RawBool IsShaderLoaded(Guid shaderId)
	{
		return ((delegate* unmanaged[Stdcall]<void*, void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, &shaderId);
	}

	internal unsafe void CreateResourceTexture(Guid? resourceId, IntPtr resourceTextureProperties, byte[] data, int[] strides, int dataSize, ResourceTexture resourceTexture)
	{
		IntPtr zero = IntPtr.Zero;
		Guid value = default(Guid);
		if (resourceId.HasValue)
		{
			value = resourceId.Value;
		}
		Result result;
		fixed (int* ptr = strides)
		{
			void* ptr2 = ptr;
			fixed (byte* ptr3 = data)
			{
				void* ptr4 = ptr3;
				void* nativePointer = _nativePointer;
				Guid* intPtr = ((!resourceId.HasValue) ? null : (&value));
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(nativePointer, intPtr, (void*)resourceTextureProperties, ptr4, ptr2, dataSize, &zero);
			}
		}
		resourceTexture.NativePointer = zero;
		result.CheckError();
	}

	public unsafe ResourceTexture FindResourceTexture(Guid resourceId)
	{
		IntPtr zero = IntPtr.Zero;
		_ = (Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, &resourceId, &zero);
		if (zero != IntPtr.Zero)
		{
			return new ResourceTexture(zero);
		}
		return null;
	}

	internal unsafe void CreateVertexBuffer(VertexBufferProperties vertexBufferProperties, Guid? resourceId, IntPtr customVertexBufferProperties, VertexBuffer buffer)
	{
		VertexBufferProperties.__Native @ref = default(VertexBufferProperties.__Native);
		IntPtr zero = IntPtr.Zero;
		vertexBufferProperties.__MarshalTo(ref @ref);
		Guid value = default(Guid);
		if (resourceId.HasValue)
		{
			value = resourceId.Value;
		}
		void* nativePointer = _nativePointer;
		VertexBufferProperties.__Native* num = &@ref;
		Guid* intPtr = ((!resourceId.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(nativePointer, num, intPtr, (void*)customVertexBufferProperties, &zero);
		buffer.NativePointer = zero;
		vertexBufferProperties.__MarshalFree(ref @ref);
		result.CheckError();
	}

	public unsafe VertexBuffer FindVertexBuffer(Guid resourceId)
	{
		IntPtr zero = IntPtr.Zero;
		_ = (Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, &resourceId, &zero);
		if (zero != IntPtr.Zero)
		{
			return new VertexBuffer(zero);
		}
		return null;
	}

	internal unsafe void CreateColorContext(ColorSpace space, byte[] rofileRef, int profileSize, ColorContext colorContext)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (byte* ptr = rofileRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, (int)space, ptr2, profileSize, &zero);
		}
		colorContext.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateColorContextFromFilename(string filename, ColorContext colorContext)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (char* ptr = filename)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, ptr, &zero);
		}
		colorContext.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateColorContextFromWicColorContext(SharpDX.WIC.ColorContext wicColorContext, ColorContext colorContext)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SharpDX.WIC.ColorContext>(wicColorContext);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		colorContext.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe Result CheckFeatureSupport(Feature feature, IntPtr featureSupportData, int featureSupportDataSize)
	{
		return ((delegate* unmanaged[Stdcall]<void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, (int)feature, (void*)featureSupportData, featureSupportDataSize);
	}

	public unsafe RawBool IsBufferPrecisionSupported(BufferPrecision bufferPrecision)
	{
		return ((delegate* unmanaged[Stdcall]<void*, int, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer, (int)bufferPrecision);
	}
}
