using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class SectionBuilder : IMessageComponentBuilder, IStaticComponentContainer, IComponentContainer
{
	public const int MaxChildCount = 3;

	private List<IMessageComponentBuilder> _components = new List<IMessageComponentBuilder>();

	public ImmutableArray<ComponentType> SupportedComponentTypes { get; } = ImmutableCollectionsMarshal.AsImmutableArray(new ComponentType[1] { ComponentType.TextDisplay });

	public ComponentType Type => ComponentType.Section;

	public int? Id { get; set; }

	public IMessageComponentBuilder Accessory { get; set; }

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

	int IComponentContainer.MaxChildCount => 3;

	private string DebuggerDisplay => string.Format("{0}: {1} child components.", "SectionBuilder", this.ComponentCount());

	public SectionBuilder()
	{
	}

	public SectionBuilder(IMessageComponentBuilder accessory = null, params IEnumerable<IMessageComponentBuilder> components)
	{
		Accessory = accessory;
		Components = components?.ToList();
	}

	public SectionBuilder(SectionComponent section)
	{
		Components = section.Components.Select((IMessageComponent x) => x.ToBuilder()).ToList();
		Accessory = section.Accessory.ToBuilder();
		Id = section.Id;
	}

	public SectionBuilder AddComponent(IMessageComponentBuilder component)
	{
		Components.Add(component);
		return this;
	}

	public SectionBuilder AddComponents(params IMessageComponentBuilder[] components)
	{
		foreach (IMessageComponentBuilder component in components)
		{
			AddComponent(component);
		}
		return this;
	}

	public SectionBuilder WithComponents(IEnumerable<IMessageComponentBuilder> components)
	{
		Components = components.ToList();
		return this;
	}

	public SectionBuilder WithAccessory(IMessageComponentBuilder accessory)
	{
		Accessory = accessory;
		return this;
	}

	public SectionComponent Build()
	{
		int count = _components.Count;
		if ((count > 3 || count == 0) ? true : false)
		{
			throw new InvalidOperationException($"Section component can only contain {3} child components.");
		}
		if (Components.Any((IMessageComponentBuilder x) => !SupportedComponentTypes.Contains(x.Type)))
		{
			throw new InvalidOperationException("This component container only supports components of types: " + string.Join(", ", SupportedComponentTypes));
		}
		if (Accessory == null)
		{
			throw new ArgumentNullException("Accessory", "A section must have an accessory.");
		}
		IMessageComponentBuilder accessory = Accessory;
		if (!(accessory is ButtonBuilder) && !(accessory is ThumbnailBuilder))
		{
			throw new InvalidOperationException("Accessory component can only be ButtonBuilder or ThumbnailBuilder.");
		}
		return new SectionComponent(Id, Components.Select((IMessageComponentBuilder x) => x.Build()).ToImmutableArray(), Accessory?.Build());
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
		return WithComponents(components.ToList());
	}
}
