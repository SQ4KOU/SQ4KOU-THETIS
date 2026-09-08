#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using Svg.DataTypes;

namespace Svg.FilterEffects;

[SvgElement("feImage")]
public class SvgImage : SvgFilterPrimitive
{
	internal static List<Type> SvgImageClassNames = new List<Type> { typeof(SvgImage) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgImageProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["href"] = new SvgPropertyDescriptor<SvgImage, string>(DescriptorType.Property, "href", "http://www.w3.org/1999/xlink", new StringConverter(), (SvgImage t) => t.Href, delegate(SvgImage t, string v)
		{
			t.Href = v;
		}),
		["preserveAspectRatio"] = new SvgPropertyDescriptor<SvgImage, SvgAspectRatio>(DescriptorType.Property, "preserveAspectRatio", "http://www.w3.org/2000/svg", new SvgPreserveAspectRatioConverter(), (SvgImage t) => t.AspectRatio, delegate(SvgImage t, SvgAspectRatio v)
		{
			t.AspectRatio = v;
		})
	};

	[SvgAttribute("href", "http://www.w3.org/1999/xlink")]
	public virtual string Href
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

	[SvgAttribute("preserveAspectRatio")]
	public SvgAspectRatio AspectRatio
	{
		get
		{
			return GetAttribute("preserveAspectRatio", inherited: false, new SvgAspectRatio(SvgPreserveAspectRatio.xMidYMid));
		}
		set
		{
			Attributes["preserveAspectRatio"] = value;
		}
	}

	internal override string AttributeName => "feImage";

	internal override List<Type> ClassNames => SvgImageClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgImageProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgImage>();
	}

	public override void Process(ImageBuffer buffer)
	{
		buffer[base.Result] = buffer[base.Input];
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgImageProperty in SvgImageProperties)
		{
			yield return svgImageProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgImageProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgImageProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgImageProperties.TryGetValue(attributeName, out var value2))
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
