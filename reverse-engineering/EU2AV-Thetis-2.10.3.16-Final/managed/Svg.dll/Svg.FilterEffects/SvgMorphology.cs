#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feMorphology")]
public class SvgMorphology : SvgFilterPrimitive
{
	internal static List<Type> SvgMorphologyClassNames = new List<Type> { typeof(SvgMorphology) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgMorphologyProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["operator"] = new SvgPropertyDescriptor<SvgMorphology, SvgMorphologyOperator>(DescriptorType.Property, "operator", "http://www.w3.org/2000/svg", new SvgMorphologyOperatorConverter(), (SvgMorphology t) => t.Operator, delegate(SvgMorphology t, SvgMorphologyOperator v)
		{
			t.Operator = v;
		}),
		["radius"] = new SvgPropertyDescriptor<SvgMorphology, SvgNumberCollection>(DescriptorType.Property, "radius", "http://www.w3.org/2000/svg", new SvgNumberCollectionConverter(), (SvgMorphology t) => t.Radius, delegate(SvgMorphology t, SvgNumberCollection v)
		{
			t.Radius = v;
		})
	};

	[SvgAttribute("operator")]
	public SvgMorphologyOperator Operator
	{
		get
		{
			return GetAttribute("operator", inherited: false, SvgMorphologyOperator.Erode);
		}
		set
		{
			Attributes["operator"] = value;
		}
	}

	[SvgAttribute("radius")]
	public SvgNumberCollection Radius
	{
		get
		{
			return GetAttribute("radius", inherited: false, new SvgNumberCollection { 0f, 0f });
		}
		set
		{
			Attributes["radius"] = value;
		}
	}

	internal override string AttributeName => "feMorphology";

	internal override List<Type> ClassNames => SvgMorphologyClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgMorphologyProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgMorphology>();
	}

	public override void Process(ImageBuffer buffer)
	{
		buffer[base.Result] = buffer[base.Input];
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgMorphologyProperty in SvgMorphologyProperties)
		{
			yield return svgMorphologyProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgMorphologyProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgMorphologyProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgMorphologyProperties.TryGetValue(attributeName, out var value2))
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
