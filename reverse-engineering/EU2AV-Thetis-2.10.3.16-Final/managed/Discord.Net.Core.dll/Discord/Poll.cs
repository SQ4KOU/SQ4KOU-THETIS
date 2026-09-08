using System;
using System.Collections.Generic;

namespace Discord;

public readonly struct Poll
{
	public readonly PollMedia Question;

	public readonly IReadOnlyCollection<PollAnswer> Answers;

	public readonly DateTimeOffset ExpiresAt;

	public readonly bool AllowMultiselect;

	public readonly PollLayout LayoutType;

	public readonly PollResults? Results;

	internal Poll(PollMedia question, IReadOnlyCollection<PollAnswer> answers, DateTimeOffset expiresAt, bool allowMultiselect, PollLayout layoutType, PollResults? results)
	{
		Question = question;
		Answers = answers;
		ExpiresAt = expiresAt;
		AllowMultiselect = allowMultiselect;
		LayoutType = layoutType;
		Results = results;
	}
}
