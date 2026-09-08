namespace Discord;

public struct DiscordError
{
	public string Code { get; }

	public string Message { get; }

	internal DiscordError(string code, string message)
	{
		Code = code;
		Message = message;
	}
}
