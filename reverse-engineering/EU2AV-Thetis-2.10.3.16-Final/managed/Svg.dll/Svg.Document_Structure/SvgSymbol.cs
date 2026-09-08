using System;
using System.Reflection;

namespace Svg.Document_Structure;

[Obsolete("Use Svg.SvgSymbol.")]
[SvgElement("")]
public class SvgSymbol : Svg.SvgSymbol
{
	public SvgSymbol()
	{
		typeof(SvgElement).GetField("_elementName", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, "symbol");
	}
}
