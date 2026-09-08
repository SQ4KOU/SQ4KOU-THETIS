using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.CodeAnalysis;

internal static class ExceptionUtilities
{
	internal static Exception UnexpectedValue(object? o)
	{
		return new InvalidOperationException(string.Format("Unexpected value '{0}' of type '{1}'", o, (o != null) ? o.GetType().FullName : "<unknown>"));
	}

	internal static Exception Unreachable([CallerFilePath] string? path = null, [CallerLineNumber] int line = 0)
	{
		return new InvalidOperationException($"This program location is thought to be unreachable. File='{path}' Line={line}");
	}

	internal static bool IsCurrentOperationBeingCancelled(Exception exception, CancellationToken cancellationToken)
	{
		if (exception is OperationCanceledException)
		{
			return cancellationToken.IsCancellationRequested;
		}
		return false;
	}
}
