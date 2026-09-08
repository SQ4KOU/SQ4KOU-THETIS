using System;

namespace Discord;

public class AutocompleteResult
{
	private object _value;

	private string _name;

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			Preconditions.NotNull(value, "Name");
			Preconditions.AtLeast(value.Length, 1, "Name");
			Preconditions.AtMost(value.Length, 100, "Name");
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
			if (!(value is string) && !value.IsNumericType())
			{
				throw new ArgumentException(string.Format("{0} must be a numeric type or a string! Value: \"{1}\"", "value", value));
			}
			_value = value;
		}
	}

	public AutocompleteResult()
	{
	}

	public AutocompleteResult(string name, object value)
	{
		Name = name;
		Value = value;
	}
}
