#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feMergeNode")]
public class SvgMergeNode : SvgElement
{
	internal static List<Type> SvgMergeNodeClassNames = new List<Type> { typeof(SvgMergeNode) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgMergeNodeProperties = new Dictionary<string, ISvgPropertyDescriptor> { ["in"] = new SvgPropertyDescriptor<SvgMergeNode, string>(DescriptorType.Property, "in", "http://www.w3.org/2000/svg", new StringConverter(), (SvgMergeNode t) => t.Input, delegate(SvgMergeNode t, string v)
	{
		t.Input = v;
	}) };

	[SvgAttribute("in")]
	public string Input
	{
		get
		{
			return GetAttribute<string>("in", inherited: false);
		}
		set
		{
			Attributes["in"] = value;
		}
	}

	internal override string AttributeName => "feMergeNode";

	internal override List<Type> ClassNames => SvgMergeNodeClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgMergeNodeProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgMergeNode>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgMergeNodeProperty in SvgMergeNodeProperties)
		{
			yield return svgMergeNodeProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgMergeNodeProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgMergeNodeProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgMergeNodeProperties.TryGetValue(attributeName, out var value2))
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
