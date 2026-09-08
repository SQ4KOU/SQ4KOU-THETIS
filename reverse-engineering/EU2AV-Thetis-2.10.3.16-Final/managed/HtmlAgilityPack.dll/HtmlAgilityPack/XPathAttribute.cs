using System;

namespace HtmlAgilityPack;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class XPathAttribute : Attribute
{
	private ReturnType _nodeReturnType;

	internal bool IsNodeReturnTypeExplicitlySet { get; private set; }

	public string XPath { get; }

	public string AttributeName { get; set; }

	public ReturnType NodeReturnType
	{
		get
		{
			return _nodeReturnType;
		}
		set
		{
			_nodeReturnType = value;
			IsNodeReturnTypeExplicitlySet = true;
		}
	}

	public XPathAttribute(string xpathString)
	{
		XPath = xpathString;
		_nodeReturnType = ReturnType.InnerText;
	}

	public XPathAttribute(string xpathString, ReturnType nodeReturnType)
	{
		XPath = xpathString;
		NodeReturnType = nodeReturnType;
	}

	public XPathAttribute(string xpathString, string attributeName)
	{
		XPath = xpathString;
		AttributeName = attributeName;
		_nodeReturnType = ReturnType.InnerText;
	}
}
