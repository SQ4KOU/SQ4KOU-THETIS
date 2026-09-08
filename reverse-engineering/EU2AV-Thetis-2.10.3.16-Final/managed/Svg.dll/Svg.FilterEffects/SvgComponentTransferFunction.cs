#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

public abstract class SvgComponentTransferFunction : SvgElement
{
	internal static List<Type> SvgComponentTransferFunctionClassNames = new List<Type> { typeof(SvgComponentTransferFunction) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgComponentTransferFunctionProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["type"] = new SvgPropertyDescriptor<SvgComponentTransferFunction, SvgComponentTransferType>(DescriptorType.Property, "type", "http://www.w3.org/2000/svg", new SvgComponentTransferTypeConverter(), (SvgComponentTransferFunction t) => t.Type, delegate(SvgComponentTransferFunction t, SvgComponentTransferType v)
		{
			t.Type = v;
		}),
		["tableValues"] = new SvgPropertyDescriptor<SvgComponentTransferFunction, SvgNumberCollection>(DescriptorType.Property, "tableValues", "http://www.w3.org/2000/svg", new SvgNumberCollectionConverter(), (SvgComponentTransferFunction t) => t.TableValues, delegate(SvgComponentTransferFunction t, SvgNumberCollection v)
		{
			t.TableValues = v;
		}),
		["slope"] = new SvgPropertyDescriptor<SvgComponentTransferFunction, float>(DescriptorType.Property, "slope", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgComponentTransferFunction t) => t.Slope, delegate(SvgComponentTransferFunction t, float v)
		{
			t.Slope = v;
		}),
		["intercept"] = new SvgPropertyDescriptor<SvgComponentTransferFunction, float>(DescriptorType.Property, "intercept", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgComponentTransferFunction t) => t.Intercept, delegate(SvgComponentTransferFunction t, float v)
		{
			t.Intercept = v;
		}),
		["amplitude"] = new SvgPropertyDescriptor<SvgComponentTransferFunction, float>(DescriptorType.Property, "amplitude", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgComponentTransferFunction t) => t.Amplitude, delegate(SvgComponentTransferFunction t, float v)
		{
			t.Amplitude = v;
		}),
		["exponent"] = new SvgPropertyDescriptor<SvgComponentTransferFunction, float>(DescriptorType.Property, "exponent", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgComponentTransferFunction t) => t.Exponent, delegate(SvgComponentTransferFunction t, float v)
		{
			t.Exponent = v;
		}),
		["offset"] = new SvgPropertyDescriptor<SvgComponentTransferFunction, float>(DescriptorType.Property, "offset", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgComponentTransferFunction t) => t.Offset, delegate(SvgComponentTransferFunction t, float v)
		{
			t.Offset = v;
		})
	};

	[SvgAttribute("type")]
	public SvgComponentTransferType Type
	{
		get
		{
			return GetAttribute("type", inherited: false, SvgComponentTransferType.Identity);
		}
		set
		{
			Attributes["type"] = value;
		}
	}

	[SvgAttribute("tableValues")]
	public SvgNumberCollection TableValues
	{
		get
		{
			return GetAttribute("tableValues", inherited: false, new SvgNumberCollection());
		}
		set
		{
			Attributes["tableValues"] = value;
		}
	}

	[SvgAttribute("slope")]
	public float Slope
	{
		get
		{
			return GetAttribute("slope", inherited: false, 1f);
		}
		set
		{
			Attributes["slope"] = value;
		}
	}

	[SvgAttribute("intercept")]
	public float Intercept
	{
		get
		{
			return GetAttribute("intercept", inherited: false, 0f);
		}
		set
		{
			Attributes["intercept"] = value;
		}
	}

	[SvgAttribute("amplitude")]
	public float Amplitude
	{
		get
		{
			return GetAttribute("amplitude", inherited: false, 1f);
		}
		set
		{
			Attributes["amplitude"] = value;
		}
	}

	[SvgAttribute("exponent")]
	public float Exponent
	{
		get
		{
			return GetAttribute("exponent", inherited: false, 1f);
		}
		set
		{
			Attributes["exponent"] = value;
		}
	}

	[SvgAttribute("offset")]
	public float Offset
	{
		get
		{
			return GetAttribute("offset", inherited: false, 0f);
		}
		set
		{
			Attributes["offset"] = value;
		}
	}

	internal override string AttributeName => "";

	internal override List<Type> ClassNames => SvgComponentTransferFunctionClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgComponentTransferFunctionProperties;

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgComponentTransferFunctionProperty in SvgComponentTransferFunctionProperties)
		{
			yield return svgComponentTransferFunctionProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgComponentTransferFunctionProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgComponentTransferFunctionProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgComponentTransferFunctionProperties.TryGetValue(attributeName, out var value2))
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
