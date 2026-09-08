using System;
using System.Threading.Tasks;

namespace Discord;

public interface IRole : ISnowflakeEntity, IEntity<ulong>, IDeletable, IMentionable, IComparable<IRole>
{
	IGuild Guild { get; }

	Color Color { get; }

	bool IsHoisted { get; }

	bool IsManaged { get; }

	bool IsMentionable { get; }

	string Name { get; }

	string Icon { get; }

	Emoji Emoji { get; }

	GuildPermissions Permissions { get; }

	int Position { get; }

	RoleTags Tags { get; }

	RoleFlags Flags { get; }

	Task ModifyAsync(Action<RoleProperties> func, RequestOptions options = null);

	string GetIconUrl();
}
