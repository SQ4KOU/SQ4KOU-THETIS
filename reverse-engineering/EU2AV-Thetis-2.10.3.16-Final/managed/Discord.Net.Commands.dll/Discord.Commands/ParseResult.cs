using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Discord.Commands;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct ParseResult : IResult
{
	public IReadOnlyList<TypeReaderResult> ArgValues { get; }

	public IReadOnlyList<TypeReaderResult> ParamValues { get; }

	public CommandError? Error { get; }

	public string ErrorReason { get; }

	public ParameterInfo ErrorParameter { get; }

	public bool IsSuccess => !Error.HasValue;

	private string DebuggerDisplay
	{
		get
		{
			if (!IsSuccess)
			{
				return $"{Error}: {ErrorReason}";
			}
			return string.Format("Success ({0}{1})", ArgValues.Count, (ParamValues.Count > 0) ? $" +{ParamValues.Count} Values" : "");
		}
	}

	private ParseResult(IReadOnlyList<TypeReaderResult> argValues, IReadOnlyList<TypeReaderResult> paramValues, CommandError? error, string errorReason, ParameterInfo errorParamInfo)
	{
		ArgValues = argValues;
		ParamValues = paramValues;
		Error = error;
		ErrorReason = errorReason;
		ErrorParameter = errorParamInfo;
	}

	public static ParseResult FromSuccess(IReadOnlyList<TypeReaderResult> argValues, IReadOnlyList<TypeReaderResult> paramValues)
	{
		for (int i = 0; i < argValues.Count; i++)
		{
			if (argValues[i].Values.Count > 1)
			{
				return new ParseResult(argValues, paramValues, CommandError.MultipleMatches, "Multiple matches found.", null);
			}
		}
		for (int j = 0; j < paramValues.Count; j++)
		{
			if (paramValues[j].Values.Count > 1)
			{
				return new ParseResult(argValues, paramValues, CommandError.MultipleMatches, "Multiple matches found.", null);
			}
		}
		return new ParseResult(argValues, paramValues, null, null, null);
	}

	public static ParseResult FromSuccess(IReadOnlyList<TypeReaderValue> argValues, IReadOnlyList<TypeReaderValue> paramValues)
	{
		TypeReaderResult[] array = new TypeReaderResult[argValues.Count];
		for (int i = 0; i < argValues.Count; i++)
		{
			array[i] = TypeReaderResult.FromSuccess(argValues[i]);
		}
		TypeReaderResult[] array2 = null;
		if (paramValues != null)
		{
			array2 = new TypeReaderResult[paramValues.Count];
			for (int j = 0; j < paramValues.Count; j++)
			{
				array2[j] = TypeReaderResult.FromSuccess(paramValues[j]);
			}
		}
		return new ParseResult(array, array2, null, null, null);
	}

	public static ParseResult FromError(CommandError error, string reason)
	{
		return new ParseResult(null, null, error, reason, null);
	}

	public static ParseResult FromError(CommandError error, string reason, ParameterInfo parameterInfo)
	{
		return new ParseResult(null, null, error, reason, parameterInfo);
	}

	public static ParseResult FromError(Exception ex)
	{
		return FromError(CommandError.Exception, ex.Message);
	}

	public static ParseResult FromError(IResult result)
	{
		return new ParseResult(null, null, result.Error, result.ErrorReason, null);
	}

	public static ParseResult FromError(IResult result, ParameterInfo parameterInfo)
	{
		return new ParseResult(null, null, result.Error, result.ErrorReason, parameterInfo);
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
