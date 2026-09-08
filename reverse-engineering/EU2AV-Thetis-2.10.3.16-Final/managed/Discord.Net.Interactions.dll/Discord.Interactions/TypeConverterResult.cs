using System;

namespace Discord.Interactions;

public struct TypeConverterResult : IResult
{
	public object Value { get; }

	public InteractionCommandError? Error { get; }

	public string ErrorReason { get; }

	public bool IsSuccess => !Error.HasValue;

	private TypeConverterResult(object value, InteractionCommandError? error, string reason)
	{
		Value = value;
		Error = error;
		ErrorReason = reason;
	}

	public static TypeConverterResult FromSuccess(object value)
	{
		return new TypeConverterResult(value, null, null);
	}

	public static TypeConverterResult FromError(Exception exception)
	{
		return new TypeConverterResult(null, InteractionCommandError.Exception, exception.Message);
	}

	public static TypeConverterResult FromError(InteractionCommandError error, string reason)
	{
		return new TypeConverterResult(null, error, reason);
	}

	public static TypeConverterResult FromError(IResult result)
	{
		return new TypeConverterResult(null, result.Error, result.ErrorReason);
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
