using System.Collections.Generic;
using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class GuildEmote : Emote
{
	public bool IsManaged { get; }

	public bool RequireColons { get; }

	public IReadOnlyList<ulong> RoleIds { get; }

	public ulong? CreatorId { get; }

	public bool? IsAvailable { get; }

	private string DebuggerDisplay => $"{base.Name} ({base.Id})";

	internal GuildEmote(ulong id, string name, bool animated, bool isManaged, bool requireColons, IReadOnlyList<ulong> roleIds, ulong? userId, bool? isAvailable)
		: base(id, name, animated)
	{
		IsManaged = isManaged;
		RequireColons = requireColons;
		RoleIds = roleIds;
		CreatorId = userId;
		IsAvailable = isAvailable;
	}

	public override string ToString()
	{
		return string.Format("<{0}:{1}:{2}>", base.Animated ? "a" : "", base.Name, base.Id);
	}
}
