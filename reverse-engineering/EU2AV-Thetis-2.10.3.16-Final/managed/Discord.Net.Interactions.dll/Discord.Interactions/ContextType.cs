using System;

namespace Discord.Interactions;

[Flags]
public enum ContextType
{
	Guild = 1,
	DM = 2,
	Group = 4
}
