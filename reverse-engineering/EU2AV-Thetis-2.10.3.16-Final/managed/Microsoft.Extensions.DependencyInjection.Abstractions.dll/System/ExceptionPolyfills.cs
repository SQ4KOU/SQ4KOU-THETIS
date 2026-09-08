using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

internal static class ExceptionPolyfills
{
	extension(ArgumentNullException)
	{
		public static void ThrowIfNull([System.Diagnostics.CodeAnalysis.NotNull] object? argument, [System.Runtime.CompilerServices.CallerArgumentExpression("argument")] string? paramName = null)
		{
			if (argument == null)
			{
				ThrowArgumentNullException(paramName);
			}
		}
	}

	extension(ObjectDisposedException)
	{
		public static void ThrowIf([System.Diagnostics.CodeAnalysis.DoesNotReturnIf(true)] bool condition, object instance)
		{
			if (condition)
			{
				ThrowObjectDisposedException(instance);
			}
		}

		public static void ThrowIf([System.Diagnostics.CodeAnalysis.DoesNotReturnIf(true)] bool condition, Type type)
		{
			if (condition)
			{
				ThrowObjectDisposedException(type);
			}
		}
	}

	[System.Diagnostics.CodeAnalysis.DoesNotReturn]
	private static void ThrowArgumentNullException(string paramName)
	{
		throw new ArgumentNullException(paramName);
	}

	[System.Diagnostics.CodeAnalysis.DoesNotReturn]
	private static void ThrowObjectDisposedException(object instance)
	{
		throw new ObjectDisposedException(instance?.GetType().FullName);
	}

	[System.Diagnostics.CodeAnalysis.DoesNotReturn]
	private static void ThrowObjectDisposedException(Type type)
	{
		throw new ObjectDisposedException(type?.FullName);
	}
}
