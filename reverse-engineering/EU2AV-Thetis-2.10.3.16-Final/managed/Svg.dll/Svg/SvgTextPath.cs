#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace Svg;

[SvgElement("textPath")]
public class SvgTextPath : SvgTextBase
{
	internal static List<Type> SvgTextPathClassNames = new List<Type> { typeof(SvgTextPath) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgTextPathProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["startOffset"] = new SvgPropertyDescriptor<SvgTextPath, SvgUnit>(DescriptorType.Property, "startOffset", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgTextPath t) => t.StartOffset, delegate(SvgTextPath t, SvgUnit v)
		{
			t.StartOffset = v;
		}),
		["method"] = new SvgPropertyDescriptor<SvgTextPath, SvgTextPathMethod>(DescriptorType.Property, "method", "http://www.w3.org/2000/svg", new SvgTextPathMethodConverter(), (SvgTextPath t) => t.Method, delegate(SvgTextPath t, SvgTextPathMethod v)
		{
			t.Method = v;
		}),
		["spacing"] = new SvgPropertyDescriptor<SvgTextPath, SvgTextPathSpacing>(DescriptorType.Property, "spacing", "http://www.w3.org/2000/svg", new SvgTextPathSpacingConverter(), (SvgTextPath t) => t.Spacing, delegate(SvgTextPath t, SvgTextPathSpacing v)
		{
			t.Spacing = v;
		}),
		["href"] = new SvgPropertyDescriptor<SvgTextPath, Uri>(DescriptorType.Property, "href", "http://www.w3.org/1999/xlink", new UriTypeConverter(), (SvgTextPath t) => t.ReferencedPath, delegate(SvgTextPath t, Uri v)
		{
			t.ReferencedPath = v;
		})
	};

	public override SvgUnitCollection Dx
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[SvgAttribute("startOffset")]
	public virtual SvgUnit StartOffset
	{
		get
		{
			if (base.Dx.Count >= 1)
			{
				return base.Dx[0];
			}
			return SvgUnit.None;
		}
		set
		{
			if (base.Dx.Count < 1)
			{
				base.Dx.Add(value);
			}
			else
			{
				base.Dx[0] = value;
			}
			Attributes["startOffset"] = value;
		}
	}

	[SvgAttribute("method")]
	public virtual SvgTextPathMethod Method
	{
		get
		{
			return GetAttribute("method", inherited: true, SvgTextPathMethod.Align);
		}
		set
		{
			Attributes["method"] = value;
		}
	}

	[SvgAttribute("spacing")]
	public virtual SvgTextPathSpacing Spacing
	{
		get
		{
			return GetAttribute("spacing", inherited: true, SvgTextPathSpacing.Exact);
		}
		set
		{
			Attributes["spacing"] = value;
		}
	}

	[SvgAttribute("href", "http://www.w3.org/1999/xlink")]
	public virtual Uri ReferencedPath
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

	internal override string AttributeName => "textPath";

	internal override List<Type> ClassNames => SvgTextPathClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgTextPathProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgTextPath>();
	}

	protected override GraphicsPath GetBaselinePath(ISvgRenderer renderer)
	{
		if (!(OwnerDocument.IdManager.GetElementById(ReferencedPath) is SvgVisualElement svgVisualElement))
		{
			return null;
		}
		GraphicsPath graphicsPath = (GraphicsPath)svgVisualElement.Path(renderer).Clone();
		if (svgVisualElement.Transforms != null && svgVisualElement.Transforms.Count > 0)
		{
			using Matrix matrix = svgVisualElement.Transforms.GetMatrix();
			graphicsPath.Transform(matrix);
		}
		return graphicsPath;
	}

	protected override float GetAuthorPathLength()
	{
		if (!(OwnerDocument.IdManager.GetElementById(ReferencedPath) is SvgPath svgPath))
		{
			return 0f;
		}
		return svgPath.PathLength;
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgTextPathProperty in SvgTextPathProperties)
		{
			yield return svgTextPathProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgTextPathProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgTextPathProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgTextPathProperties.TryGetValue(attributeName, out var value2))
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
