using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Discord;

public class ApplicationCommandOptionChoiceProperties
{
	private string _name;

	private object _value;

	private IDictionary<string, string> _nameLocalizations = new Dictionary<string, string>();

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (value != null)
			{
				Preconditions.AtLeast(value.Length, 1, "Name");
				Preconditions.AtMost(value.Length, 100, "Name");
			}
			_name = value;
		}
	}

	public object Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (value != null && !(value is string) && !value.IsNumericType())
			{
				throw new ArgumentException($"The value of a choice must be a string or a numeric type! Value: \"{value}\"");
			}
			_value = value;
		}
	}

	public IDictionary<string, string> NameLocalizations
	{
		get
		{
			return _nameLocalizations;
		}
		set
		{
			if (value != null)
			{
				foreach (var (text3, text4) in value)
				{
					if (!Regex.IsMatch(text3, "^\\w{2}(?:-\\w{2})?$"))
					{
						throw new ArgumentException("Key values of the dictionary must be valid language codes. Locale: \"" + text3 + "\"");
					}
					Preconditions.AtLeast(text4.Length, 1, "name", "Name value of locale " + text3 + " cannot be empty.");
					Preconditions.AtMost(text4.Length, 100, "name", "Name value of locale " + text3 + " have to contains 100 chars at most.");
				}
			}
			_nameLocalizations = value;
		}
	}
}
