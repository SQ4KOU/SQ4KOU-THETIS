using System;

namespace SkiaSharp;

public class SKColorFilter : SKObject, ISKReferenceCounted
{
	private sealed class SKColorFilterStatic : SKColorFilter
	{
		internal SKColorFilterStatic(IntPtr x)
			: base(x, owns: false)
		{
		}

		protected override void Dispose(bool disposing)
		{
		}
	}

	public const int ColorMatrixSize = 20;

	public const int TableMaxLength = 256;

	private static readonly SKColorFilter srgbToLinear;

	private static readonly SKColorFilter linearToSrgb;

	static SKColorFilter()
	{
		srgbToLinear = new SKColorFilterStatic(SkiaApi.sk_colorfilter_new_srgb_to_linear_gamma());
		linearToSrgb = new SKColorFilterStatic(SkiaApi.sk_colorfilter_new_linear_to_srgb_gamma());
	}

	internal static void EnsureStaticInstanceAreInitialized()
	{
	}

	internal SKColorFilter(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public static SKColorFilter CreateSrgbToLinearGamma()
	{
		return srgbToLinear;
	}

	public static SKColorFilter CreateLinearToSrgbGamma()
	{
		return linearToSrgb;
	}

	public static SKColorFilter CreateBlendMode(SKColor c, SKBlendMode mode)
	{
		return GetObject(SkiaApi.sk_colorfilter_new_mode((uint)c, mode));
	}

	public static SKColorFilter CreateLighting(SKColor mul, SKColor add)
	{
		return GetObject(SkiaApi.sk_colorfilter_new_lighting((uint)mul, (uint)add));
	}

	public static SKColorFilter CreateCompose(SKColorFilter outer, SKColorFilter inner)
	{
		if (outer == null)
		{
			throw new ArgumentNullException("outer");
		}
		if (inner == null)
		{
			throw new ArgumentNullException("inner");
		}
		return GetObject(SkiaApi.sk_colorfilter_new_compose(outer.Handle, inner.Handle));
	}

	public static SKColorFilter CreateLerp(float weight, SKColorFilter filter0, SKColorFilter filter1)
	{
		if (filter0 == null)
		{
			throw new ArgumentNullException("filter0");
		}
		if (filter1 == null)
		{
			throw new ArgumentNullException("filter1");
		}
		return GetObject(SkiaApi.sk_colorfilter_new_lerp(weight, filter0.Handle, filter1.Handle));
	}

	public static SKColorFilter CreateColorMatrix(float[] matrix)
	{
		if (matrix == null)
		{
			throw new ArgumentNullException("matrix");
		}
		return CreateColorMatrix(MemoryExtensions.AsSpan(matrix));
	}

	public unsafe static SKColorFilter CreateColorMatrix(ReadOnlySpan<float> matrix)
	{
		if (matrix.Length != 20)
		{
			throw new ArgumentException("Matrix must have a length of 20.", "matrix");
		}
		fixed (float* array = matrix)
		{
			return GetObject(SkiaApi.sk_colorfilter_new_color_matrix(array));
		}
	}

	public unsafe static SKColorFilter CreateHslaColorMatrix(ReadOnlySpan<float> matrix)
	{
		if (matrix.Length != 20)
		{
			throw new ArgumentException("Matrix must have a length of 20.", "matrix");
		}
		fixed (float* array = matrix)
		{
			return GetObject(SkiaApi.sk_colorfilter_new_hsla_matrix(array));
		}
	}

	public static SKColorFilter CreateLumaColor()
	{
		return GetObject(SkiaApi.sk_colorfilter_new_luma_color());
	}

	public static SKColorFilter CreateTable(byte[] table)
	{
		if (table == null)
		{
			throw new ArgumentNullException("table");
		}
		return CreateTable(MemoryExtensions.AsSpan(table));
	}

	public unsafe static SKColorFilter CreateTable(ReadOnlySpan<byte> table)
	{
		if (table.Length != 256)
		{
			throw new ArgumentException($"Table must have a length of {256}.", "table");
		}
		fixed (byte* table2 = table)
		{
			return GetObject(SkiaApi.sk_colorfilter_new_table(table2));
		}
	}

	public static SKColorFilter CreateTable(byte[] tableA, byte[] tableR, byte[] tableG, byte[] tableB)
	{
		if (tableA == null)
		{
			throw new ArgumentNullException("tableA");
		}
		if (tableR == null)
		{
			throw new ArgumentNullException("tableR");
		}
		if (tableG == null)
		{
			throw new ArgumentNullException("tableG");
		}
		if (tableB == null)
		{
			throw new ArgumentNullException("tableB");
		}
		return CreateTable(MemoryExtensions.AsSpan(tableA), MemoryExtensions.AsSpan(tableR), MemoryExtensions.AsSpan(tableG), MemoryExtensions.AsSpan(tableB));
	}

	public unsafe static SKColorFilter CreateTable(ReadOnlySpan<byte> tableA, ReadOnlySpan<byte> tableR, ReadOnlySpan<byte> tableG, ReadOnlySpan<byte> tableB)
	{
		if (tableA.Length != 256)
		{
			throw new ArgumentException($"Table A must have a length of {256}.", "tableA");
		}
		if (tableR.Length != 256)
		{
			throw new ArgumentException($"Table R must have a length of {256}.", "tableR");
		}
		if (tableG.Length != 256)
		{
			throw new ArgumentException($"Table G must have a length of {256}.", "tableG");
		}
		if (tableB.Length != 256)
		{
			throw new ArgumentException($"Table B must have a length of {256}.", "tableB");
		}
		fixed (byte* tableA2 = tableA)
		{
			fixed (byte* tableR2 = tableR)
			{
				fixed (byte* tableG2 = tableG)
				{
					fixed (byte* tableB2 = tableB)
					{
						return GetObject(SkiaApi.sk_colorfilter_new_table_argb(tableA2, tableR2, tableG2, tableB2));
					}
				}
			}
		}
	}

	public unsafe static SKColorFilter CreateHighContrast(SKHighContrastConfig config)
	{
		return GetObject(SkiaApi.sk_colorfilter_new_high_contrast(&config));
	}

	public static SKColorFilter CreateHighContrast(bool grayscale, SKHighContrastConfigInvertStyle invertStyle, float contrast)
	{
		return CreateHighContrast(new SKHighContrastConfig(grayscale, invertStyle, contrast));
	}

	internal static SKColorFilter GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKColorFilter(h, o));
	}
}
