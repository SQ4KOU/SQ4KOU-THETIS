using System;

namespace Discord;

public interface IThreadUser : IMentionable
{
	IThreadChannel Thread { get; }

	DateTimeOffset ThreadJoinedAt { get; }

	IGuild Guild { get; }

	IGuildUser GuildUser { get; }
}
