using System;
using System.Collections.Generic;

namespace SkiaSharp;

public readonly ref struct SKRuntimeEffectUniform
{
	internal enum DataType
	{
		Empty,
		Float,
		FloatArray,
		Int32,
		Int32Array,
		Color
	}

	private readonly float floatValue;

	private readonly ReadOnlySpan<float> floatArray;

	private readonly int intValue;

	private readonly ReadOnlySpan<int> intArray;

	private readonly SKColorF colorValue;

	public static SKRuntimeEffectUniform Empty => default(SKRuntimeEffectUniform);

	public bool IsEmpty => Type == DataType.Empty;

	public int Size { get; }

	internal DataType Type { get; }

	private SKRuntimeEffectUniform(DataType type, int size, float floatValue = 0f, ReadOnlySpan<float> floatArray = default(ReadOnlySpan<float>), int intValue = 0, ReadOnlySpan<int> intArray = default(ReadOnlySpan<int>), SKColorF colorValue = default(SKColorF))
	{
		Type = type;
		Size = size;
		this.floatValue = floatValue;
		this.floatArray = floatArray;
		this.intValue = intValue;
		this.intArray = intArray;
		this.colorValue = colorValue;
	}

	public static implicit operator SKRuntimeEffectUniform(float value)
	{
		return new SKRuntimeEffectUniform(DataType.Float, 4, value);
	}

	public static implicit operator SKRuntimeEffectUniform(float[] value)
	{
		return (ReadOnlySpan<float>)value;
	}

	public static implicit operator SKRuntimeEffectUniform(Span<float> value)
	{
		return (ReadOnlySpan<float>)value;
	}

	public static implicit operator SKRuntimeEffectUniform(ReadOnlySpan<float> value)
	{
		return new SKRuntimeEffectUniform(DataType.FloatArray, 4 * value.Length, 0f, value);
	}

	public static implicit operator SKRuntimeEffectUniform(SKPoint value)
	{
		return (ReadOnlySpan<float>)new float[2] { value.X, value.Y };
	}

	public static implicit operator SKRuntimeEffectUniform(SKSize value)
	{
		return (ReadOnlySpan<float>)new float[2] { value.Width, value.Height };
	}

	public static implicit operator SKRuntimeEffectUniform(SKPoint3 value)
	{
		return (ReadOnlySpan<float>)new float[3] { value.X, value.Y, value.Z };
	}

	public static implicit operator SKRuntimeEffectUniform(int value)
	{
		return new SKRuntimeEffectUniform(DataType.Int32, 4, 0f, default(ReadOnlySpan<float>), value);
	}

	public static implicit operator SKRuntimeEffectUniform(int[] value)
	{
		return (ReadOnlySpan<int>)value;
	}

	public static implicit operator SKRuntimeEffectUniform(Span<int> value)
	{
		return (ReadOnlySpan<int>)value;
	}

	public static implicit operator SKRuntimeEffectUniform(ReadOnlySpan<int> value)
	{
		int size = 4 * value.Length;
		ReadOnlySpan<int> readOnlySpan = value;
		return new SKRuntimeEffectUniform(DataType.Int32Array, size, 0f, default(ReadOnlySpan<float>), 0, readOnlySpan);
	}

	public static implicit operator SKRuntimeEffectUniform(SKPointI value)
	{
		return (ReadOnlySpan<int>)new int[2] { value.X, value.Y };
	}

	public static implicit operator SKRuntimeEffectUniform(SKSizeI value)
	{
		return (ReadOnlySpan<int>)new int[2] { value.Width, value.Height };
	}

	public static implicit operator SKRuntimeEffectUniform(SKColor value)
	{
		return (SKColorF)value;
	}

	public static implicit operator SKRuntimeEffectUniform(SKColorF value)
	{
		return new SKRuntimeEffectUniform(DataType.Color, 16, 0f, default(ReadOnlySpan<float>), 0, default(ReadOnlySpan<int>), value);
	}

	public static implicit operator SKRuntimeEffectUniform(float[][] value)
	{
		List<float> list = new List<float>();
		foreach (float[] collection in value)
		{
			list.AddRange(collection);
		}
		return list.ToArray();
	}

	public static implicit operator SKRuntimeEffectUniform(SKMatrix value)
	{
		return value.Values;
	}

	public unsafe void WriteTo(Span<byte> data)
	{
		switch (Type)
		{
		case DataType.Float:
			if (data.Length == 4)
			{
				fixed (float* ptr3 = &floatValue)
				{
					void* pointer5 = ptr3;
					new ReadOnlySpan<byte>(pointer5, Size).CopyTo(data);
				}
				break;
			}
			throw new ArgumentOutOfRangeException("Type", $"Unknown float data type length: {data.Length}");
		case DataType.FloatArray:
			if (data.Length == 4 * floatArray.Length)
			{
				fixed (float* ptr2 = floatArray)
				{
					void* pointer4 = ptr2;
					new ReadOnlySpan<byte>(pointer4, Size).CopyTo(data);
				}
				break;
			}
			throw new ArgumentOutOfRangeException("Type", $"Unknown float array data type length: {data.Length}");
		case DataType.Int32:
			if (data.Length == 4)
			{
				fixed (int* ptr4 = &intValue)
				{
					void* pointer6 = ptr4;
					new ReadOnlySpan<byte>(pointer6, Size).CopyTo(data);
				}
				break;
			}
			throw new ArgumentOutOfRangeException("Type", $"Unknown int data type length: {data.Length}");
		case DataType.Int32Array:
			if (data.Length == 4 * intArray.Length)
			{
				fixed (int* ptr = intArray)
				{
					void* pointer3 = ptr;
					new ReadOnlySpan<byte>(pointer3, Size).CopyTo(data);
				}
				break;
			}
			throw new ArgumentOutOfRangeException("Type", $"Unknown int array data type length: {data.Length}");
		case DataType.Color:
			if (data.Length == 12)
			{
				void* pointer = stackalloc float[3] { colorValue.Red, colorValue.Green, colorValue.Blue };
				new ReadOnlySpan<byte>(pointer, data.Length).CopyTo(data);
				break;
			}
			if (data.Length == 16)
			{
				void* pointer2 = stackalloc float[4] { colorValue.Red, colorValue.Green, colorValue.Blue, colorValue.Alpha };
				new ReadOnlySpan<byte>(pointer2, data.Length).CopyTo(data);
				break;
			}
			throw new ArgumentOutOfRangeException("Type", $"Unknown color data type length: {data.Length}");
		case DataType.Empty:
			data.Fill(0);
			break;
		default:
			throw new ArgumentOutOfRangeException("Type", $"Unknown data type: '{Type}'");
		}
	}
}
