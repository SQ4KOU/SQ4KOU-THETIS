namespace Discord;

public readonly struct PollAnswer
{
	public readonly uint AnswerId;

	public readonly PollMedia PollMedia;

	internal PollAnswer(uint id, PollMedia media)
	{
		AnswerId = id;
		PollMedia = media;
	}
}
