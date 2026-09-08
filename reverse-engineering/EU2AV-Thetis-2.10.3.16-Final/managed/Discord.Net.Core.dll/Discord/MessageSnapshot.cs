namespace Discord;

public readonly struct MessageSnapshot
{
	public readonly IMessage Message;

	internal MessageSnapshot(IMessage message)
	{
		Message = message;
	}
}
