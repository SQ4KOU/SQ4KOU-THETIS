#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feComposite")]
public class SvgComposite : SvgFilterPrimitive
{
	internal static List<Type> SvgCompositeClassNames = new List<Type> { typeof(SvgComposite) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgCompositeProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["operator"] = new SvgPropertyDescriptor<SvgComposite, SvgCompositeOperator>(DescriptorType.Property, "operator", "http://www.w3.org/2000/svg", new SvgCompositeOperatorConverter(), (SvgComposite t) => t.Operator, delegate(SvgComposite t, SvgCompositeOperator v)
		{
			t.Operator = v;
		}),
		["k1"] = new SvgPropertyDescriptor<SvgComposite, float>(DescriptorType.Property, "k1", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgComposite t) => t.K1, delegate(SvgComposite t, float v)
		{
			t.K1 = v;
		}),
		["k2"] = new SvgPropertyDescriptor<SvgComposite, float>(DescriptorType.Property, "k2", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgComposite t) => t.K2, delegate(SvgComposite t, float v)
		{
			t.K2 = v;
		}),
		["k3"] = new SvgPropertyDescriptor<SvgComposite, float>(DescriptorType.Property, "k3", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgComposite t) => t.K3, delegate(SvgComposite t, float v)
		{
			t.K3 = v;
		}),
		["k4"] = new SvgPropertyDescriptor<SvgComposite, float>(DescriptorType.Property, "k4", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgComposite t) => t.K4, delegate(SvgComposite t, float v)
		{
			t.K4 = v;
		}),
		["in2"] = new SvgPropertyDescriptor<SvgComposite, string>(DescriptorType.Property, "in2", "http://www.w3.org/2000/svg", new StringConverter(), (SvgComposite t) => t.Input2, delegate(SvgComposite t, string v)
		{
			t.Input2 = v;
		})
	};

	[SvgAttribute("operator")]
	public SvgCompositeOperator Operator
	{
		get
		{
			return GetAttribute("operator", inherited: false, SvgCompositeOperator.Over);
		}
		set
		{
			Attributes["operator"] = value;
		}
	}

	[SvgAttribute("k1")]
	public float K1
	{
		get
		{
			return GetAttribute("k1", inherited: false, 0f);
		}
		set
		{
			Attributes["k1"] = value;
		}
	}

	[SvgAttribute("k2")]
	public float K2
	{
		get
		{
			return GetAttribute("k2", inherited: false, 0f);
		}
		set
		{
			Attributes["k2"] = value;
		}
	}

	[SvgAttribute("k3")]
	public float K3
	{
		get
		{
			return GetAttribute("k3", inherited: false, 0f);
		}
		set
		{
			Attributes["k3"] = value;
		}
	}

	[SvgAttribute("k4")]
	public float K4
	{
		get
		{
			return GetAttribute("k4", inherited: false, 0f);
		}
		set
		{
			Attributes["k4"] = value;
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

	internal override string AttributeName => "feComposite";

	internal override List<Type> ClassNames => SvgCompositeClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgCompositeProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgComposite>();
	}

	public override void Process(ImageBuffer buffer)
	{
		buffer[base.Result] = buffer[base.Input];
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgCompositeProperty in SvgCompositeProperties)
		{
			yield return svgCompositeProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgCompositeProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgCompositeProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgCompositeProperties.TryGetValue(attributeName, out var value2))
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
