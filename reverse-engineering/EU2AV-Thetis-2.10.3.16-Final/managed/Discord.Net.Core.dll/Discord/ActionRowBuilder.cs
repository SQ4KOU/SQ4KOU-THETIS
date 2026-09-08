using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Discord.Utils;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class ActionRowBuilder : IMessageComponentBuilder, IInteractableComponentContainer, IComponentContainer
{
	public const int MaxChildCount = 5;

	private List<IMessageComponentBuilder> _components = new List<IMessageComponentBuilder>();

	public ImmutableArray<ComponentType> SupportedComponentTypes { get; } = ImmutableCollectionsMarshal.AsImmutableArray(new ComponentType[7]
	{
		ComponentType.Button,
		ComponentType.SelectMenu,
		ComponentType.UserSelect,
		ComponentType.RoleSelect,
		ComponentType.ChannelSelect,
		ComponentType.MentionableSelect,
		ComponentType.TextInput
	});

	public ComponentType Type => ComponentType.ActionRow;

	public int? Id { get; set; }

	public List<IMessageComponentBuilder> Components
	{
		get
		{
			return _components;
		}
		set
		{
			_components = value ?? throw new ArgumentNullException("value", "Components cannot be null.");
		}
	}

	int IComponentContainer.MaxChildCount => 5;

	private string DebuggerDisplay => string.Format("{0}: {1} child components.", "ActionRowBuilder", this.ComponentCount());

	public ActionRowBuilder()
	{
	}

	public ActionRowBuilder(params IMessageComponentBuilder[] components)
	{
		Components = components?.ToList() ?? new List<IMessageComponentBuilder>();
	}

	public ActionRowBuilder(ActionRowComponent actionRow)
	{
		Components = actionRow.Components.Select((IMessageComponent x) => x.ToBuilder()).ToList();
		Id = actionRow.Id;
	}

	public ActionRowBuilder AddComponents(params IMessageComponentBuilder[] components)
	{
		foreach (IMessageComponentBuilder component in components)
		{
			AddComponent(component);
		}
		return this;
	}

	public ActionRowBuilder WithComponents(IEnumerable<IMessageComponentBuilder> components)
	{
		Components = components.ToList();
		return this;
	}

	public ActionRowBuilder WithComponents(List<IMessageComponentBuilder> components)
	{
		Components = components;
		return this;
	}

	public ActionRowBuilder AddComponent(IMessageComponentBuilder component)
	{
		if (Components.Count >= 5)
		{
			throw new InvalidOperationException($"Components count reached {5}");
		}
		Components.Add(component);
		return this;
	}

	public ActionRowBuilder WithSelectMenu(string customId, List<SelectMenuOptionBuilder> options = null, string placeholder = null, int minValues = 1, int maxValues = 1, bool disabled = false, ComponentType type = ComponentType.SelectMenu, ChannelType[] channelTypes = null)
	{
		return WithSelectMenu(new SelectMenuBuilder().WithCustomId(customId).WithOptions(options).WithPlaceholder(placeholder)
			.WithMaxValues(maxValues)
			.WithMinValues(minValues)
			.WithDisabled(disabled)
			.WithType(type)
			.WithChannelTypes(channelTypes));
	}

	public ActionRowBuilder WithSelectMenu(SelectMenuBuilder menu)
	{
		if (menu.Options != null && menu.Options.Distinct().Count() != menu.Options.Count)
		{
			throw new InvalidOperationException("Please make sure that there is no duplicates values.");
		}
		if (Components.Count != 0)
		{
			throw new InvalidOperationException("A Select Menu cannot exist in a pre-occupied ActionRow.");
		}
		AddComponent(menu);
		return this;
	}

	public ActionRowBuilder WithButton(string label = null, string customId = null, ButtonStyle style = ButtonStyle.Primary, IEmote emote = null, string url = null, bool disabled = false)
	{
		ButtonBuilder button = new ButtonBuilder().WithLabel(label).WithStyle(style).WithEmote(emote)
			.WithCustomId(customId)
			.WithUrl(url)
			.WithDisabled(disabled);
		return WithButton(button);
	}

	public ActionRowBuilder WithButton(ButtonBuilder button)
	{
		if (Components.Count >= 5)
		{
			throw new InvalidOperationException($"Components count reached {5}");
		}
		if (Components.Any((IMessageComponentBuilder x) => x.Type.IsSelectType()))
		{
			throw new InvalidOperationException("A button cannot be added to a row with a SelectMenu");
		}
		AddComponent(button);
		return this;
	}

	public ActionRowComponent Build()
	{
		Preconditions.AtLeast(Components.Count, 1, "Components", "There must be at least 1 component in a row.");
		Preconditions.AtMost(Components.Count, 5, "Components", $"Action row can only contain {5} child components!");
		if (Components.Any((IMessageComponentBuilder x) => !SupportedComponentTypes.Contains(x.Type)))
		{
			throw new InvalidOperationException("This component container only supports components of types: " + string.Join(", ", SupportedComponentTypes));
		}
		return new ActionRowComponent(_components.Select((IMessageComponentBuilder x) => x.Build()).ToList(), Id);
	}

	IMessageComponent IMessageComponentBuilder.Build()
	{
		return Build();
	}

	internal bool CanTakeComponent(IMessageComponentBuilder component)
	{
		switch (component.Type)
		{
		case ComponentType.ActionRow:
			return false;
		case ComponentType.Button:
			if (Components.Any((IMessageComponentBuilder x) => x.Type.IsSelectType()))
			{
				return false;
			}
			return Components.Count < 5;
		case ComponentType.SelectMenu:
		case ComponentType.UserSelect:
		case ComponentType.RoleSelect:
		case ComponentType.MentionableSelect:
		case ComponentType.ChannelSelect:
			return Components.Count == 0;
		default:
			return false;
		}
	}

	IComponentContainer IComponentContainer.AddComponent(IMessageComponentBuilder component)
	{
		return AddComponent(component);
	}

	IComponentContainer IComponentContainer.AddComponents(params IMessageComponentBuilder[] components)
	{
		return AddComponents(components);
	}

	IComponentContainer IComponentContainer.WithComponents(IEnumerable<IMessageComponentBuilder> components)
	{
		return WithComponents(components);
	}
}
