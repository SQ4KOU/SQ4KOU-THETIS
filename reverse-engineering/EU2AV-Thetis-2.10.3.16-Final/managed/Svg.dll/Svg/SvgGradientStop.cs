#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

namespace Svg;

[SvgElement("stop")]
public class SvgGradientStop : SvgElement
{
	private SvgUnit _offset;

	internal static List<Type> SvgGradientStopClassNames = new List<Type> { typeof(SvgGradientStop) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgGradientStopProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["offset"] = new SvgPropertyDescriptor<SvgGradientStop, SvgUnit>(DescriptorType.Property, "offset", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgGradientStop t) => t.Offset, delegate(SvgGradientStop t, SvgUnit v)
		{
			t.Offset = v;
		}),
		["stop-color"] = new SvgPropertyDescriptor<SvgGradientStop, SvgPaintServer>(DescriptorType.Property, "stop-color", "http://www.w3.org/2000/svg", new SvgPaintServerFactory(), (SvgGradientStop t) => t.StopColor, delegate(SvgGradientStop t, SvgPaintServer v)
		{
			t.StopColor = v;
		}),
		["stop-opacity"] = new SvgPropertyDescriptor<SvgGradientStop, float>(DescriptorType.Property, "stop-opacity", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgGradientStop t) => t.StopOpacity, delegate(SvgGradientStop t, float v)
		{
			t.StopOpacity = v;
		})
	};

	[SvgAttribute("offset")]
	public SvgUnit Offset
	{
		get
		{
			return _offset;
		}
		set
		{
			SvgUnit svgUnit = value;
			if (svgUnit.Type == SvgUnitType.Percentage)
			{
				svgUnit = new SvgUnit(svgUnit.Type, Math.Min(Math.Max(svgUnit.Value, 0f), 100f));
			}
			else if (svgUnit.Type == SvgUnitType.User)
			{
				svgUnit = new SvgUnit(svgUnit.Type, Math.Min(Math.Max(svgUnit.Value, 0f), 1f));
			}
			_offset = svgUnit.ToPercentage();
			Attributes["offset"] = svgUnit;
		}
	}

	[SvgAttribute("stop-color")]
	[TypeConverter(typeof(SvgPaintServerFactory))]
	public SvgPaintServer StopColor
	{
		get
		{
			return GetAttribute("stop-color", true, (SvgPaintServer)new SvgColourServer(System.Drawing.Color.Black));
		}
		set
		{
			Attributes["stop-color"] = value;
		}
	}

	[SvgAttribute("stop-opacity")]
	public float StopOpacity
	{
		get
		{
			return GetAttribute("stop-opacity", inherited: true, 1f);
		}
		set
		{
			Attributes["stop-opacity"] = SvgElement.FixOpacityValue(value);
		}
	}

	internal override string AttributeName => "stop";

	internal override List<Type> ClassNames => SvgGradientStopClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgGradientStopProperties;

	public SvgGradientStop()
	{
		_offset = new SvgUnit(0f);
	}

	public SvgGradientStop(SvgUnit offset, Color colour)
	{
		_offset = offset;
	}

	public Color GetColor(SvgElement parent)
	{
		return (SvgDeferredPaintServer.TryGet<SvgColourServer>(StopColor, parent) ?? throw new InvalidOperationException("Invalid paint server for gradient stop detected.")).Colour;
	}

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgGradientStop>();
	}

	public override SvgElement DeepCopy<T>()
	{
		SvgGradientStop obj = base.DeepCopy<T>() as SvgGradientStop;
		obj._offset = _offset;
		return obj;
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgGradientStopProperty in SvgGradientStopProperties)
		{
			yield return svgGradientStopProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgGradientStopProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgGradientStopProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgGradientStopProperties.TryGetValue(attributeName, out var value2))
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
