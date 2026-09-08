#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace Svg;

[SvgElement("polygon")]
public class SvgPolygon : SvgMarkerElement
{
	private GraphicsPath _path;

	internal static List<Type> SvgPolygonClassNames = new List<Type> { typeof(SvgPolygon) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgPolygonProperties = new Dictionary<string, ISvgPropertyDescriptor> { ["points"] = new SvgPropertyDescriptor<SvgPolygon, SvgPointCollection>(DescriptorType.Property, "points", "http://www.w3.org/2000/svg", new SvgPointCollectionConverter(), (SvgPolygon t) => t.Points, delegate(SvgPolygon t, SvgPointCollection v)
	{
		t.Points = v;
	}) };

	[SvgAttribute("points")]
	public SvgPointCollection Points
	{
		get
		{
			return GetAttribute<SvgPointCollection>("points", inherited: false);
		}
		set
		{
			Attributes["points"] = value;
			IsPathDirty = true;
		}
	}

	internal override string AttributeName => "polygon";

	internal override List<Type> ClassNames => SvgPolygonClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgPolygonProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgPolygon>();
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		if (_path == null || IsPathDirty)
		{
			_path = new GraphicsPath();
			_path.StartFigure();
			try
			{
				SvgPointCollection points = Points;
				for (int i = 0; i + 1 < points.Count; i += 2)
				{
					PointF devicePoint = SvgUnit.GetDevicePoint(points[i], points[i + 1], renderer, this);
					if (renderer == null)
					{
						float num = (float)base.StrokeWidth * 2f;
						_path.AddEllipse(devicePoint.X - num, devicePoint.Y - num, 2f * num, 2f * num);
					}
					else if (i != 0)
					{
						if (_path.PointCount == 0)
						{
							_path.AddLine(SvgUnit.GetDevicePoint(points[i - 2], points[i - 1], renderer, this), devicePoint);
						}
						else
						{
							_path.AddLine(_path.GetLastPoint(), devicePoint);
						}
					}
				}
			}
			catch
			{
				Trace.TraceError("Error parsing points");
			}
			_path.CloseFigure();
			if (renderer != null)
			{
				IsPathDirty = false;
			}
		}
		return _path;
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgPolygonProperty in SvgPolygonProperties)
		{
			yield return svgPolygonProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgPolygonProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgPolygonProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgPolygonProperties.TryGetValue(attributeName, out var value2))
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
