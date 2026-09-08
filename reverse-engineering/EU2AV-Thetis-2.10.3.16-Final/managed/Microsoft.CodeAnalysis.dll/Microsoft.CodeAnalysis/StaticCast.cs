using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis;

internal static class StaticCast<T>
{
	internal static ImmutableArray<T> From<TDerived>(ImmutableArray<TDerived> from) where TDerived : class, T
	{
		return ImmutableArray<T>.CastUp<TDerived>(from);
	}

	internal static OneOrMany<T> From<TDerived>(OneOrMany<TDerived> from) where TDerived : class, T
	{
		return OneOrMany<T>.CastUp<TDerived>(from);
	}
}
