#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;

namespace Svg.FilterEffects;

[SvgElement("feColorMatrix")]
public class SvgColourMatrix : SvgFilterPrimitive
{
	private SvgColourMatrixType _type;

	private string _values;

	internal static List<Type> SvgColourMatrixClassNames = new List<Type> { typeof(SvgColourMatrix) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgColourMatrixProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["type"] = new SvgPropertyDescriptor<SvgColourMatrix, SvgColourMatrixType>(DescriptorType.Property, "type", "http://www.w3.org/2000/svg", new SvgColourMatrixTypeConverter(), (SvgColourMatrix t) => t.Type, delegate(SvgColourMatrix t, SvgColourMatrixType v)
		{
			t.Type = v;
		}),
		["values"] = new SvgPropertyDescriptor<SvgColourMatrix, string>(DescriptorType.Property, "values", "http://www.w3.org/2000/svg", new StringConverter(), (SvgColourMatrix t) => t.Values, delegate(SvgColourMatrix t, string v)
		{
			t.Values = v;
		})
	};

	[SvgAttribute("type")]
	public SvgColourMatrixType Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
			Attributes["type"] = value;
		}
	}

	[SvgAttribute("values")]
	public string Values
	{
		get
		{
			return _values;
		}
		set
		{
			_values = value;
			Attributes["values"] = value;
		}
	}

	internal override string AttributeName => "feColorMatrix";

	internal override List<Type> ClassNames => SvgColourMatrixClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgColourMatrixProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgColourMatrix>();
	}

	public override SvgElement DeepCopy<T>()
	{
		SvgColourMatrix obj = base.DeepCopy<T>() as SvgColourMatrix;
		obj._type = _type;
		obj._values = _values;
		return obj;
	}

	public override void Process(ImageBuffer buffer)
	{
		Bitmap bitmap = buffer[base.Input];
		if (bitmap == null)
		{
			return;
		}
		float[][] array;
		switch (Type)
		{
		case SvgColourMatrixType.HueRotate:
		{
			float num = (string.IsNullOrEmpty(Values) ? 0f : float.Parse(Values, NumberStyles.Any, CultureInfo.InvariantCulture));
			array = new float[5][]
			{
				new float[5]
				{
					(float)(0.213 + Math.Cos(num) * 0.787 + Math.Sin(num) * -0.213),
					(float)(0.715 + Math.Cos(num) * -0.715 + Math.Sin(num) * -0.715),
					(float)(0.072 + Math.Cos(num) * -0.072 + Math.Sin(num) * 0.928),
					0f,
					0f
				},
				new float[5]
				{
					(float)(0.213 + Math.Cos(num) * -0.213 + Math.Sin(num) * 0.143),
					(float)(0.715 + Math.Cos(num) * 0.285 + Math.Sin(num) * 0.14),
					(float)(0.072 + Math.Cos(num) * -0.072 + Math.Sin(num) * -0.283),
					0f,
					0f
				},
				new float[5]
				{
					(float)(0.213 + Math.Cos(num) * -0.213 + Math.Sin(num) * -0.787),
					(float)(0.715 + Math.Cos(num) * -0.715 + Math.Sin(num) * 0.715),
					(float)(0.072 + Math.Cos(num) * 0.928 + Math.Sin(num) * 0.072),
					0f,
					0f
				},
				new float[5] { 0f, 0f, 0f, 1f, 0f },
				new float[5] { 0f, 0f, 0f, 0f, 1f }
			};
			break;
		}
		case SvgColourMatrixType.LuminanceToAlpha:
			array = new float[5][]
			{
				new float[5],
				new float[5],
				new float[5],
				new float[5] { 0.2125f, 0.7154f, 0.0721f, 0f, 0f },
				new float[5] { 0f, 0f, 0f, 0f, 1f }
			};
			break;
		case SvgColourMatrixType.Saturate:
		{
			float num = (string.IsNullOrEmpty(Values) ? 1f : float.Parse(Values, NumberStyles.Any, CultureInfo.InvariantCulture));
			array = new float[5][]
			{
				new float[5]
				{
					(float)(0.213 + 0.787 * (double)num),
					(float)(0.715 - 0.715 * (double)num),
					(float)(0.072 - 0.072 * (double)num),
					0f,
					0f
				},
				new float[5]
				{
					(float)(0.213 - 0.213 * (double)num),
					(float)(0.715 + 0.285 * (double)num),
					(float)(0.072 - 0.072 * (double)num),
					0f,
					0f
				},
				new float[5]
				{
					(float)(0.213 - 0.213 * (double)num),
					(float)(0.715 - 0.715 * (double)num),
					(float)(0.072 + 0.928 * (double)num),
					0f,
					0f
				},
				new float[5] { 0f, 0f, 0f, 1f, 0f },
				new float[5] { 0f, 0f, 0f, 0f, 1f }
			};
			break;
		}
		default:
		{
			string[] source = Values.Split(new char[5] { ' ', '\t', '\n', '\r', ',' }, StringSplitOptions.RemoveEmptyEntries);
			array = new float[5][];
			for (int i = 0; i < 4; i++)
			{
				array[i] = (from v in source.Skip(i * 5).Take(5)
					select float.Parse(v, NumberStyles.Any, CultureInfo.InvariantCulture)).ToArray();
			}
			array[4] = new float[5] { 0f, 0f, 0f, 0f, 1f };
			break;
		}
		}
		ColorMatrix newColorMatrix = new ColorMatrix(array);
		using ImageAttributes imageAttributes = new ImageAttributes();
		imageAttributes.SetColorMatrix(newColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
		Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height);
		using (Graphics graphics = Graphics.FromImage(bitmap2))
		{
			graphics.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttributes);
			graphics.Flush();
		}
		buffer[base.Result] = bitmap2;
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgColourMatrixProperty in SvgColourMatrixProperties)
		{
			yield return svgColourMatrixProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgColourMatrixProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgColourMatrixProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgColourMatrixProperties.TryGetValue(attributeName, out var value2))
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
