#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using Svg.ExtensionMethods;

namespace Svg;

public abstract class SvgMarkerElement : SvgPathBasedElement
{
	internal static List<Type> SvgMarkerElementClassNames = new List<Type> { typeof(SvgMarkerElement) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgMarkerElementProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["marker-end"] = new SvgPropertyDescriptor<SvgMarkerElement, Uri>(DescriptorType.Property, "marker-end", "http://www.w3.org/2000/svg", new UriTypeConverter(), (SvgMarkerElement t) => t.MarkerEnd, delegate(SvgMarkerElement t, Uri v)
		{
			t.MarkerEnd = v;
		}),
		["marker-mid"] = new SvgPropertyDescriptor<SvgMarkerElement, Uri>(DescriptorType.Property, "marker-mid", "http://www.w3.org/2000/svg", new UriTypeConverter(), (SvgMarkerElement t) => t.MarkerMid, delegate(SvgMarkerElement t, Uri v)
		{
			t.MarkerMid = v;
		}),
		["marker-start"] = new SvgPropertyDescriptor<SvgMarkerElement, Uri>(DescriptorType.Property, "marker-start", "http://www.w3.org/2000/svg", new UriTypeConverter(), (SvgMarkerElement t) => t.MarkerStart, delegate(SvgMarkerElement t, Uri v)
		{
			t.MarkerStart = v;
		})
	};

	[SvgAttribute("marker-end")]
	public Uri MarkerEnd
	{
		get
		{
			return GetAttribute<Uri>("marker-end", inherited: true);
		}
		set
		{
			Attributes["marker-end"] = value;
		}
	}

	[SvgAttribute("marker-mid")]
	public Uri MarkerMid
	{
		get
		{
			return GetAttribute<Uri>("marker-mid", inherited: true);
		}
		set
		{
			Attributes["marker-mid"] = value;
		}
	}

	[SvgAttribute("marker-start")]
	public Uri MarkerStart
	{
		get
		{
			return GetAttribute<Uri>("marker-start", inherited: true);
		}
		set
		{
			Attributes["marker-start"] = value;
		}
	}

	internal override string AttributeName => "";

	internal override List<Type> ClassNames => SvgMarkerElementClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgMarkerElementProperties;

	protected internal override bool RenderStroke(ISvgRenderer renderer)
	{
		bool result = base.RenderStroke(renderer);
		GraphicsPath graphicsPath = Path(renderer);
		int pointCount = graphicsPath.PointCount;
		Uri uri = MarkerStart.ReplaceWithNullIfNone();
		if (uri != null)
		{
			PointF pointF = graphicsPath.PathPoints[0];
			int i;
			for (i = 1; i < pointCount && graphicsPath.PathPoints[i] == pointF; i++)
			{
			}
			if (i < pointCount)
			{
				PointF pMarkerPoint = graphicsPath.PathPoints[i];
				OwnerDocument.GetElementById<SvgMarker>(uri.ToString()).RenderMarker(renderer, this, pointF, pointF, pMarkerPoint, isStartMarker: true);
			}
		}
		Uri uri2 = MarkerMid.ReplaceWithNullIfNone();
		if (uri2 != null)
		{
			SvgMarker elementById = OwnerDocument.GetElementById<SvgMarker>(uri2.ToString());
			int num = -1;
			for (int j = 1; j <= graphicsPath.PathPoints.Length - 2; j++)
			{
				num = (((graphicsPath.PathTypes[j] & 7) != 3) ? (-1) : ((num + 1) % 3));
				if (num == -1 || num == 2)
				{
					elementById.RenderMarker(renderer, this, graphicsPath.PathPoints[j], graphicsPath.PathPoints[j - 1], graphicsPath.PathPoints[j], graphicsPath.PathPoints[j + 1]);
				}
			}
		}
		Uri uri3 = MarkerEnd.ReplaceWithNullIfNone();
		if (uri3 != null)
		{
			int num2 = pointCount - 1;
			PointF pointF2 = graphicsPath.PathPoints[num2];
			num2--;
			while (num2 > 0 && graphicsPath.PathPoints[num2] == pointF2)
			{
				num2--;
			}
			PointF pMarkerPoint2 = graphicsPath.PathPoints[num2];
			OwnerDocument.GetElementById<SvgMarker>(uri3.ToString()).RenderMarker(renderer, this, pointF2, pMarkerPoint2, graphicsPath.PathPoints[graphicsPath.PathPoints.Length - 1], isStartMarker: false);
		}
		return result;
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgMarkerElementProperty in SvgMarkerElementProperties)
		{
			yield return svgMarkerElementProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgMarkerElementProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgMarkerElementProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgMarkerElementProperties.TryGetValue(attributeName, out var value2))
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
