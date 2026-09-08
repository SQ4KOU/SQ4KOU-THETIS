#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace Svg.FilterEffects;

[SvgElement("feMerge")]
public class SvgMerge : SvgFilterPrimitive
{
	internal static List<Type> SvgMergeClassNames = new List<Type> { typeof(SvgMerge) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgMergeProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "feMerge";

	internal override List<Type> ClassNames => SvgMergeClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgMergeProperties;

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgMerge>();
	}

	public override void Process(ImageBuffer buffer)
	{
		List<SvgMergeNode> list = Children.OfType<SvgMergeNode>().ToList();
		Bitmap bitmap = buffer[list.First().Input];
		Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height);
		using (Graphics graphics = Graphics.FromImage(bitmap2))
		{
			foreach (SvgMergeNode item in list)
			{
				graphics.DrawImage(buffer[item.Input], new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel);
			}
			graphics.Flush();
		}
		buffer[base.Result] = bitmap2;
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgMergeProperty in SvgMergeProperties)
		{
			yield return svgMergeProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgMergeProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgMergeProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgMergeProperties.TryGetValue(attributeName, out var value2))
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
