using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ContentHashComparer : IEqualityComparer<ReadOnlyMemory<byte>>
{
	public static ContentHashComparer Instance { get; } = new ContentHashComparer();

	private ContentHashComparer()
	{
	}

	public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y)
	{
		return x.Span.SequenceEqual(y.Span);
	}

	public int GetHashCode(ReadOnlyMemory<byte> obj)
	{
		return BinaryPrimitives.ReadInt32LittleEndian(obj.Span);
	}
}
