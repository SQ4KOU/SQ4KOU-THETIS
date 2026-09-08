#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

namespace Svg;

[SvgElement("metadata")]
public class SvgDocumentMetadata : SvgElement
{
	internal static List<Type> SvgDocumentMetadataClassNames = new List<Type> { typeof(SvgDocumentMetadata) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgDocumentMetadataProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "metadata";

	internal override List<Type> ClassNames => SvgDocumentMetadataClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgDocumentMetadataProperties;

	public SvgDocumentMetadata()
	{
		Content = string.Empty;
	}

	protected override void WriteChildren(XmlWriter writer)
	{
		writer.WriteRaw(Content);
	}

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgDocumentMetadata>();
	}

	public override void InitialiseFromXML(XmlReader reader, SvgDocument document)
	{
		base.InitialiseFromXML(reader, document);
		Content = reader.ReadInnerXml();
	}

	protected override void Render(ISvgRenderer renderer)
	{
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgDocumentMetadataProperty in SvgDocumentMetadataProperties)
		{
			yield return svgDocumentMetadataProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgDocumentMetadataProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgDocumentMetadataProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgDocumentMetadataProperties.TryGetValue(attributeName, out var value2))
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
