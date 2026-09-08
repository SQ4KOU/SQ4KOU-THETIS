namespace Discord.Rest;

public struct MemberRoleEditInfo
{
	public string Name { get; }

	public ulong RoleId { get; }

	public bool Added { get; }

	public bool Removed { get; }

	internal MemberRoleEditInfo(string name, ulong roleId, bool added, bool removed)
	{
		Name = name;
		RoleId = roleId;
		Added = added;
		Removed = removed;
	}
}
