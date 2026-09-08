using System.Runtime.CompilerServices;

namespace System.IO.Hashing;

internal sealed class XxHash64
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ulong Avalanche(ulong hash)
	{
		hash ^= hash >> 33;
		hash *= 14029467366897019727uL;
		hash ^= hash >> 29;
		hash *= 1609587929392839161L;
		hash ^= hash >> 32;
		return hash;
	}
}
