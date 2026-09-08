using System.Collections.Generic;

namespace Discord;

public static class ComponentBuilderExtensions
{
	public static BuilderT WithId<BuilderT>(this BuilderT builder, int? id) where BuilderT : IMessageComponentBuilder
	{
		builder.Id = id;
		return builder;
	}

	public static ComponentBuilderV2 ToBuilder(this IEnumerable<IMessageComponent> components)
	{
		return new ComponentBuilderV2(components);
	}
}
