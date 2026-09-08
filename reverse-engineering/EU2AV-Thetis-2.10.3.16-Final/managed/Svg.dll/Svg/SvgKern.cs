#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg;

public abstract class SvgKern : SvgElement
{
	internal static List<Type> SvgKernClassNames = new List<Type> { typeof(SvgKern) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgKernProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["g1"] = new SvgPropertyDescriptor<SvgKern, string>(DescriptorType.Property, "g1", "http://www.w3.org/2000/svg", new StringConverter(), (SvgKern t) => t.Glyph1, delegate(SvgKern t, string v)
		{
			t.Glyph1 = v;
		}),
		["g2"] = new SvgPropertyDescriptor<SvgKern, string>(DescriptorType.Property, "g2", "http://www.w3.org/2000/svg", new StringConverter(), (SvgKern t) => t.Glyph2, delegate(SvgKern t, string v)
		{
			t.Glyph2 = v;
		}),
		["u1"] = new SvgPropertyDescriptor<SvgKern, string>(DescriptorType.Property, "u1", "http://www.w3.org/2000/svg", new StringConverter(), (SvgKern t) => t.Unicode1, delegate(SvgKern t, string v)
		{
			t.Unicode1 = v;
		}),
		["u2"] = new SvgPropertyDescriptor<SvgKern, string>(DescriptorType.Property, "u2", "http://www.w3.org/2000/svg", new StringConverter(), (SvgKern t) => t.Unicode2, delegate(SvgKern t, string v)
		{
			t.Unicode2 = v;
		}),
		["k"] = new SvgPropertyDescriptor<SvgKern, float>(DescriptorType.Property, "k", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgKern t) => t.Kerning, delegate(SvgKern t, float v)
		{
			t.Kerning = v;
		})
	};

	[SvgAttribute("g1")]
	public string Glyph1
	{
		get
		{
			return GetAttribute<string>("g1", inherited: true);
		}
		set
		{
			Attributes["g1"] = value;
		}
	}

	[SvgAttribute("g2")]
	public string Glyph2
	{
		get
		{
			return GetAttribute<string>("g2", inherited: true);
		}
		set
		{
			Attributes["g2"] = value;
		}
	}

	[SvgAttribute("u1")]
	public string Unicode1
	{
		get
		{
			return GetAttribute<string>("u1", inherited: true);
		}
		set
		{
			Attributes["u1"] = value;
		}
	}

	[SvgAttribute("u2")]
	public string Unicode2
	{
		get
		{
			return GetAttribute<string>("u2", inherited: true);
		}
		set
		{
			Attributes["u2"] = value;
		}
	}

	[SvgAttribute("k")]
	public float Kerning
	{
		get
		{
			return GetAttribute("k", inherited: true, 0f);
		}
		set
		{
			Attributes["k"] = value;
		}
	}

	internal override string AttributeName => "";

	internal override List<Type> ClassNames => SvgKernClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgKernProperties;

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgKernProperty in SvgKernProperties)
		{
			yield return svgKernProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgKernProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgKernProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgKernProperties.TryGetValue(attributeName, out var value2))
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
