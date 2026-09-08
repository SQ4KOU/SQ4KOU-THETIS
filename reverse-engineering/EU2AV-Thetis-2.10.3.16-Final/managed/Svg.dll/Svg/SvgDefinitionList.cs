#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg;

[SvgElement("defs")]
public class SvgDefinitionList : SvgElement
{
	internal static List<Type> SvgDefinitionListClassNames = new List<Type> { typeof(SvgDefinitionList) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgDefinitionListProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "defs";

	internal override List<Type> ClassNames => SvgDefinitionListClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgDefinitionListProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgDefinitionList>();
	}

	protected override void Render(ISvgRenderer renderer)
	{
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgDefinitionListProperty in SvgDefinitionListProperties)
		{
			yield return svgDefinitionListProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgDefinitionListProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgDefinitionListProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgDefinitionListProperties.TryGetValue(attributeName, out var value2))
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
