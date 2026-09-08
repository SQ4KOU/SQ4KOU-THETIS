#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg;

[SvgElement("a")]
public class SvgAnchor : SvgElement
{
	internal static List<Type> SvgAnchorClassNames = new List<Type> { typeof(SvgAnchor) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgAnchorProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["href"] = new SvgPropertyDescriptor<SvgAnchor, string>(DescriptorType.Property, "href", "http://www.w3.org/1999/xlink", new StringConverter(), (SvgAnchor t) => t.Href, delegate(SvgAnchor t, string v)
		{
			t.Href = v;
		}),
		["show"] = new SvgPropertyDescriptor<SvgAnchor, string>(DescriptorType.Property, "show", "http://www.w3.org/1999/xlink", new StringConverter(), (SvgAnchor t) => t.Show, delegate(SvgAnchor t, string v)
		{
			t.Show = v;
		}),
		["title"] = new SvgPropertyDescriptor<SvgAnchor, string>(DescriptorType.Property, "title", "http://www.w3.org/1999/xlink", new StringConverter(), (SvgAnchor t) => t.Title, delegate(SvgAnchor t, string v)
		{
			t.Title = v;
		}),
		["target"] = new SvgPropertyDescriptor<SvgAnchor, string>(DescriptorType.Property, "target", "http://www.w3.org/2000/svg", new StringConverter(), (SvgAnchor t) => t.Target, delegate(SvgAnchor t, string v)
		{
			t.Target = v;
		})
	};

	[SvgAttribute("href", "http://www.w3.org/1999/xlink")]
	public string Href
	{
		get
		{
			return GetAttribute<string>("href", inherited: false);
		}
		set
		{
			Attributes["href"] = value;
		}
	}

	[SvgAttribute("show", "http://www.w3.org/1999/xlink")]
	public string Show
	{
		get
		{
			return GetAttribute<string>("show", inherited: false);
		}
		set
		{
			Attributes["show"] = value;
		}
	}

	[SvgAttribute("title", "http://www.w3.org/1999/xlink")]
	public string Title
	{
		get
		{
			return GetAttribute<string>("title", inherited: false);
		}
		set
		{
			Attributes["title"] = value;
		}
	}

	[SvgAttribute("target")]
	public string Target
	{
		get
		{
			return GetAttribute<string>("target", inherited: false);
		}
		set
		{
			Attributes["target"] = value;
		}
	}

	internal override string AttributeName => "a";

	internal override List<Type> ClassNames => SvgAnchorClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgAnchorProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgAnchor>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgAnchorProperty in SvgAnchorProperties)
		{
			yield return svgAnchorProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgAnchorProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgAnchorProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgAnchorProperties.TryGetValue(attributeName, out var value2))
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
