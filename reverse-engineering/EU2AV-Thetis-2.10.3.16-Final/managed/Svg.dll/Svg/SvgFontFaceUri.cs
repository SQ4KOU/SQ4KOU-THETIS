#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg;

[SvgElement("font-face-uri")]
public class SvgFontFaceUri : SvgElement
{
	internal static List<Type> SvgFontFaceUriClassNames = new List<Type> { typeof(SvgFontFaceUri) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgFontFaceUriProperties = new Dictionary<string, ISvgPropertyDescriptor> { ["href"] = new SvgPropertyDescriptor<SvgFontFaceUri, Uri>(DescriptorType.Property, "href", "http://www.w3.org/1999/xlink", new UriTypeConverter(), (SvgFontFaceUri t) => t.ReferencedElement, delegate(SvgFontFaceUri t, Uri v)
	{
		t.ReferencedElement = v;
	}) };

	[SvgAttribute("href", "http://www.w3.org/1999/xlink")]
	public virtual Uri ReferencedElement
	{
		get
		{
			return GetAttribute<Uri>("href", inherited: false);
		}
		set
		{
			Attributes["href"] = value;
		}
	}

	internal override string AttributeName => "font-face-uri";

	internal override List<Type> ClassNames => SvgFontFaceUriClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgFontFaceUriProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgFontFaceUri>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgFontFaceUriProperty in SvgFontFaceUriProperties)
		{
			yield return svgFontFaceUriProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgFontFaceUriProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgFontFaceUriProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgFontFaceUriProperties.TryGetValue(attributeName, out var value2))
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
