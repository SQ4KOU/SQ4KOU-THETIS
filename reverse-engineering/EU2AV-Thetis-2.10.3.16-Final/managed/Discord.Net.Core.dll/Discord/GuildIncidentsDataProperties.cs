using System;

namespace Discord;

public class GuildIncidentsDataProperties
{
	public Optional<DateTimeOffset?> InvitesDisabledUntil { get; set; }

	public Optional<DateTimeOffset?> DmsDisabledUntil { get; set; }
}
