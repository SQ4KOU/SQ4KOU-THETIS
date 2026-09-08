using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Svg;

internal sealed class SvgTextReader : XmlTextReader
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

	public SvgTextReader(Stream stream, Dictionary<string, string> entities)
		: base(stream)
	{
		if (entities == null)
		{
			base.EntityHandling = EntityHandling.ExpandEntities;
		}
		_entities = entities ?? new Dictionary<string, string>();
	}

	public SvgTextReader(TextReader reader, Dictionary<string, string> entities)
		: base(reader)
	{
		if (entities == null)
		{
			base.EntityHandling = EntityHandling.ExpandEntities;
		}
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
				string text2 = text.Trim();
				int num = text2.IndexOf(QuoteChar);
				if (num > 0)
				{
					string value = text2.Substring(num + 1, text2.LastIndexOf(QuoteChar) - num - 1);
					text2 = text2.Substring(0, num).Trim();
					_entities.Add(text2, value);
				}
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
				_value = string.Empty;
			}
			_customValue = true;
		}
	}
}
