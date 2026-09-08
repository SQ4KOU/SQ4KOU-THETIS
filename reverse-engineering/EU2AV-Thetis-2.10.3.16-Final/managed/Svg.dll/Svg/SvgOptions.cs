using System;
using System.Collections;
using System.Collections.Generic;

namespace Svg;

public class SvgOptions : IDictionary<string, string>, ICollection<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>, IEnumerable, ICloneable
{
	private readonly IDictionary<string, string> _properties;

	private Dictionary<string, string> _entities;

	public Dictionary<string, string> Entities
	{
		get
		{
			return _entities;
		}
		set
		{
			_entities = value;
		}
	}

	public string Css
	{
		get
		{
			return GetValue("Css");
		}
		set
		{
			SetValue("Css", value);
		}
	}

	public string this[string key]
	{
		get
		{
			return GetValue(key, string.Empty);
		}
		set
		{
			SetValue(key, value);
		}
	}

	public ICollection<string> Keys => _properties.Keys;

	public ICollection<string> Values => _properties.Values;

	public int Count => _properties.Count;

	public bool IsReadOnly => _properties.IsReadOnly;

	public SvgOptions()
	{
		_properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
	}

	public SvgOptions(Dictionary<string, string> entities)
		: this()
	{
		_entities = entities;
	}

	public SvgOptions(Dictionary<string, string> entities, string css)
		: this()
	{
		_entities = entities;
		SetValue("Css", css);
	}

	public SvgOptions(string css)
		: this()
	{
		SetValue("Css", css);
	}

	public void Add(string key, string value)
	{
		if (key != null)
		{
			_properties.Add(key, value);
		}
	}

	public void Add(KeyValuePair<string, string> item)
	{
		Add(item.Key, item.Value);
	}

	public void Clear()
	{
		_properties.Clear();
	}

	public bool Contains(KeyValuePair<string, string> item)
	{
		return _properties.Contains(item);
	}

	public bool ContainsKey(string key)
	{
		return _properties.ContainsKey(key);
	}

	public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
	{
		_properties.CopyTo(array, arrayIndex);
	}

	public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
	{
		return _properties.GetEnumerator();
	}

	public bool Remove(string key)
	{
		return _properties.Remove(key);
	}

	public bool Remove(KeyValuePair<string, string> item)
	{
		return _properties.Remove(item);
	}

	public bool TryGetValue(string key, out string value)
	{
		return _properties.TryGetValue(key, out value);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_properties).GetEnumerator();
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	public SvgOptions Clone()
	{
		SvgOptions svgOptions = new SvgOptions();
		foreach (KeyValuePair<string, string> property in _properties)
		{
			svgOptions.Add(property);
		}
		if (_entities != null)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> entity in _entities)
			{
				dictionary.Add(entity.Key, entity.Value);
			}
			svgOptions._entities = dictionary;
		}
		return svgOptions;
	}

	protected string GetValue(string key, string defaultVal = null)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return defaultVal;
		}
		if (_properties.TryGetValue(key, out var value))
		{
			return value;
		}
		return defaultVal;
	}

	protected void SetValue(string key, string value)
	{
		if (!string.IsNullOrWhiteSpace(key))
		{
			_properties[key] = value;
		}
	}
}
