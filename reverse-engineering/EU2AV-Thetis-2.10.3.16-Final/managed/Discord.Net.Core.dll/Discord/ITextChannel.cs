using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord;

public interface ITextChannel : IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IMentionable, INestedChannel, IGuildChannel, IDeletable, IIntegrationChannel
{
	bool IsNsfw { get; }

	string Topic { get; }

	int SlowModeInterval { get; }

	int DefaultSlowModeInterval { get; }

	ThreadArchiveDuration DefaultArchiveDuration { get; }

	Task DeleteMessagesAsync(IEnumerable<IMessage> messages, RequestOptions options = null);

	Task DeleteMessagesAsync(IEnumerable<ulong> messageIds, RequestOptions options = null);

	Task ModifyAsync(Action<TextChannelProperties> func, RequestOptions options = null);

	Task<IThreadChannel> CreateThreadAsync(string name, ThreadType type = ThreadType.PublicThread, ThreadArchiveDuration autoArchiveDuration = ThreadArchiveDuration.OneDay, IMessage message = null, bool? invitable = null, int? slowmode = null, RequestOptions options = null);

	Task<IReadOnlyCollection<IThreadChannel>> GetActiveThreadsAsync(RequestOptions options = null);
}
