#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feFuncR")]
public class SvgFuncR : SvgComponentTransferFunction
{
	internal static List<Type> SvgFuncRClassNames = new List<Type> { typeof(SvgFuncR) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgFuncRProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "feFuncR";

	internal override List<Type> ClassNames => SvgFuncRClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgFuncRProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgFuncR>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgFuncRProperty in SvgFuncRProperties)
		{
			yield return svgFuncRProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgFuncRProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgFuncRProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgFuncRProperties.TryGetValue(attributeName, out var value2))
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
