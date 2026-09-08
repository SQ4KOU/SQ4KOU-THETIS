#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace Svg;

[Obsolete("Will be removed.Use SvgDeferredPaintServer class instead.")]
public class SvgFallbackPaintServer : SvgPaintServer
{
	private IEnumerable<SvgPaintServer> _fallbacks;

	private SvgPaintServer _primary;

	internal static List<Type> SvgFallbackPaintServerClassNames = new List<Type> { typeof(SvgFallbackPaintServer) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgFallbackPaintServerProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	internal override string AttributeName => "";

	internal override List<Type> ClassNames => SvgFallbackPaintServerClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgFallbackPaintServerProperties;

	public SvgFallbackPaintServer()
	{
	}

	public SvgFallbackPaintServer(SvgPaintServer primary, IEnumerable<SvgPaintServer> fallbacks)
		: this()
	{
		_fallbacks = fallbacks;
		_primary = primary;
	}

	public override SvgElement DeepCopy()
	{
		return base.DeepCopy<SvgFallbackPaintServer>();
	}

	public override SvgElement DeepCopy<T>()
	{
		SvgFallbackPaintServer svgFallbackPaintServer = base.DeepCopy<T>() as SvgFallbackPaintServer;
		svgFallbackPaintServer._primary = _primary?.DeepCopy() as SvgPaintServer;
		if (_fallbacks != null)
		{
			List<SvgPaintServer> list = new List<SvgPaintServer>();
			foreach (SvgPaintServer fallback in _fallbacks)
			{
				list.Add(fallback.DeepCopy() as SvgPaintServer);
			}
			svgFallbackPaintServer._fallbacks = list;
		}
		return svgFallbackPaintServer;
	}

	public override Brush GetBrush(SvgVisualElement styleOwner, ISvgRenderer renderer, float opacity, bool forStroke = false)
	{
		try
		{
			_primary.GetCallback = () => _fallbacks.FirstOrDefault();
			return _primary.GetBrush(styleOwner, renderer, opacity, forStroke);
		}
		finally
		{
			_primary.GetCallback = null;
		}
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgFallbackPaintServerProperty in SvgFallbackPaintServerProperties)
		{
			yield return svgFallbackPaintServerProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgFallbackPaintServerProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgFallbackPaintServerProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgFallbackPaintServerProperties.TryGetValue(attributeName, out var value2))
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
