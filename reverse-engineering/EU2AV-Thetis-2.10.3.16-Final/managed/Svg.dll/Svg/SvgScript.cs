#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

namespace Svg;

[SvgElement("script")]
public class SvgScript : SvgElement
{
	internal static List<Type> SvgScriptClassNames = new List<Type> { typeof(SvgScript) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgScriptProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["type"] = new SvgPropertyDescriptor<SvgScript, string>(DescriptorType.Property, "type", "http://www.w3.org/2000/svg", new StringConverter(), (SvgScript t) => t.ScriptType, delegate(SvgScript t, string v)
		{
			t.ScriptType = v;
		}),
		["crossorigin"] = new SvgPropertyDescriptor<SvgScript, string>(DescriptorType.Property, "crossorigin", "http://www.w3.org/2000/svg", new StringConverter(), (SvgScript t) => t.CrossOrigin, delegate(SvgScript t, string v)
		{
			t.CrossOrigin = v;
		}),
		["href"] = new SvgPropertyDescriptor<SvgScript, string>(DescriptorType.Property, "href", "http://www.w3.org/1999/xlink", new StringConverter(), (SvgScript t) => t.Href, delegate(SvgScript t, string v)
		{
			t.Href = v;
		})
	};

	public string Script
	{
		get
		{
			return Content;
		}
		set
		{
			Content = value;
		}
	}

	[SvgAttribute("type")]
	public string ScriptType
	{
		get
		{
			return GetAttribute<string>("type", inherited: false);
		}
		set
		{
			Attributes["type"] = value;
		}
	}

	[SvgAttribute("crossorigin")]
	public string CrossOrigin
	{
		get
		{
			return GetAttribute<string>("crossorigin", inherited: false);
		}
		set
		{
			Attributes["crossorigin"] = value;
		}
	}

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

	internal override string AttributeName => "script";

	internal override List<Type> ClassNames => SvgScriptClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgScriptProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgScript>();
	}

	protected override void WriteChildren(XmlWriter writer)
	{
		if (!string.IsNullOrEmpty(Content))
		{
			writer.WriteCData(Content);
		}
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgScriptProperty in SvgScriptProperties)
		{
			yield return svgScriptProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgScriptProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgScriptProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgScriptProperties.TryGetValue(attributeName, out var value2))
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
