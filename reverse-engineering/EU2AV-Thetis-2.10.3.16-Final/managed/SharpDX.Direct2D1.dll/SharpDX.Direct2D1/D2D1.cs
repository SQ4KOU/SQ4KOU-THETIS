using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

public static class D2D1
{
	public const float DefaultFlatteningTolerance = 0.25f;

	public const float DefaultDpi = 96f;

	public static float ComputeFlatteningTolerance(ref RawMatrix3x2 matrix, float dpiX = 96f, float dpiY = 96f, float maxZoomFactor = 1f)
	{
		float num = dpiX / 96f;
		float num2 = dpiY / 96f;
		RawMatrix3x2 matrix2 = new RawMatrix3x2
		{
			M11 = matrix.M11 * num,
			M12 = matrix.M12 * num2,
			M21 = matrix.M21 * num,
			M22 = matrix.M22 * num2,
			M31 = matrix.M31 * num,
			M32 = matrix.M32 * num2
		};
		float num3 = ((maxZoomFactor > 0f) ? maxZoomFactor : (0f - maxZoomFactor));
		return 0.25f / (num3 * ComputeMaximumScaleFactor(ref matrix2));
	}

	public unsafe static void CreateFactory(FactoryType factoryType, Guid riid, FactoryOptions? factoryOptionsRef, out IntPtr iFactoryOut)
	{
		FactoryOptions value = default(FactoryOptions);
		if (factoryOptionsRef.HasValue)
		{
			value = factoryOptionsRef.Value;
		}
		Result result;
		fixed (IntPtr* ptr = &iFactoryOut)
		{
			void* param = ptr;
			result = D2D1CreateFactory_((int)factoryType, &riid, (!factoryOptionsRef.HasValue) ? null : (&value), param);
		}
		result.CheckError();
	}

	[DllImport("d2d1.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D2D1CreateFactory")]
	private unsafe static extern int D2D1CreateFactory_(int param0, void* param1, void* param2, void* param3);

	public unsafe static void MakeRotateMatrix(float angle, RawVector2 center, out RawMatrix3x2 matrix)
	{
		matrix = default(RawMatrix3x2);
		fixed (RawMatrix3x2* ptr = &matrix)
		{
			void* param = ptr;
			D2D1MakeRotateMatrix_(angle, center, param);
		}
	}

	[DllImport("d2d1.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D2D1MakeRotateMatrix")]
	private unsafe static extern void D2D1MakeRotateMatrix_(float param0, RawVector2 param1, void* param2);

	public unsafe static void MakeSkewMatrix(float angleX, float angleY, RawVector2 center, out RawMatrix3x2 matrix)
	{
		matrix = default(RawMatrix3x2);
		fixed (RawMatrix3x2* ptr = &matrix)
		{
			void* param = ptr;
			D2D1MakeSkewMatrix_(angleX, angleY, center, param);
		}
	}

	[DllImport("d2d1.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D2D1MakeSkewMatrix")]
	private unsafe static extern void D2D1MakeSkewMatrix_(float param0, float param1, RawVector2 param2, void* param3);

	public unsafe static RawBool IsMatrixInvertible(ref RawMatrix3x2 matrix)
	{
		RawBool result;
		fixed (RawMatrix3x2* ptr = &matrix)
		{
			void* param = ptr;
			result = D2D1IsMatrixInvertible_(param);
		}
		return result;
	}

	[DllImport("d2d1.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D2D1IsMatrixInvertible")]
	private unsafe static extern RawBool D2D1IsMatrixInvertible_(void* param0);

	public unsafe static RawBool InvertMatrix(ref RawMatrix3x2 matrix)
	{
		RawBool result;
		fixed (RawMatrix3x2* ptr = &matrix)
		{
			void* param = ptr;
			result = D2D1InvertMatrix_(param);
		}
		return result;
	}

	[DllImport("d2d1.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D2D1InvertMatrix")]
	private unsafe static extern RawBool D2D1InvertMatrix_(void* param0);

	public unsafe static void CreateDevice(SharpDX.DXGI.Device dxgiDevice, CreationProperties? creationProperties, Device d2dDevice)
	{
		_ = IntPtr.Zero;
		IntPtr zero = IntPtr.Zero;
		IntPtr intPtr = CppObject.ToCallbackPtr<SharpDX.DXGI.Device>(dxgiDevice);
		CreationProperties value = default(CreationProperties);
		if (creationProperties.HasValue)
		{
			value = creationProperties.Value;
		}
		Result result = D2D1CreateDevice_((void*)intPtr, (!creationProperties.HasValue) ? null : (&value), &zero);
		d2dDevice.NativePointer = zero;
		result.CheckError();
	}

	[DllImport("d2d1.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D2D1CreateDevice")]
	private unsafe static extern int D2D1CreateDevice_(void* param0, void* param1, void* param2);

	public unsafe static void CreateDeviceContext(Surface dxgiSurface, CreationProperties? creationProperties, DeviceContext d2dDeviceContext)
	{
		_ = IntPtr.Zero;
		IntPtr zero = IntPtr.Zero;
		IntPtr intPtr = CppObject.ToCallbackPtr<Surface>(dxgiSurface);
		CreationProperties value = default(CreationProperties);
		if (creationProperties.HasValue)
		{
			value = creationProperties.Value;
		}
		Result result = D2D1CreateDeviceContext_((void*)intPtr, (!creationProperties.HasValue) ? null : (&value), &zero);
		d2dDeviceContext.NativePointer = zero;
		result.CheckError();
	}

	[DllImport("d2d1.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D2D1CreateDeviceContext")]
	private unsafe static extern int D2D1CreateDeviceContext_(void* param0, void* param1, void* param2);

	public unsafe static RawColor4 ConvertColorSpace(ColorSpace sourceColorSpace, ColorSpace destinationColorSpace, RawColor4 color)
	{
		RawColor4 result = default(RawColor4);
		D2D1ConvertColorSpace_(&result, (int)sourceColorSpace, (int)destinationColorSpace, &color);
		return result;
	}

	[DllImport("d2d1.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D2D1ConvertColorSpace")]
	private unsafe static extern void* D2D1ConvertColorSpace_(void* param0, int param1, int param2, void* param3);

	public unsafe static void SinCos(float angle, out float s, out float c)
	{
		fixed (float* ptr = &c)
		{
			void* param = ptr;
			fixed (float* ptr2 = &s)
			{
				void* param2 = ptr2;
				D2D1SinCos_(angle, param2, param);
			}
		}
	}

	[DllImport("d2d1.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D2D1SinCos")]
	private unsafe static extern void D2D1SinCos_(float param0, void* param1, void* param2);

	public static float Tan(float angle)
	{
		return D2D1Tan_(angle);
	}

	[DllImport("d2d1.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D2D1Tan")]
	private static extern float D2D1Tan_(float param0);

	public static float Vec3Length(float x, float y, float z)
	{
		return D2D1Vec3Length_(x, y, z);
	}

	[DllImport("d2d1.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D2D1Vec3Length")]
	private static extern float D2D1Vec3Length_(float param0, float param1, float param2);

	public unsafe static float ComputeMaximumScaleFactor(ref RawMatrix3x2 matrix)
	{
		float result;
		fixed (RawMatrix3x2* ptr = &matrix)
		{
			void* param = ptr;
			result = D2D1ComputeMaximumScaleFactor_(param);
		}
		return result;
	}

	[DllImport("d2d1.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D2D1ComputeMaximumScaleFactor")]
	private unsafe static extern float D2D1ComputeMaximumScaleFactor_(void* param0);

	public unsafe static void GetGradientMeshInteriorPointsFromCoonsPatch(RawVector2 point0Ref, RawVector2 point1Ref, RawVector2 point2Ref, RawVector2 point3Ref, RawVector2 point4Ref, RawVector2 point5Ref, RawVector2 point6Ref, RawVector2 point7Ref, RawVector2 point8Ref, RawVector2 point9Ref, RawVector2 point10Ref, RawVector2 point11Ref, out RawVector2 tensorPoint11Ref, out RawVector2 tensorPoint12Ref, out RawVector2 tensorPoint21Ref, out RawVector2 tensorPoint22Ref)
	{
		tensorPoint11Ref = default(RawVector2);
		tensorPoint12Ref = default(RawVector2);
		tensorPoint21Ref = default(RawVector2);
		tensorPoint22Ref = default(RawVector2);
		fixed (RawVector2* ptr = &tensorPoint22Ref)
		{
			void* param = ptr;
			fixed (RawVector2* ptr2 = &tensorPoint21Ref)
			{
				void* param2 = ptr2;
				fixed (RawVector2* ptr3 = &tensorPoint12Ref)
				{
					void* param3 = ptr3;
					fixed (RawVector2* ptr4 = &tensorPoint11Ref)
					{
						void* param4 = ptr4;
						D2D1GetGradientMeshInteriorPointsFromCoonsPatch_(&point0Ref, &point1Ref, &point2Ref, &point3Ref, &point4Ref, &point5Ref, &point6Ref, &point7Ref, &point8Ref, &point9Ref, &point10Ref, &point11Ref, param4, param3, param2, param);
					}
				}
			}
		}
	}

	[DllImport("d2d1.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D2D1GetGradientMeshInteriorPointsFromCoonsPatch")]
	private unsafe static extern void D2D1GetGradientMeshInteriorPointsFromCoonsPatch_(void* param0, void* param1, void* param2, void* param3, void* param4, void* param5, void* param6, void* param7, void* param8, void* param9, void* param10, void* param11, void* param12, void* param13, void* param14, void* param15);
}
