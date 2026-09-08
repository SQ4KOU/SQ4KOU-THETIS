using System;

namespace Discord.Interactions;

public struct ExecuteResult : IResult
{
	public Exception Exception { get; }

	public InteractionCommandError? Error { get; }

	public string ErrorReason { get; }

	public bool IsSuccess => !Error.HasValue;

	private ExecuteResult(Exception exception, InteractionCommandError? commandError, string errorReason)
	{
		Exception = exception;
		Error = commandError;
		ErrorReason = errorReason;
	}

	public static ExecuteResult FromSuccess()
	{
		return new ExecuteResult(null, null, null);
	}

	public static ExecuteResult FromError(InteractionCommandError commandError, string reason)
	{
		return new ExecuteResult(null, commandError, reason);
	}

	public static ExecuteResult FromError(Exception exception)
	{
		return new ExecuteResult(exception, InteractionCommandError.Exception, exception.Message);
	}

	public static ExecuteResult FromError(IResult result)
	{
		return new ExecuteResult(null, result.Error, result.ErrorReason);
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
