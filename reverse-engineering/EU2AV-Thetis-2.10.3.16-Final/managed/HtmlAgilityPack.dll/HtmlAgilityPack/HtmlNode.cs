using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace HtmlAgilityPack;

[DebuggerDisplay("Name: {OriginalName}")]
public class HtmlNode : IXPathNavigable
{
	public delegate bool AttributeValueParser<T>(string value, out T result);

	internal const string DepthLevelExceptionMessage = "The document is too complex to parse";

	internal HtmlAttributeCollection _attributes;

	internal HtmlNodeCollection _childnodes;

	internal HtmlNode _endnode;

	private bool _changed;

	internal string _innerhtml;

	internal int _innerlength;

	internal int _innerstartindex;

	internal int _line;

	internal int _lineposition;

	private string _name;

	internal int _namelength;

	internal int _namestartindex;

	internal HtmlNode _nextnode;

	internal HtmlNodeType _nodetype;

	internal string _outerhtml;

	internal int _outerlength;

	internal int _outerstartindex;

	private string _optimizedName;

	internal HtmlDocument _ownerdocument;

	internal HtmlNode _parentnode;

	internal HtmlNode _prevnode;

	internal HtmlNode _prevwithsamename;

	internal bool _starttag;

	internal int _streamposition;

	internal bool _isImplicitEnd;

	internal bool _isHideInnerText;

	public static readonly string HtmlNodeTypeNameComment;

	public static readonly string HtmlNodeTypeNameDocument;

	public static readonly string HtmlNodeTypeNameText;

	public static Dictionary<string, HtmlElementFlag> ElementsFlags;

	private static readonly char[] optimizeAttributesCheckedChars;

	private static readonly char[] spaceSeparator;

	public HtmlAttributeCollection Attributes
	{
		get
		{
			if (!HasAttributes)
			{
				_attributes = new HtmlAttributeCollection(this);
			}
			return _attributes;
		}
		internal set
		{
			_attributes = value;
		}
	}

	public HtmlNodeCollection ChildNodes
	{
		get
		{
			return _childnodes ?? (_childnodes = new HtmlNodeCollection(this));
		}
		internal set
		{
			_childnodes = value;
		}
	}

	public bool Closed => _endnode != null;

	public HtmlAttributeCollection ClosingAttributes
	{
		get
		{
			if (HasClosingAttributes)
			{
				return _endnode.Attributes;
			}
			return new HtmlAttributeCollection(this);
		}
	}

	public HtmlNode EndNode => _endnode;

	public HtmlNode FirstChild
	{
		get
		{
			if (HasChildNodes)
			{
				return _childnodes[0];
			}
			return null;
		}
	}

	public bool HasAttributes
	{
		get
		{
			if (_attributes != null)
			{
				return _attributes.Count > 0;
			}
			return false;
		}
	}

	public bool HasChildNodes
	{
		get
		{
			if (_childnodes == null)
			{
				return false;
			}
			if (_childnodes.Count <= 0)
			{
				return false;
			}
			return true;
		}
	}

	public bool HasClosingAttributes
	{
		get
		{
			if (_endnode == null || _endnode == this)
			{
				return false;
			}
			if (_endnode._attributes == null)
			{
				return false;
			}
			if (_endnode._attributes.Count <= 0)
			{
				return false;
			}
			return true;
		}
	}

	public string Id
	{
		get
		{
			if (_ownerdocument.Nodesid == null)
			{
				throw new Exception(HtmlDocument.HtmlExceptionUseIdAttributeFalse);
			}
			return GetId();
		}
		set
		{
			if (_ownerdocument.Nodesid == null)
			{
				throw new Exception(HtmlDocument.HtmlExceptionUseIdAttributeFalse);
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			SetId(value);
		}
	}

	public virtual string InnerHtml
	{
		get
		{
			if (_changed)
			{
				UpdateHtml();
				return _innerhtml;
			}
			if (_innerhtml != null)
			{
				return _innerhtml;
			}
			if (_innerstartindex < 0 || _innerlength < 0)
			{
				return string.Empty;
			}
			return _ownerdocument.Text.Substring(_innerstartindex, _innerlength);
		}
		set
		{
			HtmlDocument htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(value);
			RemoveAllChildren();
			AppendChildren(htmlDocument.DocumentNode.ChildNodes);
		}
	}

	public virtual string InnerText
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			int depthLevel = 0;
			string name = Name;
			if (name != null)
			{
				bool isDisplayScriptingText = string.Equals(name, "head", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "script", StringComparison.OrdinalIgnoreCase) || string.Equals(name, "style", StringComparison.OrdinalIgnoreCase);
				InternalInnerText(stringBuilder, isDisplayScriptingText, depthLevel);
			}
			else
			{
				InternalInnerText(stringBuilder, isDisplayScriptingText: false, depthLevel);
			}
			return stringBuilder.ToString();
		}
	}

	public HtmlNode LastChild
	{
		get
		{
			if (HasChildNodes)
			{
				return _childnodes[_childnodes.Count - 1];
			}
			return null;
		}
	}

	public int Line
	{
		get
		{
			return _line;
		}
		internal set
		{
			_line = value;
		}
	}

	public int LinePosition
	{
		get
		{
			return _lineposition;
		}
		internal set
		{
			_lineposition = value;
		}
	}

	public int InnerStartIndex => _innerstartindex;

	public int OuterStartIndex => _outerstartindex;

	public int InnerLength => InnerHtml.Length;

	public int OuterLength => OuterHtml.Length;

	public int OriginalOuterLength => _outerlength;

	public string Name
	{
		get
		{
			if (_optimizedName == null)
			{
				if (_name == null)
				{
					SetName(_ownerdocument.Text.Substring(_namestartindex, _namelength));
				}
				if (_name == null)
				{
					_optimizedName = string.Empty;
				}
				else if (OwnerDocument != null)
				{
					_optimizedName = (OwnerDocument.OptionDefaultUseOriginalName ? _name : _name.ToLowerInvariant());
				}
				else
				{
					_optimizedName = _name.ToLowerInvariant();
				}
			}
			return _optimizedName;
		}
		set
		{
			SetName(value);
			if (!(this is HtmlTextNode))
			{
				SetChanged();
			}
		}
	}

	public HtmlNode NextSibling
	{
		get
		{
			return _nextnode;
		}
		internal set
		{
			_nextnode = value;
		}
	}

	public HtmlNodeType NodeType
	{
		get
		{
			return _nodetype;
		}
		internal set
		{
			_nodetype = value;
		}
	}

	public string OriginalName => _name;

	public virtual string OuterHtml
	{
		get
		{
			if (_changed)
			{
				UpdateHtml();
				return _outerhtml;
			}
			if (_outerhtml != null)
			{
				return _outerhtml;
			}
			if (_outerstartindex < 0 || _outerlength < 0)
			{
				return string.Empty;
			}
			return _ownerdocument.Text.Substring(_outerstartindex, _outerlength);
		}
	}

	public HtmlDocument OwnerDocument
	{
		get
		{
			return _ownerdocument;
		}
		internal set
		{
			_ownerdocument = value;
		}
	}

	public HtmlNode ParentNode
	{
		get
		{
			return _parentnode;
		}
		internal set
		{
			_parentnode = value;
		}
	}

	public HtmlNode PreviousSibling
	{
		get
		{
			return _prevnode;
		}
		internal set
		{
			_prevnode = value;
		}
	}

	public int StreamPosition => _streamposition;

	public string XPath => ((ParentNode == null || ParentNode.NodeType == HtmlNodeType.Document) ? "/" : (ParentNode.XPath + "/")) + GetRelativeXpath();

	public int Depth { get; set; }

	static HtmlNode()
	{
		HtmlNodeTypeNameComment = "#comment";
		HtmlNodeTypeNameDocument = "#document";
		HtmlNodeTypeNameText = "#text";
		optimizeAttributesCheckedChars = new char[4] { '\n', '\r', '\t', ' ' };
		spaceSeparator = new char[1] { ' ' };
		ElementsFlags = new Dictionary<string, HtmlElementFlag>(StringComparer.OrdinalIgnoreCase);
		ElementsFlags.Add("script", HtmlElementFlag.CData);
		ElementsFlags.Add("style", HtmlElementFlag.CData);
		ElementsFlags.Add("noxhtml", HtmlElementFlag.CData);
		ElementsFlags.Add("textarea", HtmlElementFlag.CData);
		ElementsFlags.Add("title", HtmlElementFlag.CData);
		ElementsFlags.Add("base", HtmlElementFlag.Empty);
		ElementsFlags.Add("link", HtmlElementFlag.Empty);
		ElementsFlags.Add("meta", HtmlElementFlag.Empty);
		ElementsFlags.Add("isindex", HtmlElementFlag.Empty);
		ElementsFlags.Add("hr", HtmlElementFlag.Empty);
		ElementsFlags.Add("col", HtmlElementFlag.Empty);
		ElementsFlags.Add("img", HtmlElementFlag.Empty);
		ElementsFlags.Add("param", HtmlElementFlag.Empty);
		ElementsFlags.Add("embed", HtmlElementFlag.Empty);
		ElementsFlags.Add("frame", HtmlElementFlag.Empty);
		ElementsFlags.Add("wbr", HtmlElementFlag.Empty);
		ElementsFlags.Add("bgsound", HtmlElementFlag.Empty);
		ElementsFlags.Add("spacer", HtmlElementFlag.Empty);
		ElementsFlags.Add("keygen", HtmlElementFlag.Empty);
		ElementsFlags.Add("area", HtmlElementFlag.Empty);
		ElementsFlags.Add("input", HtmlElementFlag.Empty);
		ElementsFlags.Add("basefont", HtmlElementFlag.Empty);
		ElementsFlags.Add("source", HtmlElementFlag.Empty);
		ElementsFlags.Add("form", HtmlElementFlag.CanOverlap);
		ElementsFlags.Add("br", HtmlElementFlag.Empty | HtmlElementFlag.Closed);
		if (!HtmlDocument.DisableBehaviorTagP)
		{
			ElementsFlags.Add("p", HtmlElementFlag.Empty | HtmlElementFlag.Closed);
		}
	}

	public HtmlNode(HtmlNodeType type, HtmlDocument ownerdocument, int index)
	{
		_nodetype = type;
		_ownerdocument = ownerdocument;
		_outerstartindex = index;
		switch (type)
		{
		case HtmlNodeType.Comment:
			SetName(HtmlNodeTypeNameComment);
			_endnode = this;
			break;
		case HtmlNodeType.Document:
			SetName(HtmlNodeTypeNameDocument);
			_endnode = this;
			break;
		case HtmlNodeType.Text:
			SetName(HtmlNodeTypeNameText);
			_endnode = this;
			break;
		}
		if (_ownerdocument.Openednodes != null && !Closed && -1 != index)
		{
			_ownerdocument.Openednodes.Add(index, this);
		}
		if (-1 == index && type != HtmlNodeType.Comment && type != HtmlNodeType.Text)
		{
			SetChanged();
		}
	}

	internal virtual void InternalInnerText(StringBuilder sb, bool isDisplayScriptingText, int depthLevel)
	{
		depthLevel++;
		if (depthLevel > HtmlDocument.MaxDepthLevel)
		{
			throw new Exception($"Maximum deep level reached: {HtmlDocument.MaxDepthLevel}");
		}
		if (!_ownerdocument.BackwardCompatibility)
		{
			if (HasChildNodes)
			{
				AppendInnerText(sb, isDisplayScriptingText);
			}
			else
			{
				sb.Append(GetCurrentNodeText());
			}
		}
		else if (_ownerdocument.OptionEnableBreakLineForInnerText && Name == "br")
		{
			sb.AppendLine();
		}
		else if (_nodetype == HtmlNodeType.Text)
		{
			sb.Append(((HtmlTextNode)this).Text);
		}
		else
		{
			if (_nodetype == HtmlNodeType.Comment || !HasChildNodes || (_isHideInnerText && !isDisplayScriptingText))
			{
				return;
			}
			foreach (HtmlNode childNode in ChildNodes)
			{
				childNode.InternalInnerText(sb, isDisplayScriptingText, depthLevel);
			}
		}
	}

	public virtual string GetDirectInnerText()
	{
		if (!_ownerdocument.BackwardCompatibility)
		{
			if (HasChildNodes)
			{
				StringBuilder stringBuilder = new StringBuilder();
				AppendDirectInnerText(stringBuilder);
				return stringBuilder.ToString();
			}
			return GetCurrentNodeText();
		}
		if (_nodetype == HtmlNodeType.Text)
		{
			return ((HtmlTextNode)this).Text;
		}
		if (_nodetype == HtmlNodeType.Comment)
		{
			return "";
		}
		if (!HasChildNodes)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		foreach (HtmlNode childNode in ChildNodes)
		{
			if (childNode._nodetype == HtmlNodeType.Text)
			{
				stringBuilder2.Append(((HtmlTextNode)childNode).Text);
			}
		}
		return stringBuilder2.ToString();
	}

	internal string GetCurrentNodeText()
	{
		if (_nodetype == HtmlNodeType.Text)
		{
			string text = ((HtmlTextNode)this).Text;
			if (ParentNode.Name != "pre")
			{
				text = text.Replace("\n", "").Replace("\r", "").Replace("\t", "");
			}
			return text;
		}
		return "";
	}

	internal void AppendDirectInnerText(StringBuilder sb)
	{
		if (_nodetype == HtmlNodeType.Text)
		{
			sb.Append(GetCurrentNodeText());
		}
		if (!HasChildNodes)
		{
			return;
		}
		foreach (HtmlNode childNode in ChildNodes)
		{
			sb.Append(childNode.GetCurrentNodeText());
		}
	}

	internal void AppendInnerText(StringBuilder sb, bool isShowHideInnerText)
	{
		if (_nodetype == HtmlNodeType.Text)
		{
			sb.Append(GetCurrentNodeText());
		}
		if (!HasChildNodes || (_isHideInnerText && !isShowHideInnerText))
		{
			return;
		}
		foreach (HtmlNode childNode in ChildNodes)
		{
			childNode.AppendInnerText(sb, isShowHideInnerText);
		}
	}

	internal void SetName(string value)
	{
		_name = value;
		_optimizedName = null;
	}

	public static bool CanOverlapElement(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (!ElementsFlags.TryGetValue(name, out var value))
		{
			return false;
		}
		return (value & HtmlElementFlag.CanOverlap) != 0;
	}

	public static HtmlNode CreateNode(string html)
	{
		return CreateNode(html, null);
	}

	public static HtmlNode CreateNode(string html, Action<HtmlDocument> htmlDocumentBuilder)
	{
		HtmlDocument htmlDocument = new HtmlDocument();
		htmlDocumentBuilder?.Invoke(htmlDocument);
		htmlDocument.LoadHtml(html);
		if (!htmlDocument.DocumentNode.IsSingleElementNode())
		{
			throw new Exception("Multiple node elements can't be created.");
		}
		for (HtmlNode htmlNode = htmlDocument.DocumentNode.FirstChild; htmlNode != null; htmlNode = htmlNode.NextSibling)
		{
			if (htmlNode.NodeType == HtmlNodeType.Element && htmlNode.OuterHtml != "\r\n")
			{
				return htmlNode;
			}
		}
		return htmlDocument.DocumentNode.FirstChild;
	}

	public static bool IsCDataElement(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (!ElementsFlags.TryGetValue(name, out var value))
		{
			return false;
		}
		return (value & HtmlElementFlag.CData) != 0;
	}

	public static bool IsClosedElement(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (!ElementsFlags.TryGetValue(name, out var value))
		{
			return false;
		}
		return (value & HtmlElementFlag.Closed) != 0;
	}

	public static bool IsEmptyElement(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			return true;
		}
		if ('!' == name[0])
		{
			return true;
		}
		if ('?' == name[0])
		{
			return true;
		}
		if (!ElementsFlags.TryGetValue(name, out var value))
		{
			return false;
		}
		return (value & HtmlElementFlag.Empty) != 0;
	}

	public static bool IsOverlappedClosingElement(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (text.Length < 4)
		{
			return false;
		}
		if (text[0] != '<' || text[text.Length - 1] != '>' || text[1] != '/')
		{
			return false;
		}
		return CanOverlapElement(text.Substring(2, text.Length - 3));
	}

	public IEnumerable<HtmlNode> Ancestors()
	{
		HtmlNode node = ParentNode;
		if (node != null)
		{
			yield return node;
			while (node.ParentNode != null)
			{
				yield return node.ParentNode;
				node = node.ParentNode;
			}
		}
	}

	public IEnumerable<HtmlNode> Ancestors(string name)
	{
		for (HtmlNode n = ParentNode; n != null; n = n.ParentNode)
		{
			if (n.Name == name)
			{
				yield return n;
			}
		}
	}

	public IEnumerable<HtmlNode> AncestorsAndSelf()
	{
		for (HtmlNode n = this; n != null; n = n.ParentNode)
		{
			yield return n;
		}
	}

	public IEnumerable<HtmlNode> AncestorsAndSelf(string name)
	{
		for (HtmlNode n = this; n != null; n = n.ParentNode)
		{
			if (n.Name == name)
			{
				yield return n;
			}
		}
	}

	public HtmlNode AppendChild(HtmlNode newChild)
	{
		if (newChild == null)
		{
			throw new ArgumentNullException("newChild");
		}
		ChildNodes.Append(newChild);
		_ownerdocument.SetIdForNode(newChild, newChild.GetId());
		SetChildNodesId(newChild);
		HtmlNode parentnode = _parentnode;
		HtmlDocument htmlDocument = null;
		while (parentnode != null)
		{
			if (parentnode.OwnerDocument != htmlDocument)
			{
				parentnode.OwnerDocument.SetIdForNode(newChild, newChild.GetId());
				parentnode.SetChildNodesId(newChild);
				htmlDocument = parentnode.OwnerDocument;
			}
			parentnode = parentnode._parentnode;
		}
		SetChanged();
		return newChild;
	}

	public void SetChildNodesId(HtmlNode chilNode)
	{
		foreach (HtmlNode childNode in chilNode.ChildNodes)
		{
			_ownerdocument.SetIdForNode(childNode, childNode.GetId());
			if (childNode.ChildNodes == chilNode.ChildNodes)
			{
				throw new Exception("Oops! a scenario that will cause a Stack Overflow has been found. See the following issue for an example: https://github.com/zzzprojects/html-agility-pack/issues/513");
			}
			SetChildNodesId(childNode);
		}
	}

	public void AppendChildren(HtmlNodeCollection newChildren)
	{
		if (newChildren == null)
		{
			throw new ArgumentNullException("newChildren");
		}
		foreach (HtmlNode newChild in newChildren)
		{
			AppendChild(newChild);
		}
	}

	public IEnumerable<HtmlAttribute> ChildAttributes(string name)
	{
		if (!HasAttributes)
		{
			return Enumerable.Empty<HtmlAttribute>();
		}
		return Attributes.AttributesWithName(name);
	}

	public HtmlNode Clone()
	{
		return CloneNode(deep: true);
	}

	public HtmlNode CloneNode(string newName)
	{
		return CloneNode(newName, deep: true);
	}

	public HtmlNode CloneNode(string newName, bool deep)
	{
		if (newName == null)
		{
			throw new ArgumentNullException("newName");
		}
		HtmlNode htmlNode = CloneNode(deep);
		htmlNode.SetName(newName);
		return htmlNode;
	}

	public HtmlNode CloneNode(bool deep)
	{
		HtmlNode htmlNode = _ownerdocument.CreateNode(_nodetype);
		htmlNode.SetName(OriginalName);
		switch (_nodetype)
		{
		case HtmlNodeType.Comment:
			((HtmlCommentNode)htmlNode).Comment = ((HtmlCommentNode)this).Comment;
			return htmlNode;
		case HtmlNodeType.Text:
			((HtmlTextNode)htmlNode).Text = ((HtmlTextNode)this).Text;
			return htmlNode;
		default:
			if (HasAttributes)
			{
				foreach (HtmlAttribute item in (IEnumerable<HtmlAttribute>)_attributes)
				{
					HtmlAttribute newAttribute = item.Clone();
					htmlNode.Attributes.Append(newAttribute);
				}
			}
			if (HasClosingAttributes)
			{
				htmlNode._endnode = _endnode.CloneNode(deep: false);
				foreach (HtmlAttribute item2 in (IEnumerable<HtmlAttribute>)_endnode._attributes)
				{
					HtmlAttribute newAttribute2 = item2.Clone();
					htmlNode._endnode._attributes.Append(newAttribute2);
				}
			}
			if (!deep)
			{
				return htmlNode;
			}
			if (!HasChildNodes)
			{
				return htmlNode;
			}
			{
				foreach (HtmlNode childnode in _childnodes)
				{
					HtmlNode newChild = childnode.CloneNode(deep);
					htmlNode.AppendChild(newChild);
				}
				return htmlNode;
			}
		}
	}

	public void CopyFrom(HtmlNode node)
	{
		CopyFrom(node, deep: true);
	}

	public void CopyFrom(HtmlNode node, bool deep)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		Attributes.RemoveAll();
		if (node.HasAttributes)
		{
			foreach (HtmlAttribute item in (IEnumerable<HtmlAttribute>)node.Attributes)
			{
				HtmlAttribute newAttribute = item.Clone();
				Attributes.Append(newAttribute);
			}
		}
		if (!deep)
		{
			return;
		}
		RemoveAllChildren();
		if (!node.HasChildNodes)
		{
			return;
		}
		foreach (HtmlNode childNode in node.ChildNodes)
		{
			AppendChild(childNode.CloneNode(deep: true));
		}
	}

	[Obsolete("Use Descendants() instead, the results of this function will change in a future version")]
	public IEnumerable<HtmlNode> DescendantNodes(int level = 0)
	{
		if (level > HtmlDocument.MaxDepthLevel)
		{
			throw new ArgumentException("The document is too complex to parse");
		}
		foreach (HtmlNode node in ChildNodes)
		{
			yield return node;
			foreach (HtmlNode item in node.DescendantNodes(level + 1))
			{
				yield return item;
			}
		}
	}

	[Obsolete("Use DescendantsAndSelf() instead, the results of this function will change in a future version")]
	public IEnumerable<HtmlNode> DescendantNodesAndSelf()
	{
		return DescendantsAndSelf();
	}

	public IEnumerable<HtmlNode> Descendants()
	{
		return Descendants(0);
	}

	public IEnumerable<HtmlNode> Descendants(int level)
	{
		if (level > HtmlDocument.MaxDepthLevel)
		{
			throw new ArgumentException("The document is too complex to parse");
		}
		foreach (HtmlNode node in ChildNodes)
		{
			yield return node;
			foreach (HtmlNode item in node.Descendants(level + 1))
			{
				yield return item;
			}
		}
	}

	public IEnumerable<HtmlNode> Descendants(string name)
	{
		foreach (HtmlNode item in Descendants())
		{
			if (string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))
			{
				yield return item;
			}
		}
	}

	public IEnumerable<HtmlNode> DescendantsAndSelf()
	{
		yield return this;
		foreach (HtmlNode item in Descendants())
		{
			if (item != null)
			{
				yield return item;
			}
		}
	}

	public IEnumerable<HtmlNode> DescendantsAndSelf(string name)
	{
		yield return this;
		foreach (HtmlNode item in Descendants())
		{
			if (item.Name == name)
			{
				yield return item;
			}
		}
	}

	public HtmlNode Element(string name)
	{
		foreach (HtmlNode childNode in ChildNodes)
		{
			if (childNode.Name == name)
			{
				return childNode;
			}
		}
		return null;
	}

	public IEnumerable<HtmlNode> Elements(string name)
	{
		foreach (HtmlNode childNode in ChildNodes)
		{
			if (childNode.Name == name)
			{
				yield return childNode;
			}
		}
	}

	public HtmlAttribute GetDataAttribute(string key)
	{
		return Attributes.Hashitems.SingleOrDefault((KeyValuePair<string, HtmlAttribute> x) => x.Key.Equals("data-" + key, StringComparison.OrdinalIgnoreCase)).Value;
	}

	public IEnumerable<HtmlAttribute> GetDataAttributes()
	{
		return (from x in Attributes.Hashitems
			where x.Key.StartsWith("data-", StringComparison.OrdinalIgnoreCase)
			select x.Value).ToList();
	}

	public IEnumerable<HtmlAttribute> GetAttributes()
	{
		return Attributes.items;
	}

	public IEnumerable<HtmlAttribute> GetAttributes(params string[] attributeNames)
	{
		List<HtmlAttribute> list = new List<HtmlAttribute>();
		foreach (string name in attributeNames)
		{
			list.Add(Attributes[name]);
		}
		return list;
	}

	public string GetAttributeValue(string name, string def)
	{
		return GetAttributeValue(name, def, null);
	}

	public int GetAttributeValue(string name, int def)
	{
		return GetAttributeValue(name, def, int.TryParse);
	}

	public bool GetAttributeValue(string name, bool def)
	{
		return GetAttributeValue(name, def, bool.TryParse);
	}

	public T GetAttributeValue<T>(string name, T def)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (!HasAttributes)
		{
			return def;
		}
		HtmlAttribute htmlAttribute = Attributes[name];
		if (htmlAttribute == null)
		{
			return def;
		}
		try
		{
			return (T)htmlAttribute.Value.To(typeof(T));
		}
		catch
		{
			return def;
		}
	}

	public T GetAttributeValue<T>(string name, T def, AttributeValueParser<T> parser)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (!HasAttributes)
		{
			return def;
		}
		HtmlAttribute htmlAttribute = Attributes[name];
		if (htmlAttribute == null || htmlAttribute.Value == null)
		{
			return def;
		}
		string value = htmlAttribute.Value;
		if (value is T)
		{
			return (T)(object)((value is T) ? value : null);
		}
		if (parser != null && parser(htmlAttribute.Value, out var result))
		{
			return result;
		}
		return def;
	}

	public HtmlNode InsertAfter(HtmlNode newChild, HtmlNode refChild)
	{
		if (newChild == null)
		{
			throw new ArgumentNullException("newChild");
		}
		if (refChild == null)
		{
			return PrependChild(newChild);
		}
		if (newChild == refChild)
		{
			return newChild;
		}
		int num = -1;
		if (_childnodes != null)
		{
			num = _childnodes[refChild];
		}
		if (num == -1)
		{
			throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
		}
		if (_childnodes != null)
		{
			_childnodes.Insert(num + 1, newChild);
		}
		_ownerdocument.SetIdForNode(newChild, newChild.GetId());
		SetChildNodesId(newChild);
		SetChanged();
		return newChild;
	}

	public HtmlNode InsertBefore(HtmlNode newChild, HtmlNode refChild)
	{
		if (newChild == null)
		{
			throw new ArgumentNullException("newChild");
		}
		if (refChild == null)
		{
			return AppendChild(newChild);
		}
		if (newChild == refChild)
		{
			return newChild;
		}
		int num = -1;
		if (_childnodes != null)
		{
			num = _childnodes[refChild];
		}
		if (num == -1)
		{
			throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
		}
		if (_childnodes != null)
		{
			_childnodes.Insert(num, newChild);
		}
		_ownerdocument.SetIdForNode(newChild, newChild.GetId());
		SetChildNodesId(newChild);
		SetChanged();
		return newChild;
	}

	public HtmlNode PrependChild(HtmlNode newChild)
	{
		if (newChild == null)
		{
			throw new ArgumentNullException("newChild");
		}
		ChildNodes.Prepend(newChild);
		_ownerdocument.SetIdForNode(newChild, newChild.GetId());
		SetChildNodesId(newChild);
		SetChanged();
		return newChild;
	}

	public void PrependChildren(HtmlNodeCollection newChildren)
	{
		if (newChildren == null)
		{
			throw new ArgumentNullException("newChildren");
		}
		for (int num = newChildren.Count - 1; num >= 0; num--)
		{
			PrependChild(newChildren[num]);
		}
	}

	public void Remove()
	{
		if (ParentNode != null)
		{
			ParentNode.ChildNodes.Remove(this);
		}
	}

	public void RemoveAll()
	{
		RemoveAllChildren();
		if (HasAttributes)
		{
			_attributes.Clear();
		}
		if (_endnode != null && _endnode != this && _endnode._attributes != null)
		{
			_endnode._attributes.Clear();
		}
		SetChanged();
	}

	public void RemoveAllChildren()
	{
		if (!HasChildNodes)
		{
			return;
		}
		if (_ownerdocument.OptionUseIdAttribute)
		{
			foreach (HtmlNode childnode in _childnodes)
			{
				_ownerdocument.SetIdForNode(null, childnode.GetId());
				RemoveAllIDforNode(childnode);
			}
		}
		_childnodes.Clear();
		SetChanged();
	}

	public void RemoveAllIDforNode(HtmlNode node)
	{
		foreach (HtmlNode childNode in node.ChildNodes)
		{
			_ownerdocument.SetIdForNode(null, childNode.GetId());
			RemoveAllIDforNode(childNode);
		}
	}

	public void MoveChild(HtmlNode child)
	{
		if (child == null)
		{
			throw new ArgumentNullException("Oops! the 'child' parameter cannot be null.");
		}
		HtmlNode parentNode = child.ParentNode;
		AppendChild(child);
		parentNode?.RemoveChild(child);
	}

	public void MoveChildren(HtmlNodeCollection children)
	{
		if (children == null)
		{
			throw new ArgumentNullException("Oops! the 'children' parameter cannot be null.");
		}
		HtmlNode parentNode = children.ParentNode;
		AppendChildren(children);
		parentNode?.RemoveChildren(children);
	}

	public void RemoveChildren(HtmlNodeCollection oldChildren)
	{
		if (oldChildren == null)
		{
			throw new ArgumentNullException("Oops! the 'oldChildren' parameter cannot be null.");
		}
		foreach (HtmlNode item in oldChildren.ToList())
		{
			RemoveChild(item);
		}
	}

	public HtmlNode RemoveChild(HtmlNode oldChild)
	{
		if (oldChild == null)
		{
			throw new ArgumentNullException("oldChild");
		}
		int num = -1;
		if (_childnodes != null)
		{
			num = _childnodes[oldChild];
		}
		if (num == -1)
		{
			throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
		}
		if (_childnodes != null)
		{
			_childnodes.Remove(num);
		}
		_ownerdocument.SetIdForNode(null, oldChild.GetId());
		RemoveAllIDforNode(oldChild);
		SetChanged();
		return oldChild;
	}

	public HtmlNode RemoveChild(HtmlNode oldChild, bool keepGrandChildren)
	{
		if (oldChild == null)
		{
			throw new ArgumentNullException("oldChild");
		}
		if ((oldChild._childnodes != null) & keepGrandChildren)
		{
			HtmlNode refChild = oldChild.PreviousSibling;
			foreach (HtmlNode childnode in oldChild._childnodes)
			{
				refChild = InsertAfter(childnode, refChild);
			}
		}
		RemoveChild(oldChild);
		SetChanged();
		return oldChild;
	}

	public HtmlNode ReplaceChild(HtmlNode newChild, HtmlNode oldChild)
	{
		if (newChild == null)
		{
			return RemoveChild(oldChild);
		}
		if (oldChild == null)
		{
			return AppendChild(newChild);
		}
		int num = -1;
		if (_childnodes != null)
		{
			num = _childnodes[oldChild];
		}
		if (num == -1)
		{
			throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
		}
		if (_childnodes != null)
		{
			_childnodes.Replace(num, newChild);
		}
		_ownerdocument.SetIdForNode(null, oldChild.GetId());
		RemoveAllIDforNode(oldChild);
		_ownerdocument.SetIdForNode(newChild, newChild.GetId());
		SetChildNodesId(newChild);
		SetChanged();
		return newChild;
	}

	public HtmlAttribute SetAttributeValue(string name, string value)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		HtmlAttribute htmlAttribute = Attributes[name];
		if (htmlAttribute == null)
		{
			return Attributes.Append(_ownerdocument.CreateAttribute(name, value));
		}
		htmlAttribute.Value = value;
		return htmlAttribute;
	}

	public void WriteContentTo(TextWriter outText, int level = 0)
	{
		if (level > HtmlDocument.MaxDepthLevel)
		{
			throw new ArgumentException("The document is too complex to parse");
		}
		if (_childnodes == null)
		{
			return;
		}
		foreach (HtmlNode childnode in _childnodes)
		{
			childnode.WriteTo(outText, level + 1);
		}
	}

	public string WriteContentTo()
	{
		StringWriter stringWriter = new StringWriter();
		WriteContentTo(stringWriter);
		stringWriter.Flush();
		return stringWriter.ToString();
	}

	public virtual void WriteTo(TextWriter outText, int level = 0)
	{
		switch (_nodetype)
		{
		case HtmlNodeType.Comment:
		{
			string text2 = ((HtmlCommentNode)this).Comment;
			if (_ownerdocument.OptionOutputAsXml)
			{
				HtmlCommentNode htmlCommentNode = (HtmlCommentNode)this;
				if (!_ownerdocument.BackwardCompatibility && htmlCommentNode.Comment.StartsWith("<!doctype", StringComparison.OrdinalIgnoreCase))
				{
					outText.Write(htmlCommentNode.Comment);
				}
				else if (OwnerDocument.OptionXmlForceOriginalComment)
				{
					outText.Write(htmlCommentNode.Comment);
				}
				else
				{
					outText.Write("<!--" + GetXmlComment(htmlCommentNode) + "-->");
				}
			}
			else
			{
				outText.Write(text2);
			}
			break;
		}
		case HtmlNodeType.Document:
			if (_ownerdocument.OptionOutputAsXml)
			{
				outText.Write("<?xml version=\"1.0\" encoding=\"" + _ownerdocument.GetOutEncoding().BodyName + "\"?>");
				if (_ownerdocument.DocumentNode.HasChildNodes)
				{
					int num = _ownerdocument.DocumentNode._childnodes.Count;
					if (num > 0)
					{
						if (_ownerdocument.GetXmlDeclaration() != null)
						{
							num--;
						}
						if (num > 1)
						{
							if (!_ownerdocument.BackwardCompatibility)
							{
								WriteContentTo(outText, level);
							}
							else if (_ownerdocument.OptionOutputUpperCase)
							{
								outText.Write("<SPAN>");
								WriteContentTo(outText, level);
								outText.Write("</SPAN>");
							}
							else
							{
								outText.Write("<span>");
								WriteContentTo(outText, level);
								outText.Write("</span>");
							}
							break;
						}
					}
				}
			}
			WriteContentTo(outText, level);
			break;
		case HtmlNodeType.Text:
		{
			string text2 = ((HtmlTextNode)this).Text;
			outText.Write(_ownerdocument.OptionOutputAsXml ? HtmlDocument.HtmlEncodeWithCompatibility(text2, _ownerdocument.BackwardCompatibility) : text2);
			break;
		}
		case HtmlNodeType.Element:
		{
			string text = (_ownerdocument.OptionOutputUpperCase ? Name.ToUpperInvariant() : Name);
			if (_ownerdocument.OptionOutputOriginalCase)
			{
				text = OriginalName;
			}
			if (_ownerdocument.OptionOutputAsXml)
			{
				if (text.Length <= 0 || text[0] == '?' || text.Trim().Length == 0)
				{
					break;
				}
				text = HtmlDocument.GetXmlName(text, isAttribute: false, _ownerdocument.OptionPreserveXmlNamespaces);
			}
			outText.Write("<");
			outText.Write(text);
			WriteAttributes(outText, closing: false);
			if (HasChildNodes)
			{
				outText.Write(">");
				bool flag = false;
				if (_ownerdocument.OptionOutputAsXml && IsCDataElement(Name))
				{
					flag = true;
					outText.Write("\r\n//<![CDATA[\r\n");
				}
				if (flag)
				{
					if (HasChildNodes)
					{
						ChildNodes[0].WriteTo(outText, level);
					}
					outText.Write("\r\n//]]>//\r\n");
				}
				else
				{
					WriteContentTo(outText, level);
				}
				if (_ownerdocument.OptionOutputAsXml || !_isImplicitEnd)
				{
					outText.Write("</");
					outText.Write(text);
					if (!_ownerdocument.OptionOutputAsXml)
					{
						WriteAttributes(outText, closing: true);
					}
					outText.Write(">");
				}
			}
			else if (IsEmptyElement(Name))
			{
				if (_ownerdocument.OptionWriteEmptyNodes || _ownerdocument.OptionOutputAsXml)
				{
					if (_ownerdocument.OptionWriteEmptyNodesWithoutSpace)
					{
						outText.Write("/>");
					}
					else
					{
						outText.Write(" />");
					}
					break;
				}
				if (Name.Length > 0 && Name[0] == '?')
				{
					outText.Write("?");
				}
				outText.Write(">");
			}
			else if (!_isImplicitEnd)
			{
				outText.Write("></");
				outText.Write(text);
				outText.Write(">");
			}
			else
			{
				outText.Write(">");
			}
			break;
		}
		}
	}

	public void WriteTo(XmlWriter writer)
	{
		switch (_nodetype)
		{
		case HtmlNodeType.Comment:
			writer.WriteComment(GetXmlComment((HtmlCommentNode)this));
			break;
		case HtmlNodeType.Document:
			writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"" + _ownerdocument.GetOutEncoding().BodyName + "\"");
			if (!HasChildNodes)
			{
				break;
			}
			{
				foreach (HtmlNode childNode in ChildNodes)
				{
					childNode.WriteTo(writer);
				}
				break;
			}
		case HtmlNodeType.Text:
		{
			string text = ((HtmlTextNode)this).Text;
			writer.WriteString(text);
			break;
		}
		case HtmlNodeType.Element:
		{
			string localName = (_ownerdocument.OptionOutputUpperCase ? Name.ToUpperInvariant() : Name);
			if (_ownerdocument.OptionOutputOriginalCase)
			{
				localName = OriginalName;
			}
			writer.WriteStartElement(localName);
			WriteAttributes(writer, this);
			if (HasChildNodes)
			{
				foreach (HtmlNode childNode2 in ChildNodes)
				{
					childNode2.WriteTo(writer);
				}
			}
			writer.WriteEndElement();
			break;
		}
		}
	}

	public string WriteTo()
	{
		using StringWriter stringWriter = new StringWriter();
		WriteTo(stringWriter);
		stringWriter.Flush();
		return stringWriter.ToString();
	}

	public void SetParent(HtmlNode parent)
	{
		if (parent == null)
		{
			return;
		}
		ParentNode = parent;
		if (OwnerDocument.OptionMaxNestedChildNodes > 0)
		{
			Depth = parent.Depth + 1;
			if (Depth > OwnerDocument.OptionMaxNestedChildNodes)
			{
				throw new Exception($"Document has more than {OwnerDocument.OptionMaxNestedChildNodes} nested tags. This is likely due to the page not closing tags properly.");
			}
		}
	}

	internal void SetChanged()
	{
		_changed = true;
		if (ParentNode != null)
		{
			ParentNode.SetChanged();
		}
	}

	private void UpdateHtml()
	{
		_innerhtml = WriteContentTo();
		_outerhtml = WriteTo();
		_changed = false;
	}

	internal static string GetXmlComment(HtmlCommentNode comment)
	{
		string comment2 = comment.Comment;
		return comment2.Substring(4, comment2.Length - 7).Replace("--", " - -");
	}

	internal static void WriteAttributes(XmlWriter writer, HtmlNode node)
	{
		if (!node.HasAttributes)
		{
			return;
		}
		foreach (HtmlAttribute value in node.Attributes.Hashitems.Values)
		{
			writer.WriteAttributeString(value.XmlName, value.Value);
		}
	}

	internal void UpdateLastNode()
	{
		HtmlNode htmlNode = null;
		if (_prevwithsamename == null || !_prevwithsamename._starttag)
		{
			if (_ownerdocument.Openednodes != null)
			{
				foreach (KeyValuePair<int, HtmlNode> openednode in _ownerdocument.Openednodes)
				{
					if ((openednode.Key < _outerstartindex || openednode.Key > _outerstartindex + _outerlength) && openednode.Value._name == _name)
					{
						if (htmlNode == null && openednode.Value._starttag)
						{
							htmlNode = openednode.Value;
						}
						else if (htmlNode != null && htmlNode.InnerStartIndex < openednode.Key && openednode.Value._starttag)
						{
							htmlNode = openednode.Value;
						}
					}
				}
			}
		}
		else
		{
			htmlNode = _prevwithsamename;
		}
		if (htmlNode != null)
		{
			_ownerdocument.Lastnodes[htmlNode.Name] = htmlNode;
		}
	}

	internal void CloseNode(HtmlNode endnode, int level = 0)
	{
		if (level > HtmlDocument.MaxDepthLevel)
		{
			throw new ArgumentException("The document is too complex to parse");
		}
		if (!_ownerdocument.OptionAutoCloseOnEnd && _childnodes != null)
		{
			foreach (HtmlNode childnode in _childnodes)
			{
				if (!childnode.Closed)
				{
					HtmlNode htmlNode = new HtmlNode(NodeType, _ownerdocument, -1);
					htmlNode._endnode = htmlNode;
					childnode.CloseNode(htmlNode, level + 1);
				}
			}
		}
		if (Closed)
		{
			return;
		}
		_endnode = endnode;
		if (_ownerdocument.Openednodes != null)
		{
			_ownerdocument.Openednodes.Remove(_outerstartindex);
		}
		if (Utilities.GetDictionaryValueOrDefault(_ownerdocument.Lastnodes, Name) == this)
		{
			_ownerdocument.Lastnodes.Remove(Name);
			_ownerdocument.UpdateLastParentNode();
			if (_starttag && !string.IsNullOrEmpty(Name))
			{
				UpdateLastNode();
			}
		}
		if (endnode != this)
		{
			_innerstartindex = _outerstartindex + _outerlength;
			_innerlength = endnode._outerstartindex - _innerstartindex;
			_outerlength = endnode._outerstartindex + endnode._outerlength - _outerstartindex;
		}
	}

	internal string GetId()
	{
		HtmlAttribute htmlAttribute = (HasAttributes ? Attributes["id"] : null);
		if (htmlAttribute != null)
		{
			return htmlAttribute.Value;
		}
		return string.Empty;
	}

	internal void SetId(string id)
	{
		HtmlAttribute htmlAttribute = Attributes["id"] ?? _ownerdocument.CreateAttribute("id");
		htmlAttribute.Value = id;
		_ownerdocument.SetIdForNode(this, htmlAttribute.Value);
		Attributes["id"] = htmlAttribute;
		SetChanged();
	}

	internal void WriteAttribute(TextWriter outText, HtmlAttribute att)
	{
		if (att.Value == null)
		{
			return;
		}
		AttributeValueQuote attributeValueQuote = OwnerDocument.GlobalAttributeValueQuote ?? att.QuoteType;
		switch (attributeValueQuote)
		{
		case AttributeValueQuote.Initial:
			attributeValueQuote = att.QuoteType;
			break;
		case AttributeValueQuote.InitialExceptWithoutValue:
			attributeValueQuote = ((att.QuoteType == AttributeValueQuote.WithoutValue) ? AttributeValueQuote.DoubleQuote : att.QuoteType);
			break;
		}
		bool flag = attributeValueQuote == AttributeValueQuote.WithoutValue;
		string value = attributeValueQuote switch
		{
			AttributeValueQuote.SingleQuote => "'", 
			AttributeValueQuote.DoubleQuote => "\"", 
			_ => "", 
		};
		string value2;
		if (_ownerdocument.OptionOutputAsXml)
		{
			if (attributeValueQuote != AttributeValueQuote.DoubleQuote && attributeValueQuote != AttributeValueQuote.SingleQuote)
			{
				value = ((OwnerDocument.GlobalAttributeValueQuote == AttributeValueQuote.SingleQuote) ? "'" : "\"");
			}
			value2 = (_ownerdocument.OptionOutputUpperCase ? att.XmlName.ToUpperInvariant() : att.XmlName);
			if (_ownerdocument.OptionOutputOriginalCase)
			{
				value2 = att.OriginalName;
			}
			outText.Write(" ");
			outText.Write(value2);
			outText.Write("=");
			outText.Write(value);
			outText.Write(HtmlDocument.HtmlEncodeWithCompatibility(att.XmlValue, _ownerdocument.BackwardCompatibility));
			outText.Write(value);
			return;
		}
		value2 = (_ownerdocument.OptionOutputUpperCase ? att.Name.ToUpperInvariant() : att.Name);
		if (_ownerdocument.OptionOutputOriginalCase)
		{
			value2 = att.OriginalName;
		}
		if (att.Name.Length >= 4 && att.Name[0] == '<' && att.Name[1] == '%' && att.Name[att.Name.Length - 1] == '>' && att.Name[att.Name.Length - 2] == '%')
		{
			outText.Write(" ");
			outText.Write(value2);
		}
		else if (!flag)
		{
			string value3 = attributeValueQuote switch
			{
				AttributeValueQuote.SingleQuote => att.Value.Replace("'", "&#39;"), 
				AttributeValueQuote.DoubleQuote => (!att.Value.StartsWith("@")) ? att.Value.Replace("\"", "&quot;") : att.Value, 
				_ => att.Value, 
			};
			if (_ownerdocument.OptionOutputOptimizeAttributeValues)
			{
				if (att.Value.IndexOfAny(optimizeAttributesCheckedChars) < 0)
				{
					outText.Write(" ");
					outText.Write(value2);
					outText.Write("=");
					outText.Write(att.Value);
				}
				else
				{
					outText.Write(" ");
					outText.Write(value2);
					outText.Write("=");
					outText.Write(value);
					outText.Write(value3);
					outText.Write(value);
				}
			}
			else
			{
				outText.Write(" ");
				outText.Write(value2);
				outText.Write("=");
				outText.Write(value);
				outText.Write(value3);
				outText.Write(value);
			}
		}
		else
		{
			outText.Write(" ");
			outText.Write(value2);
		}
	}

	internal void WriteAttributes(TextWriter outText, bool closing)
	{
		if (_ownerdocument.OptionOutputAsXml)
		{
			if (_attributes == null)
			{
				return;
			}
			{
				foreach (HtmlAttribute value in _attributes.Hashitems.Values)
				{
					WriteAttribute(outText, value);
				}
				return;
			}
		}
		if (!closing)
		{
			if (_attributes != null)
			{
				foreach (HtmlAttribute item in (IEnumerable<HtmlAttribute>)_attributes)
				{
					WriteAttribute(outText, item);
				}
			}
			if (!_ownerdocument.OptionAddDebuggingAttributes)
			{
				return;
			}
			WriteAttribute(outText, _ownerdocument.CreateAttribute("_closed", Closed.ToString()));
			WriteAttribute(outText, _ownerdocument.CreateAttribute("_children", ChildNodes.Count.ToString()));
			int num = 0;
			{
				foreach (HtmlNode childNode in ChildNodes)
				{
					WriteAttribute(outText, _ownerdocument.CreateAttribute("_child_" + num, childNode.Name));
					num++;
				}
				return;
			}
		}
		if (_endnode == null || _endnode._attributes == null || _endnode == this)
		{
			return;
		}
		foreach (HtmlAttribute item2 in (IEnumerable<HtmlAttribute>)_endnode._attributes)
		{
			WriteAttribute(outText, item2);
		}
		if (_ownerdocument.OptionAddDebuggingAttributes)
		{
			WriteAttribute(outText, _ownerdocument.CreateAttribute("_closed", Closed.ToString()));
			WriteAttribute(outText, _ownerdocument.CreateAttribute("_children", ChildNodes.Count.ToString()));
		}
	}

	private string GetRelativeXpath()
	{
		if (ParentNode == null)
		{
			return Name;
		}
		if (NodeType == HtmlNodeType.Document)
		{
			return string.Empty;
		}
		int num = 1;
		foreach (HtmlNode childNode in ParentNode.ChildNodes)
		{
			if (!(childNode.Name != Name))
			{
				if (childNode == this)
				{
					break;
				}
				num++;
			}
		}
		return Name + "[" + num + "]";
	}

	private bool IsSingleElementNode()
	{
		int num = 0;
		for (HtmlNode htmlNode = FirstChild; htmlNode != null; htmlNode = htmlNode.NextSibling)
		{
			if (htmlNode.NodeType == HtmlNodeType.Element && htmlNode.OuterHtml != "\r\n")
			{
				num++;
			}
		}
		return num <= 1;
	}

	public void AddClass(string name)
	{
		AddClass(name, throwError: false);
	}

	public void AddClass(string name, bool throwError)
	{
		IEnumerable<HtmlAttribute> enumerable = Attributes.AttributesWithName("class");
		bool flag = true;
		foreach (HtmlAttribute item in enumerable)
		{
			flag = false;
			if (item.Value != null && Array.IndexOf(item.Value.Split(spaceSeparator), name) != -1)
			{
				if (throwError)
				{
					throw new Exception(HtmlDocument.HtmlExceptionClassExists);
				}
			}
			else
			{
				SetAttributeValue(item.Name, item.Value + " " + name);
			}
		}
		if (flag)
		{
			HtmlAttribute newAttribute = _ownerdocument.CreateAttribute("class", name);
			Attributes.Append(newAttribute);
		}
	}

	public void RemoveClass()
	{
		RemoveClass(throwError: false);
	}

	public void RemoveClass(bool throwError)
	{
		IEnumerable<HtmlAttribute> enumerable = Attributes.AttributesWithName("class");
		if (IsEmpty(enumerable) & throwError)
		{
			throw new Exception(HtmlDocument.HtmlExceptionClassDoesNotExist);
		}
		foreach (HtmlAttribute item in enumerable)
		{
			Attributes.Remove(item);
		}
	}

	public void RemoveClass(string name)
	{
		RemoveClass(name, throwError: false);
	}

	public void RemoveClass(string name, bool throwError)
	{
		IEnumerable<HtmlAttribute> enumerable = Attributes.AttributesWithName("class");
		if (IsEmpty(enumerable) & throwError)
		{
			throw new Exception(HtmlDocument.HtmlExceptionClassDoesNotExist);
		}
		foreach (HtmlAttribute item in enumerable)
		{
			if (item.Value == null)
			{
				continue;
			}
			if (item.Value.Equals(name))
			{
				Attributes.Remove(item);
			}
			else if (item.Value != null && item.Value.Split(' ').Contains(name))
			{
				string[] array = item.Value.Split(' ');
				string text = "";
				string[] array2 = array;
				foreach (string text2 in array2)
				{
					if (!text2.Equals(name))
					{
						text = text + text2 + " ";
					}
				}
				text = text.Trim();
				SetAttributeValue(item.Name, text);
			}
			else if (throwError)
			{
				throw new Exception(HtmlDocument.HtmlExceptionClassDoesNotExist);
			}
			if (string.IsNullOrEmpty(item.Value))
			{
				Attributes.Remove(item);
			}
		}
	}

	public void ReplaceClass(string newClass, string oldClass)
	{
		ReplaceClass(newClass, oldClass, throwError: false);
	}

	public void ReplaceClass(string newClass, string oldClass, bool throwError)
	{
		if (string.IsNullOrEmpty(newClass))
		{
			RemoveClass(oldClass);
		}
		if (string.IsNullOrEmpty(oldClass))
		{
			AddClass(newClass);
		}
		IEnumerable<HtmlAttribute> enumerable = Attributes.AttributesWithName("class");
		if (IsEmpty(enumerable) & throwError)
		{
			throw new Exception(HtmlDocument.HtmlExceptionClassDoesNotExist);
		}
		foreach (HtmlAttribute item in enumerable)
		{
			if (item.Value != null)
			{
				if (item.Value.Equals(oldClass) || item.Value.Contains(oldClass))
				{
					string value = item.Value.Replace(oldClass, newClass);
					SetAttributeValue(item.Name, value);
				}
				else if (throwError)
				{
					throw new Exception(HtmlDocument.HtmlExceptionClassDoesNotExist);
				}
			}
		}
	}

	public IEnumerable<string> GetClasses()
	{
		IEnumerable<HtmlAttribute> enumerable = Attributes.AttributesWithName("class");
		foreach (HtmlAttribute item in enumerable)
		{
			string[] array = item.Value.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				yield return array2[i];
			}
		}
	}

	public bool HasClass(string className)
	{
		foreach (string @class in GetClasses())
		{
			string[] array = @class.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == className)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsEmpty(IEnumerable en)
	{
		IEnumerator enumerator = en.GetEnumerator();
		try
		{
			if (enumerator.MoveNext())
			{
				_ = enumerator.Current;
				return false;
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		return true;
	}

	public T GetEncapsulatedData<T>()
	{
		return (T)GetEncapsulatedData(typeof(T));
	}

	public T GetEncapsulatedData<T>(HtmlDocument htmlDocument)
	{
		return (T)GetEncapsulatedData(typeof(T), htmlDocument);
	}

	public object GetEncapsulatedData(Type targetType, HtmlDocument htmlDocument = null)
	{
		if (targetType == null)
		{
			throw new ArgumentNullException("Parameter targetType is null");
		}
		HtmlDocument htmlDocument2 = ((htmlDocument != null) ? htmlDocument : OwnerDocument);
		if (!targetType.IsInstantiable())
		{
			throw new MissingMethodException("Parameterless Constructor excpected for " + targetType.FullName);
		}
		object obj = Activator.CreateInstance(targetType);
		if (targetType.IsDefinedAttribute(typeof(HasXPathAttribute)))
		{
			IEnumerable<PropertyInfo> propertiesDefinedXPath = targetType.GetPropertiesDefinedXPath();
			if (propertiesDefinedXPath.CountOfIEnumerable() == 0)
			{
				throw new MissingXPathException("Type " + targetType.FullName + " defined HasXPath Attribute but it does not have any property with XPath Attribte.");
			}
			{
				foreach (PropertyInfo item in propertiesDefinedXPath)
				{
					XPathAttribute xPathAttribute = ((IList)item.GetCustomAttributes(typeof(XPathAttribute), inherit: false))[0] as XPathAttribute;
					if (!item.IsIEnumerable())
					{
						HtmlNode htmlNode = null;
						try
						{
							htmlNode = htmlDocument2.DocumentNode.SelectSingleNode(xPathAttribute.XPath);
						}
						catch (XPathException ex)
						{
							throw new XPathException(ex.Message + " That means you have a syntax error in XPath property of this Property : " + item.PropertyType.FullName + " " + item.Name);
						}
						catch (Exception inner)
						{
							throw new NodeNotFoundException("Cannot find node with giving XPath to bind to " + item.PropertyType.FullName + " " + item.Name, inner);
						}
						if (htmlNode == null)
						{
							if (!item.IsDefined(typeof(SkipNodeNotFoundAttribute), inherit: false))
							{
								throw new NodeNotFoundException("Cannot find node with giving XPath to bind to " + item.PropertyType.FullName + " " + item.Name);
							}
							continue;
						}
						if (item.PropertyType.IsDefinedAttribute(typeof(HasXPathAttribute)))
						{
							HtmlDocument htmlDocument3 = new HtmlDocument();
							htmlDocument3.LoadHtml(Tools.GetHtmlForEncapsulation(htmlNode, (!xPathAttribute.IsNodeReturnTypeExplicitlySet) ? ReturnType.InnerHtml : xPathAttribute.NodeReturnType));
							object encapsulatedData = GetEncapsulatedData(item.PropertyType, htmlDocument3);
							item.SetValue(obj, encapsulatedData, null);
							continue;
						}
						string empty = string.Empty;
						if (xPathAttribute.AttributeName == null)
						{
							empty = Tools.GetNodeValueBasedOnXPathReturnType<string>(htmlNode, xPathAttribute);
						}
						else
						{
							ThrowIfNodeReturnTypeIsExplicitlySetWhenAttributeNameIsGiven(xPathAttribute);
							empty = htmlNode.GetAttributeValue(xPathAttribute.AttributeName, null);
						}
						if (empty == null)
						{
							throw new NodeAttributeNotFoundException("Can not find " + xPathAttribute.AttributeName + " Attribute in " + htmlNode.Name + " related to " + item.PropertyType.FullName + " " + item.Name);
						}
						object value;
						try
						{
							value = Convert.ChangeType(empty, item.PropertyType);
						}
						catch (FormatException)
						{
							throw new FormatException("Can not convert Invalid string to " + item.PropertyType.FullName + " " + item.Name);
						}
						catch (Exception ex3)
						{
							throw new Exception("Unhandled Exception : " + ex3.Message);
						}
						item.SetValue(obj, value, null);
						continue;
					}
					if (!(item.GetGenericTypes() is IList<Type> { Count: not 0 } list))
					{
						throw new ArgumentException(item.Name + " should have one generic argument.");
					}
					if (list.Count > 1)
					{
						throw new ArgumentException(item.Name + " should have one generic argument.");
					}
					if (list.Count != 1)
					{
						continue;
					}
					HtmlNodeCollection htmlNodeCollection;
					try
					{
						htmlNodeCollection = htmlDocument2.DocumentNode.SelectNodes(xPathAttribute.XPath);
					}
					catch (XPathException ex4)
					{
						throw new XPathException(ex4.Message + " That means you have a syntax error in XPath property of this Property : " + item.PropertyType.FullName + " " + item.Name);
					}
					catch (Exception inner2)
					{
						throw new NodeNotFoundException("Cannot find node with giving XPath to bind to " + item.PropertyType.FullName + " " + item.Name, inner2);
					}
					if (htmlNodeCollection == null || htmlNodeCollection.Count == 0)
					{
						if (!item.IsDefined(typeof(SkipNodeNotFoundAttribute), inherit: false))
						{
							throw new NodeNotFoundException("Cannot find node with giving XPath to bind to " + item.PropertyType.FullName + " " + item.Name);
						}
						continue;
					}
					IList list2 = list[0].CreateIListOfType();
					if (list[0].IsDefinedAttribute(typeof(HasXPathAttribute)))
					{
						foreach (HtmlNode item2 in htmlNodeCollection)
						{
							HtmlDocument htmlDocument4 = new HtmlDocument();
							htmlDocument4.LoadHtml(Tools.GetHtmlForEncapsulation(item2, (!xPathAttribute.IsNodeReturnTypeExplicitlySet) ? ReturnType.InnerHtml : xPathAttribute.NodeReturnType));
							object encapsulatedData2 = GetEncapsulatedData(list[0], htmlDocument4);
							list2.Add(encapsulatedData2);
						}
					}
					else if (xPathAttribute.AttributeName == null)
					{
						try
						{
							list2 = Tools.GetNodesValuesBasedOnXPathReturnType(htmlNodeCollection, xPathAttribute, list[0]);
						}
						catch (FormatException)
						{
							throw new FormatException("Can not convert Invalid string in node collection to " + list[0].FullName + " " + item.Name);
						}
						catch (Exception ex6)
						{
							throw new Exception("Unhandled Exception : " + ex6.Message);
						}
					}
					else
					{
						ThrowIfNodeReturnTypeIsExplicitlySetWhenAttributeNameIsGiven(xPathAttribute);
						foreach (HtmlNode item3 in htmlNodeCollection)
						{
							string attributeValue = item3.GetAttributeValue(xPathAttribute.AttributeName, null);
							if (attributeValue == null)
							{
								throw new NodeAttributeNotFoundException("Can not find " + xPathAttribute.AttributeName + " Attribute in " + item3.Name + " related to " + item.PropertyType.FullName + " " + item.Name);
							}
							object value2;
							try
							{
								value2 = Convert.ChangeType(attributeValue, list[0]);
							}
							catch (FormatException)
							{
								throw new FormatException("Can not convert Invalid string to " + list[0].FullName + " " + item.Name);
							}
							catch (Exception ex8)
							{
								throw new Exception("Unhandled Exception : " + ex8.Message);
							}
							list2.Add(value2);
						}
					}
					if (list2 == null || list2.Count == 0)
					{
						throw new Exception("Cannot fill " + item.PropertyType.FullName + " " + item.Name + " because it is null.");
					}
					item.SetValue(obj, list2, null);
				}
				return obj;
			}
		}
		throw new MissingXPathException("Type T must define HasXPath attribute and include properties with XPath attribute.");
	}

	private static void ThrowIfNodeReturnTypeIsExplicitlySetWhenAttributeNameIsGiven(XPathAttribute xPathAttr)
	{
		if (xPathAttr.IsNodeReturnTypeExplicitlySet && !string.IsNullOrEmpty(xPathAttr.AttributeName))
		{
			throw new InvalidNodeReturnTypeException("Specifying a ReturnType value not allowed for XPathAttribute annotations targeting element attributes");
		}
	}

	public XPathNavigator CreateNavigator()
	{
		return new HtmlNodeNavigator(OwnerDocument, this);
	}

	public XPathNavigator CreateRootNavigator()
	{
		return new HtmlNodeNavigator(OwnerDocument, OwnerDocument.DocumentNode);
	}

	public HtmlNodeCollection SelectNodes(string xpath)
	{
		HtmlNodeCollection htmlNodeCollection = new HtmlNodeCollection(null);
		XPathNodeIterator xPathNodeIterator = new HtmlNodeNavigator(OwnerDocument, this).Select(xpath);
		while (xPathNodeIterator.MoveNext())
		{
			HtmlNodeNavigator htmlNodeNavigator = (HtmlNodeNavigator)xPathNodeIterator.Current;
			htmlNodeCollection.Add(htmlNodeNavigator.CurrentNode, setParent: false);
		}
		if (htmlNodeCollection.Count == 0 && !OwnerDocument.OptionEmptyCollection)
		{
			return null;
		}
		return htmlNodeCollection;
	}

	public HtmlNodeCollection SelectNodes(XPathExpression xpath)
	{
		HtmlNodeCollection htmlNodeCollection = new HtmlNodeCollection(null);
		XPathNodeIterator xPathNodeIterator = new HtmlNodeNavigator(OwnerDocument, this).Select(xpath);
		while (xPathNodeIterator.MoveNext())
		{
			HtmlNodeNavigator htmlNodeNavigator = (HtmlNodeNavigator)xPathNodeIterator.Current;
			htmlNodeCollection.Add(htmlNodeNavigator.CurrentNode, setParent: false);
		}
		if (htmlNodeCollection.Count == 0 && !OwnerDocument.OptionEmptyCollection)
		{
			return null;
		}
		return htmlNodeCollection;
	}

	public HtmlNode SelectSingleNode(string xpath)
	{
		if (xpath == null)
		{
			throw new ArgumentNullException("xpath");
		}
		XPathNodeIterator xPathNodeIterator = new HtmlNodeNavigator(OwnerDocument, this).Select(xpath);
		if (!xPathNodeIterator.MoveNext())
		{
			return null;
		}
		return ((HtmlNodeNavigator)xPathNodeIterator.Current).CurrentNode;
	}

	public HtmlNode SelectSingleNode(XPathExpression xpath)
	{
		if (xpath == null)
		{
			throw new ArgumentNullException("xpath");
		}
		XPathNodeIterator xPathNodeIterator = new HtmlNodeNavigator(OwnerDocument, this).Select(xpath);
		if (!xPathNodeIterator.MoveNext())
		{
			return null;
		}
		return ((HtmlNodeNavigator)xPathNodeIterator.Current).CurrentNode;
	}
}
