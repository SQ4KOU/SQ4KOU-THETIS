#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feFlood")]
public class SvgFlood : SvgFilterPrimitive
{
	internal static List<Type> SvgFloodClassNames = new List<Type> { typeof(SvgFlood) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgFloodProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["flood-color"] = new SvgPropertyDescriptor<SvgFlood, SvgPaintServer>(DescriptorType.Property, "flood-color", "http://www.w3.org/2000/svg", new SvgPaintServerFactory(), (SvgFlood t) => t.FloodColor, delegate(SvgFlood t, SvgPaintServer v)
		{
			t.FloodColor = v;
		}),
		["flood-opacity"] = new SvgPropertyDescriptor<SvgFlood, float>(DescriptorType.Property, "flood-opacity", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgFlood t) => t.FloodOpacity, delegate(SvgFlood t, float v)
		{
			t.FloodOpacity = v;
		})
	};

	[SvgAttribute("flood-color")]
	public virtual SvgPaintServer FloodColor
	{
		get
		{
			return GetAttribute("flood-color", inherited: true, SvgPaintServer.NotSet);
		}
		set
		{
			Attributes["flood-color"] = value;
		}
	}

	[SvgAttribute("flood-opacity")]
	public virtual float FloodOpacity
	{
		get
		{
			return GetAttribute("flood-opacity", inherited: true, 1f);
		}
		set
		{
			Attributes["flood-opacity"] = SvgElement.FixOpacityValue(value);
		}
	}

	internal override string AttributeName => "feFlood";

	internal override List<Type> ClassNames => SvgFloodClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgFloodProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgFlood>();
	}

	public override void Process(ImageBuffer buffer)
	{
		buffer[base.Result] = buffer[base.Input];
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgFloodProperty in SvgFloodProperties)
		{
			yield return svgFloodProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgFloodProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgFloodProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgFloodProperties.TryGetValue(attributeName, out var value2))
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
