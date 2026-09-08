using System;
using System.Collections.Generic;
using System.Xml;

namespace Svg;

internal sealed class SvgNodeReader : XmlNodeReader
{
	private readonly Dictionary<string, string> _entities;

	private string _value;

	private bool _customValue;

	public override string Value
	{
		get
		{
			if (!_customValue)
			{
				return base.Value;
			}
			return _value;
		}
	}

	public SvgNodeReader(XmlNode node, Dictionary<string, string> entities)
		: base(node)
	{
		_entities = entities ?? new Dictionary<string, string>();
	}

	public override bool ReadAttributeValue()
	{
		_customValue = false;
		bool num = base.ReadAttributeValue();
		if (num && NodeType == XmlNodeType.EntityReference)
		{
			ResolveEntity();
		}
		return num;
	}

	public override bool Read()
	{
		_customValue = false;
		bool result = base.Read();
		if (NodeType == XmlNodeType.DocumentType)
		{
			ParseEntities();
		}
		return result;
	}

	private void ParseEntities()
	{
		string[] array = Value.Split(new string[1] { "<!ENTITY" }, StringSplitOptions.None);
		foreach (string text in array)
		{
			if (!string.IsNullOrEmpty(text.Trim()))
			{
				string[] array2 = text.Trim().Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				string key = array2[0];
				string value = array2[1].Split(new char[1] { QuoteChar }, StringSplitOptions.RemoveEmptyEntries)[0];
				_entities.Add(key, value);
			}
		}
	}

	public override void ResolveEntity()
	{
		if (NodeType == XmlNodeType.EntityReference)
		{
			if (_entities.ContainsKey(Name))
			{
				_value = _entities[Name];
			}
			else
			{
				base.ResolveEntity();
				_value = (ReadAttributeValue() ? base.Value : string.Empty);
			}
			_customValue = true;
		}
	}
}
