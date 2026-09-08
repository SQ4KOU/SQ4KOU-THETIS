#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg;

[SvgElement("title")]
public class SvgTitle : SvgElement, ISvgDescriptiveElement
{
	internal static List<Type> SvgTitleClassNames = new List<Type> { typeof(SvgTitle) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgTitleProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "title";

	internal override List<Type> ClassNames => SvgTitleClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgTitleProperties;

	public override string ToString()
	{
		return Content;
	}

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgTitle>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgTitleProperty in SvgTitleProperties)
		{
			yield return svgTitleProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgTitleProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgTitleProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgTitleProperties.TryGetValue(attributeName, out var value2))
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
