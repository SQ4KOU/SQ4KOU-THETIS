#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg;

[SvgElement("font-face")]
public class SvgFontFace : SvgElement
{
	internal static List<Type> SvgFontFaceClassNames = new List<Type> { typeof(SvgFontFace) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgFontFaceProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["alphabetic"] = new SvgPropertyDescriptor<SvgFontFace, float>(DescriptorType.Property, "alphabetic", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgFontFace t) => t.Alphabetic, delegate(SvgFontFace t, float v)
		{
			t.Alphabetic = v;
		}),
		["ascent"] = new SvgPropertyDescriptor<SvgFontFace, float>(DescriptorType.Property, "ascent", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgFontFace t) => t.Ascent, delegate(SvgFontFace t, float v)
		{
			t.Ascent = v;
		}),
		["ascent-height"] = new SvgPropertyDescriptor<SvgFontFace, float>(DescriptorType.Property, "ascent-height", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgFontFace t) => t.AscentHeight, delegate(SvgFontFace t, float v)
		{
			t.AscentHeight = v;
		}),
		["descent"] = new SvgPropertyDescriptor<SvgFontFace, float>(DescriptorType.Property, "descent", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgFontFace t) => t.Descent, delegate(SvgFontFace t, float v)
		{
			t.Descent = v;
		}),
		["panose-1"] = new SvgPropertyDescriptor<SvgFontFace, string>(DescriptorType.Property, "panose-1", "http://www.w3.org/2000/svg", new StringConverter(), (SvgFontFace t) => t.Panose1, delegate(SvgFontFace t, string v)
		{
			t.Panose1 = v;
		}),
		["units-per-em"] = new SvgPropertyDescriptor<SvgFontFace, float>(DescriptorType.Property, "units-per-em", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgFontFace t) => t.UnitsPerEm, delegate(SvgFontFace t, float v)
		{
			t.UnitsPerEm = v;
		}),
		["x-height"] = new SvgPropertyDescriptor<SvgFontFace, float>(DescriptorType.Property, "x-height", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgFontFace t) => t.XHeight, delegate(SvgFontFace t, float v)
		{
			t.XHeight = v;
		})
	};

	[SvgAttribute("alphabetic")]
	public float Alphabetic
	{
		get
		{
			return GetAttribute("alphabetic", inherited: true, 0f);
		}
		set
		{
			Attributes["alphabetic"] = value;
		}
	}

	[SvgAttribute("ascent")]
	public float Ascent
	{
		get
		{
			return GetAttribute("ascent", inherited: true, (Parent is SvgFont) ? (UnitsPerEm - ((SvgFont)Parent).VertOriginY) : 0f);
		}
		set
		{
			Attributes["ascent"] = value;
		}
	}

	[SvgAttribute("ascent-height")]
	public float AscentHeight
	{
		get
		{
			return GetAttribute("ascent-height", inherited: true, Ascent);
		}
		set
		{
			Attributes["ascent-height"] = value;
		}
	}

	[SvgAttribute("descent")]
	public float Descent
	{
		get
		{
			return GetAttribute("descent", inherited: true, (Parent is SvgFont) ? ((SvgFont)Parent).VertOriginY : 0f);
		}
		set
		{
			Attributes["descent"] = value;
		}
	}

	[SvgAttribute("panose-1")]
	public string Panose1
	{
		get
		{
			return GetAttribute<string>("panose-1", inherited: true);
		}
		set
		{
			Attributes["panose-1"] = value;
		}
	}

	[SvgAttribute("units-per-em")]
	public float UnitsPerEm
	{
		get
		{
			return GetAttribute("units-per-em", inherited: true, 1000f);
		}
		set
		{
			Attributes["units-per-em"] = value;
		}
	}

	[SvgAttribute("x-height")]
	public float XHeight
	{
		get
		{
			return GetAttribute("x-height", inherited: true, float.MinValue);
		}
		set
		{
			Attributes["x-height"] = value;
		}
	}

	internal override string AttributeName => "font-face";

	internal override List<Type> ClassNames => SvgFontFaceClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgFontFaceProperties;

	public override SvgElement DeepCopy()
	{
		return base.DeepCopy<SvgFontFace>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgFontFaceProperty in SvgFontFaceProperties)
		{
			yield return svgFontFaceProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgFontFaceProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgFontFaceProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgFontFaceProperties.TryGetValue(attributeName, out var value2))
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
