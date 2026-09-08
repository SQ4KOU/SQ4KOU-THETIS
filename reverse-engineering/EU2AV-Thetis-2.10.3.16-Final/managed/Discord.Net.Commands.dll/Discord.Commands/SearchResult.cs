using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Discord.Commands;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct SearchResult : IResult
{
	public string Text { get; }

	public IReadOnlyList<CommandMatch> Commands { get; }

	public CommandError? Error { get; }

	public string ErrorReason { get; }

	public bool IsSuccess => !Error.HasValue;

	private string DebuggerDisplay
	{
		get
		{
			if (!IsSuccess)
			{
				return $"{Error}: {ErrorReason}";
			}
			return $"Success ({Commands.Count} Results)";
		}
	}

	private SearchResult(string text, IReadOnlyList<CommandMatch> commands, CommandError? error, string errorReason)
	{
		Text = text;
		Commands = commands;
		Error = error;
		ErrorReason = errorReason;
	}

	public static SearchResult FromSuccess(string text, IReadOnlyList<CommandMatch> commands)
	{
		return new SearchResult(text, commands, null, null);
	}

	public static SearchResult FromError(CommandError error, string reason)
	{
		return new SearchResult(null, null, error, reason);
	}

	public static SearchResult FromError(Exception ex)
	{
		return FromError(CommandError.Exception, ex.Message);
	}

	public static SearchResult FromError(IResult result)
	{
		return new SearchResult(null, null, result.Error, result.ErrorReason);
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
