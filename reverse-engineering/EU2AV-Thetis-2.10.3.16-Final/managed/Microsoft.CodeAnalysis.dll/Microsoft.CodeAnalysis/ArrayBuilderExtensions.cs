using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal static class ArrayBuilderExtensions
{
	public static OneOrMany<T> ToOneOrManyAndFree<T>(this ArrayBuilder<T> builder)
	{
		if (builder.Count == 1)
		{
			OneOrMany<T> result = OneOrMany.Create(builder[0]);
			builder.Free();
			return result;
		}
		return OneOrMany.Create(builder.ToImmutableAndFree());
	}

	public static void AddRange<T>(this ArrayBuilder<T> builder, OneOrMany<T> items)
	{
		items.AddRangeTo(builder);
	}
}
