using System.Collections.Generic;
using System.Collections.Immutable;

namespace Discord;

public interface IComponentContainer
{
	ImmutableArray<ComponentType> SupportedComponentTypes { get; }

	int MaxChildCount { get; }

	List<IMessageComponentBuilder> Components { get; }

	IComponentContainer AddComponent(IMessageComponentBuilder component);

	IComponentContainer AddComponents(params IMessageComponentBuilder[] components);

	IComponentContainer WithComponents(IEnumerable<IMessageComponentBuilder> components);
}
