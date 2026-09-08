using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord;

public class ModalBuilder
{
	private string _customId;

	public string Title { get; set; }

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

	public ModalComponentBuilder Components { get; set; } = new ModalComponentBuilder();

	public ModalBuilder()
	{
	}

	public ModalBuilder(string title, string customId, ModalComponentBuilder components = null)
	{
		Title = title;
		CustomId = customId;
		Components = components ?? new ModalComponentBuilder();
	}

	public ModalBuilder WithTitle(string title)
	{
		Title = title;
		return this;
	}

	public ModalBuilder WithCustomId(string customId)
	{
		CustomId = customId;
		return this;
	}

	public ModalBuilder AddTextInput(TextInputBuilder component, int row = 0)
	{
		Components.WithTextInput(component, row);
		return this;
	}

	public ModalBuilder AddTextInput(string label, string customId, TextInputStyle style = TextInputStyle.Short, string placeholder = "", int? minLength = null, int? maxLength = null, bool? required = null, string value = null)
	{
		return AddTextInput(new TextInputBuilder(label, customId, style, placeholder, minLength, maxLength, required, value));
	}

	public ModalBuilder AddComponents(List<IMessageComponent> components, int row)
	{
		components.ForEach(delegate(IMessageComponent x)
		{
			Components.AddComponent(x, row);
		});
		return this;
	}

	public TMessageComponentBuilder GetComponent<TMessageComponentBuilder>(string customId) where TMessageComponentBuilder : class, IInteractableComponentBuilder
	{
		Preconditions.NotNull(customId, "customId");
		List<ActionRowBuilder> actionRows = Components.ActionRows;
		if (actionRows == null)
		{
			return null;
		}
		return actionRows.SelectMany((ActionRowBuilder r) => r.Components.OfType<TMessageComponentBuilder>()).FirstOrDefault((TMessageComponentBuilder c) => c.CustomId == customId);
	}

	public ModalBuilder UpdateTextInput(string customId, Action<TextInputBuilder> updateTextInput)
	{
		Preconditions.NotNull(customId, "customId");
		TextInputBuilder component = GetComponent<TextInputBuilder>(customId) ?? throw new ArgumentException("There is no component of type TextInputComponent with the specified custom ID in this modal builder.", "customId");
		ActionRowBuilder actionRowBuilder = Components.ActionRows.First((ActionRowBuilder r) => r.Components.Contains(component));
		TextInputBuilder textInputBuilder = new TextInputBuilder
		{
			Label = component.Label,
			CustomId = component.CustomId,
			Style = component.Style,
			Placeholder = component.Placeholder,
			MinLength = component.MinLength,
			MaxLength = component.MaxLength,
			Required = component.Required,
			Value = component.Value
		};
		updateTextInput(textInputBuilder);
		actionRowBuilder.Components.Remove(component);
		actionRowBuilder.AddComponent(textInputBuilder);
		return this;
	}

	public ModalBuilder UpdateTextInput(string customId, object value)
	{
		UpdateTextInput(customId, delegate(TextInputBuilder x)
		{
			x.Value = value?.ToString();
		});
		return this;
	}

	public ModalBuilder RemoveComponent(string customId)
	{
		Preconditions.NotNull(customId, "customId");
		Components.ActionRows?.ForEach(delegate(ActionRowBuilder r)
		{
			r.Components.RemoveAll((IMessageComponentBuilder c) => c is IInteractableComponentBuilder interactableComponentBuilder && interactableComponentBuilder.CustomId == customId);
		});
		return this;
	}

	public ModalBuilder RemoveComponentsOfType(ComponentType type)
	{
		Components.ActionRows?.ForEach(delegate(ActionRowBuilder r)
		{
			r.Components.RemoveAll((IMessageComponentBuilder c) => c.Type == type);
		});
		return this;
	}

	public Modal Build()
	{
		if (string.IsNullOrEmpty(CustomId))
		{
			throw new ArgumentException("Modals must have a custom ID.", "CustomId");
		}
		if (string.IsNullOrWhiteSpace(Title))
		{
			throw new ArgumentException("Modals must have a title.", "Title");
		}
		List<ActionRowBuilder> actionRows = Components.ActionRows;
		if (actionRows != null && actionRows.SelectMany((ActionRowBuilder r) => r.Components).Any((IMessageComponentBuilder c) => c.Type != ComponentType.TextInput))
		{
			throw new ArgumentException("Only components of type TextInputComponent are allowed.", "Components");
		}
		return new Modal(Title, CustomId, Components.Build());
	}
}
