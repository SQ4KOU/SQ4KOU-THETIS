#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feBlend")]
public class SvgBlend : SvgFilterPrimitive
{
	internal static List<Type> SvgBlendClassNames = new List<Type> { typeof(SvgBlend) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgBlendProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["mode"] = new SvgPropertyDescriptor<SvgBlend, SvgBlendMode>(DescriptorType.Property, "mode", "http://www.w3.org/2000/svg", new SvgBlendModeConverter(), (SvgBlend t) => t.Mode, delegate(SvgBlend t, SvgBlendMode v)
		{
			t.Mode = v;
		}),
		["in2"] = new SvgPropertyDescriptor<SvgBlend, string>(DescriptorType.Property, "in2", "http://www.w3.org/2000/svg", new StringConverter(), (SvgBlend t) => t.Input2, delegate(SvgBlend t, string v)
		{
			t.Input2 = v;
		})
	};

	[SvgAttribute("mode")]
	public SvgBlendMode Mode
	{
		get
		{
			return GetAttribute("mode", inherited: false, SvgBlendMode.Normal);
		}
		set
		{
			Attributes["mode"] = value;
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

	internal override string AttributeName => "feBlend";

	internal override List<Type> ClassNames => SvgBlendClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgBlendProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgBlend>();
	}

	public override void Process(ImageBuffer buffer)
	{
		buffer[base.Result] = buffer[base.Input];
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgBlendProperty in SvgBlendProperties)
		{
			yield return svgBlendProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgBlendProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgBlendProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgBlendProperties.TryGetValue(attributeName, out var value2))
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
