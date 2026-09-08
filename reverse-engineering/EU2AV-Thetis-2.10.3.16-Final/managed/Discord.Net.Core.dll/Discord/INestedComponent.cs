using System.Collections.Generic;

namespace Discord;

public interface INestedComponent
{
	IReadOnlyCollection<IMessageComponent> Components { get; }
}
