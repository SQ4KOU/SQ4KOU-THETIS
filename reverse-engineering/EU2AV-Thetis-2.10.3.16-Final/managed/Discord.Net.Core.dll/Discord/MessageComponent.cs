using System.Collections.Generic;

namespace Discord;

public class MessageComponent : INestedComponent
{
	public IReadOnlyCollection<IMessageComponent> Components { get; }

	public static MessageComponent Empty => new MessageComponent(new List<IMessageComponent>());

	internal MessageComponent(List<IMessageComponent> components)
	{
		Components = components;
	}
}
