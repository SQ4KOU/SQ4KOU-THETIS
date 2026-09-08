using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class DefaultMemberPermissionsAttribute : Attribute
{
	public GuildPermission Permissions { get; }

	public DefaultMemberPermissionsAttribute(GuildPermission permissions)
	{
		Permissions = permissions;
	}
}
