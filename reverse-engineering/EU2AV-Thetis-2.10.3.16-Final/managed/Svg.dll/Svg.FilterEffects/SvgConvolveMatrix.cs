#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feConvolveMatrix")]
public class SvgConvolveMatrix : SvgFilterPrimitive
{
	internal static List<Type> SvgConvolveMatrixClassNames = new List<Type> { typeof(SvgConvolveMatrix) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgConvolveMatrixProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["order"] = new SvgPropertyDescriptor<SvgConvolveMatrix, SvgNumberCollection>(DescriptorType.Property, "order", "http://www.w3.org/2000/svg", new SvgNumberCollectionConverter(), (SvgConvolveMatrix t) => t.Order, delegate(SvgConvolveMatrix t, SvgNumberCollection v)
		{
			t.Order = v;
		}),
		["kernelMatrix"] = new SvgPropertyDescriptor<SvgConvolveMatrix, SvgNumberCollection>(DescriptorType.Property, "kernelMatrix", "http://www.w3.org/2000/svg", new SvgNumberCollectionConverter(), (SvgConvolveMatrix t) => t.KernelMatrix, delegate(SvgConvolveMatrix t, SvgNumberCollection v)
		{
			t.KernelMatrix = v;
		}),
		["divisor"] = new SvgPropertyDescriptor<SvgConvolveMatrix, float>(DescriptorType.Property, "divisor", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgConvolveMatrix t) => t.Divisor, delegate(SvgConvolveMatrix t, float v)
		{
			t.Divisor = v;
		}),
		["bias"] = new SvgPropertyDescriptor<SvgConvolveMatrix, float>(DescriptorType.Property, "bias", "http://www.w3.org/2000/svg", new SingleConverter(), (SvgConvolveMatrix t) => t.Bias, delegate(SvgConvolveMatrix t, float v)
		{
			t.Bias = v;
		}),
		["targetX"] = new SvgPropertyDescriptor<SvgConvolveMatrix, int>(DescriptorType.Property, "targetX", "http://www.w3.org/2000/svg", new Int32Converter(), (SvgConvolveMatrix t) => t.TargetX, delegate(SvgConvolveMatrix t, int v)
		{
			t.TargetX = v;
		}),
		["targetY"] = new SvgPropertyDescriptor<SvgConvolveMatrix, int>(DescriptorType.Property, "targetY", "http://www.w3.org/2000/svg", new Int32Converter(), (SvgConvolveMatrix t) => t.TargetY, delegate(SvgConvolveMatrix t, int v)
		{
			t.TargetY = v;
		}),
		["edgeMode"] = new SvgPropertyDescriptor<SvgConvolveMatrix, SvgEdgeMode>(DescriptorType.Property, "edgeMode", "http://www.w3.org/2000/svg", new SvgEdgeModeConverter(), (SvgConvolveMatrix t) => t.EdgeMode, delegate(SvgConvolveMatrix t, SvgEdgeMode v)
		{
			t.EdgeMode = v;
		}),
		["kernelUnitLength"] = new SvgPropertyDescriptor<SvgConvolveMatrix, SvgNumberCollection>(DescriptorType.Property, "kernelUnitLength", "http://www.w3.org/2000/svg", new SvgNumberCollectionConverter(), (SvgConvolveMatrix t) => t.KernelUnitLength, delegate(SvgConvolveMatrix t, SvgNumberCollection v)
		{
			t.KernelUnitLength = v;
		}),
		["preserveAlpha"] = new SvgPropertyDescriptor<SvgConvolveMatrix, bool>(DescriptorType.Property, "preserveAlpha", "http://www.w3.org/2000/svg", new BooleanConverter(), (SvgConvolveMatrix t) => t.PreserveAlpha, delegate(SvgConvolveMatrix t, bool v)
		{
			t.PreserveAlpha = v;
		})
	};

	[SvgAttribute("order")]
	public SvgNumberCollection Order
	{
		get
		{
			return GetAttribute("order", inherited: false, new SvgNumberCollection { 3f, 3f });
		}
		set
		{
			Attributes["order"] = value;
		}
	}

	[SvgAttribute("kernelMatrix")]
	public SvgNumberCollection KernelMatrix
	{
		get
		{
			return GetAttribute<SvgNumberCollection>("kernelMatrix", inherited: false);
		}
		set
		{
			Attributes["kernelMatrix"] = value;
		}
	}

	[SvgAttribute("divisor")]
	public float Divisor
	{
		get
		{
			return GetAttribute("divisor", inherited: false, 0f);
		}
		set
		{
			Attributes["divisor"] = value;
		}
	}

	[SvgAttribute("bias")]
	public float Bias
	{
		get
		{
			return GetAttribute("bias", inherited: false, 0f);
		}
		set
		{
			Attributes["bias"] = value;
		}
	}

	[SvgAttribute("targetX")]
	public int TargetX
	{
		get
		{
			return GetAttribute("targetX", inherited: false, 0);
		}
		set
		{
			Attributes["targetX"] = value;
		}
	}

	[SvgAttribute("targetY")]
	public int TargetY
	{
		get
		{
			return GetAttribute("targetY", inherited: false, 0);
		}
		set
		{
			Attributes["targetY"] = value;
		}
	}

	[SvgAttribute("edgeMode")]
	public SvgEdgeMode EdgeMode
	{
		get
		{
			return GetAttribute("edgeMode", inherited: false, SvgEdgeMode.Duplicate);
		}
		set
		{
			Attributes["edgeMode"] = value;
		}
	}

	[SvgAttribute("kernelUnitLength")]
	public SvgNumberCollection KernelUnitLength
	{
		get
		{
			return GetAttribute("kernelUnitLength", inherited: false, new SvgNumberCollection { 1f, 1f });
		}
		set
		{
			Attributes["kernelUnitLength"] = value;
		}
	}

	[SvgAttribute("preserveAlpha")]
	public bool PreserveAlpha
	{
		get
		{
			return GetAttribute("preserveAlpha", inherited: false, defaultValue: false);
		}
		set
		{
			Attributes["preserveAlpha"] = value;
		}
	}

	internal override string AttributeName => "feConvolveMatrix";

	internal override List<Type> ClassNames => SvgConvolveMatrixClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgConvolveMatrixProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgConvolveMatrix>();
	}

	public override void Process(ImageBuffer buffer)
	{
		buffer[base.Result] = buffer[base.Input];
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgConvolveMatrixProperty in SvgConvolveMatrixProperties)
		{
			yield return svgConvolveMatrixProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgConvolveMatrixProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgConvolveMatrixProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgConvolveMatrixProperties.TryGetValue(attributeName, out var value2))
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
