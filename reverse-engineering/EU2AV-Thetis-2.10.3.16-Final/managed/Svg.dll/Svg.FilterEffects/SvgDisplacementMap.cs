#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feDisplacementMap")]
public class SvgDisplacementMap : SvgFilterPrimitive
{
	internal static List<Type> SvgDisplacementMapClassNames = new List<Type> { typeof(SvgDisplacementMap) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgDisplacementMapProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["scale"] = new SvgPropertyDescriptor<SvgDisplacementMap, float>(DescriptorType.Property, "scale", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgDisplacementMap t) => t.Scale, delegate(SvgDisplacementMap t, float v)
		{
			t.Scale = v;
		}),
		["xChannelSelector"] = new SvgPropertyDescriptor<SvgDisplacementMap, SvgChannelSelector>(DescriptorType.Property, "xChannelSelector", "http://www.w3.org/2000/svg", new SvgChannelSelectorConverter(), (SvgDisplacementMap t) => t.XChannelSelector, delegate(SvgDisplacementMap t, SvgChannelSelector v)
		{
			t.XChannelSelector = v;
		}),
		["yChannelSelector"] = new SvgPropertyDescriptor<SvgDisplacementMap, SvgChannelSelector>(DescriptorType.Property, "yChannelSelector", "http://www.w3.org/2000/svg", new SvgChannelSelectorConverter(), (SvgDisplacementMap t) => t.YChannelSelector, delegate(SvgDisplacementMap t, SvgChannelSelector v)
		{
			t.YChannelSelector = v;
		}),
		["in2"] = new SvgPropertyDescriptor<SvgDisplacementMap, string>(DescriptorType.Property, "in2", "http://www.w3.org/2000/svg", new StringConverter(), (SvgDisplacementMap t) => t.Input2, delegate(SvgDisplacementMap t, string v)
		{
			t.Input2 = v;
		})
	};

	[SvgAttribute("scale")]
	public float Scale
	{
		get
		{
			return GetAttribute("scale", inherited: false, 0f);
		}
		set
		{
			Attributes["scale"] = value;
		}
	}

	[SvgAttribute("xChannelSelector")]
	public SvgChannelSelector XChannelSelector
	{
		get
		{
			return GetAttribute("xChannelSelector", inherited: false, SvgChannelSelector.A);
		}
		set
		{
			Attributes["xChannelSelector"] = value;
		}
	}

	[SvgAttribute("yChannelSelector")]
	public SvgChannelSelector YChannelSelector
	{
		get
		{
			return GetAttribute("yChannelSelector", inherited: false, SvgChannelSelector.A);
		}
		set
		{
			Attributes["yChannelSelector"] = value;
		}
	}

	[SvgAttribute("in2")]
	public string Input2
	{
		get
		{
			return GetAttribute<string>("in2", inherited: false);
		}
		set
		{
			Attributes["in2"] = value;
		}
	}

	internal override string AttributeName => "feDisplacementMap";

	internal override List<Type> ClassNames => SvgDisplacementMapClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgDisplacementMapProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgDisplacementMap>();
	}

	public override void Process(ImageBuffer buffer)
	{
		buffer[base.Result] = buffer[base.Input];
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgDisplacementMapProperty in SvgDisplacementMapProperties)
		{
			yield return svgDisplacementMapProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgDisplacementMapProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgDisplacementMapProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgDisplacementMapProperties.TryGetValue(attributeName, out var value2))
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
