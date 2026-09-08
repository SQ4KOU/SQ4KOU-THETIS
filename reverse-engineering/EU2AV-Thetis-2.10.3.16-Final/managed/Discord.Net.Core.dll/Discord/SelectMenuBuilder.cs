using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Utils;

namespace Discord;

public class SelectMenuBuilder : IInteractableComponentBuilder, IMessageComponentBuilder
{
	public const int MaxPlaceholderLength = 100;

	public const int MaxValuesCount = 25;

	public const int MaxOptionCount = 25;

	private List<SelectMenuOptionBuilder> _options = new List<SelectMenuOptionBuilder>();

	private int _minValues = 1;

	private int _maxValues = 1;

	private string _placeholder;

	private string _customId;

	private ComponentType _type = ComponentType.SelectMenu;

	private List<SelectMenuDefaultValue> _defaultValues = new List<SelectMenuDefaultValue>();

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

	public ComponentType Type
	{
		get
		{
			return _type;
		}
		set
		{
			if (!value.IsSelectType())
			{
				throw new ArgumentException("Type must be a select menu type.", "value");
			}
			_type = value;
		}
	}

	public string Placeholder
	{
		get
		{
			return _placeholder;
		}
		set
		{
			if (value != null)
			{
				Preconditions.AtLeast(value.Length, 1, "Placeholder");
				Preconditions.AtMost(value.Length, 100, "Placeholder");
			}
			_placeholder = value;
		}
	}

	public int MinValues
	{
		get
		{
			return _minValues;
		}
		set
		{
			Preconditions.AtMost(value, 25, "MinValues");
			_minValues = value;
		}
	}

	public int MaxValues
	{
		get
		{
			return _maxValues;
		}
		set
		{
			Preconditions.AtMost(value, 25, "MaxValues");
			_maxValues = value;
		}
	}

	public List<SelectMenuOptionBuilder> Options
	{
		get
		{
			return _options;
		}
		set
		{
			if (value != null)
			{
				Preconditions.AtMost(value.Count, 25, "Options");
			}
			_options = value;
		}
	}

	public bool IsDisabled { get; set; }

	public List<ChannelType> ChannelTypes { get; set; }

	public List<SelectMenuDefaultValue> DefaultValues
	{
		get
		{
			return _defaultValues;
		}
		set
		{
			if (value != null)
			{
				Preconditions.AtMost(value.Count, 25, "DefaultValues");
			}
			_defaultValues = value;
		}
	}

	public int? Id { get; set; }

	public SelectMenuBuilder()
	{
	}

	public SelectMenuBuilder(SelectMenuComponent selectMenu)
	{
		Placeholder = selectMenu.Placeholder;
		CustomId = selectMenu.CustomId;
		MaxValues = selectMenu.MaxValues;
		MinValues = selectMenu.MinValues;
		IsDisabled = selectMenu.IsDisabled;
		Type = selectMenu.Type;
		Options = selectMenu.Options?.Select((SelectMenuOption x) => new SelectMenuOptionBuilder(x.Label, x.Value, x.Description, x.Emote, x.IsDefault)).ToList();
		DefaultValues = selectMenu.DefaultValues?.ToList();
		Id = selectMenu.Id;
	}

	public SelectMenuBuilder(string customId, List<SelectMenuOptionBuilder> options = null, string placeholder = null, int maxValues = 1, int minValues = 1, bool isDisabled = false, ComponentType type = ComponentType.SelectMenu, List<ChannelType> channelTypes = null, List<SelectMenuDefaultValue> defaultValues = null, int? id = null)
	{
		CustomId = customId;
		Options = options;
		Placeholder = placeholder;
		IsDisabled = isDisabled;
		MaxValues = maxValues;
		MinValues = minValues;
		Type = type;
		ChannelTypes = channelTypes ?? new List<ChannelType>();
		DefaultValues = defaultValues ?? new List<SelectMenuDefaultValue>();
		Id = id;
	}

	public SelectMenuBuilder WithCustomId(string customId)
	{
		CustomId = customId;
		return this;
	}

	public SelectMenuBuilder WithPlaceholder(string placeholder)
	{
		Placeholder = placeholder;
		return this;
	}

	public SelectMenuBuilder WithMinValues(int minValues)
	{
		MinValues = minValues;
		return this;
	}

	public SelectMenuBuilder WithMaxValues(int maxValues)
	{
		MaxValues = maxValues;
		return this;
	}

	public SelectMenuBuilder WithOptions(List<SelectMenuOptionBuilder> options)
	{
		Options = options;
		return this;
	}

	public SelectMenuBuilder AddOption(SelectMenuOptionBuilder option)
	{
		if (Options == null)
		{
			List<SelectMenuOptionBuilder> list = (Options = new List<SelectMenuOptionBuilder>());
		}
		if (Options.Count >= 25)
		{
			throw new InvalidOperationException($"Options count reached {25}.");
		}
		Options.Add(option);
		return this;
	}

	public SelectMenuBuilder AddOption(string label, string value, string description = null, IEmote emote = null, bool? isDefault = null)
	{
		AddOption(new SelectMenuOptionBuilder(label, value, description, emote, isDefault));
		return this;
	}

	public SelectMenuBuilder AddDefaultValue(ulong id, SelectDefaultValueType type)
	{
		return AddDefaultValue(new SelectMenuDefaultValue(id, type));
	}

	public SelectMenuBuilder AddDefaultValue(SelectMenuDefaultValue value)
	{
		if (DefaultValues.Count >= 25)
		{
			throw new InvalidOperationException($"Options count reached {25}.");
		}
		DefaultValues.Add(value);
		return this;
	}

	public SelectMenuBuilder WithDefaultValues(params SelectMenuDefaultValue[] defaultValues)
	{
		DefaultValues = defaultValues?.ToList();
		return this;
	}

	public SelectMenuBuilder WithDisabled(bool isDisabled)
	{
		IsDisabled = isDisabled;
		return this;
	}

	public SelectMenuBuilder WithType(ComponentType type)
	{
		Type = type;
		return this;
	}

	public SelectMenuBuilder WithChannelTypes(List<ChannelType> channelTypes)
	{
		ChannelTypes = channelTypes;
		return this;
	}

	public SelectMenuBuilder WithChannelTypes(params ChannelType[] channelTypes)
	{
		ChannelTypes = ((channelTypes == null) ? ChannelTypeUtils.AllChannelTypes() : channelTypes.ToList());
		return this;
	}

	public SelectMenuComponent Build()
	{
		List<SelectMenuOption> options = Options?.Select((SelectMenuOptionBuilder x) => x.Build()).ToList();
		return new SelectMenuComponent(CustomId, options, Placeholder, MinValues, MaxValues, IsDisabled, Type, Id, ChannelTypes, DefaultValues);
	}

	IMessageComponent IMessageComponentBuilder.Build()
	{
		return Build();
	}
}
