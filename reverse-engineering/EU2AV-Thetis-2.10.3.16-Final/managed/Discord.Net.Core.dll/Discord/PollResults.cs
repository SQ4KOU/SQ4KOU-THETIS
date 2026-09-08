using System.Collections.Generic;

namespace Discord;

public readonly struct PollResults
{
	public readonly bool IsFinalized;

	public readonly IReadOnlyCollection<PollAnswerCounts> AnswerCounts;

	internal PollResults(bool isFinalized, IReadOnlyCollection<PollAnswerCounts> answerCounts)
	{
		IsFinalized = isFinalized;
		AnswerCounts = answerCounts;
	}
}
