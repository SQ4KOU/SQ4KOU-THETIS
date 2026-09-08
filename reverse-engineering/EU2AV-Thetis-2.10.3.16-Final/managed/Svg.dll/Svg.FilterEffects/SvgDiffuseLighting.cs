#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feDiffuseLighting")]
public class SvgDiffuseLighting : SvgFilterPrimitive
{
	internal static List<Type> SvgDiffuseLightingClassNames = new List<Type> { typeof(SvgDiffuseLighting) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgDiffuseLightingProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["surfaceScale"] = new SvgPropertyDescriptor<SvgDiffuseLighting, float>(DescriptorType.Property, "surfaceScale", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgDiffuseLighting t) => t.SurfaceScale, delegate(SvgDiffuseLighting t, float v)
		{
			t.SurfaceScale = v;
		}),
		["diffuseConstant"] = new SvgPropertyDescriptor<SvgDiffuseLighting, float>(DescriptorType.Property, "diffuseConstant", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgDiffuseLighting t) => t.DiffuseConstant, delegate(SvgDiffuseLighting t, float v)
		{
			t.DiffuseConstant = v;
		}),
		["kernelUnitLength"] = new SvgPropertyDescriptor<SvgDiffuseLighting, SvgNumberCollection>(DescriptorType.Property, "kernelUnitLength", "http://www.w3.org/2000/svg", new SvgNumberCollectionConverter(), (SvgDiffuseLighting t) => t.KernelUnitLength, delegate(SvgDiffuseLighting t, SvgNumberCollection v)
		{
			t.KernelUnitLength = v;
		}),
		["lighting-color"] = new SvgPropertyDescriptor<SvgDiffuseLighting, SvgPaintServer>(DescriptorType.Property, "lighting-color", "http://www.w3.org/2000/svg", new SvgPaintServerFactory(), (SvgDiffuseLighting t) => t.LightingColor, delegate(SvgDiffuseLighting t, SvgPaintServer v)
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

	[SvgAttribute("diffuseConstant")]
	public float DiffuseConstant
	{
		get
		{
			return GetAttribute("diffuseConstant", inherited: false, 1f);
		}
		set
		{
			Attributes["diffuseConstant"] = value;
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

	internal override string AttributeName => "feDiffuseLighting";

	internal override List<Type> ClassNames => SvgDiffuseLightingClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgDiffuseLightingProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgDiffuseLighting>();
	}

	public override void Process(ImageBuffer buffer)
	{
		buffer[base.Result] = buffer[base.Input];
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgDiffuseLightingProperty in SvgDiffuseLightingProperties)
		{
			yield return svgDiffuseLightingProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgDiffuseLightingProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgDiffuseLightingProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgDiffuseLightingProperties.TryGetValue(attributeName, out var value2))
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
