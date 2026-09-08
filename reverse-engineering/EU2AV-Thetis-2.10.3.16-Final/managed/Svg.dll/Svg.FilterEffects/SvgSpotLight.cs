#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feSpotLight")]
public class SvgSpotLight : SvgElement
{
	internal static List<Type> SvgSpotLightClassNames = new List<Type> { typeof(SvgSpotLight) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgSpotLightProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["x"] = new SvgPropertyDescriptor<SvgSpotLight, float>(DescriptorType.Property, "x", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgSpotLight t) => t.X, delegate(SvgSpotLight t, float v)
		{
			t.X = v;
		}),
		["y"] = new SvgPropertyDescriptor<SvgSpotLight, float>(DescriptorType.Property, "y", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgSpotLight t) => t.Y, delegate(SvgSpotLight t, float v)
		{
			t.Y = v;
		}),
		["z"] = new SvgPropertyDescriptor<SvgSpotLight, float>(DescriptorType.Property, "z", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgSpotLight t) => t.Z, delegate(SvgSpotLight t, float v)
		{
			t.Z = v;
		}),
		["pointsAtX"] = new SvgPropertyDescriptor<SvgSpotLight, float>(DescriptorType.Property, "pointsAtX", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgSpotLight t) => t.PointsAtX, delegate(SvgSpotLight t, float v)
		{
			t.PointsAtX = v;
		}),
		["pointsAtY"] = new SvgPropertyDescriptor<SvgSpotLight, float>(DescriptorType.Property, "pointsAtY", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgSpotLight t) => t.PointsAtY, delegate(SvgSpotLight t, float v)
		{
			t.PointsAtY = v;
		}),
		["pointsAtZ"] = new SvgPropertyDescriptor<SvgSpotLight, float>(DescriptorType.Property, "pointsAtZ", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgSpotLight t) => t.PointsAtZ, delegate(SvgSpotLight t, float v)
		{
			t.PointsAtZ = v;
		}),
		["specularExponent"] = new SvgPropertyDescriptor<SvgSpotLight, float>(DescriptorType.Property, "specularExponent", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgSpotLight t) => t.SpecularExponent, delegate(SvgSpotLight t, float v)
		{
			t.SpecularExponent = v;
		}),
		["limitingConeAngle"] = new SvgPropertyDescriptor<SvgSpotLight, float>(DescriptorType.Property, "limitingConeAngle", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgSpotLight t) => t.LimitingConeAngle, delegate(SvgSpotLight t, float v)
		{
			t.LimitingConeAngle = v;
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

	[SvgAttribute("pointsAtX")]
	public float PointsAtX
	{
		get
		{
			return GetAttribute("pointsAtX", inherited: false, 0f);
		}
		set
		{
			Attributes["pointsAtX"] = value;
		}
	}

	[SvgAttribute("pointsAtY")]
	public float PointsAtY
	{
		get
		{
			return GetAttribute("pointsAtY", inherited: false, 0f);
		}
		set
		{
			Attributes["pointsAtY"] = value;
		}
	}

	[SvgAttribute("pointsAtZ")]
	public float PointsAtZ
	{
		get
		{
			return GetAttribute("pointsAtZ", inherited: false, 0f);
		}
		set
		{
			Attributes["pointsAtZ"] = value;
		}
	}

	[SvgAttribute("specularExponent")]
	public float SpecularExponent
	{
		get
		{
			return GetAttribute("specularExponent", inherited: false, 1f);
		}
		set
		{
			Attributes["specularExponent"] = value;
		}
	}

	[SvgAttribute("limitingConeAngle")]
	public float LimitingConeAngle
	{
		get
		{
			return GetAttribute("limitingConeAngle", inherited: false, float.NaN);
		}
		set
		{
			Attributes["limitingConeAngle"] = value;
		}
	}

	internal override string AttributeName => "feSpotLight";

	internal override List<Type> ClassNames => SvgSpotLightClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgSpotLightProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgSpotLight>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgSpotLightProperty in SvgSpotLightProperties)
		{
			yield return svgSpotLightProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgSpotLightProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgSpotLightProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgSpotLightProperties.TryGetValue(attributeName, out var value2))
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
