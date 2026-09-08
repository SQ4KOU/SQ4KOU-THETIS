#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg;

[DefaultProperty("Text")]
[SvgElement("desc")]
public class SvgDescription : SvgElement, ISvgDescriptiveElement
{
	internal static List<Type> SvgDescriptionClassNames = new List<Type> { typeof(SvgDescription) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgDescriptionProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "desc";

	internal override List<Type> ClassNames => SvgDescriptionClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgDescriptionProperties;

	public override string ToString()
	{
		return Content;
	}

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgDescription>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgDescriptionProperty in SvgDescriptionProperties)
		{
			yield return svgDescriptionProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgDescriptionProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgDescriptionProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgDescriptionProperties.TryGetValue(attributeName, out var value2))
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
