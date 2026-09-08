#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Svg;

[SvgElement("tref")]
public class SvgTextRef : SvgTextBase
{
	internal static List<Type> SvgTextRefClassNames = new List<Type> { typeof(SvgTextRef) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgTextRefProperties = new Dictionary<string, ISvgPropertyDescriptor> { ["href"] = new SvgPropertyDescriptor<SvgTextRef, Uri>(DescriptorType.Property, "href", "http://www.w3.org/1999/xlink", new UriTypeConverter(), (SvgTextRef t) => t.ReferencedElement, delegate(SvgTextRef t, Uri v)
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

	internal override string AttributeName => "tref";

	internal override List<Type> ClassNames => SvgTextRefClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgTextRefProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgTextRef>();
	}

	internal override IEnumerable<ISvgNode> GetContentNodes()
	{
		SvgTextBase svgTextBase = OwnerDocument.IdManager.GetElementById(ReferencedElement) as SvgTextBase;
		IEnumerable<ISvgNode> enumerable = null;
		enumerable = ((svgTextBase != null) ? svgTextBase.GetContentNodes() : base.GetContentNodes());
		return enumerable.Where((ISvgNode o) => !(o is ISvgDescriptiveElement));
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgTextRefProperty in SvgTextRefProperties)
		{
			yield return svgTextRefProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgTextRefProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgTextRefProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgTextRefProperties.TryGetValue(attributeName, out var value2))
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
