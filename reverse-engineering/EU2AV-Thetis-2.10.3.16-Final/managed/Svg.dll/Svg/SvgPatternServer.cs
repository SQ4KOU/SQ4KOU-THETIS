#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using Svg.DataTypes;
using Svg.Transforms;

namespace Svg;

[SvgElement("pattern")]
public class SvgPatternServer : SvgPaintServer, ISvgViewPort
{
	private SvgUnit _x = SvgUnit.None;

	private SvgUnit _y = SvgUnit.None;

	private SvgUnit _width = SvgUnit.None;

	private SvgUnit _height = SvgUnit.None;

	private SvgCoordinateUnits? _patternUnits;

	private SvgCoordinateUnits? _patternContentUnits;

	private SvgViewBox _viewBox;

	internal static List<Type> SvgPatternServerClassNames = new List<Type> { typeof(SvgPatternServer) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgPatternServerProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["x"] = new SvgPropertyDescriptor<SvgPatternServer, SvgUnit>(DescriptorType.Property, "x", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgPatternServer t) => t.X, delegate(SvgPatternServer t, SvgUnit v)
		{
			t.X = v;
		}),
		["y"] = new SvgPropertyDescriptor<SvgPatternServer, SvgUnit>(DescriptorType.Property, "y", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgPatternServer t) => t.Y, delegate(SvgPatternServer t, SvgUnit v)
		{
			t.Y = v;
		}),
		["width"] = new SvgPropertyDescriptor<SvgPatternServer, SvgUnit>(DescriptorType.Property, "width", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgPatternServer t) => t.Width, delegate(SvgPatternServer t, SvgUnit v)
		{
			t.Width = v;
		}),
		["height"] = new SvgPropertyDescriptor<SvgPatternServer, SvgUnit>(DescriptorType.Property, "height", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgPatternServer t) => t.Height, delegate(SvgPatternServer t, SvgUnit v)
		{
			t.Height = v;
		}),
		["patternUnits"] = new SvgPropertyDescriptor<SvgPatternServer, SvgCoordinateUnits>(DescriptorType.Property, "patternUnits", "http://www.w3.org/2000/svg", new SvgCoordinateUnitsConverter(), (SvgPatternServer t) => t.PatternUnits, delegate(SvgPatternServer t, SvgCoordinateUnits v)
		{
			t.PatternUnits = v;
		}),
		["patternContentUnits"] = new SvgPropertyDescriptor<SvgPatternServer, SvgCoordinateUnits>(DescriptorType.Property, "patternContentUnits", "http://www.w3.org/2000/svg", new SvgCoordinateUnitsConverter(), (SvgPatternServer t) => t.PatternContentUnits, delegate(SvgPatternServer t, SvgCoordinateUnits v)
		{
			t.PatternContentUnits = v;
		}),
		["viewBox"] = new SvgPropertyDescriptor<SvgPatternServer, SvgViewBox>(DescriptorType.Property, "viewBox", "http://www.w3.org/2000/svg", new SvgViewBoxConverter(), (SvgPatternServer t) => t.ViewBox, delegate(SvgPatternServer t, SvgViewBox v)
		{
			t.ViewBox = v;
		}),
		["href"] = new SvgPropertyDescriptor<SvgPatternServer, SvgDeferredPaintServer>(DescriptorType.Property, "href", "http://www.w3.org/1999/xlink", new SvgDeferredPaintServerFactory(), (SvgPatternServer t) => t.InheritGradient, delegate(SvgPatternServer t, SvgDeferredPaintServer v)
		{
			t.InheritGradient = v;
		}),
		["overflow"] = new SvgPropertyDescriptor<SvgPatternServer, SvgOverflow>(DescriptorType.Property, "overflow", "http://www.w3.org/2000/svg", new SvgOverflowConverter(), (SvgPatternServer t) => t.Overflow, delegate(SvgPatternServer t, SvgOverflow v)
		{
			t.Overflow = v;
		}),
		["preserveAspectRatio"] = new SvgPropertyDescriptor<SvgPatternServer, SvgAspectRatio>(DescriptorType.Property, "preserveAspectRatio", "http://www.w3.org/2000/svg", new SvgPreserveAspectRatioConverter(), (SvgPatternServer t) => t.AspectRatio, delegate(SvgPatternServer t, SvgAspectRatio v)
		{
			t.AspectRatio = v;
		}),
		["patternTransform"] = new SvgPropertyDescriptor<SvgPatternServer, SvgTransformCollection>(DescriptorType.Property, "patternTransform", "http://www.w3.org/2000/svg", new SvgTransformConverter(), (SvgPatternServer t) => t.PatternTransform, delegate(SvgPatternServer t, SvgTransformCollection v)
		{
			t.PatternTransform = v;
		})
	};

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
		}
	}

	[SvgAttribute("patternUnits")]
	public SvgCoordinateUnits PatternUnits
	{
		get
		{
			return _patternUnits.GetValueOrDefault();
		}
		set
		{
			_patternUnits = value;
			Attributes["patternUnits"] = value;
		}
	}

	[SvgAttribute("patternContentUnits")]
	public SvgCoordinateUnits PatternContentUnits
	{
		get
		{
			return _patternContentUnits ?? SvgCoordinateUnits.UserSpaceOnUse;
		}
		set
		{
			_patternContentUnits = value;
			Attributes["patternContentUnits"] = value;
		}
	}

	[SvgAttribute("viewBox")]
	public SvgViewBox ViewBox
	{
		get
		{
			return _viewBox;
		}
		set
		{
			_viewBox = value;
			Attributes["viewBox"] = value;
		}
	}

	[SvgAttribute("href", "http://www.w3.org/1999/xlink")]
	public SvgDeferredPaintServer InheritGradient
	{
		get
		{
			return GetAttribute<SvgDeferredPaintServer>("href", inherited: false);
		}
		set
		{
			Attributes["href"] = value;
		}
	}

	[SvgAttribute("overflow")]
	public SvgOverflow Overflow
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

	[SvgAttribute("patternTransform")]
	public SvgTransformCollection PatternTransform
	{
		get
		{
			return GetAttribute<SvgTransformCollection>("patternTransform", inherited: false);
		}
		set
		{
			Attributes["patternTransform"] = value;
		}
	}

	private Matrix EffectivePatternTransform
	{
		get
		{
			Matrix matrix = new Matrix();
			if (PatternTransform != null)
			{
				using Matrix matrix2 = PatternTransform.GetMatrix();
				matrix.Multiply(matrix2);
			}
			return matrix;
		}
	}

	internal override string AttributeName => "pattern";

	internal override List<Type> ClassNames => SvgPatternServerClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgPatternServerProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgPatternServer>();
	}

	public override SvgElement DeepCopy<T>()
	{
		SvgPatternServer obj = base.DeepCopy<T>() as SvgPatternServer;
		obj._x = _x;
		obj._y = _y;
		obj._width = _width;
		obj._height = _height;
		obj._patternUnits = _patternUnits;
		obj._patternContentUnits = _patternContentUnits;
		obj._viewBox = _viewBox;
		return obj;
	}

	public override Brush GetBrush(SvgVisualElement renderingElement, ISvgRenderer renderer, float opacity, bool forStroke = false)
	{
		List<SvgPatternServer> list = new List<SvgPatternServer>();
		SvgPatternServer svgPatternServer = this;
		do
		{
			list.Add(svgPatternServer);
			svgPatternServer = SvgDeferredPaintServer.TryGet<SvgPatternServer>(svgPatternServer.InheritGradient, renderingElement);
		}
		while (svgPatternServer != null);
		SvgPatternServer svgPatternServer2 = list.Find((SvgPatternServer p) => p.Children.Count > 0);
		if (svgPatternServer2 == null)
		{
			return null;
		}
		SvgPatternServer svgPatternServer3 = list.Find((SvgPatternServer p) => p.X != SvgUnit.None);
		SvgPatternServer svgPatternServer4 = list.Find((SvgPatternServer p) => p.Y != SvgUnit.None);
		SvgPatternServer svgPatternServer5 = list.Find((SvgPatternServer p) => p.Width != SvgUnit.None);
		SvgPatternServer svgPatternServer6 = list.Find((SvgPatternServer p) => p.Height != SvgUnit.None);
		if (svgPatternServer5 == null || svgPatternServer6 == null)
		{
			return null;
		}
		SvgPatternServer svgPatternServer7 = list.Find((SvgPatternServer p) => p._patternUnits.HasValue);
		SvgPatternServer svgPatternServer8 = list.Find((SvgPatternServer p) => p._patternContentUnits.HasValue);
		SvgPatternServer svgPatternServer9 = list.Find((SvgPatternServer p) => p.ViewBox != SvgViewBox.Empty);
		SvgUnit svgUnit = svgPatternServer3?.X ?? new SvgUnit(0f);
		SvgUnit svgUnit2 = svgPatternServer4?.Y ?? new SvgUnit(0f);
		SvgUnit width = svgPatternServer5.Width;
		SvgUnit height = svgPatternServer6.Height;
		SvgCoordinateUnits num = svgPatternServer7?.PatternUnits ?? SvgCoordinateUnits.ObjectBoundingBox;
		SvgCoordinateUnits svgCoordinateUnits = svgPatternServer8?.PatternContentUnits ?? SvgCoordinateUnits.UserSpaceOnUse;
		SvgViewBox svgViewBox = svgPatternServer9?.ViewBox ?? SvgViewBox.Empty;
		bool flag = num == SvgCoordinateUnits.ObjectBoundingBox;
		try
		{
			if (flag)
			{
				renderer.SetBoundable(renderingElement);
			}
			float num2 = svgUnit.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
			float num3 = svgUnit2.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
			float num4 = width.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
			float num5 = height.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
			if (flag)
			{
				RectangleF bounds = renderer.GetBoundable().Bounds;
				if (svgUnit.Type != SvgUnitType.Percentage)
				{
					num2 *= bounds.Width;
				}
				if (svgUnit2.Type != SvgUnitType.Percentage)
				{
					num3 *= bounds.Height;
				}
				if (width.Type != SvgUnitType.Percentage)
				{
					num4 *= bounds.Width;
				}
				if (height.Type != SvgUnitType.Percentage)
				{
					num5 *= bounds.Height;
				}
				num2 += bounds.X;
				num3 += bounds.Y;
			}
			if (num4 <= 0f || num5 <= 0f)
			{
				return null;
			}
			Bitmap image = new Bitmap((int)Math.Ceiling(num4), (int)Math.Ceiling(num5));
			using (ISvgRenderer svgRenderer = SvgRenderer.FromImage(image))
			{
				svgRenderer.SetBoundable(renderingElement);
				if (svgViewBox != SvgViewBox.Empty)
				{
					_ = svgRenderer.GetBoundable().Bounds;
					svgRenderer.ScaleTransform(num4 / svgViewBox.Width, num5 / svgViewBox.Height);
				}
				else if (svgCoordinateUnits == SvgCoordinateUnits.ObjectBoundingBox)
				{
					RectangleF bounds2 = svgRenderer.GetBoundable().Bounds;
					svgRenderer.ScaleTransform(bounds2.Width, bounds2.Height);
				}
				foreach (SvgElement child in svgPatternServer2.Children)
				{
					child.RenderElement(svgRenderer);
				}
			}
			using Matrix transform = EffectivePatternTransform;
			TextureBrush textureBrush = new TextureBrush(image, new RectangleF(0f, 0f, num4, num5));
			textureBrush.Transform = transform;
			textureBrush.TranslateTransform(num2, num3);
			return textureBrush;
		}
		finally
		{
			if (flag)
			{
				renderer.PopBoundable();
			}
		}
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgPatternServerProperty in SvgPatternServerProperties)
		{
			yield return svgPatternServerProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgPatternServerProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgPatternServerProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgPatternServerProperties.TryGetValue(attributeName, out var value2))
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
