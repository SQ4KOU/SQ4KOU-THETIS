using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Discord.Commands;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct TypeReaderResult : IResult
{
	public IReadOnlyCollection<TypeReaderValue> Values { get; }

	public CommandError? Error { get; }

	public string ErrorReason { get; }

	public bool IsSuccess => !Error.HasValue;

	public object BestMatch
	{
		get
		{
			if (!IsSuccess)
			{
				throw new InvalidOperationException("TypeReaderResult was not successful.");
			}
			if (Values.Count != 1)
			{
				return Values.OrderByDescending((TypeReaderValue v) => v.Score).First().Value;
			}
			return Values.Single().Value;
		}
	}

	private string DebuggerDisplay
	{
		get
		{
			if (!IsSuccess)
			{
				return $"{Error}: {ErrorReason}";
			}
			return "Success (" + string.Join(", ", Values) + ")";
		}
	}

	private TypeReaderResult(IReadOnlyCollection<TypeReaderValue> values, CommandError? error, string errorReason)
	{
		Values = values;
		Error = error;
		ErrorReason = errorReason;
	}

	public static TypeReaderResult FromSuccess(object value)
	{
		return new TypeReaderResult(ImmutableArray.Create(new TypeReaderValue(value, 1f)), null, null);
	}

	public static TypeReaderResult FromSuccess(TypeReaderValue value)
	{
		return new TypeReaderResult(ImmutableArray.Create(value), null, null);
	}

	public static TypeReaderResult FromSuccess(IReadOnlyCollection<TypeReaderValue> values)
	{
		return new TypeReaderResult(values, null, null);
	}

	public static TypeReaderResult FromError(CommandError error, string reason)
	{
		return new TypeReaderResult(null, error, reason);
	}

	public static TypeReaderResult FromError(Exception ex)
	{
		return FromError(CommandError.Exception, ex.Message);
	}

	public static TypeReaderResult FromError(IResult result)
	{
		return new TypeReaderResult(null, result.Error, result.ErrorReason);
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
