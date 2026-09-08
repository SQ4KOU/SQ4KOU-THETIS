#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg;

public class NonSvgElement : SvgElement
{
	internal static List<Type> NonSvgElementClassNames = new List<Type> { typeof(NonSvgElement) };

	internal static Dictionary<string, ISvgPropertyDescriptor> NonSvgElementProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	public string Name => base.ElementName;

	internal override string AttributeName => "";

	internal override List<Type> ClassNames => NonSvgElementClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => NonSvgElementProperties;

	public NonSvgElement()
	{
	}

	public NonSvgElement(string elementName, string elementNamespace)
	{
		base.ElementName = elementName;
		base.ElementNamespace = elementNamespace;
	}

	public override SvgElement DeepCopy()
	{
		return DeepCopy<NonSvgElement>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> nonSvgElementProperty in NonSvgElementProperties)
		{
			yield return nonSvgElementProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!NonSvgElementProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (NonSvgElementProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (NonSvgElementProperties.TryGetValue(attributeName, out var value2))
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
