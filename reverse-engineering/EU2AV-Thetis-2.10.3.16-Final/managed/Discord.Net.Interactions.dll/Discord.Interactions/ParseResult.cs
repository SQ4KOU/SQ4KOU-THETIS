using System;

namespace Discord.Interactions;

public struct ParseResult : IResult
{
	public object[] Args { get; }

	public InteractionCommandError? Error { get; }

	public string ErrorReason { get; }

	public bool IsSuccess => !Error.HasValue;

	private ParseResult(object[] args, InteractionCommandError? error, string reason)
	{
		Args = args;
		Error = error;
		ErrorReason = reason;
	}

	public static ParseResult FromSuccess(object[] args)
	{
		return new ParseResult(args, null, null);
	}

	public static ParseResult FromError(Exception exception)
	{
		return new ParseResult(null, InteractionCommandError.Exception, exception.Message);
	}

	public static ParseResult FromError(InteractionCommandError error, string reason)
	{
		return new ParseResult(null, error, reason);
	}

	public static ParseResult FromError(IResult result)
	{
		return new ParseResult(null, result.Error, result.ErrorReason);
	}

	public override string ToString()
	{
		if (!IsSuccess)
		{
			return $"{Error}: {ErrorReason}";
		}
		return "Success";
	}
}
