using System;

namespace Svg;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SvgElementAttribute : Attribute
{
	public string ElementName { get; private set; }

	public SvgElementAttribute(string elementName)
	{
		ElementName = elementName;
	}
}
