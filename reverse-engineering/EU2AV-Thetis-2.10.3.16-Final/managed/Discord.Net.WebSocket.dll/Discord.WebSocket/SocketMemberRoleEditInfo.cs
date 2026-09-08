namespace Discord.WebSocket;

public struct SocketMemberRoleEditInfo
{
	public string Name { get; }

	public ulong RoleId { get; }

	public bool Added { get; }

	public bool Removed { get; }

	internal SocketMemberRoleEditInfo(string name, ulong roleId, bool added, bool removed)
	{
		Name = name;
		RoleId = roleId;
		Added = added;
		Removed = removed;
	}
}
