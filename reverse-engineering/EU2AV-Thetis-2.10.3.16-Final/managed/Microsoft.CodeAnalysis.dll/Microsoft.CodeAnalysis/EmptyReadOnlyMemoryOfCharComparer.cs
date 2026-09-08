using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis;

internal sealed class EmptyReadOnlyMemoryOfCharComparer : IEqualityComparer<ReadOnlyMemory<char>>
{
	public static readonly EmptyReadOnlyMemoryOfCharComparer Instance = new EmptyReadOnlyMemoryOfCharComparer();

	private EmptyReadOnlyMemoryOfCharComparer()
	{
	}

	public bool Equals(ReadOnlyMemory<char> a, ReadOnlyMemory<char> b)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/InternalUtilities/ReadOnlyMemoryOfCharComparer.cs", 51);
	}

	public int GetHashCode(ReadOnlyMemory<char> s)
	{
		return 0;
	}
}
