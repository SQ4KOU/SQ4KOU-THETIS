#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace Svg;

[SvgElement("clipPath")]
public class SvgClipPath : SvgElement
{
	private GraphicsPath _path;

	internal static List<Type> SvgClipPathClassNames = new List<Type> { typeof(SvgClipPath) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgClipPathProperties = new Dictionary<string, ISvgPropertyDescriptor> { ["clipPathUnits"] = new SvgPropertyDescriptor<SvgClipPath, SvgCoordinateUnits>(DescriptorType.Property, "clipPathUnits", "http://www.w3.org/2000/svg", new SvgCoordinateUnitsConverter(), (SvgClipPath t) => t.ClipPathUnits, delegate(SvgClipPath t, SvgCoordinateUnits v)
	{
		t.ClipPathUnits = v;
	}) };

	[SvgAttribute("clipPathUnits")]
	public SvgCoordinateUnits ClipPathUnits
	{
		get
		{
			return GetAttribute("clipPathUnits", inherited: false, SvgCoordinateUnits.UserSpaceOnUse);
		}
		set
		{
			Attributes["clipPathUnits"] = value;
		}
	}

	internal override string AttributeName => "clipPath";

	internal override List<Type> ClassNames => SvgClipPathClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgClipPathProperties;

	protected override void AddElement(SvgElement child, int index)
	{
		base.AddElement(child, index);
		IsPathDirty = true;
	}

	protected override void RemoveElement(SvgElement child)
	{
		base.RemoveElement(child);
		IsPathDirty = true;
	}

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgClipPath>();
	}

	public Region GetClipRegion(SvgVisualElement owner, ISvgRenderer renderer)
	{
		if (_path == null || IsPathDirty)
		{
			_path = new GraphicsPath();
			foreach (SvgElement child in Children)
			{
				CombinePaths(_path, child, renderer);
			}
			IsPathDirty = false;
		}
		GraphicsPath graphicsPath = _path;
		if (ClipPathUnits == SvgCoordinateUnits.ObjectBoundingBox)
		{
			graphicsPath = (GraphicsPath)_path.Clone();
			using Matrix matrix = new Matrix();
			RectangleF bounds = owner.Bounds;
			matrix.Scale(bounds.Width, bounds.Height, MatrixOrder.Append);
			matrix.Translate(bounds.Left, bounds.Top, MatrixOrder.Append);
			graphicsPath.Transform(matrix);
		}
		return new Region(graphicsPath);
	}

	private void CombinePaths(GraphicsPath path, SvgElement element, ISvgRenderer renderer)
	{
		if (element is SvgVisualElement svgVisualElement)
		{
			GraphicsPath graphicsPath = svgVisualElement.Path(renderer);
			if (graphicsPath != null)
			{
				path.FillMode = ((svgVisualElement.ClipRule == SvgClipRule.NonZero) ? FillMode.Winding : FillMode.Alternate);
				if (svgVisualElement.Transforms != null)
				{
					using Matrix matrix = svgVisualElement.Transforms.GetMatrix();
					graphicsPath.Transform(matrix);
				}
				if (graphicsPath.PointCount > 0)
				{
					path.AddPath(graphicsPath, connect: false);
				}
			}
		}
		foreach (SvgElement child in element.Children)
		{
			CombinePaths(path, child, renderer);
		}
	}

	protected override void Render(ISvgRenderer renderer)
	{
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgClipPathProperty in SvgClipPathProperties)
		{
			yield return svgClipPathProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgClipPathProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgClipPathProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgClipPathProperties.TryGetValue(attributeName, out var value2))
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
