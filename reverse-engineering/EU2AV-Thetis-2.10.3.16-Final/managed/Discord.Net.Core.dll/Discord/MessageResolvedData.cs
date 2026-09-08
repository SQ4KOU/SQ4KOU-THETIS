using System.Collections.Generic;

namespace Discord;

public class MessageResolvedData
{
	public IReadOnlyCollection<IUser> Users { get; }

	public IReadOnlyCollection<IGuildUser> Members { get; }

	public IReadOnlyCollection<IRole> Roles { get; }

	public IReadOnlyCollection<IChannel> Channels { get; }

	internal MessageResolvedData(IReadOnlyCollection<IUser> users, IReadOnlyCollection<IGuildUser> members, IReadOnlyCollection<IRole> roles, IReadOnlyCollection<IChannel> channels)
	{
		Users = users;
		Members = members;
		Roles = roles;
		Channels = channels;
	}
}
