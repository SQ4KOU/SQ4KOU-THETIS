#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace Svg;

[SvgElement("rect")]
public class SvgRectangle : SvgPathBasedElement
{
	private SvgUnit _x = 0f;

	private SvgUnit _y = 0f;

	private SvgUnit _width = 0f;

	private SvgUnit _height = 0f;

	private SvgUnit _cornerRadiusX = 0f;

	private SvgUnit _cornerRadiusY = 0f;

	private GraphicsPath _path;

	internal static List<Type> SvgRectangleClassNames = new List<Type> { typeof(SvgRectangle) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgRectangleProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["x"] = new SvgPropertyDescriptor<SvgRectangle, SvgUnit>(DescriptorType.Property, "x", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgRectangle t) => t.X, delegate(SvgRectangle t, SvgUnit v)
		{
			t.X = v;
		}),
		["y"] = new SvgPropertyDescriptor<SvgRectangle, SvgUnit>(DescriptorType.Property, "y", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgRectangle t) => t.Y, delegate(SvgRectangle t, SvgUnit v)
		{
			t.Y = v;
		}),
		["width"] = new SvgPropertyDescriptor<SvgRectangle, SvgUnit>(DescriptorType.Property, "width", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgRectangle t) => t.Width, delegate(SvgRectangle t, SvgUnit v)
		{
			t.Width = v;
		}),
		["height"] = new SvgPropertyDescriptor<SvgRectangle, SvgUnit>(DescriptorType.Property, "height", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgRectangle t) => t.Height, delegate(SvgRectangle t, SvgUnit v)
		{
			t.Height = v;
		}),
		["rx"] = new SvgPropertyDescriptor<SvgRectangle, SvgUnit>(DescriptorType.Property, "rx", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgRectangle t) => t.CornerRadiusX, delegate(SvgRectangle t, SvgUnit v)
		{
			t.CornerRadiusX = v;
		}),
		["ry"] = new SvgPropertyDescriptor<SvgRectangle, SvgUnit>(DescriptorType.Property, "ry", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgRectangle t) => t.CornerRadiusY, delegate(SvgRectangle t, SvgUnit v)
		{
			t.CornerRadiusY = v;
		})
	};

	public SvgPoint Location => new SvgPoint(X, Y);

	[SvgAttribute("x")]
	public SvgUnit X
	{
		get
		{
			return _x;
		}
		set
		{
			_x = value;
			Attributes["x"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("y")]
	public SvgUnit Y
	{
		get
		{
			return _y;
		}
		set
		{
			_y = value;
			Attributes["y"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("width")]
	public SvgUnit Width
	{
		get
		{
			return _width;
		}
		set
		{
			_width = value;
			Attributes["width"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("height")]
	public SvgUnit Height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = value;
			Attributes["height"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("rx")]
	public SvgUnit CornerRadiusX
	{
		get
		{
			if (_cornerRadiusX.Value != 0f || !(_cornerRadiusY.Value > 0f))
			{
				return _cornerRadiusX;
			}
			return _cornerRadiusY;
		}
		set
		{
			_cornerRadiusX = value;
			Attributes["rx"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("ry")]
	public SvgUnit CornerRadiusY
	{
		get
		{
			if (_cornerRadiusY.Value != 0f || !(_cornerRadiusX.Value > 0f))
			{
				return _cornerRadiusY;
			}
			return _cornerRadiusX;
		}
		set
		{
			_cornerRadiusY = value;
			Attributes["ry"] = value;
			IsPathDirty = true;
		}
	}

	internal override string AttributeName => "rect";

	internal override List<Type> ClassNames => SvgRectangleClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgRectangleProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgRectangle>();
	}

	public override SvgElement DeepCopy<T>()
	{
		SvgRectangle obj = base.DeepCopy<T>() as SvgRectangle;
		obj._x = _x;
		obj._y = _y;
		obj._width = _width;
		obj._height = _height;
		obj._cornerRadiusX = _cornerRadiusX;
		obj._cornerRadiusY = _cornerRadiusY;
		return obj;
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		if (_path == null || IsPathDirty)
		{
			float num = (float)base.StrokeWidth / 2f;
			if (renderer != null)
			{
				num = 0f;
				IsPathDirty = false;
			}
			if (renderer == null || (CornerRadiusX.Value == 0f && CornerRadiusY.Value == 0f))
			{
				float num2 = Location.Y.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
				float num3 = Location.X.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
				SvgPoint svgPoint = new SvgPoint(num3 - num, num2 - num);
				float width = Width.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this) + num * 2f;
				float height = Height.ToDeviceValue(renderer, UnitRenderingType.Vertical, this) + num * 2f;
				PointF location = svgPoint.ToDeviceValue(renderer, this);
				RectangleF rect = new RectangleF(location, new SizeF(width, height));
				_path = new GraphicsPath();
				_path.StartFigure();
				_path.AddRectangle(rect);
				_path.CloseFigure();
			}
			else
			{
				_path = new GraphicsPath();
				RectangleF rect2 = default(RectangleF);
				PointF pt = default(PointF);
				PointF pt2 = default(PointF);
				float num4 = Width.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
				float num5 = Height.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
				float num6 = Math.Min(CornerRadiusX.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this) * 2f, num4);
				float num7 = Math.Min(CornerRadiusY.ToDeviceValue(renderer, UnitRenderingType.Vertical, this) * 2f, num5);
				PointF location2 = Location.ToDeviceValue(renderer, this);
				_path.StartFigure();
				rect2.Location = location2;
				rect2.Width = num6;
				rect2.Height = num7;
				_path.AddArc(rect2, 180f, 90f);
				pt.X = Math.Min(location2.X + num6, location2.X + num4 * 0.5f);
				pt.Y = location2.Y;
				pt2.X = Math.Max(location2.X + num4 - num6, location2.X + num4 * 0.5f);
				pt2.Y = pt.Y;
				_path.AddLine(pt, pt2);
				rect2.Location = new PointF(location2.X + num4 - num6, location2.Y);
				_path.AddArc(rect2, 270f, 90f);
				pt.X = location2.X + num4;
				pt.Y = Math.Min(location2.Y + num7, location2.Y + num5 * 0.5f);
				pt2.X = pt.X;
				pt2.Y = Math.Max(location2.Y + num5 - num7, location2.Y + num5 * 0.5f);
				_path.AddLine(pt, pt2);
				rect2.Location = new PointF(location2.X + num4 - num6, location2.Y + num5 - num7);
				_path.AddArc(rect2, 0f, 90f);
				pt.X = Math.Max(location2.X + num4 - num6, location2.X + num4 * 0.5f);
				pt.Y = location2.Y + num5;
				pt2.X = Math.Min(location2.X + num6, location2.X + num4 * 0.5f);
				pt2.Y = pt.Y;
				_path.AddLine(pt, pt2);
				rect2.Location = new PointF(location2.X, location2.Y + num5 - num7);
				_path.AddArc(rect2, 90f, 90f);
				pt.X = location2.X;
				pt.Y = Math.Max(location2.Y + num5 - num7, location2.Y + num5 * 0.5f);
				pt2.X = pt.X;
				pt2.Y = Math.Min(location2.Y + num7, location2.Y + num5 * 0.5f);
				_path.AddLine(pt, pt2);
				_path.CloseFigure();
			}
		}
		return _path;
	}

	protected override void Render(ISvgRenderer renderer)
	{
		if (Width.Value > 0f && Height.Value > 0f)
		{
			base.Render(renderer);
		}
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgRectangleProperty in SvgRectangleProperties)
		{
			yield return svgRectangleProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgRectangleProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgRectangleProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgRectangleProperties.TryGetValue(attributeName, out var value2))
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
