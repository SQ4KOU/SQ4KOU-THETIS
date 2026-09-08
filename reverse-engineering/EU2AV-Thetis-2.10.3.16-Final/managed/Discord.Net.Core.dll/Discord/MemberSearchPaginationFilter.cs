using System;

namespace Discord;

public struct MemberSearchPaginationFilter
{
	public ulong UserId { get; set; }

	public long GuildJoinedAt { get; set; }

	public MemberSearchPaginationFilter(ulong userId, long guildJoinedAt)
	{
		UserId = userId;
		GuildJoinedAt = guildJoinedAt;
	}

	public MemberSearchPaginationFilter(ulong userId, DateTimeOffset guildJoinedAt)
	{
		UserId = userId;
		GuildJoinedAt = guildJoinedAt.ToUnixTimeMilliseconds();
	}

	public MemberSearchPaginationFilter()
	{
		UserId = 0uL;
		GuildJoinedAt = 0L;
	}
}
