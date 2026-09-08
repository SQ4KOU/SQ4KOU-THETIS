#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("fePointLight")]
public class SvgPointLight : SvgElement
{
	internal static List<Type> SvgPointLightClassNames = new List<Type> { typeof(SvgPointLight) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgPointLightProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["x"] = new SvgPropertyDescriptor<SvgPointLight, float>(DescriptorType.Property, "x", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgPointLight t) => t.X, delegate(SvgPointLight t, float v)
		{
			t.X = v;
		}),
		["y"] = new SvgPropertyDescriptor<SvgPointLight, float>(DescriptorType.Property, "y", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgPointLight t) => t.Y, delegate(SvgPointLight t, float v)
		{
			t.Y = v;
		}),
		["z"] = new SvgPropertyDescriptor<SvgPointLight, float>(DescriptorType.Property, "z", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgPointLight t) => t.Z, delegate(SvgPointLight t, float v)
		{
			t.Z = v;
		})
	};

	[SvgAttribute("x")]
	public float X
	{
		get
		{
			return GetAttribute("x", inherited: false, 0f);
		}
		set
		{
			Attributes["x"] = value;
		}
	}

	[SvgAttribute("y")]
	public float Y
	{
		get
		{
			return GetAttribute("y", inherited: false, 0f);
		}
		set
		{
			Attributes["y"] = value;
		}
	}

	[SvgAttribute("z")]
	public float Z
	{
		get
		{
			return GetAttribute("z", inherited: false, 0f);
		}
		set
		{
			Attributes["z"] = value;
		}
	}

	internal override string AttributeName => "fePointLight";

	internal override List<Type> ClassNames => SvgPointLightClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgPointLightProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgPointLight>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgPointLightProperty in SvgPointLightProperties)
		{
			yield return svgPointLightProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgPointLightProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgPointLightProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgPointLightProperties.TryGetValue(attributeName, out var value2))
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
