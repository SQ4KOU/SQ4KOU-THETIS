#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feComponentTransfer")]
public class SvgComponentTransfer : SvgFilterPrimitive
{
	internal static List<Type> SvgComponentTransferClassNames = new List<Type> { typeof(SvgComponentTransfer) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgComponentTransferProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "feComponentTransfer";

	internal override List<Type> ClassNames => SvgComponentTransferClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgComponentTransferProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgComponentTransfer>();
	}

	public override void Process(ImageBuffer buffer)
	{
		buffer[base.Result] = buffer[base.Input];
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgComponentTransferProperty in SvgComponentTransferProperties)
		{
			yield return svgComponentTransferProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgComponentTransferProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgComponentTransferProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgComponentTransferProperties.TryGetValue(attributeName, out var value2))
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
