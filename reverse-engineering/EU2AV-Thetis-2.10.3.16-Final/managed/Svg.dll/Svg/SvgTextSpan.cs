#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg;

[SvgElement("tspan")]
public class SvgTextSpan : SvgTextBase
{
	internal static List<Type> SvgTextSpanClassNames = new List<Type> { typeof(SvgTextSpan) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgTextSpanProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "tspan";

	internal override List<Type> ClassNames => SvgTextSpanClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgTextSpanProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgTextSpan>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgTextSpanProperty in SvgTextSpanProperties)
		{
			yield return svgTextSpanProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgTextSpanProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgTextSpanProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgTextSpanProperties.TryGetValue(attributeName, out var value2))
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
