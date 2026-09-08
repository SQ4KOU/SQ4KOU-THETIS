#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;

namespace Svg;

[SvgElement("radialGradient")]
public class SvgRadialGradientServer : SvgGradientServer
{
	internal static List<Type> SvgRadialGradientServerClassNames = new List<Type> { typeof(SvgRadialGradientServer) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgRadialGradientServerProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["cx"] = new SvgPropertyDescriptor<SvgRadialGradientServer, SvgUnit>(DescriptorType.Property, "cx", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgRadialGradientServer t) => t.CenterX, delegate(SvgRadialGradientServer t, SvgUnit v)
		{
			t.CenterX = v;
		}),
		["cy"] = new SvgPropertyDescriptor<SvgRadialGradientServer, SvgUnit>(DescriptorType.Property, "cy", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgRadialGradientServer t) => t.CenterY, delegate(SvgRadialGradientServer t, SvgUnit v)
		{
			t.CenterY = v;
		}),
		["r"] = new SvgPropertyDescriptor<SvgRadialGradientServer, SvgUnit>(DescriptorType.Property, "r", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgRadialGradientServer t) => t.Radius, delegate(SvgRadialGradientServer t, SvgUnit v)
		{
			t.Radius = v;
		}),
		["fx"] = new SvgPropertyDescriptor<SvgRadialGradientServer, SvgUnit>(DescriptorType.Property, "fx", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgRadialGradientServer t) => t.FocalX, delegate(SvgRadialGradientServer t, SvgUnit v)
		{
			t.FocalX = v;
		}),
		["fy"] = new SvgPropertyDescriptor<SvgRadialGradientServer, SvgUnit>(DescriptorType.Property, "fy", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgRadialGradientServer t) => t.FocalY, delegate(SvgRadialGradientServer t, SvgUnit v)
		{
			t.FocalY = v;
		}),
		["fr"] = new SvgPropertyDescriptor<SvgRadialGradientServer, SvgUnit>(DescriptorType.Property, "fr", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgRadialGradientServer t) => t.FocalRadius, delegate(SvgRadialGradientServer t, SvgUnit v)
		{
			t.FocalRadius = v;
		})
	};

	[SvgAttribute("cx")]
	public SvgUnit CenterX
	{
		get
		{
			return GetAttribute("cx", inherited: false, SvgDeferredPaintServer.TryGet<SvgRadialGradientServer>(base.InheritGradient, null)?.CenterX ?? new SvgUnit(SvgUnitType.Percentage, 50f));
		}
		set
		{
			Attributes["cx"] = value;
		}
	}

	[SvgAttribute("cy")]
	public SvgUnit CenterY
	{
		get
		{
			return GetAttribute("cy", inherited: false, SvgDeferredPaintServer.TryGet<SvgRadialGradientServer>(base.InheritGradient, null)?.CenterY ?? new SvgUnit(SvgUnitType.Percentage, 50f));
		}
		set
		{
			Attributes["cy"] = value;
		}
	}

	[SvgAttribute("r")]
	public SvgUnit Radius
	{
		get
		{
			return GetAttribute("r", inherited: false, SvgDeferredPaintServer.TryGet<SvgRadialGradientServer>(base.InheritGradient, null)?.Radius ?? new SvgUnit(SvgUnitType.Percentage, 50f));
		}
		set
		{
			Attributes["r"] = value;
		}
	}

	[SvgAttribute("fx")]
	public SvgUnit FocalX
	{
		get
		{
			SvgUnit result = GetAttribute("fx", inherited: false, SvgDeferredPaintServer.TryGet<SvgRadialGradientServer>(base.InheritGradient, null)?.FocalX ?? SvgUnit.None);
			if (result.IsEmpty || result.IsNone)
			{
				result = CenterX;
			}
			return result;
		}
		set
		{
			Attributes["fx"] = value;
		}
	}

	[SvgAttribute("fy")]
	public SvgUnit FocalY
	{
		get
		{
			SvgUnit result = GetAttribute("fy", inherited: false, SvgDeferredPaintServer.TryGet<SvgRadialGradientServer>(base.InheritGradient, null)?.FocalY ?? SvgUnit.None);
			if (result.IsEmpty || result.IsNone)
			{
				result = CenterY;
			}
			return result;
		}
		set
		{
			Attributes["fy"] = value;
		}
	}

	[SvgAttribute("fr")]
	public SvgUnit FocalRadius
	{
		get
		{
			return GetAttribute("fr", inherited: false, SvgDeferredPaintServer.TryGet<SvgRadialGradientServer>(base.InheritGradient, null)?.FocalRadius ?? new SvgUnit(SvgUnitType.Percentage, 0f));
		}
		set
		{
			Attributes["fr"] = value;
		}
	}

	internal override string AttributeName => "radialGradient";

	internal override List<Type> ClassNames => SvgRadialGradientServerClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgRadialGradientServerProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgRadialGradientServer>();
	}

	protected override Brush CreateBrush(SvgVisualElement renderingElement, ISvgRenderer renderer, float opacity, bool forStroke)
	{
		try
		{
			if (base.GradientUnits == SvgCoordinateUnits.ObjectBoundingBox)
			{
				renderer.SetBoundable(renderingElement);
			}
			PointF pointF = new PointF(NormalizeUnit(CenterX).ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), NormalizeUnit(CenterY).ToDeviceValue(renderer, UnitRenderingType.Vertical, this));
			PointF[] array = new PointF[1]
			{
				new PointF(NormalizeUnit(FocalX).ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), NormalizeUnit(FocalY).ToDeviceValue(renderer, UnitRenderingType.Vertical, this))
			};
			float num = NormalizeUnit(Radius).ToDeviceValue(renderer, UnitRenderingType.Other, this);
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddEllipse(pointF.X - num, pointF.Y - num, num * 2f, num * 2f);
			using (Matrix matrix = base.EffectiveGradientTransform)
			{
				RectangleF bounds = renderer.GetBoundable().Bounds;
				matrix.Translate(bounds.X, bounds.Y, MatrixOrder.Prepend);
				if (base.GradientUnits == SvgCoordinateUnits.ObjectBoundingBox)
				{
					matrix.Scale(bounds.Width, bounds.Height, MatrixOrder.Prepend);
				}
				graphicsPath.Transform(matrix);
				matrix.TransformPoints(array);
			}
			RectangleF bounds2 = RectangleF.Inflate(renderingElement.Bounds, renderingElement.StrokeWidth, renderingElement.StrokeWidth);
			float outScale = CalcScale(bounds2, graphicsPath);
			if (outScale > 1f && base.SpreadMethod == SvgGradientSpreadMethod.Pad)
			{
				SvgGradientStop svgGradientStop = base.Stops.Last();
				Color color = svgGradientStop.GetColor(renderingElement);
				Color color2 = System.Drawing.Color.FromArgb((int)Math.Round(opacity * svgGradientStop.StopOpacity * 255f), color);
				Region clip = renderer.GetClip();
				try
				{
					using SolidBrush brush = new SolidBrush(color2);
					Region region = clip.Clone();
					region.Exclude(graphicsPath);
					renderer.SetClip(region);
					GraphicsPath path = renderingElement.Path(renderer);
					if (forStroke)
					{
						using Pen pen = new Pen(brush, renderingElement.StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, renderingElement));
						renderer.DrawPath(pen, path);
					}
					else
					{
						renderer.FillPath(brush, path);
					}
				}
				finally
				{
					renderer.SetClip(clip);
				}
			}
			ColorBlend interpolationColors = CalculateColorBlend(renderer, opacity, outScale, out outScale);
			RectangleF bounds3 = graphicsPath.GetBounds();
			PointF pointF2 = new PointF(bounds3.Left + bounds3.Width / 2f, bounds3.Top + bounds3.Height / 2f);
			using (Matrix matrix2 = new Matrix())
			{
				matrix2.Translate(-1f * pointF2.X, -1f * pointF2.Y, MatrixOrder.Append);
				matrix2.Scale(outScale, outScale, MatrixOrder.Append);
				matrix2.Translate(pointF2.X, pointF2.Y, MatrixOrder.Append);
				graphicsPath.Transform(matrix2);
			}
			return new PathGradientBrush(graphicsPath)
			{
				CenterPoint = array[0],
				InterpolationColors = interpolationColors
			};
		}
		finally
		{
			if (base.GradientUnits == SvgCoordinateUnits.ObjectBoundingBox)
			{
				renderer.PopBoundable();
			}
		}
	}

	private float CalcScale(RectangleF bounds, GraphicsPath path, Graphics graphics = null)
	{
		PointF[] array = new PointF[4]
		{
			new PointF(bounds.Left, bounds.Top),
			new PointF(bounds.Right, bounds.Top),
			new PointF(bounds.Right, bounds.Bottom),
			new PointF(bounds.Left, bounds.Bottom)
		};
		RectangleF bounds2 = path.GetBounds();
		PointF pointF = new PointF(bounds2.X + bounds2.Width / 2f, bounds2.Y + bounds2.Height / 2f);
		using (Matrix matrix = new Matrix())
		{
			matrix.Translate(-1f * pointF.X, -1f * pointF.Y, MatrixOrder.Append);
			matrix.Scale(0.95f, 0.95f, MatrixOrder.Append);
			matrix.Translate(pointF.X, pointF.Y, MatrixOrder.Append);
			while (!path.IsVisible(array[0]) || !path.IsVisible(array[1]) || !path.IsVisible(array[2]) || !path.IsVisible(array[3]))
			{
				PointF[] first = new PointF[4]
				{
					new PointF(array[0].X, array[0].Y),
					new PointF(array[1].X, array[1].Y),
					new PointF(array[2].X, array[2].Y),
					new PointF(array[3].X, array[3].Y)
				};
				matrix.TransformPoints(array);
				if (Enumerable.SequenceEqual(first, array))
				{
					break;
				}
			}
		}
		return bounds.Height / (array[2].Y - array[1].Y);
	}

	private static IEnumerable<GraphicsPath> GetDifference(RectangleF subject, GraphicsPath clip)
	{
		GraphicsPath obj = (GraphicsPath)clip.Clone();
		obj.Flatten();
		RectangleF bounds = obj.GetBounds();
		RectangleF rectangleF = RectangleF.Union(subject, bounds);
		rectangleF.Inflate(rectangleF.Width * 0.3f, rectangleF.Height * 0.3f);
		PointF pointF = new PointF((bounds.Left + bounds.Right) / 2f, (bounds.Top + bounds.Bottom) / 2f);
		List<PointF> list = new List<PointF>();
		List<PointF> rightPoints = new List<PointF>();
		PointF[] pathPoints = obj.PathPoints;
		for (int i = 0; i < pathPoints.Length; i++)
		{
			PointF item = pathPoints[i];
			if (item.X <= pointF.X)
			{
				list.Add(item);
			}
			else
			{
				rightPoints.Add(item);
			}
		}
		list.Sort((PointF p, PointF q) => p.Y.CompareTo(q.Y));
		rightPoints.Sort((PointF p, PointF q) => p.Y.CompareTo(q.Y));
		PointF item2 = new PointF((list.Last().X + rightPoints.Last().X) / 2f, (list.Last().Y + rightPoints.Last().Y) / 2f);
		list.Add(item2);
		rightPoints.Add(item2);
		item2 = new PointF(item2.X, rectangleF.Bottom);
		list.Add(item2);
		rightPoints.Add(item2);
		list.Add(new PointF(rectangleF.Left, rectangleF.Bottom));
		list.Add(new PointF(rectangleF.Left, rectangleF.Top));
		rightPoints.Add(new PointF(rectangleF.Right, rectangleF.Bottom));
		rightPoints.Add(new PointF(rectangleF.Right, rectangleF.Top));
		item2 = new PointF((list[0].X + rightPoints[0].X) / 2f, rectangleF.Top);
		list.Add(item2);
		rightPoints.Add(item2);
		item2 = new PointF(item2.X, (list[0].Y + rightPoints[0].Y) / 2f);
		list.Add(item2);
		rightPoints.Add(item2);
		GraphicsPath path = new GraphicsPath(FillMode.Winding);
		path.AddPolygon(list.ToArray());
		yield return path;
		path.Reset();
		path.AddPolygon(rightPoints.ToArray());
		yield return path;
	}

	private static GraphicsPath CreateGraphicsPath(PointF origin, PointF centerPoint, float effectiveRadius)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddEllipse(origin.X + centerPoint.X - effectiveRadius, origin.Y + centerPoint.Y - effectiveRadius, effectiveRadius * 2f, effectiveRadius * 2f);
		return graphicsPath;
	}

	private ColorBlend CalculateColorBlend(ISvgRenderer renderer, float opacity, float scale, out float outScale)
	{
		ColorBlend colorBlend = GetColorBlend(renderer, opacity, radial: true);
		outScale = scale;
		if (scale > 1f)
		{
			float newScale;
			switch (base.SpreadMethod)
			{
			case SvgGradientSpreadMethod.Reflect:
			{
				newScale = (float)Math.Ceiling(scale);
				List<float> list = colorBlend.Positions.Select((float p) => 1f + (p - 1f) / newScale).ToList();
				List<Color> list2 = colorBlend.Colors.ToList();
				for (int num = 1; (float)num < newScale; num++)
				{
					if (num % 2 == 1)
					{
						for (int num2 = 1; num2 < colorBlend.Positions.Length; num2++)
						{
							list.Insert(0, (newScale - (float)num - 1f) / newScale + 1f - colorBlend.Positions[num2]);
							list2.Insert(0, colorBlend.Colors[num2]);
						}
					}
					else
					{
						for (int num3 = 0; num3 < colorBlend.Positions.Length - 1; num3++)
						{
							list.Insert(num3, (newScale - (float)num - 1f) / newScale + colorBlend.Positions[num3]);
							list2.Insert(num3, colorBlend.Colors[num3]);
						}
					}
				}
				colorBlend.Positions = list.ToArray();
				colorBlend.Colors = list2.ToArray();
				outScale = newScale;
				break;
			}
			case SvgGradientSpreadMethod.Repeat:
			{
				newScale = (float)Math.Ceiling(scale);
				List<float> list = colorBlend.Positions.Select((float p) => p / newScale).ToList();
				List<Color> list2 = colorBlend.Colors.ToList();
				int i;
				for (i = 1; (float)i < newScale; i++)
				{
					list.AddRange(colorBlend.Positions.Select((float p) => ((float)i + ((p <= 0f) ? 0.001f : p)) / newScale));
					list2.AddRange(colorBlend.Colors);
				}
				colorBlend.Positions = list.ToArray();
				colorBlend.Colors = list2.ToArray();
				outScale = newScale;
				break;
			}
			default:
				outScale = 1f;
				break;
			}
		}
		return colorBlend;
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgRadialGradientServerProperty in SvgRadialGradientServerProperties)
		{
			yield return svgRadialGradientServerProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgRadialGradientServerProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgRadialGradientServerProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgRadialGradientServerProperties.TryGetValue(attributeName, out var value2))
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
