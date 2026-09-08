using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Svg;

public abstract class EnumBaseConverter<T> : TypeConverter where T : struct
{
	public enum CaseHandling
	{
		CamelCase,
		PascalCase,
		LowerCase,
		KebabCase
	}

	public CaseHandling CaseHandlingMode { get; }

	public EnumBaseConverter(CaseHandling caseHandling = CaseHandling.CamelCase)
	{
		CaseHandlingMode = caseHandling;
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		string text = value as string;
		if (text != null)
		{
			if (CaseHandlingMode == CaseHandling.KebabCase)
			{
				text = text.Replace("-", string.Empty);
			}
			if (Enum.TryParse<T>(text, ignoreCase: true, out var result))
			{
				return result;
			}
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string) && value is T val)
		{
			string text = val.ToString();
			if (CaseHandlingMode == CaseHandling.CamelCase)
			{
				return $"{text[0].ToString().ToLower()}{text.Substring(1)}";
			}
			if (CaseHandlingMode == CaseHandling.PascalCase)
			{
				return text;
			}
			if (CaseHandlingMode == CaseHandling.KebabCase)
			{
				text = Regex.Replace(text, "(\\w)([A-Z])", "$1-$2", RegexOptions.CultureInvariant);
			}
			return text.ToLower();
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
