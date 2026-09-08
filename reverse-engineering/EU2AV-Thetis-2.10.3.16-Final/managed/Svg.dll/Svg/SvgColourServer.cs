#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

namespace Svg;

public class SvgColourServer : SvgPaintServer
{
	private Color _colour;

	internal static List<Type> SvgColourServerClassNames = new List<Type> { typeof(SvgColourServer) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgColourServerProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	public Color Colour
	{
		get
		{
			return _colour;
		}
		set
		{
			_colour = value;
		}
	}

	internal override string AttributeName => "";

	internal override List<Type> ClassNames => SvgColourServerClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgColourServerProperties;

	public SvgColourServer()
		: this(System.Drawing.Color.Black)
	{
	}

	public SvgColourServer(Color colour)
	{
		_colour = colour;
	}

	public override string ToString()
	{
		if (this == SvgPaintServer.None)
		{
			return "none";
		}
		if (this == SvgPaintServer.NotSet)
		{
			return string.Empty;
		}
		if (this == SvgPaintServer.Inherit)
		{
			return "inherit";
		}
		Color colour = Colour;
		if (colour.IsKnownColor)
		{
			return colour.Name;
		}
		return string.Format("#{0}", colour.ToArgb().ToString("x8").Substring(2));
	}

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgColourServer>();
	}

	public override SvgElement DeepCopy<T>()
	{
		if (this == SvgPaintServer.None || this == SvgPaintServer.Inherit || this == SvgPaintServer.NotSet)
		{
			return this;
		}
		SvgColourServer obj = base.DeepCopy<T>() as SvgColourServer;
		obj.Colour = Colour;
		return obj;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SvgColourServer svgColourServer))
		{
			return false;
		}
		if ((this == SvgPaintServer.None && obj != SvgPaintServer.None) || (this != SvgPaintServer.None && obj == SvgPaintServer.None) || (this == SvgPaintServer.NotSet && obj != SvgPaintServer.NotSet) || (this != SvgPaintServer.NotSet && obj == SvgPaintServer.NotSet) || (this == SvgPaintServer.Inherit && obj != SvgPaintServer.Inherit) || (this != SvgPaintServer.Inherit && obj == SvgPaintServer.Inherit))
		{
			return false;
		}
		return GetHashCode() == svgColourServer.GetHashCode();
	}

	public override int GetHashCode()
	{
		return _colour.GetHashCode();
	}

	public override Brush GetBrush(SvgVisualElement styleOwner, ISvgRenderer renderer, float opacity, bool forStroke = false)
	{
		if (this == SvgPaintServer.None)
		{
			return new SolidBrush(System.Drawing.Color.Transparent);
		}
		if ((this == SvgPaintServer.NotSet) & forStroke)
		{
			return new SolidBrush(System.Drawing.Color.Transparent);
		}
		return new SolidBrush(System.Drawing.Color.FromArgb((int)Math.Round((double)opacity * ((double)(int)Colour.A / 255.0) * 255.0), Colour));
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgColourServerProperty in SvgColourServerProperties)
		{
			yield return svgColourServerProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgColourServerProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgColourServerProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgColourServerProperties.TryGetValue(attributeName, out var value2))
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
