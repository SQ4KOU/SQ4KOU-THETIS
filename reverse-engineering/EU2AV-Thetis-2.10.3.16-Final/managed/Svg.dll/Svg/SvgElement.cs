#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Xml;
using Svg.DataTypes;
using Svg.Transforms;

namespace Svg;

public abstract class SvgElement : ISvgElement, ISvgTransformable, ICloneable, ISvgNode
{
	private enum FontParseState
	{
		fontStyle,
		fontVariant,
		fontWeight,
		fontSize,
		fontFamilyNext,
		fontFamilyCurr
	}

	internal const int StyleSpecificity_PresAttribute = 0;

	internal const int StyleSpecificity_InlineStyle = 65536;

	internal SvgElement _parent;

	private string _elementName;

	private SvgAttributeCollection _attributes;

	private EventHandlerList _eventHandlers;

	private SvgElementCollection _children;

	private static readonly object _loadEventKey = new object();

	private SvgCustomAttributeCollection _customAttributes;

	private List<ISvgNode> _nodes = new List<ISvgNode>();

	private Dictionary<string, SortedDictionary<int, string>> _styles = new Dictionary<string, SortedDictionary<int, string>>();

	private string _content;

	public bool AutoPublishEvents = true;

	private Matrix _graphicsTransform;

	private Region _graphicsClip;

	private bool _dirty;

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgElementProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["color"] = new SvgPropertyDescriptor<SvgElement, SvgPaintServer>(DescriptorType.Property, "color", "http://www.w3.org/2000/svg", new SvgPaintServerFactory(), (SvgElement t) => t.Color, delegate(SvgElement t, SvgPaintServer v)
		{
			t.Color = v;
		}),
		["transform"] = new SvgPropertyDescriptor<SvgElement, SvgTransformCollection>(DescriptorType.Property, "transform", "http://www.w3.org/2000/svg", new SvgTransformConverter(), (SvgElement t) => t.Transforms, delegate(SvgElement t, SvgTransformCollection v)
		{
			t.Transforms = v;
		}),
		["id"] = new SvgPropertyDescriptor<SvgElement, string>(DescriptorType.Property, "id", "http://www.w3.org/2000/svg", new StringConverter(), (SvgElement t) => t.ID, delegate(SvgElement t, string v)
		{
			t.ID = v;
		}),
		["space"] = new SvgPropertyDescriptor<SvgElement, XmlSpaceHandling>(DescriptorType.Property, "space", "http://www.w3.org/XML/1998/namespace", new XmlSpaceHandlingConverter(), (SvgElement t) => t.SpaceHandling, delegate(SvgElement t, XmlSpaceHandling v)
		{
			t.SpaceHandling = v;
		}),
		["onclick"] = new SvgPropertyDescriptor<SvgElement, EventHandler<MouseArg>>(DescriptorType.Event, "onclick", "http://www.w3.org/2000/svg", null, (SvgElement t) => t.Click, delegate(SvgElement t, EventHandler<MouseArg> v)
		{
			t.Click += v;
		}),
		["onmousedown"] = new SvgPropertyDescriptor<SvgElement, EventHandler<MouseArg>>(DescriptorType.Event, "onmousedown", "http://www.w3.org/2000/svg", null, (SvgElement t) => t.MouseDown, delegate(SvgElement t, EventHandler<MouseArg> v)
		{
			t.MouseDown += v;
		}),
		["onmouseup"] = new SvgPropertyDescriptor<SvgElement, EventHandler<MouseArg>>(DescriptorType.Event, "onmouseup", "http://www.w3.org/2000/svg", null, (SvgElement t) => t.MouseUp, delegate(SvgElement t, EventHandler<MouseArg> v)
		{
			t.MouseUp += v;
		}),
		["onmousemove"] = new SvgPropertyDescriptor<SvgElement, EventHandler<MouseArg>>(DescriptorType.Event, "onmousemove", "http://www.w3.org/2000/svg", null, (SvgElement t) => t.MouseMove, delegate(SvgElement t, EventHandler<MouseArg> v)
		{
			t.MouseMove += v;
		}),
		["onmousescroll"] = new SvgPropertyDescriptor<SvgElement, EventHandler<MouseScrollArg>>(DescriptorType.Event, "onmousescroll", "http://www.w3.org/2000/svg", null, (SvgElement t) => t.MouseScroll, delegate(SvgElement t, EventHandler<MouseScrollArg> v)
		{
			t.MouseScroll += v;
		}),
		["onmouseover"] = new SvgPropertyDescriptor<SvgElement, EventHandler<MouseArg>>(DescriptorType.Event, "onmouseover", "http://www.w3.org/2000/svg", null, (SvgElement t) => t.MouseOver, delegate(SvgElement t, EventHandler<MouseArg> v)
		{
			t.MouseOver += v;
		}),
		["onmouseout"] = new SvgPropertyDescriptor<SvgElement, EventHandler<MouseArg>>(DescriptorType.Event, "onmouseout", "http://www.w3.org/2000/svg", null, (SvgElement t) => t.MouseOut, delegate(SvgElement t, EventHandler<MouseArg> v)
		{
			t.MouseOut += v;
		}),
		["fill"] = new SvgPropertyDescriptor<SvgElement, SvgPaintServer>(DescriptorType.Property, "fill", "http://www.w3.org/2000/svg", new SvgPaintServerFactory(), (SvgElement t) => t.Fill, delegate(SvgElement t, SvgPaintServer v)
		{
			t.Fill = v;
		}),
		["stroke"] = new SvgPropertyDescriptor<SvgElement, SvgPaintServer>(DescriptorType.Property, "stroke", "http://www.w3.org/2000/svg", new SvgPaintServerFactory(), (SvgElement t) => t.Stroke, delegate(SvgElement t, SvgPaintServer v)
		{
			t.Stroke = v;
		}),
		["fill-rule"] = new SvgPropertyDescriptor<SvgElement, SvgFillRule>(DescriptorType.Property, "fill-rule", "http://www.w3.org/2000/svg", new SvgFillRuleConverter(), (SvgElement t) => t.FillRule, delegate(SvgElement t, SvgFillRule v)
		{
			t.FillRule = v;
		}),
		["fill-opacity"] = new SvgPropertyDescriptor<SvgElement, float>(DescriptorType.Property, "fill-opacity", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgElement t) => t.FillOpacity, delegate(SvgElement t, float v)
		{
			t.FillOpacity = v;
		}),
		["stroke-width"] = new SvgPropertyDescriptor<SvgElement, SvgUnit>(DescriptorType.Property, "stroke-width", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgElement t) => t.StrokeWidth, delegate(SvgElement t, SvgUnit v)
		{
			t.StrokeWidth = v;
		}),
		["stroke-linecap"] = new SvgPropertyDescriptor<SvgElement, SvgStrokeLineCap>(DescriptorType.Property, "stroke-linecap", "http://www.w3.org/2000/svg", new SvgStrokeLineCapConverter(), (SvgElement t) => t.StrokeLineCap, delegate(SvgElement t, SvgStrokeLineCap v)
		{
			t.StrokeLineCap = v;
		}),
		["stroke-linejoin"] = new SvgPropertyDescriptor<SvgElement, SvgStrokeLineJoin>(DescriptorType.Property, "stroke-linejoin", "http://www.w3.org/2000/svg", new SvgStrokeLineJoinConverter(), (SvgElement t) => t.StrokeLineJoin, delegate(SvgElement t, SvgStrokeLineJoin v)
		{
			t.StrokeLineJoin = v;
		}),
		["stroke-miterlimit"] = new SvgPropertyDescriptor<SvgElement, float>(DescriptorType.Property, "stroke-miterlimit", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgElement t) => t.StrokeMiterLimit, delegate(SvgElement t, float v)
		{
			t.StrokeMiterLimit = v;
		}),
		["stroke-dasharray"] = new SvgPropertyDescriptor<SvgElement, SvgUnitCollection>(DescriptorType.Property, "stroke-dasharray", "http://www.w3.org/2000/svg", new SvgStrokeDashArrayConverter(), (SvgElement t) => t.StrokeDashArray, delegate(SvgElement t, SvgUnitCollection v)
		{
			t.StrokeDashArray = v;
		}),
		["stroke-dashoffset"] = new SvgPropertyDescriptor<SvgElement, SvgUnit>(DescriptorType.Property, "stroke-dashoffset", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgElement t) => t.StrokeDashOffset, delegate(SvgElement t, SvgUnit v)
		{
			t.StrokeDashOffset = v;
		}),
		["stroke-opacity"] = new SvgPropertyDescriptor<SvgElement, float>(DescriptorType.Property, "stroke-opacity", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgElement t) => t.StrokeOpacity, delegate(SvgElement t, float v)
		{
			t.StrokeOpacity = v;
		}),
		["opacity"] = new SvgPropertyDescriptor<SvgElement, float>(DescriptorType.Property, "opacity", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgElement t) => t.Opacity, delegate(SvgElement t, float v)
		{
			t.Opacity = v;
		}),
		["shape-rendering"] = new SvgPropertyDescriptor<SvgElement, SvgShapeRendering>(DescriptorType.Property, "shape-rendering", "http://www.w3.org/2000/svg", new SvgShapeRenderingConverter(), (SvgElement t) => t.ShapeRendering, delegate(SvgElement t, SvgShapeRendering v)
		{
			t.ShapeRendering = v;
		}),
		["color-interpolation"] = new SvgPropertyDescriptor<SvgElement, SvgColourInterpolation>(DescriptorType.Property, "color-interpolation", "http://www.w3.org/2000/svg", new SvgColourInterpolationConverter(), (SvgElement t) => t.ColorInterpolation, delegate(SvgElement t, SvgColourInterpolation v)
		{
			t.ColorInterpolation = v;
		}),
		["color-interpolation-filters"] = new SvgPropertyDescriptor<SvgElement, SvgColourInterpolation>(DescriptorType.Property, "color-interpolation-filters", "http://www.w3.org/2000/svg", new SvgColourInterpolationConverter(), (SvgElement t) => t.ColorInterpolationFilters, delegate(SvgElement t, SvgColourInterpolation v)
		{
			t.ColorInterpolationFilters = v;
		}),
		["visibility"] = new SvgPropertyDescriptor<SvgElement, string>(DescriptorType.Property, "visibility", "http://www.w3.org/2000/svg", new StringConverter(), (SvgElement t) => t.Visibility, delegate(SvgElement t, string v)
		{
			t.Visibility = v;
		}),
		["display"] = new SvgPropertyDescriptor<SvgElement, string>(DescriptorType.Property, "display", "http://www.w3.org/2000/svg", new StringConverter(), (SvgElement t) => t.Display, delegate(SvgElement t, string v)
		{
			t.Display = v;
		}),
		["text-anchor"] = new SvgPropertyDescriptor<SvgElement, SvgTextAnchor>(DescriptorType.Property, "text-anchor", "http://www.w3.org/2000/svg", new SvgTextAnchorConverter(), (SvgElement t) => t.TextAnchor, delegate(SvgElement t, SvgTextAnchor v)
		{
			t.TextAnchor = v;
		}),
		["baseline-shift"] = new SvgPropertyDescriptor<SvgElement, string>(DescriptorType.Property, "baseline-shift", "http://www.w3.org/2000/svg", new StringConverter(), (SvgElement t) => t.BaselineShift, delegate(SvgElement t, string v)
		{
			t.BaselineShift = v;
		}),
		["font-family"] = new SvgPropertyDescriptor<SvgElement, string>(DescriptorType.Property, "font-family", "http://www.w3.org/2000/svg", new StringConverter(), (SvgElement t) => t.FontFamily, delegate(SvgElement t, string v)
		{
			t.FontFamily = v;
		}),
		["font-size"] = new SvgPropertyDescriptor<SvgElement, SvgUnit>(DescriptorType.Property, "font-size", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgElement t) => t.FontSize, delegate(SvgElement t, SvgUnit v)
		{
			t.FontSize = v;
		}),
		["font-style"] = new SvgPropertyDescriptor<SvgElement, SvgFontStyle>(DescriptorType.Property, "font-style", "http://www.w3.org/2000/svg", new SvgFontStyleConverter(), (SvgElement t) => t.FontStyle, delegate(SvgElement t, SvgFontStyle v)
		{
			t.FontStyle = v;
		}),
		["font-variant"] = new SvgPropertyDescriptor<SvgElement, SvgFontVariant>(DescriptorType.Property, "font-variant", "http://www.w3.org/2000/svg", new SvgFontVariantConverter(), (SvgElement t) => t.FontVariant, delegate(SvgElement t, SvgFontVariant v)
		{
			t.FontVariant = v;
		}),
		["text-decoration"] = new SvgPropertyDescriptor<SvgElement, SvgTextDecoration>(DescriptorType.Property, "text-decoration", "http://www.w3.org/2000/svg", new SvgTextDecorationConverter(), (SvgElement t) => t.TextDecoration, delegate(SvgElement t, SvgTextDecoration v)
		{
			t.TextDecoration = v;
		}),
		["font-weight"] = new SvgPropertyDescriptor<SvgElement, SvgFontWeight>(DescriptorType.Property, "font-weight", "http://www.w3.org/2000/svg", new SvgFontWeightConverter(), (SvgElement t) => t.FontWeight, delegate(SvgElement t, SvgFontWeight v)
		{
			t.FontWeight = v;
		}),
		["font-stretch"] = new SvgPropertyDescriptor<SvgElement, SvgFontStretch>(DescriptorType.Property, "font-stretch", "http://www.w3.org/2000/svg", new SvgFontStretchConverter(), (SvgElement t) => t.FontStretch, delegate(SvgElement t, SvgFontStretch v)
		{
			t.FontStretch = v;
		}),
		["text-transform"] = new SvgPropertyDescriptor<SvgElement, SvgTextTransformation>(DescriptorType.Property, "text-transform", "http://www.w3.org/2000/svg", new SvgTextTransformationConverter(), (SvgElement t) => t.TextTransformation, delegate(SvgElement t, SvgTextTransformation v)
		{
			t.TextTransformation = v;
		}),
		["font"] = new SvgPropertyDescriptor<SvgElement, string>(DescriptorType.Property, "font", "http://www.w3.org/2000/svg", new StringConverter(), (SvgElement t) => t.Font, delegate(SvgElement t, string v)
		{
			t.Font = v;
		})
	};

	protected internal static HttpClient HttpClient { get; } = new HttpClient();

	public Dictionary<string, string> Namespaces { get; } = new Dictionary<string, string>();

	protected internal string ElementNamespace { get; protected set; } = "http://www.w3.org/2000/svg";

	protected internal string ElementName
	{
		get
		{
			if (string.IsNullOrEmpty(_elementName))
			{
				if (SvgElements.ElementNames.TryGetValue(GetType(), out var value))
				{
					_elementName = value;
				}
				else if (this is SvgDocument)
				{
					_elementName = "svg";
				}
			}
			return _elementName;
		}
		internal set
		{
			_elementName = value;
		}
	}

	[SvgAttribute("color")]
	public virtual SvgPaintServer Color
	{
		get
		{
			return GetAttribute("color", inherited: true, SvgPaintServer.NotSet);
		}
		set
		{
			Attributes["color"] = value;
		}
	}

	public virtual string Content
	{
		get
		{
			return _content;
		}
		set
		{
			if (_content != null)
			{
				string content = _content;
				_content = value;
				if (_content != content)
				{
					OnContentChanged(new ContentEventArgs
					{
						Content = value
					});
				}
			}
			else
			{
				_content = value;
				OnContentChanged(new ContentEventArgs
				{
					Content = value
				});
			}
		}
	}

	protected virtual EventHandlerList Events => _eventHandlers;

	public virtual SvgElementCollection Children => _children;

	public IList<ISvgNode> Nodes => _nodes;

	public virtual SvgElement Parent => _parent;

	public IEnumerable<SvgElement> Parents
	{
		get
		{
			SvgElement curr = this;
			while (curr.Parent != null)
			{
				curr = curr.Parent;
				yield return curr;
			}
		}
	}

	public IEnumerable<SvgElement> ParentsAndSelf
	{
		get
		{
			SvgElement curr = this;
			yield return curr;
			while (curr.Parent != null)
			{
				curr = curr.Parent;
				yield return curr;
			}
		}
	}

	public virtual SvgDocument OwnerDocument
	{
		get
		{
			if (this is SvgDocument)
			{
				return this as SvgDocument;
			}
			if (Parent != null)
			{
				return Parent.OwnerDocument;
			}
			return null;
		}
	}

	protected internal virtual SvgAttributeCollection Attributes
	{
		get
		{
			if (_attributes == null)
			{
				_attributes = new SvgAttributeCollection(this);
			}
			return _attributes;
		}
	}

	protected bool Writing { get; set; }

	public SvgCustomAttributeCollection CustomAttributes => _customAttributes;

	[SvgAttribute("transform")]
	public SvgTransformCollection Transforms
	{
		get
		{
			return GetAttribute<SvgTransformCollection>("transform", inherited: false);
		}
		set
		{
			SvgTransformCollection transforms = Transforms;
			if (transforms != null)
			{
				transforms.TransformChanged -= Attributes_AttributeChanged;
			}
			value.TransformChanged += Attributes_AttributeChanged;
			Attributes["transform"] = value;
		}
	}

	[SvgAttribute("id")]
	public string ID
	{
		get
		{
			return GetAttribute<string>("id", inherited: false);
		}
		set
		{
			SetAndForceUniqueID(value, autoForceUniqueID: false);
		}
	}

	[SvgAttribute("space", "http://www.w3.org/XML/1998/namespace")]
	public virtual XmlSpaceHandling SpaceHandling
	{
		get
		{
			return GetAttribute("space", inherited: true, XmlSpaceHandling.Inherit);
		}
		set
		{
			Attributes["space"] = value;
		}
	}

	protected virtual bool IsPathDirty
	{
		get
		{
			return _dirty;
		}
		set
		{
			_dirty = value;
		}
	}

	[SvgAttribute("fill")]
	public virtual SvgPaintServer Fill
	{
		get
		{
			return GetAttribute("fill", inherited: true, SvgPaintServer.NotSet);
		}
		set
		{
			Attributes["fill"] = value;
		}
	}

	[SvgAttribute("stroke")]
	public virtual SvgPaintServer Stroke
	{
		get
		{
			return GetAttribute<SvgPaintServer>("stroke", inherited: true);
		}
		set
		{
			Attributes["stroke"] = value;
		}
	}

	[SvgAttribute("fill-rule")]
	public virtual SvgFillRule FillRule
	{
		get
		{
			return GetAttribute("fill-rule", inherited: true, SvgFillRule.NonZero);
		}
		set
		{
			Attributes["fill-rule"] = value;
		}
	}

	[SvgAttribute("fill-opacity")]
	public virtual float FillOpacity
	{
		get
		{
			return GetAttribute("fill-opacity", inherited: true, 1f);
		}
		set
		{
			Attributes["fill-opacity"] = FixOpacityValue(value);
		}
	}

	[SvgAttribute("stroke-width")]
	public virtual SvgUnit StrokeWidth
	{
		get
		{
			return GetAttribute("stroke-width", true, (SvgUnit)1f);
		}
		set
		{
			Attributes["stroke-width"] = value;
		}
	}

	[SvgAttribute("stroke-linecap")]
	public virtual SvgStrokeLineCap StrokeLineCap
	{
		get
		{
			return GetAttribute("stroke-linecap", inherited: true, SvgStrokeLineCap.Butt);
		}
		set
		{
			Attributes["stroke-linecap"] = value;
		}
	}

	[SvgAttribute("stroke-linejoin")]
	public virtual SvgStrokeLineJoin StrokeLineJoin
	{
		get
		{
			return GetAttribute("stroke-linejoin", inherited: true, SvgStrokeLineJoin.Miter);
		}
		set
		{
			Attributes["stroke-linejoin"] = value;
		}
	}

	[SvgAttribute("stroke-miterlimit")]
	public virtual float StrokeMiterLimit
	{
		get
		{
			return GetAttribute("stroke-miterlimit", inherited: true, 4f);
		}
		set
		{
			Attributes["stroke-miterlimit"] = value;
		}
	}

	[TypeConverter(typeof(SvgStrokeDashArrayConverter))]
	[SvgAttribute("stroke-dasharray")]
	public virtual SvgUnitCollection StrokeDashArray
	{
		get
		{
			return GetAttribute<SvgUnitCollection>("stroke-dasharray", inherited: true);
		}
		set
		{
			Attributes["stroke-dasharray"] = value;
		}
	}

	[SvgAttribute("stroke-dashoffset")]
	public virtual SvgUnit StrokeDashOffset
	{
		get
		{
			return GetAttribute("stroke-dashoffset", inherited: true, SvgUnit.Empty);
		}
		set
		{
			Attributes["stroke-dashoffset"] = value;
		}
	}

	[SvgAttribute("stroke-opacity")]
	public virtual float StrokeOpacity
	{
		get
		{
			return GetAttribute("stroke-opacity", inherited: true, 1f);
		}
		set
		{
			Attributes["stroke-opacity"] = FixOpacityValue(value);
		}
	}

	[SvgAttribute("opacity")]
	public virtual float Opacity
	{
		get
		{
			return GetAttribute("opacity", inherited: false, 1f);
		}
		set
		{
			Attributes["opacity"] = FixOpacityValue(value);
		}
	}

	[SvgAttribute("shape-rendering")]
	public virtual SvgShapeRendering ShapeRendering
	{
		get
		{
			return GetAttribute("shape-rendering", inherited: true, SvgShapeRendering.Auto);
		}
		set
		{
			Attributes["shape-rendering"] = value;
		}
	}

	[SvgAttribute("color-interpolation")]
	public SvgColourInterpolation ColorInterpolation
	{
		get
		{
			return GetAttribute("color-interpolation", inherited: true, SvgColourInterpolation.SRGB);
		}
		set
		{
			Attributes["color-interpolation"] = value;
		}
	}

	[SvgAttribute("color-interpolation-filters")]
	public SvgColourInterpolation ColorInterpolationFilters
	{
		get
		{
			return GetAttribute("color-interpolation-filters", inherited: true, SvgColourInterpolation.LinearRGB);
		}
		set
		{
			Attributes["color-interpolation-filters"] = value;
		}
	}

	[SvgAttribute("visibility")]
	public virtual string Visibility
	{
		get
		{
			return GetAttribute("visibility", inherited: true, "visible");
		}
		set
		{
			Attributes["visibility"] = value;
		}
	}

	[SvgAttribute("display")]
	public virtual string Display
	{
		get
		{
			return GetAttribute("display", inherited: false, "inline");
		}
		set
		{
			Attributes["display"] = value;
		}
	}

	[SvgAttribute("text-anchor")]
	public virtual SvgTextAnchor TextAnchor
	{
		get
		{
			return GetAttribute("text-anchor", inherited: true, SvgTextAnchor.Start);
		}
		set
		{
			Attributes["text-anchor"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("baseline-shift")]
	public virtual string BaselineShift
	{
		get
		{
			return GetAttribute("baseline-shift", inherited: false, "baseline");
		}
		set
		{
			Attributes["baseline-shift"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("font-family")]
	public virtual string FontFamily
	{
		get
		{
			return GetAttribute<string>("font-family", inherited: true);
		}
		set
		{
			Attributes["font-family"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("font-size")]
	public virtual SvgUnit FontSize
	{
		get
		{
			return GetAttribute("font-size", inherited: true, SvgUnit.Empty);
		}
		set
		{
			Attributes["font-size"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("font-style")]
	public virtual SvgFontStyle FontStyle
	{
		get
		{
			return GetAttribute("font-style", inherited: true, SvgFontStyle.Normal);
		}
		set
		{
			Attributes["font-style"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("font-variant")]
	public virtual SvgFontVariant FontVariant
	{
		get
		{
			return GetAttribute("font-variant", inherited: true, SvgFontVariant.Normal);
		}
		set
		{
			Attributes["font-variant"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("text-decoration")]
	public virtual SvgTextDecoration TextDecoration
	{
		get
		{
			return GetAttribute("text-decoration", inherited: true, SvgTextDecoration.None);
		}
		set
		{
			Attributes["text-decoration"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("font-weight")]
	public virtual SvgFontWeight FontWeight
	{
		get
		{
			return GetAttribute("font-weight", inherited: true, SvgFontWeight.Normal);
		}
		set
		{
			Attributes["font-weight"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("font-stretch")]
	public virtual SvgFontStretch FontStretch
	{
		get
		{
			return GetAttribute("font-stretch", inherited: true, SvgFontStretch.Normal);
		}
		set
		{
			Attributes["font-stretch"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("text-transform")]
	public virtual SvgTextTransformation TextTransformation
	{
		get
		{
			return GetAttribute("text-transform", inherited: true, SvgTextTransformation.Inherit);
		}
		set
		{
			Attributes["text-transform"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("font")]
	public virtual string Font
	{
		get
		{
			return GetAttribute("font", inherited: true, string.Empty);
		}
		set
		{
			FontParseState fontParseState = FontParseState.fontStyle;
			string[] array = value.Split(' ');
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				bool flag = false;
				while (!flag)
				{
					switch (fontParseState)
					{
					case FontParseState.fontStyle:
					{
						flag = Enum.TryParse<SvgFontStyle>(text, out var result3);
						if (flag)
						{
							FontStyle = result3;
						}
						fontParseState++;
						break;
					}
					case FontParseState.fontVariant:
					{
						flag = Enum.TryParse<SvgFontVariant>(text, out var result);
						if (flag)
						{
							FontVariant = result;
						}
						fontParseState++;
						break;
					}
					case FontParseState.fontWeight:
					{
						flag = Enum.TryParse<SvgFontWeight>(text, out var result2);
						if (flag)
						{
							FontWeight = result2;
						}
						fontParseState++;
						break;
					}
					case FontParseState.fontSize:
					{
						string[] array2 = text.Split('/');
						try
						{
							SvgUnit fontSize = (SvgUnit)new SvgUnitConverter().ConvertFromInvariantString(array2[0]);
							flag = true;
							FontSize = fontSize;
						}
						catch
						{
						}
						fontParseState++;
						break;
					}
					case FontParseState.fontFamilyNext:
						fontParseState++;
						flag = true;
						break;
					}
				}
				switch (fontParseState)
				{
				case FontParseState.fontFamilyNext:
					FontFamily = string.Join(" ", array, i + 1, array.Length - (i + 1));
					i = 2147483645;
					break;
				case FontParseState.fontFamilyCurr:
					FontFamily = string.Join(" ", array, i, array.Length - i);
					i = 2147483645;
					break;
				}
			}
			Attributes["font"] = value;
			IsPathDirty = true;
		}
	}

	internal abstract string AttributeName { get; }

	internal abstract List<Type> ClassNames { get; }

	internal virtual Dictionary<string, ISvgPropertyDescriptor> Properties => SvgElementProperties;

	public event EventHandler Load
	{
		add
		{
			Events.AddHandler(_loadEventKey, value);
		}
		remove
		{
			Events.RemoveHandler(_loadEventKey, value);
		}
	}

	public event EventHandler<ChildAddedEventArgs> ChildAdded;

	public event EventHandler<AttributeEventArgs> AttributeChanged;

	public event EventHandler<ContentEventArgs> ContentChanged;

	[SvgAttribute("onclick")]
	public event EventHandler<MouseArg> Click;

	[SvgAttribute("onmousedown")]
	public event EventHandler<MouseArg> MouseDown;

	[SvgAttribute("onmouseup")]
	public event EventHandler<MouseArg> MouseUp;

	[SvgAttribute("onmousemove")]
	public event EventHandler<MouseArg> MouseMove;

	[SvgAttribute("onmousescroll")]
	public event EventHandler<MouseScrollArg> MouseScroll;

	[SvgAttribute("onmouseover")]
	public event EventHandler<MouseArg> MouseOver;

	[SvgAttribute("onmouseout")]
	public event EventHandler<MouseArg> MouseOut;

	public void AddStyle(string name, string value, int specificity)
	{
		if (!_styles.TryGetValue(name, out var value2))
		{
			value2 = new SortedDictionary<int, string>();
			_styles[name] = value2;
		}
		while (value2.ContainsKey(specificity))
		{
			specificity++;
		}
		value2[specificity] = value;
	}

	public void FlushStyles(bool children = false)
	{
		FlushStyles();
		if (!children)
		{
			return;
		}
		foreach (SvgElement child in Children)
		{
			child.FlushStyles(children);
		}
	}

	private void FlushStyles()
	{
		if (!_styles.Any())
		{
			return;
		}
		Dictionary<string, SortedDictionary<int, string>> dictionary = new Dictionary<string, SortedDictionary<int, string>>();
		foreach (KeyValuePair<string, SortedDictionary<int, string>> style in _styles)
		{
			if (!SvgElementFactory.SetPropertyValue(this, string.Empty, style.Key, style.Value.Last().Value, OwnerDocument, isStyle: true))
			{
				dictionary.Add(style.Key, style.Value);
			}
		}
		_styles = dictionary;
	}

	public bool ContainsAttribute(string name)
	{
		if (!Attributes.ContainsKey(name) && !CustomAttributes.ContainsKey(name))
		{
			if (_styles.TryGetValue(name, out var value))
			{
				if (!value.ContainsKey(65536))
				{
					return value.ContainsKey(0);
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public bool TryGetAttribute(string name, out string value)
	{
		if (Attributes.TryGetValue(name, out var value2))
		{
			value = value2.ToString();
			return true;
		}
		if (CustomAttributes.TryGetValue(name, out value))
		{
			return true;
		}
		if (_styles.TryGetValue(name, out var value3))
		{
			if (value3.TryGetValue(65536, out value))
			{
				return true;
			}
			if (value3.TryGetValue(0, out value))
			{
				return true;
			}
		}
		return false;
	}

	public IEnumerable<SvgElement> Descendants()
	{
		return AsEnumerable().Descendants();
	}

	private IEnumerable<SvgElement> AsEnumerable()
	{
		yield return this;
	}

	public virtual bool HasChildren()
	{
		return Children.Count > 0;
	}

	protected internal TAttributeType GetAttribute<TAttributeType>(string attributeName, bool inherited, TAttributeType defaultValue = default(TAttributeType))
	{
		if (Writing)
		{
			return Attributes.GetAttribute(attributeName, defaultValue);
		}
		return Attributes.GetInheritedAttribute(attributeName, inherited, defaultValue);
	}

	public void SetAndForceUniqueID(string value, bool autoForceUniqueID = true, Action<SvgElement, string, string> logElementOldIDNewID = null)
	{
		if (string.Compare(ID, value) != 0)
		{
			if (OwnerDocument != null)
			{
				OwnerDocument.IdManager.Remove(this);
			}
			Attributes["id"] = value;
			if (OwnerDocument != null)
			{
				OwnerDocument.IdManager.AddAndForceUniqueID(this, null, autoForceUniqueID, logElementOldIDNewID);
			}
		}
	}

	internal void ForceUniqueID(string newID)
	{
		Attributes["id"] = newID;
	}

	protected virtual void AddElement(SvgElement child, int index)
	{
	}

	internal void OnElementAdded(SvgElement child, int index)
	{
		AddElement(child, index);
		SvgElement beforeSibling = null;
		if (index < Children.Count - 1)
		{
			beforeSibling = Children[index + 1];
		}
		ChildAdded?.Invoke(this, new ChildAddedEventArgs
		{
			NewChild = child,
			BeforeSibling = beforeSibling
		});
	}

	protected virtual void RemoveElement(SvgElement child)
	{
	}

	internal void OnElementRemoved(SvgElement child)
	{
		RemoveElement(child);
	}

	public SvgElement()
	{
		_children = new SvgElementCollection(this);
		_eventHandlers = new EventHandlerList();
		_elementName = string.Empty;
		_customAttributes = new SvgCustomAttributeCollection(this);
		Attributes.AttributeChanged += Attributes_AttributeChanged;
		CustomAttributes.AttributeChanged += Attributes_AttributeChanged;
	}

	private void Attributes_AttributeChanged(object sender, AttributeEventArgs e)
	{
		OnAttributeChanged(e);
	}

	public virtual void InitialiseFromXML(XmlReader reader, SvgDocument document)
	{
		throw new NotImplementedException();
	}

	public virtual bool ShouldWriteElement()
	{
		return !string.IsNullOrEmpty(ElementName);
	}

	protected virtual void WriteStartElement(XmlWriter writer)
	{
		if (!string.IsNullOrEmpty(ElementName))
		{
			if (string.IsNullOrEmpty(ElementNamespace))
			{
				writer.WriteStartElement(ElementName);
			}
			else
			{
				string text = writer.LookupPrefix(ElementNamespace);
				if (text == null && !ElementNamespace.Equals("http://www.w3.org/2000/svg"))
				{
					foreach (KeyValuePair<string, string> @namespace in Namespaces)
					{
						if (@namespace.Value.Equals(ElementNamespace) && !string.IsNullOrEmpty(@namespace.Key))
						{
							text = @namespace.Key;
							break;
						}
					}
				}
				if (text == null)
				{
					writer.WriteStartElement(ElementName, ElementNamespace);
				}
				else
				{
					writer.WriteStartElement(text, ElementName, ElementNamespace);
				}
			}
		}
		WriteAttributes(writer);
	}

	protected virtual void WriteEndElement(XmlWriter writer)
	{
		if (!string.IsNullOrEmpty(ElementName))
		{
			writer.WriteEndElement();
		}
	}

	protected virtual void WriteAttributes(XmlWriter writer)
	{
		foreach (KeyValuePair<string, string> @namespace in Namespaces)
		{
			if (!@namespace.Value.Equals("http://www.w3.org/2000/svg") || string.IsNullOrEmpty(@namespace.Key))
			{
				writer.WriteAttributeString("xmlns", @namespace.Key, null, @namespace.Value);
			}
		}
		Dictionary<string, string> source = WritePropertyAttributes(writer);
		if (AutoPublishEvents)
		{
			foreach (ISvgPropertyDescriptor item in from x in GetProperties()
				where x.DescriptorType == DescriptorType.Event
				select x)
			{
				if (item.GetValue(this) != null && !string.IsNullOrEmpty(ID))
				{
					string value = ID + "/" + item.AttributeName;
					WriteAttributeString(writer, item.AttributeName, null, value);
				}
			}
		}
		string element = string.Empty;
		foreach (KeyValuePair<string, string> customAttribute in _customAttributes)
		{
			if (customAttribute.Key.Equals("style") && source.Any())
			{
				element = customAttribute.Value;
				continue;
			}
			int num = customAttribute.Key.LastIndexOf(":");
			if (num >= 0)
			{
				string ns = customAttribute.Key.Substring(0, num);
				string name = customAttribute.Key.Substring(num + 1);
				WriteAttributeString(writer, name, ns, customAttribute.Value);
			}
			else
			{
				WriteAttributeString(writer, customAttribute.Key, null, customAttribute.Value);
			}
		}
		if (source.Any())
		{
			IEnumerable<string> values = source.Select((KeyValuePair<string, string> s) => s.Key + ":" + s.Value).Concat(Enumerable.Repeat(element, 1));
			WriteAttributeString(writer, "style", null, string.Join(";", values));
		}
	}

	private Dictionary<string, string> WritePropertyAttributes(XmlWriter writer)
	{
		Dictionary<string, string> dictionary = _styles.ToDictionary((KeyValuePair<string, SortedDictionary<int, string>> _styles) => _styles.Key, (KeyValuePair<string, SortedDictionary<int, string>> _styles) => _styles.Value.Last().Value);
		List<ISvgPropertyDescriptor> list = new List<ISvgPropertyDescriptor>();
		Dictionary<string, float> dictionary2 = new Dictionary<string, float>();
		try
		{
			Writing = true;
			foreach (ISvgPropertyDescriptor property in GetProperties())
			{
				if (property.Converter == null || !property.Converter.CanConvertTo(typeof(string)))
				{
					continue;
				}
				if (property.AttributeName == "fill-opacity" || property.AttributeName == "stroke-opacity")
				{
					list.Add(property);
				}
				else
				{
					if (!Attributes.ContainsKey(property.AttributeName))
					{
						continue;
					}
					object value = property.GetValue(this);
					bool flag = false;
					bool flag2 = property.AttributeName == "fill" || property.AttributeName == "stroke";
					if (Parent != null)
					{
						if (flag2 && value == SvgPaintServer.NotSet)
						{
							continue;
						}
						if (TryResolveParentAttributeValue(property.AttributeName, out var parentAttributeValue))
						{
							if (parentAttributeValue == value || (parentAttributeValue != null && parentAttributeValue.Equals(value)))
							{
								if (flag2)
								{
									continue;
								}
							}
							else
							{
								flag = true;
							}
						}
					}
					if (flag2 && value is SvgColourServer && ((SvgColourServer)value).Colour.A < byte.MaxValue)
					{
						float value2 = (float)(int)((SvgColourServer)value).Colour.A / 255f;
						dictionary2.Add(property.AttributeName + "-opacity", value2);
					}
					string value3 = (string)property.Converter.ConvertTo(value, typeof(string));
					if (value != null)
					{
						if (flag || !string.IsNullOrEmpty(value3))
						{
							if (flag2)
							{
								dictionary[property.AttributeName] = value3;
							}
							else
							{
								WriteAttributeString(writer, property.AttributeName, property.AttributeNamespace, value3);
							}
						}
					}
					else if (property.AttributeName == "fill")
					{
						if (flag2)
						{
							dictionary[property.AttributeName] = value3;
						}
						else
						{
							WriteAttributeString(writer, property.AttributeName, property.AttributeNamespace, value3);
						}
					}
				}
			}
			foreach (ISvgPropertyDescriptor item in list)
			{
				float num = 1f;
				bool flag3 = false;
				string attributeName = item.AttributeName;
				if (dictionary2.ContainsKey(attributeName))
				{
					num = dictionary2[attributeName];
					flag3 = true;
				}
				if (Attributes.ContainsKey(attributeName))
				{
					num *= (float)item.GetValue(this);
					flag3 = true;
				}
				if (flag3)
				{
					num = (float)Math.Round(num, 2, MidpointRounding.AwayFromZero);
					string value4 = (string)item.Converter.ConvertTo(num, typeof(string));
					if (!string.IsNullOrEmpty(value4))
					{
						WriteAttributeString(writer, item.AttributeName, item.AttributeNamespace, value4);
					}
				}
			}
			return dictionary;
		}
		finally
		{
			Writing = false;
		}
	}

	private static void WriteAttributeString(XmlWriter writer, string name, string ns, string value)
	{
		if (string.IsNullOrEmpty(ns))
		{
			writer.WriteAttributeString(name, value);
			return;
		}
		string text = writer.LookupPrefix(ns);
		if (text != null)
		{
			ns = null;
		}
		writer.WriteAttributeString(text, name, ns, value);
	}

	private bool TryResolveParentAttributeValue(string attributeKey, out object parentAttributeValue)
	{
		parentAttributeValue = null;
		SvgElement parent = Parent;
		bool result = false;
		while (parent != null)
		{
			if (parent.Attributes.ContainsKey(attributeKey))
			{
				result = true;
				parentAttributeValue = parent.Attributes[attributeKey];
				if (parentAttributeValue != null)
				{
					break;
				}
			}
			parent = parent.Parent;
		}
		return result;
	}

	public virtual void Write(XmlWriter writer)
	{
		if (ShouldWriteElement())
		{
			WriteStartElement(writer);
			WriteChildren(writer);
			WriteEndElement(writer);
		}
	}

	protected virtual void WriteChildren(XmlWriter writer)
	{
		if (Nodes.Any())
		{
			foreach (ISvgNode node in Nodes)
			{
				if (!(node is SvgContentNode svgContentNode))
				{
					((SvgElement)node).Write(writer);
				}
				else if (!string.IsNullOrEmpty(svgContentNode.Content))
				{
					writer.WriteString(svgContentNode.Content);
				}
			}
			return;
		}
		if (!string.IsNullOrEmpty(Content))
		{
			writer.WriteString(Content);
		}
		foreach (SvgElement child in Children)
		{
			child.Write(writer);
		}
	}

	public virtual object Clone()
	{
		return DeepCopy();
	}

	public abstract SvgElement DeepCopy();

	ISvgNode ISvgNode.DeepCopy()
	{
		return DeepCopy();
	}

	public virtual SvgElement DeepCopy<T>() where T : SvgElement, new()
	{
		T val = new T
		{
			Content = Content,
			ElementName = ElementName
		};
		foreach (KeyValuePair<string, object> attribute in Attributes)
		{
			object value = ((attribute.Value is ICloneable) ? ((ICloneable)attribute.Value).Clone() : attribute.Value);
			val.Attributes.Add(attribute.Key, value);
		}
		foreach (SvgElement child in Children)
		{
			val.Children.Add(child.DeepCopy());
		}
		foreach (ISvgPropertyDescriptor item in from x in GetProperties()
			where x.DescriptorType == DescriptorType.Event
			select x)
		{
			if (item.GetValue(this) != null)
			{
				if (item.AttributeName == "MouseDown")
				{
					val.MouseDown += delegate
					{
					};
				}
				else if (item.AttributeName == "MouseUp")
				{
					val.MouseUp += delegate
					{
					};
				}
				else if (item.AttributeName == "MouseOver")
				{
					val.MouseOver += delegate
					{
					};
				}
				else if (item.AttributeName == "MouseOut")
				{
					val.MouseOut += delegate
					{
					};
				}
				else if (item.AttributeName == "MouseMove")
				{
					val.MouseMove += delegate
					{
					};
				}
				else if (item.AttributeName == "MouseScroll")
				{
					val.MouseScroll += delegate
					{
					};
				}
				else if (item.AttributeName == "Click")
				{
					val.Click += delegate
					{
					};
				}
				else if (item.AttributeName == "Change")
				{
					(val as SvgText).Change += delegate
					{
					};
				}
			}
		}
		foreach (KeyValuePair<string, string> customAttribute in CustomAttributes)
		{
			val.CustomAttributes.Add(customAttribute.Key, customAttribute.Value);
		}
		foreach (ISvgNode node in Nodes)
		{
			if (node is SvgElement)
			{
				int num = Children.IndexOf((SvgElement)node);
				if (num >= 0)
				{
					val.Nodes.Add(val.Children[num]);
					continue;
				}
			}
			val.Nodes.Add(node.DeepCopy());
		}
		foreach (KeyValuePair<string, SortedDictionary<int, string>> style in _styles)
		{
			foreach (KeyValuePair<int, string> item2 in style.Value)
			{
				val.AddStyle(style.Key, item2.Value, item2.Key);
			}
		}
		return val;
	}

	protected void OnAttributeChanged(AttributeEventArgs args)
	{
		AttributeChanged?.Invoke(this, args);
	}

	protected void OnContentChanged(ContentEventArgs args)
	{
		ContentChanged?.Invoke(this, args);
	}

	public virtual void RegisterEvents(ISvgEventCaller caller)
	{
		if (caller != null && !string.IsNullOrEmpty(ID))
		{
			string text = ID + "/";
			caller.RegisterAction(text + "onclick", CreateMouseEventAction(RaiseMouseClick));
			caller.RegisterAction(text + "onmousedown", CreateMouseEventAction(RaiseMouseDown));
			caller.RegisterAction(text + "onmouseup", CreateMouseEventAction(RaiseMouseUp));
			caller.RegisterAction(text + "onmousemove", CreateMouseEventAction(RaiseMouseMove));
			caller.RegisterAction(text + "onmouseover", CreateMouseEventAction(RaiseMouseOver));
			caller.RegisterAction(text + "onmouseout", CreateMouseEventAction(RaiseMouseOut));
			caller.RegisterAction<int, bool, bool, bool, string>(text + "onmousescroll", OnMouseScroll);
		}
	}

	public virtual void UnregisterEvents(ISvgEventCaller caller)
	{
		if (caller != null && !string.IsNullOrEmpty(ID))
		{
			string text = ID + "/";
			caller.UnregisterAction(text + "onclick");
			caller.UnregisterAction(text + "onmousedown");
			caller.UnregisterAction(text + "onmouseup");
			caller.UnregisterAction(text + "onmousemove");
			caller.UnregisterAction(text + "onmousescroll");
			caller.UnregisterAction(text + "onmouseover");
			caller.UnregisterAction(text + "onmouseout");
		}
	}

	protected Action<float, float, int, int, bool, bool, bool, string> CreateMouseEventAction(Action<object, MouseArg> eventRaiser)
	{
		return delegate(float x, float y, int button, int clickCount, bool altKey, bool shiftKey, bool ctrlKey, string sessionID)
		{
			eventRaiser(this, new MouseArg
			{
				x = x,
				y = y,
				Button = button,
				ClickCount = clickCount,
				AltKey = altKey,
				ShiftKey = shiftKey,
				CtrlKey = ctrlKey,
				SessionID = sessionID
			});
		};
	}

	protected void RaiseMouseClick(object sender, MouseArg e)
	{
		Click?.Invoke(sender, e);
	}

	protected void RaiseMouseDown(object sender, MouseArg e)
	{
		MouseDown?.Invoke(sender, e);
	}

	protected void RaiseMouseUp(object sender, MouseArg e)
	{
		MouseUp?.Invoke(sender, e);
	}

	protected void RaiseMouseMove(object sender, MouseArg e)
	{
		MouseMove?.Invoke(sender, e);
	}

	protected void RaiseMouseOver(object sender, MouseArg args)
	{
		MouseOver?.Invoke(sender, args);
	}

	protected void RaiseMouseOut(object sender, MouseArg args)
	{
		MouseOut?.Invoke(sender, args);
	}

	protected void OnMouseScroll(int scroll, bool ctrlKey, bool shiftKey, bool altKey, string sessionID)
	{
		RaiseMouseScroll(this, new MouseScrollArg
		{
			Scroll = scroll,
			AltKey = altKey,
			ShiftKey = shiftKey,
			CtrlKey = ctrlKey,
			SessionID = sessionID
		});
	}

	protected void RaiseMouseScroll(object sender, MouseScrollArg e)
	{
		MouseScroll?.Invoke(sender, e);
	}

	protected internal virtual bool PushTransforms(ISvgRenderer renderer)
	{
		_graphicsTransform = renderer.Transform;
		_graphicsClip = renderer.GetClip();
		SvgTransformCollection transforms = Transforms;
		if (transforms == null || transforms.Count == 0)
		{
			return true;
		}
		using (Matrix matrix = transforms.GetMatrix())
		{
			using (Matrix matrix2 = new Matrix(0f, 0f, 0f, 0f, 0f, 0f))
			{
				if (matrix2.Equals(matrix))
				{
					return false;
				}
			}
			using Matrix matrix3 = _graphicsTransform.Clone();
			matrix3.Multiply(matrix);
			renderer.Transform = matrix3;
		}
		return true;
	}

	protected internal virtual void PopTransforms(ISvgRenderer renderer)
	{
		renderer.Transform = _graphicsTransform;
		_graphicsTransform.Dispose();
		_graphicsTransform = null;
		renderer.SetClip(_graphicsClip);
		_graphicsClip = null;
	}

	void ISvgTransformable.PushTransforms(ISvgRenderer renderer)
	{
		PushTransforms(renderer);
	}

	void ISvgTransformable.PopTransforms(ISvgRenderer renderer)
	{
		PopTransforms(renderer);
	}

	protected RectangleF TransformedBounds(RectangleF bounds)
	{
		if (Transforms != null && Transforms.Count > 0)
		{
			using (GraphicsPath graphicsPath = new GraphicsPath())
			{
				using Matrix matrix = Transforms.GetMatrix();
				graphicsPath.AddRectangle(bounds);
				graphicsPath.Transform(matrix);
				return graphicsPath.GetBounds();
			}
		}
		return bounds;
	}

	public void RenderElement(ISvgRenderer renderer)
	{
		Render(renderer);
	}

	protected virtual void Render(ISvgRenderer renderer)
	{
		try
		{
			PushTransforms(renderer);
			RenderChildren(renderer);
		}
		finally
		{
			PopTransforms(renderer);
		}
	}

	protected virtual void RenderChildren(ISvgRenderer renderer)
	{
		foreach (SvgElement child in Children)
		{
			child.Render(renderer);
		}
	}

	void ISvgElement.Render(ISvgRenderer renderer)
	{
		Render(renderer);
	}

	protected void AddPaths(SvgElement elem, GraphicsPath path)
	{
		foreach (SvgElement child in elem.Children)
		{
			if (child is SvgSymbol)
			{
				continue;
			}
			if (child is SvgVisualElement && !(child is SvgGroup))
			{
				GraphicsPath graphicsPath = ((SvgVisualElement)child).Path(null);
				if (graphicsPath != null)
				{
					using (graphicsPath = (GraphicsPath)graphicsPath.Clone())
					{
						if (child.Transforms != null)
						{
							using Matrix matrix = child.Transforms.GetMatrix();
							graphicsPath.Transform(matrix);
						}
						if (graphicsPath.PointCount > 0)
						{
							path.AddPath(graphicsPath, connect: false);
						}
					}
				}
			}
			if (!(child is SvgPaintServer))
			{
				AddPaths(child, path);
			}
		}
	}

	protected GraphicsPath GetPaths(SvgElement elem, ISvgRenderer renderer)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		foreach (SvgElement child in elem.Children)
		{
			if (!(child is SvgVisualElement))
			{
				continue;
			}
			if (child is SvgGroup)
			{
				GraphicsPath paths = GetPaths(child, renderer);
				if (paths.PointCount <= 0)
				{
					continue;
				}
				if (child.Transforms != null)
				{
					using Matrix matrix = child.Transforms.GetMatrix();
					paths.Transform(matrix);
				}
				graphicsPath.AddPath(paths, connect: false);
				continue;
			}
			GraphicsPath graphicsPath2 = ((SvgVisualElement)child).Path(renderer);
			graphicsPath2 = ((graphicsPath2 != null) ? ((GraphicsPath)graphicsPath2.Clone()) : new GraphicsPath());
			if (child.Children.Count > 0)
			{
				GraphicsPath paths2 = GetPaths(child, renderer);
				if (paths2.PointCount > 0)
				{
					graphicsPath2.AddPath(paths2, connect: false);
				}
			}
			if (graphicsPath2.PointCount <= 0)
			{
				continue;
			}
			if (child.Transforms != null)
			{
				using Matrix matrix2 = child.Transforms.GetMatrix();
				graphicsPath2.Transform(matrix2);
			}
			graphicsPath.AddPath(graphicsPath2, connect: false);
		}
		return graphicsPath;
	}

	public void InvalidateChildPaths()
	{
		IsPathDirty = true;
		foreach (SvgElement child in Children)
		{
			child.InvalidateChildPaths();
		}
	}

	protected static float FixOpacityValue(float value)
	{
		return Math.Min(Math.Max(value, 0f), 1f);
	}

	internal IFontDefn GetFont(ISvgRenderer renderer, SvgFontManager fontManager)
	{
		SvgUnit fontSize = FontSize;
		float num = ((!(fontSize == SvgUnit.None) && !(fontSize == SvgUnit.Empty)) ? fontSize.ToDeviceValue(renderer, UnitRenderingType.Vertical, this) : ((float)new SvgUnit(SvgUnitType.Em, 1f)));
		object obj = ValidateFontFamily(FontFamily, OwnerDocument, fontManager ?? OwnerDocument.FontManager);
		IEnumerable<SvgFontFace> enumerable = obj as IEnumerable<SvgFontFace>;
		int num2 = OwnerDocument?.Ppi ?? SvgDocument.PointsPerInch;
		if (enumerable == null)
		{
			FontStyle fontStyle = System.Drawing.FontStyle.Regular;
			switch (FontWeight)
			{
			case SvgFontWeight.Bold:
			case SvgFontWeight.W600:
			case SvgFontWeight.W700:
			case SvgFontWeight.W800:
			case SvgFontWeight.W900:
				fontStyle |= System.Drawing.FontStyle.Bold;
				break;
			case SvgFontWeight.Bolder:
			{
				SvgFontWeight svgFontWeight = Parent?.FontWeight ?? SvgFontWeight.Normal;
				if (svgFontWeight != SvgFontWeight.W100 && svgFontWeight != SvgFontWeight.W200 && svgFontWeight != SvgFontWeight.W300)
				{
					fontStyle |= System.Drawing.FontStyle.Bold;
				}
				break;
			}
			case SvgFontWeight.Lighter:
			{
				SvgFontWeight svgFontWeight = Parent?.FontWeight ?? SvgFontWeight.Normal;
				if (svgFontWeight == SvgFontWeight.W800 || svgFontWeight == SvgFontWeight.W900)
				{
					fontStyle |= System.Drawing.FontStyle.Bold;
				}
				break;
			}
			}
			SvgFontStyle fontStyle2 = FontStyle;
			if (fontStyle2 == SvgFontStyle.Oblique || fontStyle2 == SvgFontStyle.Italic)
			{
				fontStyle |= System.Drawing.FontStyle.Italic;
			}
			SvgTextDecoration textDecoration = TextDecoration;
			if (!textDecoration.HasFlag(SvgTextDecoration.None))
			{
				if (textDecoration.HasFlag(SvgTextDecoration.LineThrough))
				{
					fontStyle |= System.Drawing.FontStyle.Strikeout;
				}
				if (textDecoration.HasFlag(SvgTextDecoration.Underline))
				{
					fontStyle |= System.Drawing.FontStyle.Underline;
				}
			}
			FontFamily obj2 = obj as FontFamily;
			obj2.IsStyleAvailable(fontStyle);
			return new GdiFontDefn(new Font(obj2, num, fontStyle, GraphicsUnit.Pixel), num2);
		}
		SvgFont svgFont = enumerable.First().Parent as SvgFont;
		if (svgFont == null)
		{
			Uri referencedElement = enumerable.First().Descendants().OfType<SvgFontFaceUri>()
				.First()
				.ReferencedElement;
			svgFont = OwnerDocument.IdManager.GetElementById(referencedElement) as SvgFont;
		}
		return new SvgFontDefn(svgFont, num, num2);
	}

	public static object ValidateFontFamily(string fontFamilyList, SvgDocument doc, SvgFontManager fontManager)
	{
		foreach (string item in from fontName in (fontFamilyList ?? string.Empty).Split(',')
			select fontName.Trim('"', ' ', '\''))
		{
			if (doc != null && doc.FontDefns().TryGetValue(item, out var value))
			{
				return value;
			}
			FontFamily fontFamily = fontManager.FindFont(item);
			if (fontFamily != null)
			{
				return fontFamily;
			}
		}
		return System.Drawing.FontFamily.GenericSansSerif;
	}

	internal virtual IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgElementProperty in SvgElementProperties)
		{
			yield return svgElementProperty.Value;
		}
	}

	internal virtual object GetValue(string attributeName)
	{
		if (SvgElementProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return null;
	}

	internal virtual bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgElementProperties.TryGetValue(attributeName, out var value2))
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
		return false;
	}
}
