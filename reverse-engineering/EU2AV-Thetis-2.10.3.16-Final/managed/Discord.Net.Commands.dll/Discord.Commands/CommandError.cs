namespace Discord.Commands;

public enum CommandError
{
	UnknownCommand = 1,
	ParseFailed,
	BadArgCount,
	ObjectNotFound,
	MultipleMatches,
	UnmetPrecondition,
	Exception,
	Unsuccessful
}
