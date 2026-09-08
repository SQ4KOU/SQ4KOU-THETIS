using System;

namespace Svg;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Event)]
public class SvgAttributeAttribute : Attribute
{
	public const string XLinkNamespace = "http://www.w3.org/1999/xlink";

	public const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";

	public string Name { get; }

	public string NameSpace { get; }

	public override bool Equals(object obj)
	{
		if (!(obj is SvgAttributeAttribute))
		{
			return false;
		}
		SvgAttributeAttribute svgAttributeAttribute = (SvgAttributeAttribute)obj;
		if (svgAttributeAttribute.Name == string.Empty)
		{
			return false;
		}
		return string.Equals(Name, svgAttributeAttribute.Name);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	internal SvgAttributeAttribute()
		: this(string.Empty)
	{
	}

	internal SvgAttributeAttribute(string name)
		: this(name, "http://www.w3.org/2000/svg")
	{
	}

	public SvgAttributeAttribute(string name, string nameSpace)
	{
		Name = name;
		NameSpace = nameSpace;
	}
}
