namespace Discord;

public class ApplicationCommandPermission
{
	public ulong TargetId { get; }

	public ApplicationCommandPermissionTarget TargetType { get; }

	public bool Permission { get; }

	internal ApplicationCommandPermission()
	{
	}

	public ApplicationCommandPermission(ulong targetId, ApplicationCommandPermissionTarget targetType, bool allow)
	{
		TargetId = targetId;
		TargetType = targetType;
		Permission = allow;
	}

	public ApplicationCommandPermission(IUser target, bool allow)
	{
		TargetId = target.Id;
		Permission = allow;
		TargetType = ApplicationCommandPermissionTarget.User;
	}

	public ApplicationCommandPermission(IRole target, bool allow)
	{
		TargetId = target.Id;
		Permission = allow;
		TargetType = ApplicationCommandPermissionTarget.Role;
	}

	public ApplicationCommandPermission(IChannel channel, bool allow)
	{
		TargetId = channel.Id;
		Permission = allow;
		TargetType = ApplicationCommandPermissionTarget.Channel;
	}

	public static ApplicationCommandPermission ForEveryone(ulong guildId, bool allow)
	{
		return new ApplicationCommandPermission(guildId, ApplicationCommandPermissionTarget.User, allow);
	}

	public static ApplicationCommandPermission ForEveryone(IGuild guild, bool allow)
	{
		return ForEveryone(guild.Id, allow);
	}

	public static ApplicationCommandPermission ForAllChannels(ulong guildId, bool allow)
	{
		return new ApplicationCommandPermission(guildId - 1, ApplicationCommandPermissionTarget.Channel, allow);
	}

	public static ApplicationCommandPermission ForAllChannels(IGuild guild, bool allow)
	{
		return ForAllChannels(guild.Id, allow);
	}
}
