using System;

namespace Discord;

public class GuildIncidentsData
{
	public DateTimeOffset? InvitesDisabledUntil { get; set; }

	public DateTimeOffset? DmsDisabledUntil { get; set; }
}
