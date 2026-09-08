#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;

namespace Svg.FilterEffects;

[SvgElement("filter")]
public class SvgFilter : SvgElement
{
	internal static List<Type> SvgFilterClassNames = new List<Type> { typeof(SvgFilter) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgFilterProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["filterUnits"] = new SvgPropertyDescriptor<SvgFilter, SvgCoordinateUnits>(DescriptorType.Property, "filterUnits", "http://www.w3.org/2000/svg", new SvgCoordinateUnitsConverter(), (SvgFilter t) => t.FilterUnits, delegate(SvgFilter t, SvgCoordinateUnits v)
		{
			t.FilterUnits = v;
		}),
		["primitiveUnits"] = new SvgPropertyDescriptor<SvgFilter, SvgCoordinateUnits>(DescriptorType.Property, "primitiveUnits", "http://www.w3.org/2000/svg", new SvgCoordinateUnitsConverter(), (SvgFilter t) => t.PrimitiveUnits, delegate(SvgFilter t, SvgCoordinateUnits v)
		{
			t.PrimitiveUnits = v;
		}),
		["x"] = new SvgPropertyDescriptor<SvgFilter, SvgUnit>(DescriptorType.Property, "x", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgFilter t) => t.X, delegate(SvgFilter t, SvgUnit v)
		{
			t.X = v;
		}),
		["y"] = new SvgPropertyDescriptor<SvgFilter, SvgUnit>(DescriptorType.Property, "y", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgFilter t) => t.Y, delegate(SvgFilter t, SvgUnit v)
		{
			t.Y = v;
		}),
		["width"] = new SvgPropertyDescriptor<SvgFilter, SvgUnit>(DescriptorType.Property, "width", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgFilter t) => t.Width, delegate(SvgFilter t, SvgUnit v)
		{
			t.Width = v;
		}),
		["height"] = new SvgPropertyDescriptor<SvgFilter, SvgUnit>(DescriptorType.Property, "height", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgFilter t) => t.Height, delegate(SvgFilter t, SvgUnit v)
		{
			t.Height = v;
		}),
		["href"] = new SvgPropertyDescriptor<SvgFilter, Uri>(DescriptorType.Property, "href", "http://www.w3.org/1999/xlink", new UriTypeConverter(), (SvgFilter t) => t.Href, delegate(SvgFilter t, Uri v)
		{
			t.Href = v;
		})
	};

	[SvgAttribute("filterUnits")]
	public SvgCoordinateUnits FilterUnits
	{
		get
		{
			return GetAttribute("filterUnits", inherited: false, SvgCoordinateUnits.ObjectBoundingBox);
		}
		set
		{
			Attributes["filterUnits"] = value;
		}
	}

	[SvgAttribute("primitiveUnits")]
	public SvgCoordinateUnits PrimitiveUnits
	{
		get
		{
			return GetAttribute("primitiveUnits", inherited: false, SvgCoordinateUnits.UserSpaceOnUse);
		}
		set
		{
			Attributes["primitiveUnits"] = value;
		}
	}

	[SvgAttribute("x")]
	public SvgUnit X
	{
		get
		{
			return GetAttribute("x", inherited: false, new SvgUnit(SvgUnitType.Percentage, -10f));
		}
		set
		{
			Attributes["x"] = value;
		}
	}

	[SvgAttribute("y")]
	public SvgUnit Y
	{
		get
		{
			return GetAttribute("y", inherited: false, new SvgUnit(SvgUnitType.Percentage, -10f));
		}
		set
		{
			Attributes["y"] = value;
		}
	}

	[SvgAttribute("width")]
	public SvgUnit Width
	{
		get
		{
			return GetAttribute("width", inherited: false, new SvgUnit(SvgUnitType.Percentage, 120f));
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
			return GetAttribute("height", inherited: false, new SvgUnit(SvgUnitType.Percentage, 120f));
		}
		set
		{
			Attributes["height"] = value;
		}
	}

	[SvgAttribute("href", "http://www.w3.org/1999/xlink")]
	public Uri Href
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

	internal override string AttributeName => "filter";

	internal override List<Type> ClassNames => SvgFilterClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgFilterProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgFilter>();
	}

	protected override void Render(ISvgRenderer renderer)
	{
		RenderChildren(renderer);
	}

	private Matrix GetTransform(SvgVisualElement element)
	{
		Matrix matrix = new Matrix();
		if (element.Transforms != null)
		{
			using Matrix matrix2 = element.Transforms.GetMatrix();
			matrix.Multiply(matrix2);
		}
		return matrix;
	}

	private RectangleF GetPathBounds(SvgVisualElement element, ISvgRenderer renderer, Matrix transform)
	{
		RectangleF rectangleF = ((element is SvgGroup) ? element.Path(renderer).GetBounds() : element.Bounds);
		PointF[] array = new PointF[2]
		{
			rectangleF.Location,
			new PointF(rectangleF.Right, rectangleF.Bottom)
		};
		transform.TransformPoints(array);
		return new RectangleF(Math.Min(array[0].X, array[1].X), Math.Min(array[0].Y, array[1].Y), Math.Abs(array[0].X - array[1].X), Math.Abs(array[0].Y - array[1].Y));
	}

	public void ApplyFilter(SvgVisualElement element, ISvgRenderer renderer, Action<ISvgRenderer> renderMethod)
	{
		using Matrix transform = GetTransform(element);
		RectangleF pathBounds = GetPathBounds(element, renderer, transform);
		if (pathBounds.Width == 0f || pathBounds.Height == 0f)
		{
			return;
		}
		float num = 0.5f;
		using ImageBuffer imageBuffer = new ImageBuffer(pathBounds, num, renderer, renderMethod)
		{
			Transform = transform
		};
		foreach (SvgFilterPrimitive item in Children.OfType<SvgFilterPrimitive>())
		{
			item.Process(imageBuffer);
		}
		Bitmap buffer = imageBuffer.Buffer;
		RectangleF rectangleF = RectangleF.Inflate(pathBounds, num * pathBounds.Width, num * pathBounds.Height);
		Region clip = renderer.GetClip();
		try
		{
			renderer.SetClip(new Region(rectangleF));
			renderer.DrawImage(buffer, rectangleF, new RectangleF(pathBounds.X, pathBounds.Y, rectangleF.Width, rectangleF.Height), GraphicsUnit.Pixel);
		}
		finally
		{
			renderer.SetClip(clip);
		}
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgFilterProperty in SvgFilterProperties)
		{
			yield return svgFilterProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgFilterProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgFilterProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgFilterProperties.TryGetValue(attributeName, out var value2))
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
