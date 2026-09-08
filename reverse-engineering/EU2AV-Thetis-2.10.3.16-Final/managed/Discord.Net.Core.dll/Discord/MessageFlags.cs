using System;

namespace Discord;

[Flags]
public enum MessageFlags
{
	None = 0,
	Crossposted = 1,
	IsCrosspost = 2,
	SuppressEmbeds = 4,
	SourceMessageDeleted = 8,
	Urgent = 0x10,
	HasThread = 0x20,
	Ephemeral = 0x40,
	Loading = 0x80,
	FailedToMentionRolesInThread = 0x100,
	SuppressNotification = 0x1000,
	VoiceMessage = 0x2000,
	ComponentsV2 = 0x8000
}
