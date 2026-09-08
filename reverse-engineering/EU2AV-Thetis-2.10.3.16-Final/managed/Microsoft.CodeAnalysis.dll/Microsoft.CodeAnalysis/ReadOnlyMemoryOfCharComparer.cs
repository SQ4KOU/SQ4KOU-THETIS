using System;
using System.Collections.Generic;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class ReadOnlyMemoryOfCharComparer : IEqualityComparer<ReadOnlyMemory<char>>
{
	public static readonly ReadOnlyMemoryOfCharComparer Instance = new ReadOnlyMemoryOfCharComparer();

	private ReadOnlyMemoryOfCharComparer()
	{
	}

	public static bool Equals(ReadOnlySpan<char> x, ReadOnlyMemory<char> y)
	{
		return x.SequenceEqual(y.Span);
	}

	public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y)
	{
		return x.Span.SequenceEqual(y.Span);
	}

	public int GetHashCode(ReadOnlyMemory<char> obj)
	{
		return Hash.GetFNVHashCode(obj.Span);
	}
}
