#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feFuncB")]
public class SvgFuncB : SvgComponentTransferFunction
{
	internal static List<Type> SvgFuncBClassNames = new List<Type> { typeof(SvgFuncB) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgFuncBProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "feFuncB";

	internal override List<Type> ClassNames => SvgFuncBClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgFuncBProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgFuncB>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgFuncBProperty in SvgFuncBProperties)
		{
			yield return svgFuncBProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgFuncBProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgFuncBProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgFuncBProperties.TryGetValue(attributeName, out var value2))
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
