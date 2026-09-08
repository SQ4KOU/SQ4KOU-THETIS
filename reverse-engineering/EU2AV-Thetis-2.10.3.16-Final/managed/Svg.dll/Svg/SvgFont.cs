#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Svg;

[SvgElement("font")]
public class SvgFont : SvgElement
{
	internal static List<Type> SvgFontClassNames = new List<Type> { typeof(SvgFont) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgFontProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["horiz-adv-x"] = new SvgPropertyDescriptor<SvgFont, float>(DescriptorType.Property, "horiz-adv-x", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgFont t) => t.HorizAdvX, delegate(SvgFont t, float v)
		{
			t.HorizAdvX = v;
		}),
		["horiz-origin-x"] = new SvgPropertyDescriptor<SvgFont, float>(DescriptorType.Property, "horiz-origin-x", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgFont t) => t.HorizOriginX, delegate(SvgFont t, float v)
		{
			t.HorizOriginX = v;
		}),
		["horiz-origin-y"] = new SvgPropertyDescriptor<SvgFont, float>(DescriptorType.Property, "horiz-origin-y", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgFont t) => t.HorizOriginY, delegate(SvgFont t, float v)
		{
			t.HorizOriginY = v;
		}),
		["vert-adv-y"] = new SvgPropertyDescriptor<SvgFont, float>(DescriptorType.Property, "vert-adv-y", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgFont t) => t.VertAdvY, delegate(SvgFont t, float v)
		{
			t.VertAdvY = v;
		}),
		["vert-origin-x"] = new SvgPropertyDescriptor<SvgFont, float>(DescriptorType.Property, "vert-origin-x", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgFont t) => t.VertOriginX, delegate(SvgFont t, float v)
		{
			t.VertOriginX = v;
		}),
		["vert-origin-y"] = new SvgPropertyDescriptor<SvgFont, float>(DescriptorType.Property, "vert-origin-y", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgFont t) => t.VertOriginY, delegate(SvgFont t, float v)
		{
			t.VertOriginY = v;
		})
	};

	[SvgAttribute("horiz-adv-x")]
	public float HorizAdvX
	{
		get
		{
			return GetAttribute("horiz-adv-x", inherited: true, 0f);
		}
		set
		{
			Attributes["horiz-adv-x"] = value;
		}
	}

	[SvgAttribute("horiz-origin-x")]
	public float HorizOriginX
	{
		get
		{
			return GetAttribute("horiz-origin-x", inherited: true, 0f);
		}
		set
		{
			Attributes["horiz-origin-x"] = value;
		}
	}

	[SvgAttribute("horiz-origin-y")]
	public float HorizOriginY
	{
		get
		{
			return GetAttribute("horiz-origin-y", inherited: true, 0f);
		}
		set
		{
			Attributes["horiz-origin-y"] = value;
		}
	}

	[SvgAttribute("vert-adv-y")]
	public float VertAdvY
	{
		get
		{
			return GetAttribute("vert-adv-y", inherited: true, Children.OfType<SvgFontFace>().First().UnitsPerEm);
		}
		set
		{
			Attributes["vert-adv-y"] = value;
		}
	}

	[SvgAttribute("vert-origin-x")]
	public float VertOriginX
	{
		get
		{
			return GetAttribute("vert-origin-x", inherited: true, HorizAdvX / 2f);
		}
		set
		{
			Attributes["vert-origin-x"] = value;
		}
	}

	[SvgAttribute("vert-origin-y")]
	public float VertOriginY
	{
		get
		{
			float valueOrDefault = (Children.OfType<SvgFontFace>().First().Attributes["ascent"] as float?).GetValueOrDefault();
			return GetAttribute("vert-origin-y", inherited: true, valueOrDefault);
		}
		set
		{
			Attributes["vert-origin-y"] = value;
		}
	}

	internal override string AttributeName => "font";

	internal override List<Type> ClassNames => SvgFontClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgFontProperties;

	public override SvgElement DeepCopy()
	{
		return base.DeepCopy<SvgFont>();
	}

	protected override void Render(ISvgRenderer renderer)
	{
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgFontProperty in SvgFontProperties)
		{
			yield return svgFontProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgFontProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgFontProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgFontProperties.TryGetValue(attributeName, out var value2))
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
