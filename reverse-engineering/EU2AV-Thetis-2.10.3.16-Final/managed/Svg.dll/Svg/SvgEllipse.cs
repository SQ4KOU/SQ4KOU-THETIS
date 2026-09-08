#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace Svg;

[SvgElement("ellipse")]
public class SvgEllipse : SvgPathBasedElement
{
	private SvgUnit _centerX = 0f;

	private SvgUnit _centerY = 0f;

	private SvgUnit _radiusX = 0f;

	private SvgUnit _radiusY = 0f;

	private GraphicsPath _path;

	internal static List<Type> SvgEllipseClassNames = new List<Type> { typeof(SvgEllipse) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgEllipseProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["cx"] = new SvgPropertyDescriptor<SvgEllipse, SvgUnit>(DescriptorType.Property, "cx", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgEllipse t) => t.CenterX, delegate(SvgEllipse t, SvgUnit v)
		{
			t.CenterX = v;
		}),
		["cy"] = new SvgPropertyDescriptor<SvgEllipse, SvgUnit>(DescriptorType.Property, "cy", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgEllipse t) => t.CenterY, delegate(SvgEllipse t, SvgUnit v)
		{
			t.CenterY = v;
		}),
		["rx"] = new SvgPropertyDescriptor<SvgEllipse, SvgUnit>(DescriptorType.Property, "rx", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgEllipse t) => t.RadiusX, delegate(SvgEllipse t, SvgUnit v)
		{
			t.RadiusX = v;
		}),
		["ry"] = new SvgPropertyDescriptor<SvgEllipse, SvgUnit>(DescriptorType.Property, "ry", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgEllipse t) => t.RadiusY, delegate(SvgEllipse t, SvgUnit v)
		{
			t.RadiusY = v;
		})
	};

	[SvgAttribute("cx")]
	public virtual SvgUnit CenterX
	{
		get
		{
			return _centerX;
		}
		set
		{
			_centerX = value;
			Attributes["cx"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("cy")]
	public virtual SvgUnit CenterY
	{
		get
		{
			return _centerY;
		}
		set
		{
			_centerY = value;
			Attributes["cy"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("rx")]
	public virtual SvgUnit RadiusX
	{
		get
		{
			return _radiusX;
		}
		set
		{
			_radiusX = value;
			Attributes["rx"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("ry")]
	public virtual SvgUnit RadiusY
	{
		get
		{
			return _radiusY;
		}
		set
		{
			_radiusY = value;
			Attributes["ry"] = value;
			IsPathDirty = true;
		}
	}

	internal override string AttributeName => "ellipse";

	internal override List<Type> ClassNames => SvgEllipseClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgEllipseProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgEllipse>();
	}

	public override SvgElement DeepCopy<T>()
	{
		SvgEllipse obj = base.DeepCopy<T>() as SvgEllipse;
		obj._centerX = _centerX;
		obj._centerY = _centerY;
		obj._radiusX = _radiusX;
		obj._radiusY = _radiusY;
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
			PointF devicePoint = SvgUnit.GetDevicePoint(CenterX, CenterY, renderer, this);
			float num2 = RadiusX.ToDeviceValue(renderer, UnitRenderingType.Other, this) + num;
			float num3 = RadiusY.ToDeviceValue(renderer, UnitRenderingType.Other, this) + num;
			_path = new GraphicsPath();
			_path.StartFigure();
			_path.AddEllipse(devicePoint.X - num2, devicePoint.Y - num3, 2f * num2, 2f * num3);
			_path.CloseFigure();
		}
		return _path;
	}

	protected override void Render(ISvgRenderer renderer)
	{
		if (RadiusX.Value > 0f && RadiusY.Value > 0f)
		{
			base.Render(renderer);
		}
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgEllipseProperty in SvgEllipseProperties)
		{
			yield return svgEllipseProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgEllipseProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgEllipseProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgEllipseProperties.TryGetValue(attributeName, out var value2))
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
