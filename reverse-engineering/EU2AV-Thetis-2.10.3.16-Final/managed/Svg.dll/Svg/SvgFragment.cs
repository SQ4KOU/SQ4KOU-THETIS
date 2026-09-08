#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using Svg.DataTypes;

namespace Svg;

[SvgElement("svg")]
public class SvgFragment : SvgElement, ISvgViewPort, ISvgBoundable
{
	private SvgUnit _x = 0f;

	private SvgUnit _y = 0f;

	public static readonly Uri Namespace = new Uri("http://www.w3.org/2000/svg");

	internal static List<Type> SvgFragmentClassNames = new List<Type> { typeof(SvgFragment) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgFragmentProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["x"] = new SvgPropertyDescriptor<SvgFragment, SvgUnit>(DescriptorType.Property, "x", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgFragment t) => t.X, delegate(SvgFragment t, SvgUnit v)
		{
			t.X = v;
		}),
		["y"] = new SvgPropertyDescriptor<SvgFragment, SvgUnit>(DescriptorType.Property, "y", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgFragment t) => t.Y, delegate(SvgFragment t, SvgUnit v)
		{
			t.Y = v;
		}),
		["width"] = new SvgPropertyDescriptor<SvgFragment, SvgUnit>(DescriptorType.Property, "width", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgFragment t) => t.Width, delegate(SvgFragment t, SvgUnit v)
		{
			t.Width = v;
		}),
		["height"] = new SvgPropertyDescriptor<SvgFragment, SvgUnit>(DescriptorType.Property, "height", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgFragment t) => t.Height, delegate(SvgFragment t, SvgUnit v)
		{
			t.Height = v;
		}),
		["overflow"] = new SvgPropertyDescriptor<SvgFragment, SvgOverflow>(DescriptorType.Property, "overflow", "http://www.w3.org/2000/svg", new SvgOverflowConverter(), (SvgFragment t) => t.Overflow, delegate(SvgFragment t, SvgOverflow v)
		{
			t.Overflow = v;
		}),
		["viewBox"] = new SvgPropertyDescriptor<SvgFragment, SvgViewBox>(DescriptorType.Property, "viewBox", "http://www.w3.org/2000/svg", new SvgViewBoxConverter(), (SvgFragment t) => t.ViewBox, delegate(SvgFragment t, SvgViewBox v)
		{
			t.ViewBox = v;
		}),
		["preserveAspectRatio"] = new SvgPropertyDescriptor<SvgFragment, SvgAspectRatio>(DescriptorType.Property, "preserveAspectRatio", "http://www.w3.org/2000/svg", new SvgPreserveAspectRatioConverter(), (SvgFragment t) => t.AspectRatio, delegate(SvgFragment t, SvgAspectRatio v)
		{
			t.AspectRatio = v;
		}),
		["font-size"] = new SvgPropertyDescriptor<SvgFragment, SvgUnit>(DescriptorType.Property, "font-size", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgFragment t) => t.FontSize, delegate(SvgFragment t, SvgUnit v)
		{
			t.FontSize = v;
		}),
		["font-family"] = new SvgPropertyDescriptor<SvgFragment, string>(DescriptorType.Property, "font-family", "http://www.w3.org/2000/svg", new StringConverter(), (SvgFragment t) => t.FontFamily, delegate(SvgFragment t, string v)
		{
			t.FontFamily = v;
		})
	};

	[SvgAttribute("x")]
	public virtual SvgUnit X
	{
		get
		{
			return _x;
		}
		set
		{
			if (_x != value)
			{
				_x = value;
			}
			Attributes["x"] = value;
		}
	}

	[SvgAttribute("y")]
	public virtual SvgUnit Y
	{
		get
		{
			return _y;
		}
		set
		{
			if (_y != value)
			{
				_y = value;
			}
			Attributes["y"] = value;
		}
	}

	[SvgAttribute("width")]
	public SvgUnit Width
	{
		get
		{
			return GetAttribute("width", inherited: false, new SvgUnit(SvgUnitType.Percentage, 100f));
		}
		set
		{
			Attributes["width"] = value;
		}
	}

	[SvgAttribute("height")]
	public SvgUnit Height
	{
		get
		{
			return GetAttribute("height", inherited: false, new SvgUnit(SvgUnitType.Percentage, 100f));
		}
		set
		{
			Attributes["height"] = value;
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
	public SvgViewBox ViewBox
	{
		get
		{
			return GetAttribute("viewBox", inherited: false, SvgViewBox.Empty);
		}
		set
		{
			Attributes["viewBox"] = value;
		}
	}

	[SvgAttribute("preserveAspectRatio")]
	public SvgAspectRatio AspectRatio
	{
		get
		{
			return GetAttribute("preserveAspectRatio", inherited: false, new SvgAspectRatio(SvgPreserveAspectRatio.xMidYMid));
		}
		set
		{
			Attributes["preserveAspectRatio"] = value;
		}
	}

	[SvgAttribute("font-size")]
	public override SvgUnit FontSize
	{
		get
		{
			return GetAttribute("font-size", inherited: true, SvgUnit.Empty);
		}
		set
		{
			Attributes["font-size"] = value;
		}
	}

	[SvgAttribute("font-family")]
	public override string FontFamily
	{
		get
		{
			return GetAttribute<string>("font-family", inherited: true);
		}
		set
		{
			Attributes["font-family"] = value;
		}
	}

	public override XmlSpaceHandling SpaceHandling
	{
		get
		{
			return GetAttribute("space", inherited: true, XmlSpaceHandling.Default);
		}
		set
		{
			base.SpaceHandling = value;
			IsPathDirty = true;
		}
	}

	PointF ISvgBoundable.Location => PointF.Empty;

	SizeF ISvgBoundable.Size
	{
		get
		{
			if (Width.Type == SvgUnitType.Percentage || Height.Type == SvgUnitType.Percentage)
			{
				return default(SizeF);
			}
			return GetDimensions();
		}
	}

	RectangleF ISvgBoundable.Bounds => new RectangleF(((ISvgBoundable)this).Location, ((ISvgBoundable)this).Size);

	public GraphicsPath Path
	{
		get
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			AddPaths(this, graphicsPath);
			return graphicsPath;
		}
	}

	public RectangleF Bounds
	{
		get
		{
			RectangleF rectangleF = default(RectangleF);
			foreach (SvgElement child in Children)
			{
				RectangleF rectangleF2 = default(RectangleF);
				if (child is SvgFragment)
				{
					rectangleF2 = ((SvgFragment)child).Bounds;
					rectangleF2.Offset(((SvgFragment)child).X, ((SvgFragment)child).Y);
				}
				else if (child is SvgVisualElement)
				{
					rectangleF2 = ((SvgVisualElement)child).Bounds;
				}
				if (!rectangleF2.IsEmpty)
				{
					rectangleF = ((!rectangleF.IsEmpty) ? RectangleF.Union(rectangleF, rectangleF2) : rectangleF2);
				}
			}
			return TransformedBounds(rectangleF);
		}
	}

	internal override string AttributeName => "svg";

	internal override List<Type> ClassNames => SvgFragmentClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgFragmentProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgFragment>();
	}

	public override SvgElement DeepCopy<T>()
	{
		SvgFragment obj = base.DeepCopy<T>() as SvgFragment;
		obj._x = _x;
		obj._y = _y;
		return obj;
	}

	protected internal override bool PushTransforms(ISvgRenderer renderer)
	{
		if (!base.PushTransforms(renderer))
		{
			return false;
		}
		ViewBox.AddViewBoxTransform(AspectRatio, renderer, this);
		return true;
	}

	protected override void Render(ISvgRenderer renderer)
	{
		SvgOverflow overflow = Overflow;
		if ((uint)(overflow - 1) <= 2u)
		{
			base.Render(renderer);
			return;
		}
		Region clip = renderer.GetClip();
		try
		{
			SizeF sizeF = ((this is SvgDocument) ? renderer.GetBoundable().Bounds.Size : GetDimensions(renderer));
			RectangleF rect = new RectangleF(X.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), Y.ToDeviceValue(renderer, UnitRenderingType.Vertical, this), sizeF.Width, sizeF.Height);
			renderer.SetClip(new Region(rect), CombineMode.Intersect);
			try
			{
				renderer.SetBoundable(new GenericBoundable(rect));
				base.Render(renderer);
			}
			finally
			{
				renderer.PopBoundable();
			}
		}
		finally
		{
			renderer.SetClip(clip);
		}
	}

	public SizeF GetDimensions()
	{
		return GetDimensions(null);
	}

	internal SizeF GetDimensions(ISvgRenderer renderer)
	{
		bool num = Width.Type == SvgUnitType.Percentage;
		bool flag = Height.Type == SvgUnitType.Percentage;
		RectangleF rectangleF = default(RectangleF);
		if (num | flag)
		{
			rectangleF = ((!(ViewBox.Width > 0f) || !(ViewBox.Height > 0f)) ? Bounds : new RectangleF(ViewBox.MinX, ViewBox.MinY, ViewBox.Width, ViewBox.Height));
		}
		float width = ((!num || !(this is SvgDocument)) ? Width.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this) : ((rectangleF.Width + rectangleF.X) * (Width.Value * 0.01f)));
		float height = ((!flag || !(this is SvgDocument)) ? Height.ToDeviceValue(renderer, UnitRenderingType.Vertical, this) : ((rectangleF.Height + rectangleF.Y) * (Height.Value * 0.01f)));
		return new SizeF(width, height);
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgFragmentProperty in SvgFragmentProperties)
		{
			yield return svgFragmentProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgFragmentProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgFragmentProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgFragmentProperties.TryGetValue(attributeName, out var value2))
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
