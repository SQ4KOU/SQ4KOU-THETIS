#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg;

[SvgElement("text")]
public class SvgText : SvgTextBase
{
	internal static List<Type> SvgTextClassNames = new List<Type> { typeof(SvgText) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgTextProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "text";

	internal override List<Type> ClassNames => SvgTextClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgTextProperties;

	public SvgText()
	{
	}

	public SvgText(string text)
	{
		Text = text;
	}

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgText>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgTextProperty in SvgTextProperties)
		{
			yield return svgTextProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgTextProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgTextProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgTextProperties.TryGetValue(attributeName, out var value2))
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
