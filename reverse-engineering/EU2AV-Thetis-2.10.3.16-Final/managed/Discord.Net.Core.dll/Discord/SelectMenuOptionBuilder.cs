using System;

namespace Discord;

public class SelectMenuOptionBuilder
{
	public const int MaxSelectLabelLength = 100;

	public const int MaxDescriptionLength = 100;

	public const int MaxSelectValueLength = 100;

	private string _label;

	private string _value;

	private string _description;

	public string Label
	{
		get
		{
			return _label;
		}
		set
		{
			if (value != null)
			{
				Preconditions.AtLeast(value.Length, 1, "Label");
				Preconditions.AtMost(value.Length, 100, "Label");
			}
			_label = value;
		}
	}

	public string Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (value != null)
			{
				Preconditions.AtLeast(value.Length, 1, "Value");
				Preconditions.AtMost(value.Length, 100, "Value");
			}
			_value = value;
		}
	}

	public string Description
	{
		get
		{
			return _description;
		}
		set
		{
			if (value != null)
			{
				Preconditions.AtLeast(value.Length, 1, "Description");
				Preconditions.AtMost(value.Length, 100, "Description");
			}
			_description = value;
		}
	}

	public IEmote Emote { get; set; }

	public bool? IsDefault { get; set; }

	public SelectMenuOptionBuilder()
	{
	}

	public SelectMenuOptionBuilder(string label, string value, string description = null, IEmote emote = null, bool? isDefault = null)
	{
		Label = label;
		Value = value;
		Description = description;
		Emote = emote;
		IsDefault = isDefault;
	}

	public SelectMenuOptionBuilder(SelectMenuOption option)
	{
		Label = option.Label;
		Value = option.Value;
		Description = option.Description;
		Emote = option.Emote;
		IsDefault = option.IsDefault;
	}

	public SelectMenuOptionBuilder WithLabel(string label)
	{
		Label = label;
		return this;
	}

	public SelectMenuOptionBuilder WithValue(string value)
	{
		Value = value;
		return this;
	}

	public SelectMenuOptionBuilder WithDescription(string description)
	{
		Description = description;
		return this;
	}

	public SelectMenuOptionBuilder WithEmote(IEmote emote)
	{
		Emote = emote;
		return this;
	}

	public SelectMenuOptionBuilder WithDefault(bool isDefault)
	{
		IsDefault = isDefault;
		return this;
	}

	public SelectMenuOption Build()
	{
		if (string.IsNullOrWhiteSpace(Label))
		{
			throw new ArgumentNullException("Label", "Option must have a label.");
		}
		Preconditions.AtMost(Label.Length, 100, "Label", $"Label length must be less or equal to {100}.");
		if (string.IsNullOrWhiteSpace(Value))
		{
			throw new ArgumentNullException("Value", "Option must have a value.");
		}
		Preconditions.AtMost(Value.Length, 100, "Value", $"Value length must be less or equal to {100}.");
		return new SelectMenuOption(Label, Value, Description, Emote, IsDefault);
	}
}
