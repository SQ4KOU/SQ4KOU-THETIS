using System.Collections.Generic;

namespace Discord;

public readonly struct BulkBanResult
{
	public IReadOnlyCollection<ulong> BannedUsers { get; }

	public IReadOnlyCollection<ulong> FailedUsers { get; }

	internal BulkBanResult(IReadOnlyCollection<ulong> bannedUsers, IReadOnlyCollection<ulong> failedUsers)
	{
		BannedUsers = bannedUsers;
		FailedUsers = failedUsers;
	}
}
