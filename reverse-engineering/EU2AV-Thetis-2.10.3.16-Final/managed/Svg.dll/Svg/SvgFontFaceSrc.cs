#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg;

[SvgElement("font-face-src")]
public class SvgFontFaceSrc : SvgElement
{
	internal static List<Type> SvgFontFaceSrcClassNames = new List<Type> { typeof(SvgFontFaceSrc) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgFontFaceSrcProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "font-face-src";

	internal override List<Type> ClassNames => SvgFontFaceSrcClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgFontFaceSrcProperties;

	public override SvgElement DeepCopy()
	{
		return base.DeepCopy<SvgFontFaceSrc>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgFontFaceSrcProperty in SvgFontFaceSrcProperties)
		{
			yield return svgFontFaceSrcProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgFontFaceSrcProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgFontFaceSrcProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgFontFaceSrcProperties.TryGetValue(attributeName, out var value2))
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
