using System;
using System.Threading;

namespace Microsoft.CodeAnalysis;

internal static class ExceptionExtensions
{
	internal static bool IsCurrentOperationBeingCancelled(this Exception exception, CancellationToken cancellationToken)
	{
		if (exception is OperationCanceledException)
		{
			return cancellationToken.IsCancellationRequested;
		}
		return false;
	}
}
