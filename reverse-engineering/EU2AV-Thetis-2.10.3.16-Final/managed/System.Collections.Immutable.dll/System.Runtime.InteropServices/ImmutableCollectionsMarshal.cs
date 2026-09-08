using System.Collections.Immutable;

namespace System.Runtime.InteropServices;

public static class ImmutableCollectionsMarshal
{
	public static ImmutableArray<T> AsImmutableArray<T>(T[]? array)
	{
		return new ImmutableArray<T>(array);
	}

	public static T[]? AsArray<T>(ImmutableArray<T> array)
	{
		return array.array;
	}

	public static Memory<T> AsMemory<T>(ImmutableArray<T>.Builder? builder)
	{
		return builder?.AsMemory() ?? default(Memory<T>);
	}
}
