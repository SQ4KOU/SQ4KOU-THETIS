#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg;

public class SvgUnknownElement : SvgElement
{
	internal static List<Type> SvgUnknownElementClassNames = new List<Type> { typeof(SvgUnknownElement) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgUnknownElementProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "";

	internal override List<Type> ClassNames => SvgUnknownElementClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgUnknownElementProperties;

	public SvgUnknownElement()
	{
	}

	public SvgUnknownElement(string elementName)
	{
		base.ElementName = elementName;
	}

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgUnknownElement>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgUnknownElementProperty in SvgUnknownElementProperties)
		{
			yield return svgUnknownElementProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgUnknownElementProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgUnknownElementProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgUnknownElementProperties.TryGetValue(attributeName, out var value2))
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
