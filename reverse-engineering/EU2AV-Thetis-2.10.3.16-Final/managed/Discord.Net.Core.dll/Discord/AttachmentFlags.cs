using System;

namespace Discord;

[Flags]
public enum AttachmentFlags
{
	None = 0,
	IsClip = 1,
	IsThumbnail = 2,
	IsRemix = 4
}
