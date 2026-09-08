#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Svg;

[TypeConverter(typeof(SvgDeferredPaintServerFactory))]
public class SvgDeferredPaintServer : SvgPaintServer
{
	private bool _serverLoaded;

	private SvgPaintServer _concreteServer;

	private SvgPaintServer _fallbackServer;

	internal static List<Type> SvgDeferredPaintServerClassNames = new List<Type> { typeof(SvgDeferredPaintServer) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgDeferredPaintServerProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	[Obsolete("Will be removed.")]
	public SvgDocument Document { get; set; }

	public string DeferredId { get; set; }

	public SvgPaintServer FallbackServer { get; private set; }

	internal override string AttributeName => "";

	internal override List<Type> ClassNames => SvgDeferredPaintServerClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgDeferredPaintServerProperties;

	public SvgDeferredPaintServer()
	{
	}

	[Obsolete("Will be removed.")]
	public SvgDeferredPaintServer(SvgDocument document, string id)
	{
		Document = document;
		DeferredId = id;
	}

	public SvgDeferredPaintServer(string id)
		: this(id, null)
	{
	}

	public SvgDeferredPaintServer(string id, SvgPaintServer fallbackServer)
	{
		DeferredId = id;
		FallbackServer = fallbackServer;
	}

	public void EnsureServer(SvgElement styleOwner)
	{
		if (_serverLoaded || styleOwner == null)
		{
			return;
		}
		if (DeferredId == "currentColor")
		{
			_concreteServer = styleOwner.ParentsAndSelf.OfType<SvgElement>().FirstOrDefault((SvgElement e) => e.Color != SvgPaintServer.None && e.Color != SvgPaintServer.NotSet && e.Color != SvgPaintServer.Inherit)?.Color;
		}
		else
		{
			_concreteServer = styleOwner.OwnerDocument.IdManager.GetElementById(DeferredId) as SvgPaintServer;
			_fallbackServer = FallbackServer;
			if (_fallbackServer == null)
			{
				_fallbackServer = SvgPaintServer.None;
			}
			else if (!(_fallbackServer is SvgColourServer) && (!(_fallbackServer is SvgDeferredPaintServer) || !string.Equals(((SvgDeferredPaintServer)_fallbackServer).DeferredId, "currentColor")))
			{
				_fallbackServer = SvgPaintServer.Inherit;
			}
		}
		_serverLoaded = true;
	}

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgDeferredPaintServer>();
	}

	public override SvgElement DeepCopy<T>()
	{
		SvgDeferredPaintServer obj = base.DeepCopy<T>() as SvgDeferredPaintServer;
		obj.DeferredId = DeferredId;
		obj.FallbackServer = FallbackServer?.DeepCopy() as SvgPaintServer;
		return obj;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SvgDeferredPaintServer svgDeferredPaintServer))
		{
			return false;
		}
		return DeferredId == svgDeferredPaintServer.DeferredId;
	}

	public override int GetHashCode()
	{
		if (DeferredId != null)
		{
			return DeferredId.GetHashCode();
		}
		return 0;
	}

	public override string ToString()
	{
		if (string.IsNullOrEmpty(DeferredId))
		{
			return string.Empty;
		}
		if (FallbackServer == null)
		{
			return DeferredId;
		}
		return new StringBuilder(DeferredId).Append(" ").Append(FallbackServer.ToString()).ToString();
	}

	public static T TryGet<T>(SvgPaintServer server, SvgElement parent) where T : SvgPaintServer
	{
		if (!(server is SvgDeferredPaintServer))
		{
			return server as T;
		}
		SvgDeferredPaintServer svgDeferredPaintServer = (SvgDeferredPaintServer)server;
		svgDeferredPaintServer.EnsureServer(parent);
		return (svgDeferredPaintServer._concreteServer ?? svgDeferredPaintServer._fallbackServer) as T;
	}

	public override Brush GetBrush(SvgVisualElement styleOwner, ISvgRenderer renderer, float opacity, bool forStroke = false)
	{
		EnsureServer(styleOwner);
		object obj = _concreteServer?.GetBrush(styleOwner, renderer, opacity, forStroke);
		if (obj == null)
		{
			obj = _fallbackServer?.GetBrush(styleOwner, renderer, opacity, forStroke);
			if (obj == null)
			{
				SvgPaintServer notSet = SvgPaintServer.NotSet;
				if (notSet == null)
				{
					return null;
				}
				obj = notSet.GetBrush(styleOwner, renderer, opacity, forStroke);
			}
		}
		return (Brush)obj;
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgDeferredPaintServerProperty in SvgDeferredPaintServerProperties)
		{
			yield return svgDeferredPaintServerProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgDeferredPaintServerProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgDeferredPaintServerProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgDeferredPaintServerProperties.TryGetValue(attributeName, out var value2))
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
