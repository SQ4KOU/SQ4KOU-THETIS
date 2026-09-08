using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class ComponentBuilderV2 : IStaticComponentContainer, IComponentContainer
{
	public const int MaxChildCount = 40;

	private List<IMessageComponentBuilder> _components = new List<IMessageComponentBuilder>();

	public ImmutableArray<ComponentType> SupportedComponentTypes { get; } = ImmutableCollectionsMarshal.AsImmutableArray(new ComponentType[7]
	{
		ComponentType.ActionRow,
		ComponentType.Section,
		ComponentType.MediaGallery,
		ComponentType.Separator,
		ComponentType.Container,
		ComponentType.File,
		ComponentType.TextDisplay
	});

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

	int IComponentContainer.MaxChildCount => 40;

	private string DebuggerDisplay => string.Format("{0}: {1} child components.", "ComponentBuilderV2", this.ComponentCount());

	public ComponentBuilderV2()
	{
	}

	public ComponentBuilderV2(params IEnumerable<IMessageComponentBuilder> components)
	{
		Components = components?.ToList();
	}

	public ComponentBuilderV2(IEnumerable<IMessageComponent> components)
	{
		Components = components?.Select((IMessageComponent x) => x.ToBuilder()).ToList();
	}

	public ComponentBuilderV2 AddComponent(IMessageComponentBuilder component)
	{
		Components.Add(component);
		return this;
	}

	public ComponentBuilderV2 AddComponents(params IMessageComponentBuilder[] components)
	{
		foreach (IMessageComponentBuilder item in components)
		{
			Components.Add(item);
		}
		return this;
	}

	public ComponentBuilderV2 WithComponents(IEnumerable<IMessageComponentBuilder> components)
	{
		Components = components.ToList();
		return this;
	}

	public MessageComponent Build()
	{
		Preconditions.NotNull(Components, "Components");
		Preconditions.AtLeast(Components.Count, 1, "Count", "At least 1 component must be added to this container.");
		Preconditions.AtMost(this.ComponentCount(), 40, "Count", $"A message must contain {40} components or less.");
		List<int> list = this.GetComponentIds().ToList();
		if (list.Count != list.Distinct().Count())
		{
			throw new InvalidOperationException("Components must have unique ids.");
		}
		if (Components.Any((IMessageComponentBuilder x) => !SupportedComponentTypes.Contains(x.Type)))
		{
			throw new InvalidOperationException("This component container only supports components of types: " + string.Join(", ", SupportedComponentTypes));
		}
		return new MessageComponent(Components.Select((IMessageComponentBuilder x) => x.Build()).ToList());
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
