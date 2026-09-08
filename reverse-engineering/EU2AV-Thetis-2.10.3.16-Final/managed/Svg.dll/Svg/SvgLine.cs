#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace Svg;

[SvgElement("line")]
public class SvgLine : SvgMarkerElement
{
	private SvgUnit _startX = 0f;

	private SvgUnit _startY = 0f;

	private SvgUnit _endX = 0f;

	private SvgUnit _endY = 0f;

	private GraphicsPath _path;

	internal static List<Type> SvgLineClassNames = new List<Type> { typeof(SvgLine) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgLineProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["x1"] = new SvgPropertyDescriptor<SvgLine, SvgUnit>(DescriptorType.Property, "x1", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgLine t) => t.StartX, delegate(SvgLine t, SvgUnit v)
		{
			t.StartX = v;
		}),
		["y1"] = new SvgPropertyDescriptor<SvgLine, SvgUnit>(DescriptorType.Property, "y1", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgLine t) => t.StartY, delegate(SvgLine t, SvgUnit v)
		{
			t.StartY = v;
		}),
		["x2"] = new SvgPropertyDescriptor<SvgLine, SvgUnit>(DescriptorType.Property, "x2", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgLine t) => t.EndX, delegate(SvgLine t, SvgUnit v)
		{
			t.EndX = v;
		}),
		["y2"] = new SvgPropertyDescriptor<SvgLine, SvgUnit>(DescriptorType.Property, "y2", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgLine t) => t.EndY, delegate(SvgLine t, SvgUnit v)
		{
			t.EndY = v;
		})
	};

	[SvgAttribute("x1")]
	public SvgUnit StartX
	{
		get
		{
			return _startX;
		}
		set
		{
			if (_startX != value)
			{
				_startX = value;
				IsPathDirty = true;
			}
			Attributes["x1"] = value;
		}
	}

	[SvgAttribute("y1")]
	public SvgUnit StartY
	{
		get
		{
			return _startY;
		}
		set
		{
			if (_startY != value)
			{
				_startY = value;
				IsPathDirty = true;
			}
			Attributes["y1"] = value;
		}
	}

	[SvgAttribute("x2")]
	public SvgUnit EndX
	{
		get
		{
			return _endX;
		}
		set
		{
			if (_endX != value)
			{
				_endX = value;
				IsPathDirty = true;
			}
			Attributes["x2"] = value;
		}
	}

	[SvgAttribute("y2")]
	public SvgUnit EndY
	{
		get
		{
			return _endY;
		}
		set
		{
			if (_endY != value)
			{
				_endY = value;
				IsPathDirty = true;
			}
			Attributes["y2"] = value;
		}
	}

	public override SvgPaintServer Fill
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	internal override string AttributeName => "line";

	internal override List<Type> ClassNames => SvgLineClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgLineProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgLine>();
	}

	public override SvgElement DeepCopy<T>()
	{
		SvgLine obj = base.DeepCopy<T>() as SvgLine;
		obj._startX = _startX;
		obj._startY = _startY;
		obj._endX = _endX;
		obj._endY = _endY;
		return obj;
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		if ((_path == null || IsPathDirty) && (float)base.StrokeWidth > 0f)
		{
			PointF pt = new PointF(StartX.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), StartY.ToDeviceValue(renderer, UnitRenderingType.Vertical, this));
			PointF pt2 = new PointF(EndX.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), EndY.ToDeviceValue(renderer, UnitRenderingType.Vertical, this));
			_path = new GraphicsPath();
			if (renderer != null)
			{
				_path.AddLine(pt, pt2);
				IsPathDirty = false;
			}
			else
			{
				_path.StartFigure();
				float num = (float)base.StrokeWidth / 2f;
				_path.AddEllipse(pt.X - num, pt.Y - num, 2f * num, 2f * num);
				_path.AddEllipse(pt2.X - num, pt2.Y - num, 2f * num, 2f * num);
				_path.CloseFigure();
			}
		}
		return _path;
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgLineProperty in SvgLineProperties)
		{
			yield return svgLineProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgLineProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgLineProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgLineProperties.TryGetValue(attributeName, out var value2))
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
