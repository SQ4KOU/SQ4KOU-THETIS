#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using Svg.Pathing;

namespace Svg;

[SvgElement("path")]
public class SvgPath : SvgMarkerElement, ISvgPathElement
{
	private GraphicsPath _path;

	internal static List<Type> SvgPathClassNames = new List<Type> { typeof(SvgPath) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgPathProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["d"] = new SvgPropertyDescriptor<SvgPath, SvgPathSegmentList>(DescriptorType.Property, "d", "http://www.w3.org/2000/svg", new SvgPathBuilder(), (SvgPath t) => t.PathData, delegate(SvgPath t, SvgPathSegmentList v)
		{
			t.PathData = v;
		}),
		["pathLength"] = new SvgPropertyDescriptor<SvgPath, float>(DescriptorType.Property, "pathLength", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgPath t) => t.PathLength, delegate(SvgPath t, float v)
		{
			t.PathLength = v;
		})
	};

	[SvgAttribute("d")]
	public SvgPathSegmentList PathData
	{
		get
		{
			return GetAttribute<SvgPathSegmentList>("d", inherited: false);
		}
		set
		{
			SvgPathSegmentList pathData = PathData;
			if (pathData != null)
			{
				pathData.Owner = null;
			}
			Attributes["d"] = value;
			value.Owner = this;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("pathLength")]
	public float PathLength
	{
		get
		{
			return GetAttribute("pathLength", inherited: false, 0f);
		}
		set
		{
			Attributes["pathLength"] = value;
		}
	}

	internal override string AttributeName => "path";

	internal override List<Type> ClassNames => SvgPathClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgPathProperties;

	public void OnPathUpdated()
	{
		IsPathDirty = true;
		OnAttributeChanged(new AttributeEventArgs
		{
			Attribute = "d",
			Value = Attributes.GetAttribute<SvgPathSegmentList>("d")
		});
	}

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgPath>();
	}

	public override SvgElement DeepCopy<T>()
	{
		SvgPath svgPath = base.DeepCopy<T>() as SvgPath;
		if (svgPath.PathData != null)
		{
			svgPath.PathData.Owner = svgPath;
			svgPath.IsPathDirty = true;
		}
		return svgPath;
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		if (_path == null || IsPathDirty)
		{
			_path = new GraphicsPath();
			SvgPathSegmentList pathData = PathData;
			if (pathData != null && pathData.Count > 0 && pathData.First is SvgMoveToSegment)
			{
				PointF start = PointF.Empty;
				foreach (SvgPathSegment item in pathData)
				{
					start = item.AddToPath(_path, start, pathData);
				}
				if (_path.PointCount == 0)
				{
					if (pathData.Count > 0)
					{
						SvgPathSegment last = pathData.Last;
						_path.AddLine(last.End, last.End);
						Fill = SvgPaintServer.None;
						Stroke = SvgPaintServer.None;
					}
					else
					{
						_path = null;
					}
				}
				else if (renderer == null)
				{
					float num = (float)StrokeWidth * 2f;
					RectangleF bounds = _path.GetBounds();
					_path.AddEllipse(bounds.Left - num, bounds.Top - num, 2f * num, 2f * num);
					_path.AddEllipse(bounds.Right - num, bounds.Bottom - num, 2f * num, 2f * num);
				}
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
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgPathProperty in SvgPathProperties)
		{
			yield return svgPathProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgPathProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgPathProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgPathProperties.TryGetValue(attributeName, out var value2))
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
