using System;

namespace Microsoft.CodeAnalysis.ErrorReporting;

internal sealed class OperationCanceledIgnoringCallerTokenException : OperationCanceledException
{
	public OperationCanceledIgnoringCallerTokenException(Exception innerException)
		: base(innerException.Message, innerException)
	{
	}
}
