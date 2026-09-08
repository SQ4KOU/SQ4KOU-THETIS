#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg;

[SvgElement("vkern")]
public class SvgVerticalKern : SvgKern
{
	internal static List<Type> SvgVerticalKernClassNames = new List<Type> { typeof(SvgVerticalKern) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgVerticalKernProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "vkern";

	internal override List<Type> ClassNames => SvgVerticalKernClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgVerticalKernProperties;

	public override SvgElement DeepCopy()
	{
		return base.DeepCopy<SvgVerticalKern>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgVerticalKernProperty in SvgVerticalKernProperties)
		{
			yield return svgVerticalKernProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgVerticalKernProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgVerticalKernProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgVerticalKernProperties.TryGetValue(attributeName, out var value2))
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
