#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feDistantLight")]
public class SvgDistantLight : SvgElement
{
	internal static List<Type> SvgDistantLightClassNames = new List<Type> { typeof(SvgDistantLight) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgDistantLightProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["azimuth"] = new SvgPropertyDescriptor<SvgDistantLight, float>(DescriptorType.Property, "azimuth", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgDistantLight t) => t.Azimuth, delegate(SvgDistantLight t, float v)
		{
			t.Azimuth = v;
		}),
		["elevation"] = new SvgPropertyDescriptor<SvgDistantLight, float>(DescriptorType.Property, "elevation", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgDistantLight t) => t.Elevation, delegate(SvgDistantLight t, float v)
		{
			t.Elevation = v;
		})
	};

	[SvgAttribute("azimuth")]
	public float Azimuth
	{
		get
		{
			return GetAttribute("azimuth", inherited: false, 0f);
		}
		set
		{
			Attributes["azimuth"] = value;
		}
	}

	[SvgAttribute("elevation")]
	public float Elevation
	{
		get
		{
			return GetAttribute("elevation", inherited: false, 0f);
		}
		set
		{
			Attributes["elevation"] = value;
		}
	}

	internal override string AttributeName => "feDistantLight";

	internal override List<Type> ClassNames => SvgDistantLightClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgDistantLightProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgDistantLight>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgDistantLightProperty in SvgDistantLightProperties)
		{
			yield return svgDistantLightProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgDistantLightProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgDistantLightProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgDistantLightProperties.TryGetValue(attributeName, out var value2))
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
