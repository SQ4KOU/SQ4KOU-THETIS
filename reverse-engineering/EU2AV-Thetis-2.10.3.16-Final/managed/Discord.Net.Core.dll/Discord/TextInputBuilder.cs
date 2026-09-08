using System;
using System.Linq;

namespace Discord;

public class TextInputBuilder : IInteractableComponentBuilder, IMessageComponentBuilder
{
	public const int MaxPlaceholderLength = 100;

	public const int LargestMaxLength = 4000;

	private string _customId;

	private int? _maxLength;

	private int? _minLength;

	private string _placeholder;

	private string _value;

	public ComponentType Type => ComponentType.TextInput;

	public string CustomId
	{
		get
		{
			return _customId;
		}
		set
		{
			if (value != null)
			{
				Preconditions.AtLeast(value.Length, 1, "CustomId");
				Preconditions.AtMost(value.Length, 100, "CustomId");
			}
			_customId = value;
		}
	}

	public TextInputStyle Style { get; set; } = TextInputStyle.Short;

	public string Label { get; set; }

	public string Placeholder
	{
		get
		{
			return _placeholder;
		}
		set
		{
			if ((value?.Length ?? 0) > 100)
			{
				throw new ArgumentException($"Placeholder cannot have more than {100} characters. Value: \"{value}\"");
			}
			_placeholder = value;
		}
	}

	public int? MinLength
	{
		get
		{
			return _minLength;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", "MinLength must not be less than 0");
			}
			if (value > 4000)
			{
				throw new ArgumentOutOfRangeException("value", $"MinLength must not be greater than {4000}");
			}
			if (value > (MaxLength ?? 4000))
			{
				throw new ArgumentOutOfRangeException("value", "MinLength must be less than MaxLength");
			}
			_minLength = value;
		}
	}

	public int? MaxLength
	{
		get
		{
			return _maxLength;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", "MaxLength must not be less than 0");
			}
			if (value > 4000)
			{
				throw new ArgumentOutOfRangeException("value", $"MaxLength most not be greater than {4000}");
			}
			if (value < (MinLength ?? (-1)))
			{
				throw new ArgumentOutOfRangeException("value", $"MaxLength must be greater than MinLength ({MinLength})");
			}
			_maxLength = value;
		}
	}

	public bool? Required { get; set; }

	public int? Id { get; set; }

	public string Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (value?.Length > (MaxLength ?? 4000))
			{
				throw new ArgumentOutOfRangeException("value", $"Value must not be longer than {MaxLength ?? 4000}. Value: \"{value}\"");
			}
			if (value?.Length < MinLength.GetValueOrDefault())
			{
				throw new ArgumentOutOfRangeException("value", $"Value must not be shorter than {MinLength}. Value: \"{value}\"");
			}
			_value = value;
		}
	}

	public TextInputBuilder(string label, string customId, TextInputStyle style = TextInputStyle.Short, string placeholder = null, int? minLength = null, int? maxLength = null, bool? required = null, string value = null, int? id = null)
	{
		Label = label;
		Style = style;
		CustomId = customId;
		Placeholder = placeholder;
		MinLength = minLength;
		MaxLength = maxLength;
		Required = required;
		Value = value;
		Id = id;
	}

	public TextInputBuilder()
	{
	}

	public TextInputBuilder(TextInputComponent textInput)
	{
		Label = textInput.Label;
		Style = textInput.Style;
		CustomId = textInput.CustomId;
		Placeholder = textInput.Placeholder;
		MinLength = textInput.MinLength;
		MaxLength = textInput.MaxLength;
		Required = textInput.Required;
		Value = textInput.Value;
		Id = textInput.Id;
	}

	public TextInputBuilder WithLabel(string label)
	{
		Label = label;
		return this;
	}

	public TextInputBuilder WithStyle(TextInputStyle style)
	{
		Style = style;
		return this;
	}

	public TextInputBuilder WithCustomId(string customId)
	{
		CustomId = customId;
		return this;
	}

	public TextInputBuilder WithPlaceholder(string placeholder)
	{
		Placeholder = placeholder;
		return this;
	}

	public TextInputBuilder WithValue(string value)
	{
		Value = value;
		return this;
	}

	public TextInputBuilder WithMinLength(int minLength)
	{
		MinLength = minLength;
		return this;
	}

	public TextInputBuilder WithMaxLength(int maxLength)
	{
		MaxLength = maxLength;
		return this;
	}

	public TextInputBuilder WithRequired(bool required)
	{
		Required = required;
		return this;
	}

	public TextInputComponent Build()
	{
		if (string.IsNullOrEmpty(CustomId))
		{
			throw new ArgumentException("TextInputComponents must have a custom id.", "CustomId");
		}
		if (string.IsNullOrWhiteSpace(Label))
		{
			throw new ArgumentException("TextInputComponents must have a label.", "Label");
		}
		if (Style == TextInputStyle.Short && (Value?.Any((char x) => x == '\n') ?? false))
		{
			throw new ArgumentException($"Value must not contain new line characters when style is {TextInputStyle.Short}.", "Value");
		}
		return new TextInputComponent(CustomId, Label, Placeholder, MinLength, MaxLength, Style, Required, Value, Id);
	}

	IMessageComponent IMessageComponentBuilder.Build()
	{
		return Build();
	}
}
