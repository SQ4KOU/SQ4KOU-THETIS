using System;
using System.Threading.Tasks;

namespace Discord;

public interface IRestInteractionContext : IInteractionContext
{
	Func<string, Task> InteractionResponseCallback { get; }
}
