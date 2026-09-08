#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using Svg.Transforms;

namespace Svg;

public abstract class SvgGradientServer : SvgPaintServer
{
	internal static List<Type> SvgGradientServerClassNames = new List<Type> { typeof(SvgGradientServer) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgGradientServerProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["spreadMethod"] = new SvgPropertyDescriptor<SvgGradientServer, SvgGradientSpreadMethod>(DescriptorType.Property, "spreadMethod", "http://www.w3.org/2000/svg", new SvgGradientSpreadMethodConverter(), (SvgGradientServer t) => t.SpreadMethod, delegate(SvgGradientServer t, SvgGradientSpreadMethod v)
		{
			t.SpreadMethod = v;
		}),
		["gradientUnits"] = new SvgPropertyDescriptor<SvgGradientServer, SvgCoordinateUnits>(DescriptorType.Property, "gradientUnits", "http://www.w3.org/2000/svg", new SvgCoordinateUnitsConverter(), (SvgGradientServer t) => t.GradientUnits, delegate(SvgGradientServer t, SvgCoordinateUnits v)
		{
			t.GradientUnits = v;
		}),
		["href"] = new SvgPropertyDescriptor<SvgGradientServer, SvgDeferredPaintServer>(DescriptorType.Property, "href", "http://www.w3.org/1999/xlink", new SvgDeferredPaintServerFactory(), (SvgGradientServer t) => t.InheritGradient, delegate(SvgGradientServer t, SvgDeferredPaintServer v)
		{
			t.InheritGradient = v;
		}),
		["gradientTransform"] = new SvgPropertyDescriptor<SvgGradientServer, SvgTransformCollection>(DescriptorType.Property, "gradientTransform", "http://www.w3.org/2000/svg", new SvgTransformConverter(), (SvgGradientServer t) => t.GradientTransform, delegate(SvgGradientServer t, SvgTransformCollection v)
		{
			t.GradientTransform = v;
		}),
		["stop-color"] = new SvgPropertyDescriptor<SvgGradientServer, SvgPaintServer>(DescriptorType.Property, "stop-color", "http://www.w3.org/2000/svg", new SvgPaintServerFactory(), (SvgGradientServer t) => t.StopColor, delegate(SvgGradientServer t, SvgPaintServer v)
		{
			t.StopColor = v;
		}),
		["stop-opacity"] = new SvgPropertyDescriptor<SvgGradientServer, float>(DescriptorType.Property, "stop-opacity", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgGradientServer t) => t.StopOpacity, delegate(SvgGradientServer t, float v)
		{
			t.StopOpacity = v;
		})
	};

	public List<SvgGradientStop> Stops { get; } = new List<SvgGradientStop>();

	[SvgAttribute("spreadMethod")]
	public SvgGradientSpreadMethod SpreadMethod
	{
		get
		{
			return GetAttribute("spreadMethod", inherited: false, SvgDeferredPaintServer.TryGet<SvgGradientServer>(InheritGradient, null)?.SpreadMethod ?? SvgGradientSpreadMethod.Pad);
		}
		set
		{
			Attributes["spreadMethod"] = value;
		}
	}

	[SvgAttribute("gradientUnits")]
	public SvgCoordinateUnits GradientUnits
	{
		get
		{
			return GetAttribute("gradientUnits", inherited: false, SvgDeferredPaintServer.TryGet<SvgGradientServer>(InheritGradient, null)?.GradientUnits ?? SvgCoordinateUnits.ObjectBoundingBox);
		}
		set
		{
			Attributes["gradientUnits"] = value;
		}
	}

	[SvgAttribute("href", "http://www.w3.org/1999/xlink")]
	public SvgDeferredPaintServer InheritGradient
	{
		get
		{
			return GetAttribute<SvgDeferredPaintServer>("href", inherited: false);
		}
		set
		{
			Attributes["href"] = value;
		}
	}

	[SvgAttribute("gradientTransform")]
	public SvgTransformCollection GradientTransform
	{
		get
		{
			return GetAttribute("gradientTransform", inherited: false, SvgDeferredPaintServer.TryGet<SvgGradientServer>(InheritGradient, null)?.GradientTransform);
		}
		set
		{
			Attributes["gradientTransform"] = value;
		}
	}

	[SvgAttribute("stop-color")]
	[TypeConverter(typeof(SvgPaintServerFactory))]
	public SvgPaintServer StopColor
	{
		get
		{
			return GetAttribute("stop-color", inherited: false, SvgDeferredPaintServer.TryGet<SvgGradientServer>(InheritGradient, null)?.StopColor ?? new SvgColourServer(System.Drawing.Color.Black));
		}
		set
		{
			Attributes["stop-color"] = value;
		}
	}

	[SvgAttribute("stop-opacity")]
	public float StopOpacity
	{
		get
		{
			return GetAttribute("stop-opacity", inherited: false, SvgDeferredPaintServer.TryGet<SvgGradientServer>(InheritGradient, null)?.StopOpacity ?? 1f);
		}
		set
		{
			Attributes["stop-opacity"] = SvgElement.FixOpacityValue(value);
		}
	}

	protected Matrix EffectiveGradientTransform
	{
		get
		{
			Matrix matrix = new Matrix();
			if (GradientTransform != null)
			{
				using Matrix matrix2 = GradientTransform.GetMatrix();
				matrix.Multiply(matrix2);
			}
			return matrix;
		}
	}

	internal override string AttributeName => "";

	internal override List<Type> ClassNames => SvgGradientServerClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgGradientServerProperties;

	protected override void AddElement(SvgElement child, int index)
	{
		if (child is SvgGradientStop)
		{
			Stops.Add((SvgGradientStop)child);
		}
		base.AddElement(child, index);
	}

	protected override void RemoveElement(SvgElement child)
	{
		if (child is SvgGradientStop)
		{
			Stops.Remove((SvgGradientStop)child);
		}
		base.RemoveElement(child);
	}

	protected static double CalculateDistance(PointF first, PointF second)
	{
		return Math.Sqrt(Math.Pow(first.X - second.X, 2.0) + Math.Pow(first.Y - second.Y, 2.0));
	}

	protected static float CalculateLength(PointF vector)
	{
		return (float)Math.Sqrt(Math.Pow(vector.X, 2.0) + Math.Pow(vector.Y, 2.0));
	}

	private void LoadStops(SvgVisualElement parent)
	{
		Stops.RemoveAll((SvgGradientStop s) => s.Parent != this);
		SvgGradientServer svgGradientServer = this;
		while (svgGradientServer != null && svgGradientServer.Stops.Count == 0)
		{
			svgGradientServer = SvgDeferredPaintServer.TryGet<SvgGradientServer>(svgGradientServer.InheritGradient, parent);
		}
		if (svgGradientServer != this && svgGradientServer != null)
		{
			Stops.AddRange(svgGradientServer.Stops);
		}
	}

	public override Brush GetBrush(SvgVisualElement styleOwner, ISvgRenderer renderer, float opacity, bool forStroke = false)
	{
		LoadStops(styleOwner);
		if (Stops.Count == 0)
		{
			return null;
		}
		if (Stops.Count == 1)
		{
			Color color = Stops[0].GetColor(styleOwner);
			return new SolidBrush(System.Drawing.Color.FromArgb((int)Math.Round(opacity * ((float)(int)color.A / 255f) * 255f), color));
		}
		return CreateBrush(styleOwner, renderer, opacity, forStroke);
	}

	protected abstract Brush CreateBrush(SvgVisualElement renderingElement, ISvgRenderer renderer, float opacity, bool forStroke);

	protected ColorBlend GetColorBlend(ISvgRenderer renderer, float opacity, bool radial)
	{
		int num = Stops.Count;
		bool flag = false;
		bool flag2 = false;
		if (Stops[0].Offset.Value > 0f)
		{
			num++;
			if (radial)
			{
				flag2 = true;
			}
			else
			{
				flag = true;
			}
		}
		float value = Stops[Stops.Count - 1].Offset.Value;
		if (value < 100f || value < 1f)
		{
			num++;
			if (radial)
			{
				flag = true;
			}
			else
			{
				flag2 = true;
			}
		}
		ColorBlend colorBlend = new ColorBlend(num);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			SvgGradientStop svgGradientStop = Stops[radial ? (Stops.Count - 1 - num2) : num2];
			float width = renderer.GetBoundable().Bounds.Width;
			float num3 = opacity * svgGradientStop.StopOpacity;
			float val = (radial ? (1f - svgGradientStop.Offset.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this) / width) : (svgGradientStop.Offset.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this) / width));
			val = Math.Min(Math.Max(val, 0f), 1f);
			Color color = System.Drawing.Color.FromArgb((int)Math.Round(num3 * 255f), svgGradientStop.GetColor(this));
			num2++;
			if (flag && i == 0)
			{
				colorBlend.Positions[i] = 0f;
				colorBlend.Colors[i] = color;
				i++;
			}
			colorBlend.Positions[i] = val;
			colorBlend.Colors[i] = color;
			if (flag2 && i == num - 2)
			{
				i++;
				colorBlend.Positions[i] = 1f;
				colorBlend.Colors[i] = color;
			}
		}
		return colorBlend;
	}

	protected SvgUnit NormalizeUnit(SvgUnit orig)
	{
		if (orig.Type != SvgUnitType.Percentage || GradientUnits != SvgCoordinateUnits.ObjectBoundingBox)
		{
			return orig;
		}
		return new SvgUnit(SvgUnitType.User, orig.Value / 100f);
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgGradientServerProperty in SvgGradientServerProperties)
		{
			yield return svgGradientServerProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgGradientServerProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgGradientServerProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgGradientServerProperties.TryGetValue(attributeName, out var value2))
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
