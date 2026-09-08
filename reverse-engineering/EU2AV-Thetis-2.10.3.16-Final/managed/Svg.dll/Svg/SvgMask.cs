#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg;

[SvgElement("mask")]
public class SvgMask : SvgElement
{
	internal static List<Type> SvgMaskClassNames = new List<Type> { typeof(SvgMask) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgMaskProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["maskUnits"] = new SvgPropertyDescriptor<SvgMask, SvgCoordinateUnits>(DescriptorType.Property, "maskUnits", "http://www.w3.org/2000/svg", new SvgCoordinateUnitsConverter(), (SvgMask t) => t.MaskUnits, delegate(SvgMask t, SvgCoordinateUnits v)
		{
			t.MaskUnits = v;
		}),
		["maskContentUnits"] = new SvgPropertyDescriptor<SvgMask, SvgCoordinateUnits>(DescriptorType.Property, "maskContentUnits", "http://www.w3.org/2000/svg", new SvgCoordinateUnitsConverter(), (SvgMask t) => t.MaskContentUnits, delegate(SvgMask t, SvgCoordinateUnits v)
		{
			t.MaskContentUnits = v;
		}),
		["x"] = new SvgPropertyDescriptor<SvgMask, SvgUnit>(DescriptorType.Property, "x", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgMask t) => t.X, delegate(SvgMask t, SvgUnit v)
		{
			t.X = v;
		}),
		["y"] = new SvgPropertyDescriptor<SvgMask, SvgUnit>(DescriptorType.Property, "y", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgMask t) => t.Y, delegate(SvgMask t, SvgUnit v)
		{
			t.Y = v;
		}),
		["width"] = new SvgPropertyDescriptor<SvgMask, SvgUnit>(DescriptorType.Property, "width", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgMask t) => t.Width, delegate(SvgMask t, SvgUnit v)
		{
			t.Width = v;
		}),
		["height"] = new SvgPropertyDescriptor<SvgMask, SvgUnit>(DescriptorType.Property, "height", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgMask t) => t.Height, delegate(SvgMask t, SvgUnit v)
		{
			t.Height = v;
		})
	};

	[SvgAttribute("maskUnits")]
	public SvgCoordinateUnits MaskUnits
	{
		get
		{
			return GetAttribute("maskUnits", inherited: false, SvgCoordinateUnits.ObjectBoundingBox);
		}
		set
		{
			Attributes["maskUnits"] = value;
		}
	}

	[SvgAttribute("maskContentUnits")]
	public SvgCoordinateUnits MaskContentUnits
	{
		get
		{
			return GetAttribute("maskContentUnits", inherited: false, SvgCoordinateUnits.UserSpaceOnUse);
		}
		set
		{
			Attributes["maskContentUnits"] = value;
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

	internal override string AttributeName => "mask";

	internal override List<Type> ClassNames => SvgMaskClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgMaskProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgMask>();
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgMaskProperty in SvgMaskProperties)
		{
			yield return svgMaskProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgMaskProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgMaskProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgMaskProperties.TryGetValue(attributeName, out var value2))
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
