using System;
using System.ComponentModel;
using System.Globalization;

namespace Svg;

internal class SvgViewBoxConverter : TypeConverter
{
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			string[] array = ((string)value).Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length != 4)
			{
				throw new SvgException("The 'viewBox' attribute must be in the format 'minX, minY, width, height'.");
			}
			return new SvgViewBox(float.Parse(array[0], NumberStyles.Float, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Float, CultureInfo.InvariantCulture), float.Parse(array[2], NumberStyles.Float, CultureInfo.InvariantCulture), float.Parse(array[3], NumberStyles.Float, CultureInfo.InvariantCulture));
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			SvgViewBox svgViewBox = (SvgViewBox)value;
			return $"{svgViewBox.MinX.ToString(CultureInfo.InvariantCulture)}, {svgViewBox.MinY.ToSvgString()}, {svgViewBox.Width.ToString(CultureInfo.InvariantCulture)}, {svgViewBox.Height.ToSvgString()}";
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
