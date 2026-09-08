using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Roslyn.Utilities;

internal static class RoslynLazyInitializer
{
	public static T EnsureInitialized<T>([NotNull] ref T? target) where T : class
	{
		return LazyInitializer.EnsureInitialized(ref target);
	}

	public static T EnsureInitialized<T>([NotNull] ref T? target, Func<T> valueFactory) where T : class
	{
		return LazyInitializer.EnsureInitialized(ref target, valueFactory);
	}

	public static T EnsureInitialized<T>([NotNull] ref T? target, ref bool initialized, [NotNullIfNotNull("syncLock")] ref object? syncLock)
	{
		return LazyInitializer.EnsureInitialized(ref target, ref initialized, ref syncLock);
	}

	public static T EnsureInitialized<T>([NotNull] ref T? target, ref bool initialized, [NotNullIfNotNull("syncLock")] ref object? syncLock, Func<T> valueFactory)
	{
		return LazyInitializer.EnsureInitialized<T>(ref target, ref initialized, ref syncLock, valueFactory);
	}
}
