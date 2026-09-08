using System;

namespace Discord;

[Flags]
public enum AllowedMentionTypes
{
	None = 0,
	Roles = 1,
	Users = 2,
	Everyone = 4
}
