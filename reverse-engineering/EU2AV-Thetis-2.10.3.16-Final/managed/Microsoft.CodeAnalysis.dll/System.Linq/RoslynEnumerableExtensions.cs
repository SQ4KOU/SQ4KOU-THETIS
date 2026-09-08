using System.Collections.Generic;
using Roslyn.Utilities;

namespace System.Linq;

internal static class RoslynEnumerableExtensions
{
	public static bool Contains<T>(this IEnumerable<T> sequence, Func<T, bool> predicate)
	{
		return sequence.Any(predicate);
	}

	public static int Count<T, TArg>(this IEnumerable<T> source, Func<T, TArg, bool> predicate, TArg arg)
	{
		int num = 0;
		foreach (T item in source)
		{
			if (predicate(item, arg))
			{
				num++;
			}
		}
		return num;
	}

	public static T? FirstOrDefault<T, TArg>(this IEnumerable<T> source, Func<T, TArg, bool> predicate, TArg arg)
	{
		foreach (T item in source)
		{
			if (predicate(item, arg))
			{
				return item;
			}
		}
		return default(T);
	}

	public static bool Any<T, TArg>(this IEnumerable<T> source, Func<T, TArg, bool> predicate, TArg arg)
	{
		foreach (T item in source)
		{
			if (predicate(item, arg))
			{
				return true;
			}
		}
		return false;
	}

	public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, IComparer<T>? comparer)
	{
		return source.OrderBy(Functions<T>.Identity, comparer);
	}

	public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> source, IComparer<T>? comparer)
	{
		return source.OrderByDescending(Functions<T>.Identity, comparer);
	}

	public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, Comparison<T> compare)
	{
		return source.OrderBy(Comparer<T>.Create(compare));
	}

	public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> source, Comparison<T> compare)
	{
		return source.OrderByDescending(Comparer<T>.Create(compare));
	}

	public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, IComparer<T>? comparer)
	{
		return source.ThenBy(Functions<T>.Identity, comparer);
	}

	public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, Comparison<T> compare)
	{
		return source.ThenBy(Comparer<T>.Create(compare));
	}

	public static IOrderedEnumerable<T> Order<T>(this IEnumerable<T> source) where T : IComparable<T>
	{
		return source.OrderBy(Comparer<T>.Default);
	}

	public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T value)
	{
		foreach (T item in source)
		{
			yield return item;
		}
		yield return value;
	}

	public static bool SequenceEqual<T>(this IEnumerable<T>? first, IEnumerable<T>? second, Func<T, T, bool> comparer)
	{
		if (first == second)
		{
			return true;
		}
		if (first == null || second == null)
		{
			return false;
		}
		using (IEnumerator<T> enumerator = first.GetEnumerator())
		{
			using IEnumerator<T> enumerator2 = second.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (!enumerator2.MoveNext() || !comparer(enumerator.Current, enumerator2.Current))
				{
					return false;
				}
			}
			if (enumerator2.MoveNext())
			{
				return false;
			}
		}
		return true;
	}

	public static T? AggregateOrDefault<T>(this IEnumerable<T> source, Func<T, T, T> func)
	{
		using IEnumerator<T> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return default(T);
		}
		T val = enumerator.Current;
		while (enumerator.MoveNext())
		{
			val = func(val, enumerator.Current);
		}
		return val;
	}

	public static IEnumerable<T> Reverse<T>(this T[] source)
	{
		return Enumerable.Reverse(source);
	}

	public static IEnumerable<TSource[]> Chunk<TSource>(this IEnumerable<TSource> source, int size)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (size < 1)
		{
			throw new ArgumentOutOfRangeException("size");
		}
		if (source is TSource[] array)
		{
			if (array.Length == 0)
			{
				return Array.Empty<TSource[]>();
			}
			return ArrayChunkIterator(array, size);
		}
		return EnumerableChunkIterator(source, size);
	}

	private static IEnumerable<TSource[]> ArrayChunkIterator<TSource>(TSource[] source, int size)
	{
		int index = 0;
		while (index < source.Length)
		{
			TSource[] array = new ReadOnlySpan<TSource>(source, index, Math.Min(size, source.Length - index)).ToArray();
			index += array.Length;
			yield return array;
		}
	}

	private static IEnumerable<TSource[]> EnumerableChunkIterator<TSource>(IEnumerable<TSource> source, int size)
	{
		using IEnumerator<TSource> e = source.GetEnumerator();
		if (!e.MoveNext())
		{
			yield break;
		}
		int arraySize = Math.Min(size, 4);
		int i;
		do
		{
			TSource[] array = new TSource[arraySize];
			array[0] = e.Current;
			i = 1;
			if (size != array.Length)
			{
				for (; i < size; i++)
				{
					if (!e.MoveNext())
					{
						break;
					}
					if (i >= array.Length)
					{
						arraySize = (int)Math.Min((uint)size, (uint)(2 * array.Length));
						Array.Resize(ref array, arraySize);
					}
					array[i] = e.Current;
				}
			}
			else
			{
				for (TSource[] array2 = array; (uint)i < (uint)array2.Length; i++)
				{
					if (!e.MoveNext())
					{
						break;
					}
					array2[i] = e.Current;
				}
			}
			if (i != array.Length)
			{
				Array.Resize(ref array, i);
			}
			yield return array;
		}
		while (i >= size && e.MoveNext());
	}

	public static IEnumerable<(int Index, TSource Item)> Index<TSource>(this IEnumerable<TSource> source)
	{
		if (!(source is TSource[] array) || array.Length != 0)
		{
			return IndexIterator(source);
		}
		return Array.Empty<(int, TSource)>();
		static IEnumerable<(int Index, TSource Item)> IndexIterator(IEnumerable<TSource> enumerable)
		{
			int index = -1;
			foreach (TSource item in enumerable)
			{
				index = checked(index + 1);
				yield return (Index: index, Item: item);
			}
		}
	}
}
