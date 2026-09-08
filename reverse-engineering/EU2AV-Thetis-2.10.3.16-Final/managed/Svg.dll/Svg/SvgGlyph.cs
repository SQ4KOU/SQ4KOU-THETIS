#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using Svg.Pathing;

namespace Svg;

[SvgElement("glyph")]
public class SvgGlyph : SvgPathBasedElement, ISvgPathElement
{
	private GraphicsPath _path;

	internal static List<Type> SvgGlyphClassNames = new List<Type> { typeof(SvgGlyph) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgGlyphProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["d"] = new SvgPropertyDescriptor<SvgGlyph, SvgPathSegmentList>(DescriptorType.Property, "d", "http://www.w3.org/2000/svg", new SvgPathBuilder(), (SvgGlyph t) => t.PathData, delegate(SvgGlyph t, SvgPathSegmentList v)
		{
			t.PathData = v;
		}),
		["glyph-name"] = new SvgPropertyDescriptor<SvgGlyph, string>(DescriptorType.Property, "glyph-name", "http://www.w3.org/2000/svg", new StringConverter(), (SvgGlyph t) => t.GlyphName, delegate(SvgGlyph t, string v)
		{
			t.GlyphName = v;
		}),
		["horiz-adv-x"] = new SvgPropertyDescriptor<SvgGlyph, float>(DescriptorType.Property, "horiz-adv-x", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgGlyph t) => t.HorizAdvX, delegate(SvgGlyph t, float v)
		{
			t.HorizAdvX = v;
		}),
		["unicode"] = new SvgPropertyDescriptor<SvgGlyph, string>(DescriptorType.Property, "unicode", "http://www.w3.org/2000/svg", new StringConverter(), (SvgGlyph t) => t.Unicode, delegate(SvgGlyph t, string v)
		{
			t.Unicode = v;
		}),
		["vert-adv-y"] = new SvgPropertyDescriptor<SvgGlyph, float>(DescriptorType.Property, "vert-adv-y", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgGlyph t) => t.VertAdvY, delegate(SvgGlyph t, float v)
		{
			t.VertAdvY = v;
		}),
		["vert-origin-x"] = new SvgPropertyDescriptor<SvgGlyph, float>(DescriptorType.Property, "vert-origin-x", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgGlyph t) => t.VertOriginX, delegate(SvgGlyph t, float v)
		{
			t.VertOriginX = v;
		}),
		["vert-origin-y"] = new SvgPropertyDescriptor<SvgGlyph, float>(DescriptorType.Property, "vert-origin-y", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgGlyph t) => t.VertOriginY, delegate(SvgGlyph t, float v)
		{
			t.VertOriginY = v;
		})
	};

	[SvgAttribute("d")]
	public SvgPathSegmentList PathData
	{
		get
		{
			return GetAttribute<SvgPathSegmentList>("d", inherited: false);
		}
		set
		{
			SvgPathSegmentList pathData = PathData;
			if (pathData != null)
			{
				pathData.Owner = null;
			}
			Attributes["d"] = value;
			value.Owner = this;
		}
	}

	[SvgAttribute("glyph-name")]
	public virtual string GlyphName
	{
		get
		{
			return GetAttribute<string>("glyph-name", inherited: true);
		}
		set
		{
			Attributes["glyph-name"] = value;
		}
	}

	[SvgAttribute("horiz-adv-x")]
	public float HorizAdvX
	{
		get
		{
			return GetAttribute("horiz-adv-x", inherited: true, base.Parents.OfType<SvgFont>().First().HorizAdvX);
		}
		set
		{
			Attributes["horiz-adv-x"] = value;
		}
	}

	[SvgAttribute("unicode")]
	public string Unicode
	{
		get
		{
			return GetAttribute<string>("unicode", inherited: true);
		}
		set
		{
			Attributes["unicode"] = value;
		}
	}

	[SvgAttribute("vert-adv-y")]
	public float VertAdvY
	{
		get
		{
			return GetAttribute("vert-adv-y", inherited: true, base.Parents.OfType<SvgFont>().First().VertAdvY);
		}
		set
		{
			Attributes["vert-adv-y"] = value;
		}
	}

	[SvgAttribute("vert-origin-x")]
	public float VertOriginX
	{
		get
		{
			return GetAttribute("vert-origin-x", inherited: true, base.Parents.OfType<SvgFont>().First().VertOriginX);
		}
		set
		{
			Attributes["vert-origin-x"] = value;
		}
	}

	[SvgAttribute("vert-origin-y")]
	public float VertOriginY
	{
		get
		{
			return GetAttribute("vert-origin-y", inherited: true, base.Parents.OfType<SvgFont>().First().VertOriginY);
		}
		set
		{
			Attributes["vert-origin-y"] = value;
		}
	}

	internal override string AttributeName => "glyph";

	internal override List<Type> ClassNames => SvgGlyphClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgGlyphProperties;

	public void OnPathUpdated()
	{
	}

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgGlyph>();
	}

	public override SvgElement DeepCopy<T>()
	{
		SvgGlyph svgGlyph = base.DeepCopy<T>() as SvgGlyph;
		if (svgGlyph.PathData != null)
		{
			svgGlyph.PathData.Owner = svgGlyph;
		}
		return svgGlyph;
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		if (_path == null || IsPathDirty)
		{
			_path = new GraphicsPath();
			if (PathData != null)
			{
				PointF start = PointF.Empty;
				foreach (SvgPathSegment pathDatum in PathData)
				{
					start = pathDatum.AddToPath(_path, start, PathData);
				}
			}
			IsPathDirty = false;
		}
		return _path;
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgGlyphProperty in SvgGlyphProperties)
		{
			yield return svgGlyphProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgGlyphProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgGlyphProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgGlyphProperties.TryGetValue(attributeName, out var value2))
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
