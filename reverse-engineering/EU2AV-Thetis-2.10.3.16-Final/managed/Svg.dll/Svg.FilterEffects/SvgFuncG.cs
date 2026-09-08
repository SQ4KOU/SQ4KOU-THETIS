#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feFuncG")]
public class SvgFuncG : SvgComponentTransferFunction
{
	internal static List<Type> SvgFuncGClassNames = new List<Type> { typeof(SvgFuncG) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgFuncGProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "feFuncG";

	internal override List<Type> ClassNames => SvgFuncGClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgFuncGProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgFuncG>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgFuncGProperty in SvgFuncGProperties)
		{
			yield return svgFuncGProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgFuncGProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgFuncGProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgFuncGProperties.TryGetValue(attributeName, out var value2))
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
