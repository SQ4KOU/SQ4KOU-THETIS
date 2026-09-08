using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.CodeAnalysis.Text;

internal class SourceTextComparer : IEqualityComparer<SourceText?>
{
	public static readonly SourceTextComparer Instance = new SourceTextComparer();

	public bool Equals(SourceText? x, SourceText? y)
	{
		if (x == null)
		{
			return y == null;
		}
		if (y == null)
		{
			return false;
		}
		return x.ContentEquals(y);
	}

	public int GetHashCode(SourceText? obj)
	{
		if (obj == null)
		{
			return 0;
		}
		return MemoryMarshal.Read<int>(obj.GetContentHash().AsSpan());
	}
}
