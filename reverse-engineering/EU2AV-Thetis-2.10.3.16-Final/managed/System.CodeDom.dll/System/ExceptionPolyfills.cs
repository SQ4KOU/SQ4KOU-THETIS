using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

internal static class ExceptionPolyfills
{
	extension(ArgumentNullException)
	{
		public static void ThrowIfNull([NotNull] object argument, [CallerArgumentExpression("argument")] string paramName = null)
		{
			if (argument == null)
			{
				ThrowArgumentNullException(paramName);
			}
		}
	}

	extension(ObjectDisposedException)
	{
		public static void ThrowIf([DoesNotReturnIf(true)] bool condition, object instance)
		{
			if (condition)
			{
				ThrowObjectDisposedException(instance);
			}
		}

		public static void ThrowIf([DoesNotReturnIf(true)] bool condition, Type type)
		{
			if (condition)
			{
				ThrowObjectDisposedException(type);
			}
		}
	}

	[DoesNotReturn]
	private static void ThrowArgumentNullException(string paramName)
	{
		throw new ArgumentNullException(paramName);
	}

	[DoesNotReturn]
	private static void ThrowObjectDisposedException(object instance)
	{
		throw new ObjectDisposedException(instance?.GetType().FullName);
	}

	[DoesNotReturn]
	private static void ThrowObjectDisposedException(Type type)
	{
		throw new ObjectDisposedException(type?.FullName);
	}
}
