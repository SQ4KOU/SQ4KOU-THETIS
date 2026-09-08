using System;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class ConvolveMatrix : Effect
{
	private float[] kernelMatrix;

	public float KernelUnitLength
	{
		get
		{
			return GetFloatValue(0);
		}
		set
		{
			SetValue(0, value);
		}
	}

	public ConvoleMatrixScaleMode ScaleMode
	{
		get
		{
			return GetEnumValue<ConvoleMatrixScaleMode>(1);
		}
		set
		{
			SetEnumValue(1, value);
		}
	}

	public int KernelSizeX
	{
		get
		{
			return (int)GetUIntValue(2);
		}
		set
		{
			SetValue(2, (uint)value);
		}
	}

	public int KernelSizeY
	{
		get
		{
			return (int)GetUIntValue(3);
		}
		set
		{
			SetValue(3, (uint)value);
		}
	}

	public unsafe float[] KernelMatrix
	{
		get
		{
			if (kernelMatrix == null)
			{
				kernelMatrix = new float[KernelSizeX * KernelSizeY];
			}
			if (kernelMatrix.Length != 0)
			{
				fixed (float* ptr = kernelMatrix)
				{
					void* ptr2 = ptr;
					GetValue(4, PropertyType.Blob, (IntPtr)ptr2, 4 * kernelMatrix.Length);
				}
			}
			return kernelMatrix;
		}
		set
		{
			if (value.Length != KernelSizeX * KernelSizeY)
			{
				throw new ArgumentException("Size of the array doesn't match KernelSizeX * KernelSizeY");
			}
			kernelMatrix = value;
			fixed (float* ptr = kernelMatrix)
			{
				void* ptr2 = ptr;
				SetValue(4, PropertyType.Blob, (IntPtr)ptr2, 4 * kernelMatrix.Length);
			}
		}
	}

	public float Divisor
	{
		get
		{
			return GetFloatValue(5);
		}
		set
		{
			SetValue(5, value);
		}
	}

	public float Bias
	{
		get
		{
			return GetFloatValue(6);
		}
		set
		{
			SetValue(6, value);
		}
	}

	public RawVector2 KernelOffset
	{
		get
		{
			return GetVector2Value(7);
		}
		set
		{
			SetValue(7, value);
		}
	}

	public bool PreserveAlpha
	{
		get
		{
			return GetBoolValue(8);
		}
		set
		{
			SetValue(8, value);
		}
	}

	public BorderMode BorderMode
	{
		get
		{
			return GetEnumValue<BorderMode>(9);
		}
		set
		{
			SetEnumValue(9, value);
		}
	}

	public bool ClampOutput
	{
		get
		{
			return GetBoolValue(2);
		}
		set
		{
			SetValue(2, value);
		}
	}

	public ConvolveMatrix(DeviceContext context)
		: base(context, Effect.ConvolveMatrix)
	{
	}
}
