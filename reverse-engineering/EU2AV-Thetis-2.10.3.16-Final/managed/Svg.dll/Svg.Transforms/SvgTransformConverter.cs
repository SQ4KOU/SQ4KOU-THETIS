using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Svg.Helpers;

namespace Svg.Transforms;

public class SvgTransformConverter : TypeConverter
{
	private enum TransformType
	{
		Invalid,
		Translate,
		Rotate,
		Scale,
		Matrix,
		Shear,
		SkewX,
		SkewY
	}

	private static readonly char[] SplitChars = new char[2] { ' ', ',' };

	private const string TranslateTransform = "translate";

	private const string RotateTransform = "rotate";

	private const string ScaleTransform = "scale";

	private const string MatrixTransform = "matrix";

	private const string ShearTransform = "shear";

	private const string SkewXTransform = "skewX";

	private const string SkewYTransform = "skewY";

	private static TransformType GetTransformType(ref ReadOnlySpan<char> transformName)
	{
		if (transformName.SequenceEqual("translate".AsSpan()))
		{
			return TransformType.Translate;
		}
		if (transformName.SequenceEqual("rotate".AsSpan()))
		{
			return TransformType.Rotate;
		}
		if (transformName.SequenceEqual("scale".AsSpan()))
		{
			return TransformType.Scale;
		}
		if (transformName.SequenceEqual("matrix".AsSpan()))
		{
			return TransformType.Matrix;
		}
		if (transformName.SequenceEqual("shear".AsSpan()))
		{
			return TransformType.Shear;
		}
		if (transformName.SequenceEqual("skewX".AsSpan()))
		{
			return TransformType.SkewX;
		}
		if (transformName.SequenceEqual("skewY".AsSpan()))
		{
			return TransformType.SkewY;
		}
		return TransformType.Invalid;
	}

	public static SvgTransformCollection Parse(ReadOnlySpan<char> transform)
	{
		SvgTransformCollection svgTransformCollection = new SvgTransformCollection();
		ReadOnlySpan<char> span = transform.TrimStart();
		int length = span.Length;
		Span<char> span2 = SplitChars.AsSpan();
		do
		{
			int num = 0;
			int num2 = span.IndexOf('(');
			int num3 = span.IndexOf(')');
			if (num2 < 0 || num3 <= num2)
			{
				break;
			}
			ReadOnlySpan<char> transformName = span.Slice(num, num2 - num).Trim().Trim(',')
				.Trim();
			ReadOnlySpan<char> str = span.Slice(num2 + 1, num3 - num2 - 1).Trim();
			StringSplitEnumerator stringSplitEnumerator = new StringSplitEnumerator(str, span2);
			StringSplitEnumerator enumerator;
			switch (GetTransformType(ref transformName))
			{
			case TransformType.Translate:
			{
				int num5 = 0;
				float x = 0f;
				float y2 = 0f;
				enumerator = stringSplitEnumerator.GetEnumerator();
				while (enumerator.MoveNext())
				{
					ReadOnlySpan<char> value2 = enumerator.Current.Value;
					switch (num5)
					{
					case 0:
						x = StringParser.ToFloat(value2);
						break;
					case 1:
						y2 = StringParser.ToFloat(value2);
						break;
					}
					num5++;
				}
				if (num5 == 0 || num5 > 2)
				{
					throw new FormatException("Translate transforms must be in the format 'translate(x [y])'");
				}
				svgTransformCollection.Add((num5 > 1) ? new SvgTranslate(x, y2) : new SvgTranslate(x));
				break;
			}
			case TransformType.Rotate:
			{
				int num6 = 0;
				float angle = 0f;
				float centerX = 0f;
				float centerY = 0f;
				enumerator = stringSplitEnumerator.GetEnumerator();
				while (enumerator.MoveNext())
				{
					ReadOnlySpan<char> value3 = enumerator.Current.Value;
					switch (num6)
					{
					case 0:
						angle = StringParser.ToFloat(value3);
						break;
					case 1:
						centerX = StringParser.ToFloat(value3);
						break;
					case 2:
						centerY = StringParser.ToFloat(value3);
						break;
					}
					num6++;
				}
				if (num6 != 1 && num6 != 3)
				{
					throw new FormatException("Rotate transforms must be in the format 'rotate(angle [cx cy])'");
				}
				svgTransformCollection.Add((num6 == 1) ? new SvgRotate(angle) : new SvgRotate(angle, centerX, centerY));
				break;
			}
			case TransformType.Scale:
			{
				int num10 = 0;
				float x4 = 0f;
				float y4 = 0f;
				enumerator = stringSplitEnumerator.GetEnumerator();
				while (enumerator.MoveNext())
				{
					ReadOnlySpan<char> value7 = enumerator.Current.Value;
					switch (num10)
					{
					case 0:
						x4 = StringParser.ToFloat(value7);
						break;
					case 1:
						y4 = StringParser.ToFloat(value7);
						break;
					}
					num10++;
				}
				if (num10 == 0 || num10 > 2)
				{
					throw new FormatException("Scale transforms must be in the format 'scale(x [y])'");
				}
				svgTransformCollection.Add((num10 > 1) ? new SvgScale(x4, y4) : new SvgScale(x4));
				break;
			}
			case TransformType.Matrix:
			{
				int num7 = 0;
				float item = 0f;
				float item2 = 0f;
				float item3 = 0f;
				float item4 = 0f;
				float item5 = 0f;
				float item6 = 0f;
				enumerator = stringSplitEnumerator.GetEnumerator();
				while (enumerator.MoveNext())
				{
					ReadOnlySpan<char> value4 = enumerator.Current.Value;
					switch (num7)
					{
					case 0:
						item = StringParser.ToFloat(value4);
						break;
					case 1:
						item2 = StringParser.ToFloat(value4);
						break;
					case 2:
						item3 = StringParser.ToFloat(value4);
						break;
					case 3:
						item4 = StringParser.ToFloat(value4);
						break;
					case 4:
						item5 = StringParser.ToFloat(value4);
						break;
					case 5:
						item6 = StringParser.ToFloat(value4);
						break;
					}
					num7++;
				}
				if (num7 != 6)
				{
					throw new FormatException("Matrix transforms must be in the format 'matrix(m11 m12 m21 m22 dx dy)'");
				}
				svgTransformCollection.Add(new SvgMatrix(new List<float>(6) { item, item2, item3, item4, item5, item6 }));
				break;
			}
			case TransformType.Shear:
			{
				int num8 = 0;
				float x2 = 0f;
				float y3 = 0f;
				enumerator = stringSplitEnumerator.GetEnumerator();
				while (enumerator.MoveNext())
				{
					ReadOnlySpan<char> value5 = enumerator.Current.Value;
					switch (num8)
					{
					case 0:
						x2 = StringParser.ToFloat(value5);
						break;
					case 1:
						y3 = StringParser.ToFloat(value5);
						break;
					}
					num8++;
				}
				if (num8 == 0 || num8 > 2)
				{
					throw new FormatException("Shear transforms must be in the format 'shear(x [y])'");
				}
				svgTransformCollection.Add((num8 > 1) ? new SvgShear(x2, y3) : new SvgShear(x2));
				break;
			}
			case TransformType.SkewX:
			{
				int num9 = 0;
				float x3 = 0f;
				enumerator = stringSplitEnumerator.GetEnumerator();
				while (enumerator.MoveNext())
				{
					ReadOnlySpan<char> value6 = enumerator.Current.Value;
					if (num9 == 0)
					{
						x3 = StringParser.ToFloat(value6);
					}
					num9++;
				}
				if (num9 != 1)
				{
					throw new FormatException("SkewX transforms must be in the format 'skewX(a)'");
				}
				svgTransformCollection.Add(new SvgSkew(x3, 0f));
				break;
			}
			case TransformType.SkewY:
			{
				int num4 = 0;
				float y = 0f;
				enumerator = stringSplitEnumerator.GetEnumerator();
				while (enumerator.MoveNext())
				{
					ReadOnlySpan<char> value = enumerator.Current.Value;
					if (num4 == 0)
					{
						y = StringParser.ToFloat(value);
					}
					num4++;
				}
				if (num4 != 1)
				{
					throw new FormatException("SkewY transforms must be in the format 'skewY(a)'");
				}
				svgTransformCollection.Add(new SvgSkew(0f, y));
				break;
			}
			}
			num = num3;
			if (num + 1 > length)
			{
				break;
			}
			span = span.Slice(num + 1, length - num - 1).TrimStart();
			length = span.Length;
		}
		while (length > 0);
		return svgTransformCollection;
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (!(value is string text))
		{
			return base.ConvertFrom(context, culture, value);
		}
		return Parse(text.AsSpan());
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (!(sourceType == typeof(string)))
		{
			return base.CanConvertFrom(context, sourceType);
		}
		return true;
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (!(destinationType == typeof(string)))
		{
			return base.CanConvertTo(context, destinationType);
		}
		return true;
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string) && value is SvgTransformCollection source)
		{
			return string.Join(" ", source.Select((SvgTransform t) => t.WriteToString()).ToArray());
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
