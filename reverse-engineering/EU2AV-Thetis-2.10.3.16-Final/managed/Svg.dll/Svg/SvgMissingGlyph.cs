#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg;

[SvgElement("missing-glyph")]
public class SvgMissingGlyph : SvgGlyph
{
	internal static List<Type> SvgMissingGlyphClassNames = new List<Type> { typeof(SvgMissingGlyph) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgMissingGlyphProperties = new Dictionary<string, ISvgPropertyDescriptor> { ["glyph-name"] = new SvgPropertyDescriptor<SvgMissingGlyph, string>(DescriptorType.Property, "glyph-name", "http://www.w3.org/2000/svg", new StringConverter(), (SvgMissingGlyph t) => t.GlyphName, delegate(SvgMissingGlyph t, string v)
	{
		t.GlyphName = v;
	}) };

	[SvgAttribute("glyph-name")]
	public override string GlyphName
	{
		get
		{
			return GetAttribute("glyph-name", inherited: true, "__MISSING_GLYPH__");
		}
		set
		{
			Attributes["glyph-name"] = value;
		}
	}

	internal override string AttributeName => "missing-glyph";

	internal override List<Type> ClassNames => SvgMissingGlyphClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgMissingGlyphProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgMissingGlyph>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgMissingGlyphProperty in SvgMissingGlyphProperties)
		{
			yield return svgMissingGlyphProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgMissingGlyphProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgMissingGlyphProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgMissingGlyphProperties.TryGetValue(attributeName, out var value2))
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
