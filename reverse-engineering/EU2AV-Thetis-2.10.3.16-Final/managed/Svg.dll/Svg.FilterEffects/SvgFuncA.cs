#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feFuncA")]
public class SvgFuncA : SvgComponentTransferFunction
{
	internal static List<Type> SvgFuncAClassNames = new List<Type> { typeof(SvgFuncA) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgFuncAProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "feFuncA";

	internal override List<Type> ClassNames => SvgFuncAClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgFuncAProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgFuncA>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgFuncAProperty in SvgFuncAProperties)
		{
			yield return svgFuncAProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgFuncAProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgFuncAProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgFuncAProperties.TryGetValue(attributeName, out var value2))
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
