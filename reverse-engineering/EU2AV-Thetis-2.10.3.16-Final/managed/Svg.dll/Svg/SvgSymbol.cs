#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using Svg.DataTypes;

namespace Svg;

[SvgElement("symbol")]
public class SvgSymbol : SvgVisualElement
{
	internal static List<Type> SvgSymbolClassNames = new List<Type> { typeof(SvgSymbol) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgSymbolProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["viewBox"] = new SvgPropertyDescriptor<SvgSymbol, SvgViewBox>(DescriptorType.Property, "viewBox", "http://www.w3.org/2000/svg", new SvgViewBoxConverter(), (SvgSymbol t) => t.ViewBox, delegate(SvgSymbol t, SvgViewBox v)
		{
			t.ViewBox = v;
		}),
		["preserveAspectRatio"] = new SvgPropertyDescriptor<SvgSymbol, SvgAspectRatio>(DescriptorType.Property, "preserveAspectRatio", "http://www.w3.org/2000/svg", new SvgPreserveAspectRatioConverter(), (SvgSymbol t) => t.AspectRatio, delegate(SvgSymbol t, SvgAspectRatio v)
		{
			t.AspectRatio = v;
		})
	};

	[SvgAttribute("viewBox")]
	public SvgViewBox ViewBox
	{
		get
		{
			return GetAttribute<SvgViewBox>("viewBox", inherited: false);
		}
		set
		{
			Attributes["viewBox"] = value;
		}
	}

	[SvgAttribute("preserveAspectRatio")]
	public SvgAspectRatio AspectRatio
	{
		get
		{
			return GetAttribute<SvgAspectRatio>("preserveAspectRatio", inherited: false);
		}
		set
		{
			Attributes["preserveAspectRatio"] = value;
		}
	}

	protected override bool Renderable => false;

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

	internal override string AttributeName => "symbol";

	internal override List<Type> ClassNames => SvgSymbolClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgSymbolProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgSymbol>();
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		return GetPaths(this, renderer);
	}

	protected internal override bool PushTransforms(ISvgRenderer renderer)
	{
		if (!base.PushTransforms(renderer))
		{
			return false;
		}
		ViewBox.AddViewBoxTransform(AspectRatio, renderer, null);
		return true;
	}

	protected override void Render(ISvgRenderer renderer)
	{
		if (_parent is SvgUse)
		{
			base.Render(renderer);
		}
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgSymbolProperty in SvgSymbolProperties)
		{
			yield return svgSymbolProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgSymbolProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgSymbolProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgSymbolProperties.TryGetValue(attributeName, out var value2))
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
