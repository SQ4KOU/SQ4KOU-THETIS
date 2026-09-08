namespace Discord;

public struct Overwrite
{
	public ulong TargetId { get; }

	public PermissionTarget TargetType { get; }

	public OverwritePermissions Permissions { get; }

	public Overwrite(ulong targetId, PermissionTarget targetType, OverwritePermissions permissions)
	{
		TargetId = targetId;
		TargetType = targetType;
		Permissions = permissions;
	}
}
