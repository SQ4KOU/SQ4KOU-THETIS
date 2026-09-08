using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord;

public interface IThreadChannel : ITextChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IMentionable, INestedChannel, IGuildChannel, IDeletable, IIntegrationChannel
{
	ThreadType Type { get; }

	bool HasJoined { get; }

	bool IsArchived { get; }

	ThreadArchiveDuration AutoArchiveDuration { get; }

	DateTimeOffset ArchiveTimestamp { get; }

	bool IsLocked { get; }

	int MemberCount { get; }

	int MessageCount { get; }

	bool? IsInvitable { get; }

	IReadOnlyCollection<ulong> AppliedTags { get; }

	new DateTimeOffset CreatedAt { get; }

	ulong OwnerId { get; }

	Task JoinAsync(RequestOptions options = null);

	Task LeaveAsync(RequestOptions options = null);

	Task AddUserAsync(IGuildUser user, RequestOptions options = null);

	Task RemoveUserAsync(IGuildUser user, RequestOptions options = null);

	Task ModifyAsync(Action<ThreadChannelProperties> func, RequestOptions options = null);
}
