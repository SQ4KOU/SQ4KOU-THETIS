using System.Collections.Generic;

namespace Discord;

public struct MemberSearchResult
{
	public ulong GuildId { get; }

	public IReadOnlyCollection<MemberSearchData> Members { get; }

	public int PageResultCount { get; }

	public int TotalResultCount { get; }

	public MemberSearchResult(ulong guildId, IReadOnlyCollection<MemberSearchData> members, int pageResultCount, int totalResultCount)
	{
		GuildId = guildId;
		Members = members;
		PageResultCount = pageResultCount;
		TotalResultCount = totalResultCount;
	}
}
