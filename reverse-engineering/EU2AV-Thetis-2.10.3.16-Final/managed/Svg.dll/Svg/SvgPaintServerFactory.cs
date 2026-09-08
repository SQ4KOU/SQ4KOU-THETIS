using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace Svg;

internal class SvgPaintServerFactory : TypeConverter
{
	private static readonly SvgColourConverter _colourConverter;

	static SvgPaintServerFactory()
	{
		_colourConverter = new SvgColourConverter();
	}

	public static SvgPaintServer Create(string value, SvgDocument document)
	{
		if (value == null)
		{
			return SvgPaintServer.NotSet;
		}
		string text = value.Trim();
		if (string.IsNullOrEmpty(text))
		{
			return SvgPaintServer.NotSet;
		}
		if (text.Equals("none", StringComparison.OrdinalIgnoreCase))
		{
			return SvgPaintServer.None;
		}
		if (text.Equals("currentColor", StringComparison.OrdinalIgnoreCase))
		{
			return new SvgDeferredPaintServer("currentColor");
		}
		if (text.Equals("inherit", StringComparison.OrdinalIgnoreCase))
		{
			return SvgPaintServer.Inherit;
		}
		if (text.StartsWith("url(", StringComparison.OrdinalIgnoreCase))
		{
			int num = text.IndexOf(')', 4) + 1;
			string id = text.Substring(0, num);
			text = text.Substring(num).Trim();
			SvgPaintServer fallbackServer = (string.IsNullOrEmpty(text) ? null : Create(text, document));
			return new SvgDeferredPaintServer(id, fallbackServer);
		}
		return new SvgColourServer((Color)_colourConverter.ConvertFrom(text));
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			return Create((string)value, (SvgDocument)context);
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
		if (destinationType == typeof(string))
		{
			if (value == SvgPaintServer.None || value == SvgPaintServer.Inherit || value == SvgPaintServer.NotSet)
			{
				return value.ToString();
			}
			if (value is SvgColourServer svgColourServer)
			{
				return new SvgColourConverter().ConvertTo(svgColourServer.Colour, typeof(string));
			}
			if (value is SvgDeferredPaintServer svgDeferredPaintServer)
			{
				return svgDeferredPaintServer.ToString();
			}
			if (value != null)
			{
				return string.Format(CultureInfo.InvariantCulture, "url(#{0})", ((SvgPaintServer)value).ID);
			}
			return "none";
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
