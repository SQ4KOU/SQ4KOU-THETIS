#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using Svg.DataTypes;

namespace Svg;

[SvgElement("marker")]
public class SvgMarker : SvgPathBasedElement, ISvgViewPort
{
	private SvgVisualElement _markerElement;

	internal static List<Type> SvgMarkerClassNames = new List<Type> { typeof(SvgMarker) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgMarkerProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["refX"] = new SvgPropertyDescriptor<SvgMarker, SvgUnit>(DescriptorType.Property, "refX", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgMarker t) => t.RefX, delegate(SvgMarker t, SvgUnit v)
		{
			t.RefX = v;
		}),
		["refY"] = new SvgPropertyDescriptor<SvgMarker, SvgUnit>(DescriptorType.Property, "refY", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgMarker t) => t.RefY, delegate(SvgMarker t, SvgUnit v)
		{
			t.RefY = v;
		}),
		["orient"] = new SvgPropertyDescriptor<SvgMarker, SvgOrient>(DescriptorType.Property, "orient", "http://www.w3.org/2000/svg", new SvgOrientConverter(), (SvgMarker t) => t.Orient, delegate(SvgMarker t, SvgOrient v)
		{
			t.Orient = v;
		}),
		["overflow"] = new SvgPropertyDescriptor<SvgMarker, SvgOverflow>(DescriptorType.Property, "overflow", "http://www.w3.org/2000/svg", new SvgOverflowConverter(), (SvgMarker t) => t.Overflow, delegate(SvgMarker t, SvgOverflow v)
		{
			t.Overflow = v;
		}),
		["viewBox"] = new SvgPropertyDescriptor<SvgMarker, SvgViewBox>(DescriptorType.Property, "viewBox", "http://www.w3.org/2000/svg", new SvgViewBoxConverter(), (SvgMarker t) => t.ViewBox, delegate(SvgMarker t, SvgViewBox v)
		{
			t.ViewBox = v;
		}),
		["preserveAspectRatio"] = new SvgPropertyDescriptor<SvgMarker, SvgAspectRatio>(DescriptorType.Property, "preserveAspectRatio", "http://www.w3.org/2000/svg", new SvgPreserveAspectRatioConverter(), (SvgMarker t) => t.AspectRatio, delegate(SvgMarker t, SvgAspectRatio v)
		{
			t.AspectRatio = v;
		}),
		["markerWidth"] = new SvgPropertyDescriptor<SvgMarker, SvgUnit>(DescriptorType.Property, "markerWidth", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgMarker t) => t.MarkerWidth, delegate(SvgMarker t, SvgUnit v)
		{
			t.MarkerWidth = v;
		}),
		["markerHeight"] = new SvgPropertyDescriptor<SvgMarker, SvgUnit>(DescriptorType.Property, "markerHeight", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgMarker t) => t.MarkerHeight, delegate(SvgMarker t, SvgUnit v)
		{
			t.MarkerHeight = v;
		}),
		["markerUnits"] = new SvgPropertyDescriptor<SvgMarker, SvgMarkerUnits>(DescriptorType.Property, "markerUnits", "http://www.w3.org/2000/svg", new SvgMarkerUnitsConverter(), (SvgMarker t) => t.MarkerUnits, delegate(SvgMarker t, SvgMarkerUnits v)
		{
			t.MarkerUnits = v;
		})
	};

	private SvgVisualElement MarkerElement
	{
		get
		{
			if (_markerElement == null)
			{
				_markerElement = (SvgVisualElement)Children.FirstOrDefault((SvgElement x) => x is SvgVisualElement);
			}
			return _markerElement;
		}
	}

	[SvgAttribute("refX")]
	public virtual SvgUnit RefX
	{
		get
		{
			return GetAttribute("refX", false, (SvgUnit)0f);
		}
		set
		{
			Attributes["refX"] = value;
		}
	}

	[SvgAttribute("refY")]
	public virtual SvgUnit RefY
	{
		get
		{
			return GetAttribute("refY", false, (SvgUnit)0f);
		}
		set
		{
			Attributes["refY"] = value;
		}
	}

	[SvgAttribute("orient")]
	public virtual SvgOrient Orient
	{
		get
		{
			return GetAttribute("orient", false, (SvgOrient)0f);
		}
		set
		{
			Attributes["orient"] = value;
		}
	}

	[SvgAttribute("overflow")]
	public virtual SvgOverflow Overflow
	{
		get
		{
			return GetAttribute("overflow", inherited: false, SvgOverflow.Hidden);
		}
		set
		{
			Attributes["overflow"] = value;
		}
	}

	[SvgAttribute("viewBox")]
	public virtual SvgViewBox ViewBox
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
	public virtual SvgAspectRatio AspectRatio
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

	[SvgAttribute("markerWidth")]
	public virtual SvgUnit MarkerWidth
	{
		get
		{
			return GetAttribute("markerWidth", false, (SvgUnit)3f);
		}
		set
		{
			Attributes["markerWidth"] = value;
		}
	}

	[SvgAttribute("markerHeight")]
	public virtual SvgUnit MarkerHeight
	{
		get
		{
			return GetAttribute("markerHeight", false, (SvgUnit)3f);
		}
		set
		{
			Attributes["markerHeight"] = value;
		}
	}

	[SvgAttribute("markerUnits")]
	public virtual SvgMarkerUnits MarkerUnits
	{
		get
		{
			return GetAttribute("markerUnits", inherited: false, SvgMarkerUnits.StrokeWidth);
		}
		set
		{
			Attributes["markerUnits"] = value;
		}
	}

	public override SvgPaintServer Fill
	{
		get
		{
			if (MarkerElement != null)
			{
				return MarkerElement.Fill;
			}
			return base.Fill;
		}
	}

	public override SvgPaintServer Stroke
	{
		get
		{
			if (MarkerElement != null)
			{
				return MarkerElement.Stroke;
			}
			return base.Stroke;
		}
	}

	internal override string AttributeName => "marker";

	internal override List<Type> ClassNames => SvgMarkerClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgMarkerProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgMarker>();
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		if (MarkerElement != null)
		{
			return MarkerElement.Path(renderer);
		}
		return null;
	}

	public void RenderMarker(ISvgRenderer pRenderer, SvgVisualElement pOwner, PointF pRefPoint, PointF pMarkerPoint1, PointF pMarkerPoint2, bool isStartMarker)
	{
		float num = 0f;
		if (Orient.IsAuto)
		{
			float num2 = pMarkerPoint2.X - pMarkerPoint1.X;
			num = (float)(Math.Atan2(pMarkerPoint2.Y - pMarkerPoint1.Y, num2) * 180.0 / Math.PI);
			if (isStartMarker && Orient.IsAutoStartReverse)
			{
				num += 180f;
			}
		}
		RenderPart2(num, pRenderer, pOwner, pRefPoint);
	}

	public void RenderMarker(ISvgRenderer pRenderer, SvgVisualElement pOwner, PointF pRefPoint, PointF pMarkerPoint1, PointF pMarkerPoint2, PointF pMarkerPoint3)
	{
		float num = pMarkerPoint2.X - pMarkerPoint1.X;
		float num2 = (float)(Math.Atan2(pMarkerPoint2.Y - pMarkerPoint1.Y, num) * 180.0 / Math.PI);
		num = pMarkerPoint3.X - pMarkerPoint2.X;
		float num3 = (float)(Math.Atan2(pMarkerPoint3.Y - pMarkerPoint2.Y, num) * 180.0 / Math.PI);
		RenderPart2((num2 + num3) / 2f, pRenderer, pOwner, pRefPoint);
	}

	private void RenderPart2(float fAngle, ISvgRenderer pRenderer, SvgVisualElement pOwner, PointF pMarkerPoint)
	{
		using Pen pen = CreatePen(pOwner, pRenderer);
		using GraphicsPath graphicsPath = GetClone(pOwner, pRenderer);
		using Matrix matrix = new Matrix();
		matrix.Translate(pMarkerPoint.X, pMarkerPoint.Y);
		if (Orient.IsAuto)
		{
			matrix.Rotate(fAngle);
		}
		else
		{
			matrix.Rotate(Orient.Angle);
		}
		switch (MarkerUnits)
		{
		case SvgMarkerUnits.StrokeWidth:
			if (ViewBox.Width > 0f && ViewBox.Height > 0f)
			{
				matrix.Scale(MarkerWidth, MarkerHeight);
				float num = pOwner.StrokeWidth.ToDeviceValue(pRenderer, UnitRenderingType.Other, this);
				matrix.Translate(AdjustForViewBoxWidth((0f - RefX.ToDeviceValue(pRenderer, UnitRenderingType.Horizontal, this)) * num), AdjustForViewBoxHeight((0f - RefY.ToDeviceValue(pRenderer, UnitRenderingType.Vertical, this)) * num));
			}
			else
			{
				matrix.Translate(0f - RefX.ToDeviceValue(pRenderer, UnitRenderingType.Horizontal, this), 0f - RefY.ToDeviceValue(pRenderer, UnitRenderingType.Vertical, this));
			}
			break;
		case SvgMarkerUnits.UserSpaceOnUse:
			matrix.Translate(0f - RefX.ToDeviceValue(pRenderer, UnitRenderingType.Horizontal, this), 0f - RefY.ToDeviceValue(pRenderer, UnitRenderingType.Vertical, this));
			break;
		}
		if (MarkerElement != null && MarkerElement.Transforms != null)
		{
			using Matrix matrix2 = MarkerElement.Transforms.GetMatrix();
			matrix.Multiply(matrix2);
		}
		graphicsPath.Transform(matrix);
		if (pen != null)
		{
			pRenderer.DrawPath(pen, graphicsPath);
		}
		SvgPaintServer fill = Children.First().Fill;
		_ = FillRule;
		if (fill != null)
		{
			using (Brush brush = fill.GetBrush(this, pRenderer, SvgElement.FixOpacityValue(FillOpacity)))
			{
				pRenderer.FillPath(brush, graphicsPath);
				return;
			}
		}
	}

	private Pen CreatePen(SvgVisualElement pPath, ISvgRenderer renderer)
	{
		if (Stroke == null)
		{
			return null;
		}
		Brush brush = Stroke.GetBrush(this, renderer, SvgElement.FixOpacityValue(Opacity));
		return MarkerUnits switch
		{
			SvgMarkerUnits.StrokeWidth => new Pen(brush, pPath.StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, this)), 
			SvgMarkerUnits.UserSpaceOnUse => new Pen(brush, StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, this)), 
			_ => new Pen(brush, StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, this)), 
		};
	}

	private GraphicsPath GetClone(SvgVisualElement pPath, ISvgRenderer renderer)
	{
		GraphicsPath graphicsPath = Path(renderer).Clone() as GraphicsPath;
		switch (MarkerUnits)
		{
		case SvgMarkerUnits.StrokeWidth:
		{
			using (Matrix matrix = new Matrix())
			{
				matrix.Scale(AdjustForViewBoxWidth(pPath.StrokeWidth), AdjustForViewBoxHeight(pPath.StrokeWidth));
				graphicsPath.Transform(matrix);
			}
			break;
		}
		}
		return graphicsPath;
	}

	private float AdjustForViewBoxWidth(float fWidth)
	{
		if (!(ViewBox.Width <= 0f))
		{
			return fWidth / ViewBox.Width;
		}
		return 1f;
	}

	private float AdjustForViewBoxHeight(float fHeight)
	{
		if (!(ViewBox.Height <= 0f))
		{
			return fHeight / ViewBox.Height;
		}
		return 1f;
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgMarkerProperty in SvgMarkerProperties)
		{
			yield return svgMarkerProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgMarkerProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgMarkerProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgMarkerProperties.TryGetValue(attributeName, out var value2))
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
