using System;

namespace Microsoft.CodeAnalysis.Emit;

public readonly struct RuntimeRudeEdit
{
	public string Message { get; }

	public int ErrorCode { get; }

	[Obsolete("Specify errorCode")]
	public RuntimeRudeEdit(string message)
		: this(message, 0)
	{
	}

	public RuntimeRudeEdit(string message, int errorCode)
	{
		if (errorCode < 0)
		{
			throw new ArgumentOutOfRangeException("errorCode");
		}
		Message = message;
		ErrorCode = errorCode;
	}

	internal RuntimeRudeEdit(HotReloadExceptionCode code)
	{
		Message = code.GetExceptionMessage();
		ErrorCode = code.GetExceptionCodeValue();
	}
}
