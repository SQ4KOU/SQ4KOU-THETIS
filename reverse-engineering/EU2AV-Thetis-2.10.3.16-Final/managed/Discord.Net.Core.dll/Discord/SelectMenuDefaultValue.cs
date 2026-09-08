namespace Discord;

public readonly struct SelectMenuDefaultValue
{
	public ulong Id { get; }

	public SelectDefaultValueType Type { get; }

	public SelectMenuDefaultValue(ulong id, SelectDefaultValueType type)
	{
		Id = id;
		Type = type;
	}

	public static SelectMenuDefaultValue FromChannel(IChannel channel)
	{
		return new SelectMenuDefaultValue(channel.Id, SelectDefaultValueType.Channel);
	}

	public static SelectMenuDefaultValue FromRole(IRole role)
	{
		return new SelectMenuDefaultValue(role.Id, SelectDefaultValueType.Role);
	}

	public static SelectMenuDefaultValue FromUser(IUser user)
	{
		return new SelectMenuDefaultValue(user.Id, SelectDefaultValueType.User);
	}
}
