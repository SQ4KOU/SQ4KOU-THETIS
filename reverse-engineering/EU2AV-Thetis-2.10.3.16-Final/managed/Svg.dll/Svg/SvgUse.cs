#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace Svg;

[SvgElement("use")]
public class SvgUse : SvgVisualElement
{
	internal static List<Type> SvgUseClassNames = new List<Type> { typeof(SvgUse) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgUseProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["href"] = new SvgPropertyDescriptor<SvgUse, Uri>(DescriptorType.Property, "href", "http://www.w3.org/1999/xlink", new UriTypeConverter(), (SvgUse t) => t.ReferencedElement, delegate(SvgUse t, Uri v)
		{
			t.ReferencedElement = v;
		}),
		["x"] = new SvgPropertyDescriptor<SvgUse, SvgUnit>(DescriptorType.Property, "x", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgUse t) => t.X, delegate(SvgUse t, SvgUnit v)
		{
			t.X = v;
		}),
		["y"] = new SvgPropertyDescriptor<SvgUse, SvgUnit>(DescriptorType.Property, "y", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgUse t) => t.Y, delegate(SvgUse t, SvgUnit v)
		{
			t.Y = v;
		}),
		["width"] = new SvgPropertyDescriptor<SvgUse, SvgUnit>(DescriptorType.Property, "width", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgUse t) => t.Width, delegate(SvgUse t, SvgUnit v)
		{
			t.Width = v;
		}),
		["height"] = new SvgPropertyDescriptor<SvgUse, SvgUnit>(DescriptorType.Property, "height", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgUse t) => t.Height, delegate(SvgUse t, SvgUnit v)
		{
			t.Height = v;
		})
	};

	[SvgAttribute("href", "http://www.w3.org/1999/xlink")]
	public virtual Uri ReferencedElement
	{
		get
		{
			return GetAttribute<Uri>("href", inherited: false);
		}
		set
		{
			Attributes["href"] = value;
		}
	}

	[SvgAttribute("x")]
	public virtual SvgUnit X
	{
		get
		{
			return GetAttribute("x", false, (SvgUnit)0f);
		}
		set
		{
			Attributes["x"] = value;
		}
	}

	[SvgAttribute("y")]
	public virtual SvgUnit Y
	{
		get
		{
			return GetAttribute("y", false, (SvgUnit)0f);
		}
		set
		{
			Attributes["y"] = value;
		}
	}

	[SvgAttribute("width")]
	public virtual SvgUnit Width
	{
		get
		{
			return GetAttribute("width", false, (SvgUnit)0f);
		}
		set
		{
			Attributes["width"] = value;
		}
	}

	[SvgAttribute("height")]
	public virtual SvgUnit Height
	{
		get
		{
			return GetAttribute("height", false, (SvgUnit)0f);
		}
		set
		{
			Attributes["height"] = value;
		}
	}

	public SvgPoint Location => new SvgPoint(X, Y);

	protected override bool Renderable => false;

	public override RectangleF Bounds
	{
		get
		{
			float num = Width.ToDeviceValue(null, UnitRenderingType.Horizontal, this);
			float num2 = Height.ToDeviceValue(null, UnitRenderingType.Vertical, this);
			if (num > 0f && num2 > 0f)
			{
				return TransformedBounds(new RectangleF(Location.ToDeviceValue(null, this), new SizeF(num, num2)));
			}
			if (OwnerDocument.IdManager.GetElementById(ReferencedElement) is SvgVisualElement svgVisualElement)
			{
				return svgVisualElement.Bounds;
			}
			return default(RectangleF);
		}
	}

	internal override string AttributeName => "use";

	internal override List<Type> ClassNames => SvgUseClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgUseProperties;

	private bool ElementReferencesUri(SvgElement element, List<Uri> elementUris)
	{
		if (element is SvgUse svgUse)
		{
			if (elementUris.Contains(svgUse.ReferencedElement))
			{
				return true;
			}
			if (OwnerDocument.IdManager.GetElementById(svgUse.ReferencedElement) is SvgUse)
			{
				elementUris.Add(svgUse.ReferencedElement);
			}
			return svgUse.ReferencedElementReferencesUri(elementUris);
		}
		if (element is SvgGroup svgGroup)
		{
			foreach (SvgElement child in svgGroup.Children)
			{
				if (ElementReferencesUri(child, elementUris))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool ReferencedElementReferencesUri(List<Uri> elementUris)
	{
		SvgElement elementById = OwnerDocument.IdManager.GetElementById(ReferencedElement);
		return ElementReferencesUri(elementById, elementUris);
	}

	private bool HasRecursiveReference()
	{
		SvgElement elementById = OwnerDocument.IdManager.GetElementById(ReferencedElement);
		List<Uri> elementUris = new List<Uri> { ReferencedElement };
		return ElementReferencesUri(elementById, elementUris);
	}

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgUse>();
	}

	protected internal override bool PushTransforms(ISvgRenderer renderer)
	{
		if (!base.PushTransforms(renderer))
		{
			return false;
		}
		renderer.TranslateTransform(X.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), Y.ToDeviceValue(renderer, UnitRenderingType.Vertical, this), MatrixOrder.Prepend);
		return true;
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		SvgVisualElement svgVisualElement = (SvgVisualElement)OwnerDocument.IdManager.GetElementById(ReferencedElement);
		if (svgVisualElement == null || HasRecursiveReference())
		{
			return null;
		}
		return svgVisualElement.Path(renderer);
	}

	protected override void RenderChildren(ISvgRenderer renderer)
	{
		if (!(ReferencedElement != null) || HasRecursiveReference() || !(OwnerDocument.IdManager.GetElementById(ReferencedElement) is SvgVisualElement svgVisualElement))
		{
			return;
		}
		float num = Width.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
		float num2 = Height.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
		if (num > 0f && num2 > 0f)
		{
			SvgViewBox attribute = svgVisualElement.Attributes.GetAttribute<SvgViewBox>("viewBox");
			if (attribute != SvgViewBox.Empty && Math.Abs(num - attribute.Width) > float.Epsilon && Math.Abs(num2 - attribute.Height) > float.Epsilon)
			{
				float sx = num / attribute.Width;
				float sy = num2 / attribute.Height;
				renderer.ScaleTransform(sx, sy, MatrixOrder.Prepend);
			}
		}
		SvgElement parent = svgVisualElement.Parent;
		svgVisualElement._parent = this;
		svgVisualElement.InvalidateChildPaths();
		svgVisualElement.RenderElement(renderer);
		svgVisualElement._parent = parent;
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgUseProperty in SvgUseProperties)
		{
			yield return svgUseProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgUseProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgUseProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgUseProperties.TryGetValue(attributeName, out var value2))
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
