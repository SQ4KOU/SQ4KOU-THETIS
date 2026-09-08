#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

namespace Svg;

[TypeConverter(typeof(SvgPaintServerFactory))]
public abstract class SvgPaintServer : SvgElement
{
	public static readonly SvgPaintServer None = new SvgColourServer();

	public static readonly SvgPaintServer Inherit = new SvgColourServer();

	public static readonly SvgPaintServer NotSet = new SvgColourServer();

	internal static List<Type> SvgPaintServerClassNames = new List<Type> { typeof(SvgPaintServer) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgPaintServerProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	public Func<SvgPaintServer> GetCallback { get; set; }

	internal override string AttributeName => "";

	internal override List<Type> ClassNames => SvgPaintServerClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgPaintServerProperties;

	public override string ToString()
	{
		return $"url(#{base.ID})";
	}

	protected override void Render(ISvgRenderer renderer)
	{
	}

	public abstract Brush GetBrush(SvgVisualElement styleOwner, ISvgRenderer renderer, float opacity, bool forStroke = false);

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgPaintServerProperty in SvgPaintServerProperties)
		{
			yield return svgPaintServerProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgPaintServerProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgPaintServerProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgPaintServerProperties.TryGetValue(attributeName, out var value2))
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
