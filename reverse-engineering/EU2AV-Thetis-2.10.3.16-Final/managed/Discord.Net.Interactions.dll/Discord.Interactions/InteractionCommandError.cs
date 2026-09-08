namespace Discord.Interactions;

public enum InteractionCommandError
{
	UnknownCommand,
	ConvertFailed,
	BadArgs,
	Exception,
	Unsuccessful,
	UnmetPrecondition,
	ParseFailed
}
