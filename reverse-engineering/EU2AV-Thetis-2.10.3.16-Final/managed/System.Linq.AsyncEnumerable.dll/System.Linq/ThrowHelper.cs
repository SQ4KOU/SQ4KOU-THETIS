using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Linq;

internal static class ThrowHelper
{
	internal static void ThrowIfNegative(int value, [CallerArgumentExpression("value")] string paramName = null)
	{
		if (value < 0)
		{
			ThrowArgumentOutOfRangeException(paramName);
		}
	}

	internal static void ThrowIfNegativeOrZero(int value, [CallerArgumentExpression("value")] string paramName = null)
	{
		if (value <= 0)
		{
			ThrowArgumentOutOfRangeException(paramName);
		}
	}

	[DoesNotReturn]
	internal static void ThrowArgumentNullException(string paramName)
	{
		throw new ArgumentNullException(paramName);
	}

	[DoesNotReturn]
	internal static void ThrowArgumentOutOfRangeException(string paramName)
	{
		throw new ArgumentOutOfRangeException(paramName);
	}

	[DoesNotReturn]
	internal static void ThrowMoreThanOneElementException()
	{
		throw new InvalidOperationException(System.SR.MoreThanOneElement);
	}

	[DoesNotReturn]
	internal static void ThrowMoreThanOneMatchException()
	{
		throw new InvalidOperationException(System.SR.MoreThanOneMatch);
	}

	[DoesNotReturn]
	internal static void ThrowNoElementsException()
	{
		throw new InvalidOperationException(System.SR.NoElements);
	}

	[DoesNotReturn]
	internal static void ThrowNoMatchException()
	{
		throw new InvalidOperationException(System.SR.NoMatch);
	}
}
