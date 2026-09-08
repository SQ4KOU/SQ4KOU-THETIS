using System;

namespace Discord;

public readonly struct MessageCallData
{
	public readonly ulong[] Participants;

	public readonly DateTimeOffset? EndedTimestamp;

	internal MessageCallData(ulong[] participants, DateTimeOffset? endedTimestamp)
	{
		Participants = participants;
		EndedTimestamp = endedTimestamp;
	}
}
