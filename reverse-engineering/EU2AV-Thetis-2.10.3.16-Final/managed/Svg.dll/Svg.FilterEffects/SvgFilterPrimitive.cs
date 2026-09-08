#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

public abstract class SvgFilterPrimitive : SvgElement
{
	public const string SourceGraphic = "SourceGraphic";

	public const string SourceAlpha = "SourceAlpha";

	public const string BackgroundImage = "BackgroundImage";

	public const string BackgroundAlpha = "BackgroundAlpha";

	public const string FillPaint = "FillPaint";

	public const string StrokePaint = "StrokePaint";

	internal static List<Type> SvgFilterPrimitiveClassNames = new List<Type> { typeof(SvgFilterPrimitive) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgFilterPrimitiveProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["x"] = new SvgPropertyDescriptor<SvgFilterPrimitive, SvgUnit>(DescriptorType.Property, "x", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgFilterPrimitive t) => t.X, delegate(SvgFilterPrimitive t, SvgUnit v)
		{
			t.X = v;
		}),
		["y"] = new SvgPropertyDescriptor<SvgFilterPrimitive, SvgUnit>(DescriptorType.Property, "y", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgFilterPrimitive t) => t.Y, delegate(SvgFilterPrimitive t, SvgUnit v)
		{
			t.Y = v;
		}),
		["width"] = new SvgPropertyDescriptor<SvgFilterPrimitive, SvgUnit>(DescriptorType.Property, "width", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgFilterPrimitive t) => t.Width, delegate(SvgFilterPrimitive t, SvgUnit v)
		{
			t.Width = v;
		}),
		["height"] = new SvgPropertyDescriptor<SvgFilterPrimitive, SvgUnit>(DescriptorType.Property, "height", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgFilterPrimitive t) => t.Height, delegate(SvgFilterPrimitive t, SvgUnit v)
		{
			t.Height = v;
		}),
		["in"] = new SvgPropertyDescriptor<SvgFilterPrimitive, string>(DescriptorType.Property, "in", "http://www.w3.org/2000/svg", new StringConverter(), (SvgFilterPrimitive t) => t.Input, delegate(SvgFilterPrimitive t, string v)
		{
			t.Input = v;
		}),
		["result"] = new SvgPropertyDescriptor<SvgFilterPrimitive, string>(DescriptorType.Property, "result", "http://www.w3.org/2000/svg", new StringConverter(), (SvgFilterPrimitive t) => t.Result, delegate(SvgFilterPrimitive t, string v)
		{
			t.Result = v;
		})
	};

	[SvgAttribute("x")]
	public SvgUnit X
	{
		get
		{
			return GetAttribute("x", inherited: false, new SvgUnit(SvgUnitType.Percentage, 0f));
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
			return GetAttribute("y", inherited: false, new SvgUnit(SvgUnitType.Percentage, 0f));
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

	[SvgAttribute("in")]
	public string Input
	{
		get
		{
			return GetAttribute<string>("in", inherited: false);
		}
		set
		{
			Attributes["in"] = value;
		}
	}

	[SvgAttribute("result")]
	public string Result
	{
		get
		{
			return GetAttribute<string>("result", inherited: false);
		}
		set
		{
			Attributes["result"] = value;
		}
	}

	protected SvgFilter Owner => (SvgFilter)Parent;

	internal override string AttributeName => "";

	internal override List<Type> ClassNames => SvgFilterPrimitiveClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgFilterPrimitiveProperties;

	public abstract void Process(ImageBuffer buffer);

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgFilterPrimitiveProperty in SvgFilterPrimitiveProperties)
		{
			yield return svgFilterPrimitiveProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgFilterPrimitiveProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgFilterPrimitiveProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgFilterPrimitiveProperties.TryGetValue(attributeName, out var value2))
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
