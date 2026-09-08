#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace Svg;

[SvgElement("polyline")]
public class SvgPolyline : SvgPolygon
{
	private GraphicsPath _path;

	internal static List<Type> SvgPolylineClassNames = new List<Type> { typeof(SvgPolyline) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgPolylineProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "polyline";

	internal override List<Type> ClassNames => SvgPolylineClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgPolylineProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgPolyline>();
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		if (_path == null || IsPathDirty)
		{
			_path = new GraphicsPath();
			try
			{
				for (int i = 0; i + 1 < base.Points.Count; i += 2)
				{
					PointF pointF = new PointF(base.Points[i].ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), base.Points[i + 1].ToDeviceValue(renderer, UnitRenderingType.Vertical, this));
					if (renderer == null)
					{
						float num = (float)base.StrokeWidth / 2f;
						_path.AddEllipse(pointF.X - num, pointF.Y - num, 2f * num, 2f * num);
					}
					else if (_path.PointCount == 0)
					{
						_path.AddLine(pointF, pointF);
					}
					else
					{
						_path.AddLine(_path.GetLastPoint(), pointF);
					}
				}
			}
			catch (Exception ex)
			{
				Trace.TraceError("Error rendering points: " + ex.Message);
			}
			if (renderer != null)
			{
				IsPathDirty = false;
			}
		}
		return _path;
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgPolylineProperty in SvgPolylineProperties)
		{
			yield return svgPolylineProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgPolylineProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgPolylineProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgPolylineProperties.TryGetValue(attributeName, out var value2))
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
