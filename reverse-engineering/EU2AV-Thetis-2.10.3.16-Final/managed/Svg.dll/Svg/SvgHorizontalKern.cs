#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg;

[SvgElement("hkern")]
public class SvgHorizontalKern : SvgKern
{
	internal static List<Type> SvgHorizontalKernClassNames = new List<Type> { typeof(SvgHorizontalKern) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgHorizontalKernProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "hkern";

	internal override List<Type> ClassNames => SvgHorizontalKernClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgHorizontalKernProperties;

	public override SvgElement DeepCopy()
	{
		return base.DeepCopy<SvgHorizontalKern>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgHorizontalKernProperty in SvgHorizontalKernProperties)
		{
			yield return svgHorizontalKernProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgHorizontalKernProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgHorizontalKernProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgHorizontalKernProperties.TryGetValue(attributeName, out var value2))
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
