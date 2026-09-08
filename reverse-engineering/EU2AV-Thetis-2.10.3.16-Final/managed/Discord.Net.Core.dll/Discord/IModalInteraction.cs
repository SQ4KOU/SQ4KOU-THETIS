using System;
using System.Threading.Tasks;

namespace Discord;

public interface IModalInteraction : IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	new IModalInteractionData Data { get; }

	IUserMessage Message { get; }

	Task UpdateAsync(Action<MessageProperties> func, RequestOptions options = null);

	Task DeferLoadingAsync(bool ephemeral = false, RequestOptions options = null);
}
