using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class ContainerBuilder : IMessageComponentBuilder, IStaticComponentContainer, IComponentContainer
{
	public const int MaxChildCount = 39;

	private List<IMessageComponentBuilder> _components = new List<IMessageComponentBuilder>();

	public ImmutableArray<ComponentType> SupportedComponentTypes { get; } = ImmutableCollectionsMarshal.AsImmutableArray(new ComponentType[8]
	{
		ComponentType.ActionRow,
		ComponentType.Section,
		ComponentType.Button,
		ComponentType.MediaGallery,
		ComponentType.Separator,
		ComponentType.File,
		ComponentType.SelectMenu,
		ComponentType.TextDisplay
	});

	public ComponentType Type => ComponentType.Container;

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

	public Color? AccentColor { get; set; }

	public bool? IsSpoiler { get; set; }

	int IComponentContainer.MaxChildCount => 39;

	private string DebuggerDisplay => string.Format("{0}: {1} child components.", "ContainerBuilder", this.ComponentCount());

	public ContainerBuilder()
	{
	}

	public ContainerBuilder(params IEnumerable<IMessageComponentBuilder> components)
	{
		Components = components?.ToList();
	}

	public ContainerBuilder(ContainerComponent container)
	{
		Components = container.Components.Select((IMessageComponent x) => x.ToBuilder()).ToList();
		AccentColor = container.AccentColor;
		IsSpoiler = container.IsSpoiler;
		Id = container.Id;
	}

	public ContainerBuilder WithAccentColor(Color? color)
	{
		AccentColor = color;
		return this;
	}

	public ContainerBuilder WithSpoiler(bool? isSpoiler)
	{
		IsSpoiler = isSpoiler;
		return this;
	}

	public ContainerBuilder AddComponent(IMessageComponentBuilder component)
	{
		Components.Add(component);
		return this;
	}

	public ContainerBuilder AddComponents(params IMessageComponentBuilder[] components)
	{
		foreach (IMessageComponentBuilder item in components)
		{
			Components.Add(item);
		}
		return this;
	}

	public ContainerBuilder WithComponents(IEnumerable<IMessageComponentBuilder> components)
	{
		Components = components.ToList();
		return this;
	}

	public ContainerComponent Build()
	{
		Preconditions.NotNull(Components, "Components");
		Preconditions.AtLeast(Components.Count, 1, "Count", "At least 1 component must be added to this container.");
		if (Components.Any((IMessageComponentBuilder x) => !SupportedComponentTypes.Contains(x.Type)))
		{
			throw new InvalidOperationException("This component container only supports components of types: " + string.Join(", ", SupportedComponentTypes));
		}
		return new ContainerComponent(Components.ConvertAll((IMessageComponentBuilder x) => x.Build()).ToImmutableArray(), AccentColor, IsSpoiler, Id);
	}

	IMessageComponent IMessageComponentBuilder.Build()
	{
		return Build();
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
