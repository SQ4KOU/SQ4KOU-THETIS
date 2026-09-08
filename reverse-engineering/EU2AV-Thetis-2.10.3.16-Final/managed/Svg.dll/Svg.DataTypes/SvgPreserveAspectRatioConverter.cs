using System;
using System.ComponentModel;
using System.Globalization;

namespace Svg.DataTypes;

public sealed class SvgPreserveAspectRatioConverter : TypeConverter
{
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			return new SvgAspectRatio();
		}
		if (!(value is string))
		{
			throw new ArgumentOutOfRangeException("value must be a string.");
		}
		SvgPreserveAspectRatio result = SvgPreserveAspectRatio.none;
		bool defer = false;
		bool slice = false;
		string[] array = (value as string).Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		int num = 0;
		if (array[0].Equals("defer"))
		{
			defer = true;
			num++;
			if (array.Length < 2)
			{
				throw new ArgumentOutOfRangeException("value is not a member of SvgPreserveAspectRatio");
			}
		}
		if (!Enum.TryParse<SvgPreserveAspectRatio>(array[num], out result))
		{
			throw new ArgumentOutOfRangeException("value is not a member of SvgPreserveAspectRatio");
		}
		num++;
		if (array.Length > num)
		{
			string text = array[num];
			if (!(text == "meet"))
			{
				if (!(text == "slice"))
				{
					throw new ArgumentOutOfRangeException("value is not a member of SvgPreserveAspectRatio");
				}
				slice = true;
			}
		}
		num++;
		if (array.Length > num)
		{
			throw new ArgumentOutOfRangeException("value is not a member of SvgPreserveAspectRatio");
		}
		return new SvgAspectRatio(result, slice, defer);
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
