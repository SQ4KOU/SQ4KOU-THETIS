#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feSpecularLighting")]
public class SvgSpecularLighting : SvgFilterPrimitive
{
	internal static List<Type> SvgSpecularLightingClassNames = new List<Type> { typeof(SvgSpecularLighting) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgSpecularLightingProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["surfaceScale"] = new SvgPropertyDescriptor<SvgSpecularLighting, float>(DescriptorType.Property, "surfaceScale", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgSpecularLighting t) => t.SurfaceScale, delegate(SvgSpecularLighting t, float v)
		{
			t.SurfaceScale = v;
		}),
		["specularConstant"] = new SvgPropertyDescriptor<SvgSpecularLighting, float>(DescriptorType.Property, "specularConstant", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgSpecularLighting t) => t.SpecularConstant, delegate(SvgSpecularLighting t, float v)
		{
			t.SpecularConstant = v;
		}),
		["specularExponent"] = new SvgPropertyDescriptor<SvgSpecularLighting, float>(DescriptorType.Property, "specularExponent", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgSpecularLighting t) => t.SpecularExponent, delegate(SvgSpecularLighting t, float v)
		{
			t.SpecularExponent = v;
		}),
		["kernelUnitLength"] = new SvgPropertyDescriptor<SvgSpecularLighting, SvgNumberCollection>(DescriptorType.Property, "kernelUnitLength", "http://www.w3.org/2000/svg", new SvgNumberCollectionConverter(), (SvgSpecularLighting t) => t.KernelUnitLength, delegate(SvgSpecularLighting t, SvgNumberCollection v)
		{
			t.KernelUnitLength = v;
		}),
		["lighting-color"] = new SvgPropertyDescriptor<SvgSpecularLighting, SvgPaintServer>(DescriptorType.Property, "lighting-color", "http://www.w3.org/2000/svg", new SvgPaintServerFactory(), (SvgSpecularLighting t) => t.LightingColor, delegate(SvgSpecularLighting t, SvgPaintServer v)
		{
			t.LightingColor = v;
		})
	};

	[SvgAttribute("surfaceScale")]
	public float SurfaceScale
	{
		get
		{
			return GetAttribute("surfaceScale", inherited: false, 1f);
		}
		set
		{
			Attributes["surfaceScale"] = value;
		}
	}

	[SvgAttribute("specularConstant")]
	public float SpecularConstant
	{
		get
		{
			return GetAttribute("specularConstant", inherited: false, 1f);
		}
		set
		{
			Attributes["specularConstant"] = value;
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

	[SvgAttribute("kernelUnitLength")]
	public SvgNumberCollection KernelUnitLength
	{
		get
		{
			return GetAttribute("kernelUnitLength", inherited: false, new SvgNumberCollection { 1f, 1f });
		}
		set
		{
			Attributes["kernelUnitLength"] = value;
		}
	}

	[SvgAttribute("lighting-color")]
	[TypeConverter(typeof(SvgPaintServerFactory))]
	public SvgPaintServer LightingColor
	{
		get
		{
			return GetAttribute("lighting-color", true, (SvgPaintServer)new SvgColourServer(System.Drawing.Color.White));
		}
		set
		{
			Attributes["lighting-color"] = value;
		}
	}

	public SvgElement LightSource
	{
		get
		{
			foreach (SvgElement child in Children)
			{
				if (child is SvgDistantLight || child is SvgPointLight || child is SvgSpotLight)
				{
					return child;
				}
			}
			return null;
		}
	}

	internal override string AttributeName => "feSpecularLighting";

	internal override List<Type> ClassNames => SvgSpecularLightingClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgSpecularLightingProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgSpecularLighting>();
	}

	public override void Process(ImageBuffer buffer)
	{
		buffer[base.Result] = buffer[base.Input];
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgSpecularLightingProperty in SvgSpecularLightingProperties)
		{
			yield return svgSpecularLightingProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgSpecularLightingProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgSpecularLightingProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgSpecularLightingProperties.TryGetValue(attributeName, out var value2))
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
