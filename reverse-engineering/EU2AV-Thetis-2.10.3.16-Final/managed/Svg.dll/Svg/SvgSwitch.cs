#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace Svg;

[SvgElement("switch")]
public class SvgSwitch : SvgVisualElement
{
	internal static List<Type> SvgSwitchClassNames = new List<Type> { typeof(SvgSwitch) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgSwitchProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	public override RectangleF Bounds
	{
		get
		{
			RectangleF rectangleF = default(RectangleF);
			foreach (SvgElement child in Children)
			{
				if (!(child is SvgVisualElement))
				{
					continue;
				}
				if (rectangleF.IsEmpty)
				{
					rectangleF = ((SvgVisualElement)child).Bounds;
					continue;
				}
				RectangleF bounds = ((SvgVisualElement)child).Bounds;
				if (!bounds.IsEmpty)
				{
					rectangleF = RectangleF.Union(rectangleF, bounds);
				}
			}
			return TransformedBounds(rectangleF);
		}
	}

	internal override string AttributeName => "switch";

	internal override List<Type> ClassNames => SvgSwitchClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgSwitchProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgSwitch>();
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		return GetPaths(this, renderer);
	}

	protected override void Render(ISvgRenderer renderer)
	{
		if (!Visible || !Displayable)
		{
			return;
		}
		try
		{
			if (PushTransforms(renderer))
			{
				SetClip(renderer);
				base.RenderChildren(renderer);
				ResetClip(renderer);
			}
		}
		finally
		{
			PopTransforms(renderer);
		}
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgSwitchProperty in SvgSwitchProperties)
		{
			yield return svgSwitchProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgSwitchProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgSwitchProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgSwitchProperties.TryGetValue(attributeName, out var value2))
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
