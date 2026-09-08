using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord;

public class ComponentBuilder
{
	public const int MaxCustomIdLength = 100;

	public const int MaxActionRowCount = 5;

	private List<ActionRowBuilder> _actionRows;

	public List<ActionRowBuilder> ActionRows
	{
		get
		{
			return _actionRows;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value", "ActionRows cannot be null.");
			}
			if (value.Count > 5)
			{
				throw new ArgumentOutOfRangeException("value", $"Action row count must be less than or equal to {5}.");
			}
			_actionRows = value;
		}
	}

	public static ComponentBuilder FromMessage(IMessage message)
	{
		return FromComponents(message.Components);
	}

	public static ComponentBuilder FromComponents(IReadOnlyCollection<IMessageComponent> components)
	{
		ComponentBuilder componentBuilder = new ComponentBuilder();
		for (int i = 0; i != components.Count; i++)
		{
			IMessageComponent component = components.ElementAt(i);
			componentBuilder.AddComponent(component, i);
		}
		return componentBuilder;
	}

	internal void AddComponent(IMessageComponent component, int row)
	{
		if (!(component is ButtonComponent buttonComponent))
		{
			if (component is ActionRowComponent actionRowComponent)
			{
				foreach (IMessageComponent component2 in actionRowComponent.Components)
				{
					AddComponent(component2, row);
				}
				return;
			}
			if (component is SelectMenuComponent selectMenuComponent)
			{
				WithSelectMenu(selectMenuComponent.CustomId, selectMenuComponent.Options?.Select((SelectMenuOption x) => new SelectMenuOptionBuilder(x.Label, x.Value, x.Description, x.Emote, x.IsDefault)).ToList(), selectMenuComponent.Placeholder, selectMenuComponent.MinValues, selectMenuComponent.MaxValues, selectMenuComponent.IsDisabled, row);
			}
		}
		else
		{
			WithButton(buttonComponent.Label, buttonComponent.CustomId, buttonComponent.Style, buttonComponent.Emote, buttonComponent.Url, buttonComponent.IsDisabled, row);
		}
	}

	public ComponentBuilder RemoveComponentsOfType(ComponentType t)
	{
		ActionRows.ForEach(delegate(ActionRowBuilder ar)
		{
			ar.Components.RemoveAll((IMessageComponentBuilder c) => c.Type == t);
		});
		return this;
	}

	public ComponentBuilder RemoveComponent(string customId)
	{
		ActionRows.ForEach(delegate(ActionRowBuilder ar)
		{
			ar.Components.RemoveAll((IMessageComponentBuilder c) => c is IInteractableComponent interactableComponent && interactableComponent.CustomId == customId);
		});
		return this;
	}

	public ComponentBuilder RemoveButtonByURL(string url)
	{
		ActionRows.ForEach(delegate(ActionRowBuilder ar)
		{
			ar.Components.RemoveAll((IMessageComponentBuilder c) => c is ButtonComponent buttonComponent && buttonComponent.Url == url);
		});
		return this;
	}

	public ComponentBuilder WithSelectMenu(string customId, List<SelectMenuOptionBuilder> options = null, string placeholder = null, int minValues = 1, int maxValues = 1, bool disabled = false, int row = 0, ComponentType type = ComponentType.SelectMenu, ChannelType[] channelTypes = null, SelectMenuDefaultValue[] defaultValues = null)
	{
		return WithSelectMenu(new SelectMenuBuilder().WithCustomId(customId).WithOptions(options).WithPlaceholder(placeholder)
			.WithMaxValues(maxValues)
			.WithMinValues(minValues)
			.WithDisabled(disabled)
			.WithType(type)
			.WithChannelTypes(channelTypes)
			.WithDefaultValues(defaultValues), row);
	}

	public ComponentBuilder WithSelectMenu(SelectMenuBuilder menu, int row = 0)
	{
		Preconditions.LessThan(row, 5, "row");
		if (menu.Options != null && menu.Options.Distinct().Count() != menu.Options.Count)
		{
			throw new InvalidOperationException("Please make sure that there is no duplicates values.");
		}
		if (_actionRows == null)
		{
			_actionRows = new List<ActionRowBuilder>(1) { new ActionRowBuilder().AddComponent(menu) };
		}
		else if (_actionRows.Count == row)
		{
			_actionRows.Add(new ActionRowBuilder().AddComponent(menu));
		}
		else
		{
			ActionRowBuilder actionRowBuilder;
			if (_actionRows.Count > row)
			{
				actionRowBuilder = _actionRows.ElementAt(row);
			}
			else
			{
				actionRowBuilder = new ActionRowBuilder();
				_actionRows.Add(actionRowBuilder);
			}
			if (actionRowBuilder.CanTakeComponent(menu))
			{
				actionRowBuilder.AddComponent(menu);
			}
			else
			{
				if (row >= 5)
				{
					throw new InvalidOperationException("There is no more row to add a menu");
				}
				WithSelectMenu(menu, row + 1);
			}
		}
		return this;
	}

	public ComponentBuilder WithButton(string label = null, string customId = null, ButtonStyle style = ButtonStyle.Primary, IEmote emote = null, string url = null, bool disabled = false, int row = 0, ulong? skuId = null)
	{
		ButtonBuilder button = new ButtonBuilder().WithLabel(label).WithStyle(style).WithEmote(emote)
			.WithCustomId(customId)
			.WithUrl(url)
			.WithDisabled(disabled)
			.WithSkuId(skuId);
		return WithButton(button, row);
	}

	public ComponentBuilder WithButton(ButtonBuilder button, int row = 0)
	{
		Preconditions.LessThan(row, 5, "row");
		if (_actionRows == null)
		{
			_actionRows = new List<ActionRowBuilder> { new ActionRowBuilder().AddComponent(button) };
		}
		else if (_actionRows.Count == row)
		{
			_actionRows.Add(new ActionRowBuilder().AddComponent(button));
		}
		else
		{
			ActionRowBuilder actionRowBuilder;
			if (_actionRows.Count > row)
			{
				actionRowBuilder = _actionRows.ElementAt(row);
			}
			else
			{
				actionRowBuilder = new ActionRowBuilder();
				_actionRows.Add(actionRowBuilder);
			}
			if (actionRowBuilder.CanTakeComponent(button))
			{
				actionRowBuilder.AddComponent(button);
			}
			else
			{
				if (row >= 5)
				{
					throw new InvalidOperationException("There is no more row to add a button");
				}
				WithButton(button, row + 1);
			}
		}
		return this;
	}

	public ComponentBuilder AddRow(ActionRowBuilder row)
	{
		if (_actionRows == null)
		{
			_actionRows = new List<ActionRowBuilder>();
		}
		if (_actionRows.Count >= 5)
		{
			throw new IndexOutOfRangeException("The max amount of rows has been reached");
		}
		ActionRows.Add(row);
		return this;
	}

	public ComponentBuilder WithRows(IEnumerable<ActionRowBuilder> rows)
	{
		if (rows.Count() > 5)
		{
			throw new IndexOutOfRangeException($"Cannot have more than {5} rows");
		}
		_actionRows = new List<ActionRowBuilder>(rows);
		return this;
	}

	public MessageComponent Build()
	{
		if (_actionRows?.SelectMany((ActionRowBuilder x) => x.Components)?.Any((IMessageComponentBuilder x) => x.Type == ComponentType.TextInput) == true)
		{
			throw new ArgumentException("TextInputComponents are not allowed in messages.", "ActionRows");
		}
		List<ActionRowBuilder> actionRows = _actionRows;
		if (actionRows != null && actionRows.Count > 0)
		{
			for (int num = 0; num < _actionRows?.Count; num++)
			{
				ActionRowBuilder actionRowBuilder = _actionRows[num];
				if (actionRowBuilder != null && actionRowBuilder.Components?.Count == 0)
				{
					_actionRows.RemoveAt(num);
				}
			}
		}
		if (_actionRows == null)
		{
			return MessageComponent.Empty;
		}
		return new MessageComponent(_actionRows.Select((ActionRowBuilder x) => x.Build()).OfType<IMessageComponent>().ToList());
	}
}
