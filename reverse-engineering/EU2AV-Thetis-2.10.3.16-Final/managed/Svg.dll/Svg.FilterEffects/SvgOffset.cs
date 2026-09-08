#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace Svg.FilterEffects;

[SvgElement("feOffset")]
public class SvgOffset : SvgFilterPrimitive
{
	private SvgUnit _dx = 0f;

	private SvgUnit _dy = 0f;

	internal static List<Type> SvgOffsetClassNames = new List<Type> { typeof(SvgOffset) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgOffsetProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["dx"] = new SvgPropertyDescriptor<SvgOffset, SvgUnit>(DescriptorType.Property, "dx", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgOffset t) => t.Dx, delegate(SvgOffset t, SvgUnit v)
		{
			t.Dx = v;
		}),
		["dy"] = new SvgPropertyDescriptor<SvgOffset, SvgUnit>(DescriptorType.Property, "dy", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgOffset t) => t.Dy, delegate(SvgOffset t, SvgUnit v)
		{
			t.Dy = v;
		})
	};

	[SvgAttribute("dx")]
	public SvgUnit Dx
	{
		get
		{
			return _dx;
		}
		set
		{
			_dx = value;
			Attributes["dx"] = value;
		}
	}

	[SvgAttribute("dy")]
	public SvgUnit Dy
	{
		get
		{
			return _dy;
		}
		set
		{
			_dy = value;
			Attributes["dy"] = value;
		}
	}

	internal override string AttributeName => "feOffset";

	internal override List<Type> ClassNames => SvgOffsetClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgOffsetProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgOffset>();
	}

	public override void Process(ImageBuffer buffer)
	{
		Bitmap bitmap = buffer[base.Input];
		Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height);
		PointF[] array = new PointF[1]
		{
			new PointF(Dx.ToDeviceValue(null, UnitRenderingType.Horizontal, null), Dy.ToDeviceValue(null, UnitRenderingType.Vertical, null))
		};
		using (Matrix matrix = buffer.Transform)
		{
			matrix.TransformVectors(array);
		}
		using (Graphics graphics = Graphics.FromImage(bitmap2))
		{
			graphics.DrawImage(bitmap, new Rectangle((int)array[0].X, (int)array[0].Y, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel);
			graphics.Flush();
		}
		buffer[base.Result] = bitmap2;
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgOffsetProperty in SvgOffsetProperties)
		{
			yield return svgOffsetProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgOffsetProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgOffsetProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgOffsetProperties.TryGetValue(attributeName, out var value2))
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
