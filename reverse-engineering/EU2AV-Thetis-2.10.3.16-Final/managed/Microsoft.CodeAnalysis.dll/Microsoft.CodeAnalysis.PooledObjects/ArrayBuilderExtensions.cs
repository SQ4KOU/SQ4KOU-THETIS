using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.PooledObjects;

internal static class ArrayBuilderExtensions
{
	public static ImmutableArray<T> ToImmutableOrEmptyAndFree<T>(this ArrayBuilder<T>? builder)
	{
		return builder?.ToImmutableAndFree() ?? ImmutableArray<T>.Empty;
	}

	public static void AddIfNotNull<T>(this ArrayBuilder<T> builder, T? value) where T : struct
	{
		if (value.HasValue)
		{
			builder.Add(value.Value);
		}
	}

	public static void AddIfNotNull<T>(this ArrayBuilder<T> builder, T? value) where T : class
	{
		if (value != null)
		{
			builder.Add(value);
		}
	}
}
