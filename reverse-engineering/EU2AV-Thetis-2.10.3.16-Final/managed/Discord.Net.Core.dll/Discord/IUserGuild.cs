namespace Discord;

public interface IUserGuild : IDeletable, ISnowflakeEntity, IEntity<ulong>
{
	string Name { get; }

	string IconUrl { get; }

	bool IsOwner { get; }

	GuildPermissions Permissions { get; }

	GuildFeatures Features { get; }

	int? ApproximateMemberCount { get; }

	int? ApproximatePresenceCount { get; }
}
