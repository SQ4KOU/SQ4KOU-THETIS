using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Collections;

internal static class ThrowHelper
{
	[DoesNotReturn]
	public static void ThrowIfDestinationTooSmall()
	{
		throw new ArgumentException(System.SR.CapacityMustBeGreaterThanOrEqualToCount, "destination");
	}

	[DoesNotReturn]
	public static void ThrowArgumentNullException(string paramName)
	{
		throw new ArgumentNullException(paramName);
	}

	[DoesNotReturn]
	public static void ThrowKeyNotFoundException()
	{
		throw new KeyNotFoundException();
	}

	[DoesNotReturn]
	public static void ThrowKeyNotFoundException<TKey>(TKey key)
	{
		throw new KeyNotFoundException(System.SR.Format(System.SR.Arg_KeyNotFoundWithKey, key));
	}

	[DoesNotReturn]
	public static void ThrowInvalidOperationException()
	{
		throw new InvalidOperationException();
	}

	[DoesNotReturn]
	internal static void ThrowIncompatibleComparer()
	{
		throw new InvalidOperationException(System.SR.InvalidOperation_IncompatibleComparer);
	}
}
