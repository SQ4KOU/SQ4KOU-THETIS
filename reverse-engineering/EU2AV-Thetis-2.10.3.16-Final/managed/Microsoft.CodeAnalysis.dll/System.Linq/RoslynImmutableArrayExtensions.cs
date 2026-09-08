using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;

namespace System.Linq;

internal static class RoslynImmutableArrayExtensions
{
	public static TValue? FirstOrDefault<TValue, TArg>(this ImmutableArray<TValue> array, Func<TValue, TArg, bool> predicate, TArg arg)
	{
		foreach (TValue item in array)
		{
			if (predicate(item, arg))
			{
				return item;
			}
		}
		return default(TValue);
	}

	public static TValue Single<TValue, TArg>(this ImmutableArray<TValue> array, Func<TValue, TArg, bool> predicate, TArg arg)
	{
		bool flag = false;
		TValue result = default(TValue);
		foreach (TValue item in array)
		{
			if (predicate(item, arg))
			{
				if (flag)
				{
					throw ExceptionUtilities.Unreachable("/_/src/Dependencies/Collections/Extensions/ImmutableArrayExtensions.cs", 1016);
				}
				result = item;
				flag = true;
			}
		}
		if (!flag)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Dependencies/Collections/Extensions/ImmutableArrayExtensions.cs", 1026);
		}
		return result;
	}

	public static bool SequenceEqual<TElement, TArg>(this ImmutableArray<TElement> array1, ImmutableArray<TElement> array2, TArg arg, Func<TElement, TElement, TArg, bool> predicate)
	{
		if (array1.IsDefault)
		{
			throw new NullReferenceException();
		}
		if (array2.IsDefault)
		{
			throw new NullReferenceException();
		}
		if (array1.Length != array2.Length)
		{
			return false;
		}
		for (int i = 0; i < array1.Length; i++)
		{
			if (!predicate(array1[i], array2[i], arg))
			{
				return false;
			}
		}
		return true;
	}

	public static int Count<T>(this ImmutableArray<T> items, Func<T, bool> predicate)
	{
		if (items.IsEmpty)
		{
			return 0;
		}
		int num = 0;
		foreach (T item in items)
		{
			if (predicate(item))
			{
				num++;
			}
		}
		return num;
	}

	public static int Sum<T>(this ImmutableArray<T> items, Func<T, int> selector)
	{
		int num = 0;
		foreach (T item in items)
		{
			num += selector(item);
		}
		return num;
	}

	public static int Sum<T>(this ImmutableArray<T> items, Func<T, int, int> selector)
	{
		int num = 0;
		for (int i = 0; i < items.Length; i++)
		{
			num += selector(items[i], i);
		}
		return num;
	}

	public static ImmutableArray<T> Concat<T>(this ImmutableArray<T> first, ImmutableArray<T> second)
	{
		return first.AddRange(second);
	}

	public static ImmutableArray<T> Concat<T>(this ImmutableArray<T> first, T second)
	{
		return first.Add(second);
	}

	public static ImmutableArray<T> Concat<T>(this ImmutableArray<T> first, ImmutableArray<T> second, ImmutableArray<T> third)
	{
		FixedSizeArrayBuilder<T> fixedSizeArrayBuilder = new FixedSizeArrayBuilder<T>(first.Length + second.Length + third.Length);
		fixedSizeArrayBuilder.AddRange(first);
		fixedSizeArrayBuilder.AddRange(second);
		fixedSizeArrayBuilder.AddRange(third);
		return fixedSizeArrayBuilder.MoveToImmutable();
	}

	public static ImmutableArray<T> Concat<T>(this ImmutableArray<T> first, ImmutableArray<T> second, ImmutableArray<T> third, ImmutableArray<T> fourth)
	{
		FixedSizeArrayBuilder<T> fixedSizeArrayBuilder = new FixedSizeArrayBuilder<T>(first.Length + second.Length + third.Length + fourth.Length);
		fixedSizeArrayBuilder.AddRange(first);
		fixedSizeArrayBuilder.AddRange(second);
		fixedSizeArrayBuilder.AddRange(third);
		fixedSizeArrayBuilder.AddRange(fourth);
		return fixedSizeArrayBuilder.MoveToImmutable();
	}

	public static ImmutableArray<T> Concat<T>(this ImmutableArray<T> first, ImmutableArray<T> second, ImmutableArray<T> third, ImmutableArray<T> fourth, ImmutableArray<T> fifth)
	{
		FixedSizeArrayBuilder<T> fixedSizeArrayBuilder = new FixedSizeArrayBuilder<T>(first.Length + second.Length + third.Length + fourth.Length + fifth.Length);
		fixedSizeArrayBuilder.AddRange(first);
		fixedSizeArrayBuilder.AddRange(second);
		fixedSizeArrayBuilder.AddRange(third);
		fixedSizeArrayBuilder.AddRange(fourth);
		fixedSizeArrayBuilder.AddRange(fifth);
		return fixedSizeArrayBuilder.MoveToImmutable();
	}

	public static ImmutableArray<T> Concat<T>(this ImmutableArray<T> first, ImmutableArray<T> second, ImmutableArray<T> third, ImmutableArray<T> fourth, ImmutableArray<T> fifth, ImmutableArray<T> sixth)
	{
		FixedSizeArrayBuilder<T> fixedSizeArrayBuilder = new FixedSizeArrayBuilder<T>(first.Length + second.Length + third.Length + fourth.Length + fifth.Length + sixth.Length);
		fixedSizeArrayBuilder.AddRange(first);
		fixedSizeArrayBuilder.AddRange(second);
		fixedSizeArrayBuilder.AddRange(third);
		fixedSizeArrayBuilder.AddRange(fourth);
		fixedSizeArrayBuilder.AddRange(fifth);
		fixedSizeArrayBuilder.AddRange(sixth);
		return fixedSizeArrayBuilder.MoveToImmutable();
	}

	public static ImmutableArray<T> Distinct<T>(this ImmutableArray<T> array, IEqualityComparer<T>? comparer = null)
	{
		if (array.Length < 2)
		{
			return array;
		}
		HashSet<T> hashSet = new HashSet<T>(comparer);
		ArrayBuilder<T> instance = ArrayBuilder<T>.GetInstance();
		foreach (T item in array)
		{
			if (hashSet.Add(item))
			{
				instance.Add(item);
			}
		}
		object result = ((instance.Count == array.Length) ? ((object)array) : ((object)instance.ToImmutable()));
		instance.Free();
		return (ImmutableArray<T>)result;
	}

	public static bool Any<T, TArg>(this ImmutableArray<T> array, Func<T, TArg, bool> predicate, TArg arg)
	{
		foreach (T item in array)
		{
			if (predicate(item, arg))
			{
				return true;
			}
		}
		return false;
	}

	public static bool All<T, TArg>(this ImmutableArray<T> array, Func<T, TArg, bool> predicate, TArg arg)
	{
		foreach (T item in array)
		{
			if (!predicate(item, arg))
			{
				return false;
			}
		}
		return true;
	}
}
