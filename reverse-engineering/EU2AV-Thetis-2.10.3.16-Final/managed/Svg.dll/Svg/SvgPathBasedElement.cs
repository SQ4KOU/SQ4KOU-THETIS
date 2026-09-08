#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace Svg;

public abstract class SvgPathBasedElement : SvgVisualElement
{
	internal static List<Type> SvgPathBasedElementClassNames = new List<Type> { typeof(SvgPathBasedElement) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgPathBasedElementProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	public override RectangleF Bounds
	{
		get
		{
			GraphicsPath graphicsPath = Path(null);
			if (graphicsPath == null)
			{
				return default(RectangleF);
			}
			if (base.Transforms == null || base.Transforms.Count == 0)
			{
				return graphicsPath.GetBounds();
			}
			using (graphicsPath = (GraphicsPath)graphicsPath.Clone())
			{
				using Matrix matrix = base.Transforms.GetMatrix();
				graphicsPath.Transform(matrix);
				return graphicsPath.GetBounds();
			}
		}
	}

	internal override string AttributeName => "";

	internal override List<Type> ClassNames => SvgPathBasedElementClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgPathBasedElementProperties;

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgPathBasedElementProperty in SvgPathBasedElementProperties)
		{
			yield return svgPathBasedElementProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgPathBasedElementProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgPathBasedElementProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgPathBasedElementProperties.TryGetValue(attributeName, out var value2))
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
