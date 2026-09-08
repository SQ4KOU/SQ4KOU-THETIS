using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Discord.Interactions;

public struct AutocompletionResult : IResult
{
	public InteractionCommandError? Error { get; }

	public string ErrorReason { get; }

	public bool IsSuccess => !Error.HasValue;

	public IReadOnlyCollection<AutocompleteResult> Suggestions { get; }

	private AutocompletionResult(IEnumerable<AutocompleteResult> suggestions, InteractionCommandError? error, string reason)
	{
		Suggestions = suggestions?.ToImmutableArray();
		Error = error;
		ErrorReason = reason;
	}

	public static AutocompletionResult FromSuccess()
	{
		return new AutocompletionResult(null, null, null);
	}

	public static AutocompletionResult FromSuccess(IEnumerable<AutocompleteResult> suggestions)
	{
		return new AutocompletionResult(suggestions, null, null);
	}

	public static AutocompletionResult FromError(IResult result)
	{
		return new AutocompletionResult(null, result.Error, result.ErrorReason);
	}

	public static AutocompletionResult FromError(Exception exception)
	{
		return new AutocompletionResult(null, InteractionCommandError.Exception, exception.Message);
	}

	public static AutocompletionResult FromError(InteractionCommandError error, string reason)
	{
		return new AutocompletionResult(null, error, reason);
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
