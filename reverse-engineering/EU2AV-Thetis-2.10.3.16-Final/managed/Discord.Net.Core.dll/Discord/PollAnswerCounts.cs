namespace Discord;

public readonly struct PollAnswerCounts
{
	public readonly ulong AnswerId;

	public readonly uint Count;

	public readonly bool MeVoted;

	internal PollAnswerCounts(ulong answerId, uint count, bool meVoted)
	{
		AnswerId = answerId;
		Count = count;
		MeVoted = meVoted;
	}
}
