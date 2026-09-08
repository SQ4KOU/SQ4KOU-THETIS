#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feTile")]
public class SvgTile : SvgFilterPrimitive
{
	internal static List<Type> SvgTileClassNames = new List<Type> { typeof(SvgTile) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgTileProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "feTile";

	internal override List<Type> ClassNames => SvgTileClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgTileProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgTile>();
	}

	public override void Process(ImageBuffer buffer)
	{
		buffer[base.Result] = buffer[base.Input];
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgTileProperty in SvgTileProperties)
		{
			yield return svgTileProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgTileProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgTileProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgTileProperties.TryGetValue(attributeName, out var value2))
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
