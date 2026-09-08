using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord;

public class SelectMenuComponent : IInteractableComponent, IMessageComponent
{
	public ComponentType Type { get; }

	public int? Id { get; }

	public string CustomId { get; }

	public IReadOnlyCollection<SelectMenuOption> Options { get; }

	public string Placeholder { get; }

	public int MinValues { get; }

	public int MaxValues { get; }

	public bool IsDisabled { get; }

	public IReadOnlyCollection<ChannelType> ChannelTypes { get; }

	public IReadOnlyCollection<SelectMenuDefaultValue> DefaultValues { get; }

	public SelectMenuBuilder ToBuilder()
	{
		return new SelectMenuBuilder(this);
	}

	internal SelectMenuComponent(string customId, List<SelectMenuOption> options, string placeholder, int minValues, int maxValues, bool disabled, ComponentType type, int? id, IEnumerable<ChannelType> channelTypes = null, IEnumerable<SelectMenuDefaultValue> defaultValues = null)
	{
		CustomId = customId;
		Options = options;
		Placeholder = placeholder;
		MinValues = minValues;
		MaxValues = maxValues;
		IsDisabled = disabled;
		Type = type;
		Id = id;
		ChannelTypes = channelTypes?.ToArray() ?? Array.Empty<ChannelType>();
		DefaultValues = defaultValues?.ToArray() ?? Array.Empty<SelectMenuDefaultValue>();
	}

	IMessageComponentBuilder IMessageComponent.ToBuilder()
	{
		return ToBuilder();
	}
}
