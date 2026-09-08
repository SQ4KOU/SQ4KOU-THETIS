#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace Svg;

[SvgElement("circle")]
public class SvgCircle : SvgPathBasedElement
{
	private SvgUnit _centerX = 0f;

	private SvgUnit _centerY = 0f;

	private SvgUnit _radius = 0f;

	private GraphicsPath _path;

	internal static List<Type> SvgCircleClassNames = new List<Type> { typeof(SvgCircle) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgCircleProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["cx"] = new SvgPropertyDescriptor<SvgCircle, SvgUnit>(DescriptorType.Property, "cx", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgCircle t) => t.CenterX, delegate(SvgCircle t, SvgUnit v)
		{
			t.CenterX = v;
		}),
		["cy"] = new SvgPropertyDescriptor<SvgCircle, SvgUnit>(DescriptorType.Property, "cy", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgCircle t) => t.CenterY, delegate(SvgCircle t, SvgUnit v)
		{
			t.CenterY = v;
		}),
		["r"] = new SvgPropertyDescriptor<SvgCircle, SvgUnit>(DescriptorType.Property, "r", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgCircle t) => t.Radius, delegate(SvgCircle t, SvgUnit v)
		{
			t.Radius = v;
		})
	};

	public SvgPoint Center => new SvgPoint(CenterX, CenterY);

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

	[SvgAttribute("r")]
	public virtual SvgUnit Radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
			Attributes["r"] = value;
			IsPathDirty = true;
		}
	}

	internal override string AttributeName => "circle";

	internal override List<Type> ClassNames => SvgCircleClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgCircleProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgCircle>();
	}

	public override SvgElement DeepCopy<T>()
	{
		SvgCircle obj = base.DeepCopy<T>() as SvgCircle;
		obj._centerX = _centerX;
		obj._centerY = _centerY;
		obj._radius = _radius;
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
			_path = new GraphicsPath();
			_path.StartFigure();
			PointF pointF = Center.ToDeviceValue(renderer, this);
			float num2 = Radius.ToDeviceValue(renderer, UnitRenderingType.Other, this) + num;
			_path.AddEllipse(pointF.X - num2, pointF.Y - num2, 2f * num2, 2f * num2);
			_path.CloseFigure();
		}
		return _path;
	}

	protected override void Render(ISvgRenderer renderer)
	{
		if (Radius.Value > 0f)
		{
			base.Render(renderer);
		}
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgCircleProperty in SvgCircleProperties)
		{
			yield return svgCircleProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgCircleProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgCircleProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgCircleProperties.TryGetValue(attributeName, out var value2))
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
