#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feGaussianBlur")]
public class SvgGaussianBlur : SvgFilterPrimitive
{
	private float _stdDeviationX = float.NaN;

	private float _stdDeviationY = float.NaN;

	private bool _isPrecalculated;

	private int[] _kernel;

	private int _kernelSum;

	private int[,] _multable;

	internal static List<Type> SvgGaussianBlurClassNames = new List<Type> { typeof(SvgGaussianBlur) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgGaussianBlurProperties = new Dictionary<string, ISvgPropertyDescriptor> { ["stdDeviation"] = new SvgPropertyDescriptor<SvgGaussianBlur, SvgNumberCollection>(DescriptorType.Property, "stdDeviation", "http://www.w3.org/2000/svg", new SvgNumberCollectionConverter(), (SvgGaussianBlur t) => t.StdDeviation, delegate(SvgGaussianBlur t, SvgNumberCollection v)
	{
		t.StdDeviation = v;
	}) };

	[SvgAttribute("stdDeviation")]
	public SvgNumberCollection StdDeviation
	{
		get
		{
			return GetAttribute("stdDeviation", inherited: false, new SvgNumberCollection { 0f });
		}
		set
		{
			Attributes["stdDeviation"] = value;
		}
	}

	internal override string AttributeName => "feGaussianBlur";

	internal override List<Type> ClassNames => SvgGaussianBlurClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgGaussianBlurProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgGaussianBlur>();
	}

	private void PreCalculate()
	{
		float num = 0f;
		float num2 = 0f;
		if (StdDeviation.Count == 1)
		{
			num = StdDeviation[0];
			num2 = num;
		}
		else if (StdDeviation.Count == 2)
		{
			num = StdDeviation[0];
			num2 = StdDeviation[1];
		}
		if (num < 0f || num2 < 0f)
		{
			_isPrecalculated = false;
		}
		else
		{
			if (_stdDeviationX == num && _stdDeviationY == num2)
			{
				return;
			}
			int num3 = (int)(num * 2f + 1f);
			_kernel = new int[num3];
			_multable = new int[num3, 256];
			for (int i = 1; (float)i <= num; i++)
			{
				int num4 = (int)(num - (float)i);
				int num5 = (int)(num + (float)i);
				_kernel[num5] = (_kernel[num4] = (num4 + 1) * (num4 + 1));
				_kernelSum += _kernel[num5] + _kernel[num4];
				for (int j = 0; j < 256; j++)
				{
					_multable[num5, j] = (_multable[num4, j] = _kernel[num5] * j);
				}
			}
			_kernel[(int)num] = (int)((num + 1f) * (num + 1f));
			_kernelSum += _kernel[(int)num];
			for (int k = 0; k < 256; k++)
			{
				_multable[(int)num, k] = _kernel[(int)num] * k;
			}
			_stdDeviationX = num;
			_stdDeviationY = num2;
			_isPrecalculated = true;
		}
	}

	public Bitmap Apply(Image inputImage)
	{
		Bitmap bitmap = inputImage as Bitmap;
		if (bitmap == null)
		{
			bitmap = new Bitmap(inputImage);
		}
		PreCalculate();
		if (!_isPrecalculated)
		{
			return bitmap;
		}
		using RawBitmap rawBitmap = new RawBitmap(bitmap);
		using RawBitmap rawBitmap2 = new RawBitmap(new Bitmap(inputImage.Width, inputImage.Height));
		int num = rawBitmap.Width * rawBitmap.Height;
		int[] array = new int[num];
		int[] array2 = new int[num];
		int[] array3 = new int[num];
		int[] array4 = new int[num];
		int[] array5 = new int[num];
		int[] array6 = new int[num];
		int[] array7 = new int[num];
		int[] array8 = new int[num];
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			array[i] = rawBitmap.ArgbValues[num2];
			array2[i] = rawBitmap.ArgbValues[++num2];
			array3[i] = rawBitmap.ArgbValues[++num2];
			array4[i] = rawBitmap.ArgbValues[++num2];
			num2++;
		}
		int num3 = 0;
		int num4 = 0;
		if (_stdDeviationX > 0f)
		{
			for (int j = 0; j < num; j++)
			{
				int num6;
				int num7;
				int num8;
				int num5 = (num6 = (num7 = (num8 = 0)));
				int num9 = (int)((float)j - _stdDeviationX);
				for (int k = 0; k < _kernel.Length; k++)
				{
					num2 = ((num9 >= num3) ? ((num9 <= num3 + rawBitmap.Width - 1) ? num9 : (num3 + rawBitmap.Width - 1)) : num3);
					num5 += _multable[k, array[num2]];
					num6 += _multable[k, array2[num2]];
					num7 += _multable[k, array3[num2]];
					num8 += _multable[k, array4[num2]];
					num9++;
				}
				array5[j] = num5 / _kernelSum;
				array6[j] = num6 / _kernelSum;
				array7[j] = num7 / _kernelSum;
				array8[j] = num8 / _kernelSum;
				if (_stdDeviationX > 0f && _stdDeviationY <= 0f)
				{
					rawBitmap2.ArgbValues[num4] = (byte)(num5 / _kernelSum);
					rawBitmap2.ArgbValues[++num4] = (byte)(num6 / _kernelSum);
					rawBitmap2.ArgbValues[++num4] = (byte)(num7 / _kernelSum);
					rawBitmap2.ArgbValues[++num4] = (byte)(num8 / _kernelSum);
					num4++;
				}
				if (j > 0 && j % rawBitmap.Width == 0)
				{
					num3 += rawBitmap.Width;
				}
			}
		}
		if (_stdDeviationX > 0f && _stdDeviationY <= 0f)
		{
			return rawBitmap2.Bitmap;
		}
		num4 = 0;
		for (int l = 0; l < rawBitmap.Height; l++)
		{
			int num10 = (int)((float)l - _stdDeviationY);
			num3 = num10 * rawBitmap.Width;
			for (int m = 0; m < rawBitmap.Width; m++)
			{
				int num6;
				int num7;
				int num8;
				int num5 = (num6 = (num7 = (num8 = 0)));
				int num9 = num3 + m;
				int num11 = num10;
				for (int n = 0; n < _kernel.Length; n++)
				{
					if (_stdDeviationX <= 0f && _stdDeviationY > 0f)
					{
						num2 = ((num11 < 0) ? m : ((num11 <= rawBitmap.Height - 1) ? num9 : (num - (rawBitmap.Width - m))));
						num5 += _multable[n, array[num2]];
						num6 += _multable[n, array2[num2]];
						num7 += _multable[n, array3[num2]];
						num8 += _multable[n, array4[num2]];
					}
					else
					{
						num2 = ((num11 < 0) ? m : ((num11 <= rawBitmap.Height - 1) ? num9 : (num - (rawBitmap.Width - m))));
						num5 += _multable[n, array5[num2]];
						num6 += _multable[n, array6[num2]];
						num7 += _multable[n, array7[num2]];
						num8 += _multable[n, array8[num2]];
					}
					num9 += rawBitmap.Width;
					num11++;
				}
				rawBitmap2.ArgbValues[num4] = (byte)(num5 / _kernelSum);
				rawBitmap2.ArgbValues[++num4] = (byte)(num6 / _kernelSum);
				rawBitmap2.ArgbValues[++num4] = (byte)(num7 / _kernelSum);
				rawBitmap2.ArgbValues[++num4] = (byte)(num8 / _kernelSum);
				num4++;
			}
		}
		return rawBitmap2.Bitmap;
	}

	public override void Process(ImageBuffer buffer)
	{
		Bitmap inputImage = buffer[base.Input];
		Bitmap value = Apply(inputImage);
		buffer[base.Result] = value;
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgGaussianBlurProperty in SvgGaussianBlurProperties)
		{
			yield return svgGaussianBlurProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgGaussianBlurProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgGaussianBlurProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgGaussianBlurProperties.TryGetValue(attributeName, out var value2))
		{
			try
			{
				value2.SetValue(this, context, culture, value);
			}
			catch
			{
				Trace.TraceWarning($"Attribute '{attributeName}' cannot be set - type '{GetType().FullName}' cannot convert from string '{value}'.");
			}
			return true;
		}
		return base.SetValue(attributeName, context, culture, value);
	}
}
