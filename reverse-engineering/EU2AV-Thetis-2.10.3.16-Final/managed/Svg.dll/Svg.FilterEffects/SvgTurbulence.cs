#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feTurbulence")]
public class SvgTurbulence : SvgFilterPrimitive
{
	internal static List<Type> SvgTurbulenceClassNames = new List<Type> { typeof(SvgTurbulence) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgTurbulenceProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["baseFrequency"] = new SvgPropertyDescriptor<SvgTurbulence, SvgNumberCollection>(DescriptorType.Property, "baseFrequency", "http://www.w3.org/2000/svg", new SvgNumberCollectionConverter(), (SvgTurbulence t) => t.BaseFrequency, delegate(SvgTurbulence t, SvgNumberCollection v)
		{
			t.BaseFrequency = v;
		}),
		["numOctaves"] = new SvgPropertyDescriptor<SvgTurbulence, int>(DescriptorType.Property, "numOctaves", "http://www.w3.org/2000/svg", new Int32Converter(), (SvgTurbulence t) => t.NumOctaves, delegate(SvgTurbulence t, int v)
		{
			t.NumOctaves = v;
		}),
		["seed"] = new SvgPropertyDescriptor<SvgTurbulence, float>(DescriptorType.Property, "seed", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgTurbulence t) => t.Seed, delegate(SvgTurbulence t, float v)
		{
			t.Seed = v;
		}),
		["stitchTiles"] = new SvgPropertyDescriptor<SvgTurbulence, SvgStitchType>(DescriptorType.Property, "stitchTiles", "http://www.w3.org/2000/svg", new SvgStitchTypeConverter(), (SvgTurbulence t) => t.StitchTiles, delegate(SvgTurbulence t, SvgStitchType v)
		{
			t.StitchTiles = v;
		}),
		["type"] = new SvgPropertyDescriptor<SvgTurbulence, SvgTurbulenceType>(DescriptorType.Property, "type", "http://www.w3.org/2000/svg", new SvgTurbulenceTypeConverter(), (SvgTurbulence t) => t.Type, delegate(SvgTurbulence t, SvgTurbulenceType v)
		{
			t.Type = v;
		})
	};

	[SvgAttribute("baseFrequency")]
	public SvgNumberCollection BaseFrequency
	{
		get
		{
			return GetAttribute("baseFrequency", inherited: false, new SvgNumberCollection { 0f, 0f });
		}
		set
		{
			Attributes["baseFrequency"] = value;
		}
	}

	[SvgAttribute("numOctaves")]
	public int NumOctaves
	{
		get
		{
			return GetAttribute("numOctaves", inherited: false, 1);
		}
		set
		{
			Attributes["numOctaves"] = value;
		}
	}

	[SvgAttribute("seed")]
	public float Seed
	{
		get
		{
			return GetAttribute("seed", inherited: false, 0f);
		}
		set
		{
			Attributes["seed"] = value;
		}
	}

	[SvgAttribute("stitchTiles")]
	public SvgStitchType StitchTiles
	{
		get
		{
			return GetAttribute("stitchTiles", inherited: false, SvgStitchType.NoStitch);
		}
		set
		{
			Attributes["stitchTiles"] = value;
		}
	}

	[SvgAttribute("type")]
	public SvgTurbulenceType Type
	{
		get
		{
			return GetAttribute("type", inherited: false, SvgTurbulenceType.Turbulence);
		}
		set
		{
			Attributes["type"] = value;
		}
	}

	internal override string AttributeName => "feTurbulence";

	internal override List<Type> ClassNames => SvgTurbulenceClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgTurbulenceProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgTurbulence>();
	}

	public override void Process(ImageBuffer buffer)
	{
		buffer[base.Result] = buffer[base.Input];
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgTurbulenceProperty in SvgTurbulenceProperties)
		{
			yield return svgTurbulenceProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgTurbulenceProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgTurbulenceProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgTurbulenceProperties.TryGetValue(attributeName, out var value2))
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
