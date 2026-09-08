using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Roslyn.Utilities;

internal static class EnumerableExtensions
{
	private static readonly Func<object, bool> s_notNullTest = (object x) => x != null;

	public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (source is IList<T> list)
		{
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				action(list[i]);
			}
		}
		else
		{
			foreach (T item in source)
			{
				action(item);
			}
		}
		return source;
	}

	public static ImmutableArray<T> ToImmutableArrayOrEmpty<T>(this IEnumerable<T>? items)
	{
		if (items == null)
		{
			return ImmutableArray.Create<T>();
		}
		if (items is ImmutableArray<T> array)
		{
			return array.NullToEmpty();
		}
		return ImmutableArray.CreateRange(items);
	}

	public static IReadOnlyList<T> ToBoxedImmutableArray<T>(this IEnumerable<T>? items)
	{
		if (items == null)
		{
			return SpecializedCollections.EmptyBoxedImmutableArray<T>();
		}
		if (items is ImmutableArray<T> immutableArray)
		{
			if (!immutableArray.IsDefaultOrEmpty)
			{
				return (IReadOnlyList<T>)items;
			}
			return SpecializedCollections.EmptyBoxedImmutableArray<T>();
		}
		if (items is ICollection<T> { Count: 0 })
		{
			return SpecializedCollections.EmptyBoxedImmutableArray<T>();
		}
		return ImmutableArray.CreateRange(items);
	}

	public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return new ReadOnlyCollection<T>(source.ToList());
	}

	public static bool SetEquals<T>(this IEnumerable<T> source1, IEnumerable<T> source2, IEqualityComparer<T>? comparer)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		return source1.ToSet(comparer).SetEquals(source2);
	}

	public static bool SetEquals<T>(this IEnumerable<T> source1, IEnumerable<T> source2)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		return source1.ToSet().SetEquals(source2);
	}

	public static ISet<T> ToSet<T>(this IEnumerable<T> source, IEqualityComparer<T>? comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return new HashSet<T>(source, comparer);
	}

	public static ISet<T> ToSet<T>(this IEnumerable<T> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return (source as ISet<T>) ?? new HashSet<T>(source);
	}

	public static IReadOnlyCollection<T> ToCollection<T>(this IEnumerable<T> sequence)
	{
		if (!(sequence is IReadOnlyCollection<T> result))
		{
			return sequence.ToList();
		}
		return result;
	}

	public static T? FirstOrNull<T>(this IEnumerable<T> source) where T : struct
	{
		return source.Cast<T?>().FirstOrDefault();
	}

	public static T? FirstOrNull<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : struct
	{
		return source.Cast<T?>().FirstOrDefault((T? v, Func<T, bool> func) => func(v.Value), predicate);
	}

	public static T? FirstOrNull<T, TArg>(this IEnumerable<T> source, Func<T, TArg, bool> predicate, TArg arg) where T : struct
	{
		return source.Cast<T?>().FirstOrDefault((T? v, (Func<T, TArg, bool> predicate, TArg arg) tuple) => tuple.predicate(v.Value, tuple.arg), (predicate, arg));
	}

	public static T? LastOrNull<T>(this IEnumerable<T> source) where T : struct
	{
		return source.Cast<T?>().LastOrDefault();
	}

	public static T? SingleOrNull<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : struct
	{
		return source.Cast<T?>().SingleOrDefault((T? v) => predicate(v.Value));
	}

	public static bool IsSingle<T>(this IEnumerable<T> list)
	{
		using IEnumerator<T> enumerator = list.GetEnumerator();
		return enumerator.MoveNext() && !enumerator.MoveNext();
	}

	public static bool IsEmpty<T>(this IEnumerable<T> source)
	{
		if (source is IReadOnlyCollection<T> readOnlyCollection)
		{
			return readOnlyCollection.Count == 0;
		}
		if (source is ICollection<T> collection)
		{
			return collection.Count == 0;
		}
		if (source is ICollection collection2)
		{
			return collection2.Count == 0;
		}
		if (source is string text)
		{
			return text.Length == 0;
		}
		using (IEnumerator<T> enumerator = source.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				_ = enumerator.Current;
				return false;
			}
		}
		return true;
	}

	public static bool IsEmpty<T>(this IReadOnlyCollection<T> source)
	{
		return source.Count == 0;
	}

	public static bool IsEmpty<T>(this ICollection<T> source)
	{
		return source.Count == 0;
	}

	public static bool IsEmpty(this string source)
	{
		return source.Length == 0;
	}

	public static bool IsEmpty<T>(this T[] source)
	{
		return source.Length == 0;
	}

	public static bool IsEmpty<T>(this List<T> source)
	{
		return source.Count == 0;
	}

	public static bool HasDuplicates<T>(this IEnumerable<T> source)
	{
		return source.HasDuplicates(EqualityComparer<T>.Default);
	}

	public static bool HasDuplicates<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
	{
		return source.HasDuplicates((T x) => x, comparer);
	}

	public static bool HasDuplicates<TItem, TValue>(this IEnumerable<TItem> source, Func<TItem, TValue> selector)
	{
		return source.HasDuplicates(selector, EqualityComparer<TValue>.Default);
	}

	public static bool HasDuplicates<TItem, TValue>(this IEnumerable<TItem> source, Func<TItem, TValue> selector, IEqualityComparer<TValue> comparer)
	{
		if (source is IReadOnlyList<TItem> source2)
		{
			return source2.HasDuplicates(selector, comparer);
		}
		TItem arg = default(TItem);
		HashSet<TValue> hashSet = null;
		bool flag = true;
		bool result = false;
		foreach (TItem item in source)
		{
			if (flag)
			{
				arg = item;
				flag = false;
				continue;
			}
			TValue val = selector(item);
			if (hashSet == null)
			{
				TValue val2 = selector(arg);
				if (comparer.Equals(val, val2))
				{
					result = true;
					break;
				}
				hashSet = ((comparer == EqualityComparer<TValue>.Default) ? PooledHashSet<TValue>.GetInstance() : new HashSet<TValue>(comparer));
				hashSet.Add(val2);
				hashSet.Add(val);
			}
			else if (!hashSet.Add(val))
			{
				result = true;
				break;
			}
		}
		(hashSet as PooledHashSet<TValue>)?.Free();
		return result;
	}

	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class
	{
		if (source == null)
		{
			return SpecializedCollections.EmptyEnumerable<T>();
		}
		return source.Where<T>((Func<T, bool>)s_notNullTest);
	}

	private static bool TryGetBuilder<TSource, TResult>([NotNullWhen(true)] IEnumerable<TSource>? source, bool useCountForBuilder, [NotNullWhen(true)] out ArrayBuilder<TResult>? builder)
	{
		if (source == null)
		{
			builder = null;
			return false;
		}
		builder = ArrayBuilder<TResult>.GetInstance();
		return true;
	}

	public static ImmutableArray<T> WhereAsArray<T>(this IEnumerable<T> values, Func<T, bool> predicate)
	{
		if (!TryGetBuilder(values, false, out ArrayBuilder<T> builder))
		{
			return ImmutableArray<T>.Empty;
		}
		foreach (T value in values)
		{
			if (predicate(value))
			{
				builder.Add(value);
			}
		}
		return builder.ToImmutableAndFree();
	}

	public static ImmutableArray<T> WhereAsArray<T, TArg>(this IEnumerable<T> values, Func<T, TArg, bool> predicate, TArg arg)
	{
		if (!TryGetBuilder(values, false, out ArrayBuilder<T> builder))
		{
			return ImmutableArray<T>.Empty;
		}
		foreach (T value in values)
		{
			if (predicate(value, arg))
			{
				builder.Add(value);
			}
		}
		return builder.ToImmutableAndFree();
	}

	public static T[] AsArray<T>(this IEnumerable<T> source)
	{
		return (source as T[]) ?? source.ToArray();
	}

	public static ImmutableArray<TResult> SelectAsArray<TSource, TResult>(this IEnumerable<TSource>? source, Func<TSource, TResult> selector)
	{
		if (!TryGetBuilder(source, true, out ArrayBuilder<TResult> builder))
		{
			return ImmutableArray<TResult>.Empty;
		}
		builder.AddRange(source.Select(selector));
		return builder.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> SelectAsArray<TItem, TResult>(this IEnumerable<TItem>? source, Func<TItem, bool> predicate, Func<TItem, TResult> selector)
	{
		if (!TryGetBuilder(source, false, out ArrayBuilder<TResult> builder))
		{
			return ImmutableArray<TResult>.Empty;
		}
		foreach (TItem item in source)
		{
			if (predicate(item))
			{
				builder.Add(selector(item));
			}
		}
		return builder.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> SelectAsArray<TSource, TResult>(this IEnumerable<TSource>? source, Func<TSource, int, TResult> selector)
	{
		if (!TryGetBuilder(source, true, out ArrayBuilder<TResult> builder))
		{
			return ImmutableArray<TResult>.Empty;
		}
		int num = 0;
		foreach (TSource item in source)
		{
			builder.Add(selector(item, num));
			num++;
		}
		return builder.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> SelectAsArray<TSource, TResult, TArg>(this IEnumerable<TSource>? source, Func<TSource, TArg, TResult> selector, TArg arg)
	{
		if (source == null)
		{
			return ImmutableArray<TResult>.Empty;
		}
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance();
		foreach (TSource item in source)
		{
			instance.Add(selector(item, arg));
		}
		return instance.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> SelectAsArray<TSource, TResult>(this IReadOnlyCollection<TSource>? source, Func<TSource, TResult> selector)
	{
		if ((source == null || source.Count == 0) ? true : false)
		{
			return ImmutableArray<TResult>.Empty;
		}
		FixedSizeArrayBuilder<TResult> fixedSizeArrayBuilder = new FixedSizeArrayBuilder<TResult>(source.Count);
		foreach (TSource item in source)
		{
			fixedSizeArrayBuilder.Add(selector(item));
		}
		return fixedSizeArrayBuilder.MoveToImmutable();
	}

	public static ImmutableArray<TResult> SelectAsArray<TSource, TResult, TArg>(this IReadOnlyCollection<TSource>? source, Func<TSource, TArg, TResult> selector, TArg arg)
	{
		if ((source == null || source.Count == 0) ? true : false)
		{
			return ImmutableArray<TResult>.Empty;
		}
		FixedSizeArrayBuilder<TResult> fixedSizeArrayBuilder = new FixedSizeArrayBuilder<TResult>(source.Count);
		foreach (TSource item in source)
		{
			fixedSizeArrayBuilder.Add(selector(item, arg));
		}
		return fixedSizeArrayBuilder.MoveToImmutable();
	}

	public static ImmutableArray<TResult> SelectManyAsArray<TSource, TResult>(this IEnumerable<TSource>? source, Func<TSource, IEnumerable<TResult>> selector)
	{
		if (!TryGetBuilder(source, false, out ArrayBuilder<TResult> builder))
		{
			return ImmutableArray<TResult>.Empty;
		}
		foreach (TSource item in source)
		{
			builder.AddRange(selector(item));
		}
		return builder.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> SelectManyAsArray<TItem, TArg, TResult>(this IEnumerable<TItem>? source, Func<TItem, TArg, IEnumerable<TResult>> selector, TArg arg)
	{
		if (!TryGetBuilder(source, false, out ArrayBuilder<TResult> builder))
		{
			return ImmutableArray<TResult>.Empty;
		}
		foreach (TItem item in source)
		{
			builder.AddRange(selector(item, arg));
		}
		return builder.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> SelectManyAsArray<TItem, TResult>(this IReadOnlyCollection<TItem>? source, Func<TItem, IEnumerable<TResult>> selector)
	{
		if ((source == null || source.Count == 0) ? true : false)
		{
			return ImmutableArray<TResult>.Empty;
		}
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance(source.Count);
		foreach (TItem item in source)
		{
			instance.AddRange(selector(item));
		}
		return instance.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> SelectManyAsArray<TItem, TArg, TResult>(this IReadOnlyCollection<TItem>? source, Func<TItem, TArg, IEnumerable<TResult>> selector, TArg arg)
	{
		if ((source == null || source.Count == 0) ? true : false)
		{
			return ImmutableArray<TResult>.Empty;
		}
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance(source.Count);
		foreach (TItem item in source)
		{
			instance.AddRange(selector(item, arg));
		}
		return instance.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> SelectManyAsArray<TSource, TResult>(this IEnumerable<TSource>? source, Func<TSource, OneOrMany<TResult>> selector)
	{
		if (!TryGetBuilder(source, false, out ArrayBuilder<TResult> builder))
		{
			return ImmutableArray<TResult>.Empty;
		}
		foreach (TSource item in source)
		{
			selector(item).AddRangeTo(builder);
		}
		return builder.ToImmutableAndFree();
	}

	public static async ValueTask<ImmutableArray<TResult>> SelectAsArrayAsync<TItem, TResult>(this IEnumerable<TItem> source, Func<TItem, ValueTask<TResult>> selector)
	{
		if (!TryGetBuilder(source, true, out ArrayBuilder<TResult> builder))
		{
			return ImmutableArray<TResult>.Empty;
		}
		foreach (TItem item in source)
		{
			ArrayBuilder<TResult> arrayBuilder = builder;
			arrayBuilder.Add(await selector(item).ConfigureAwait(continueOnCapturedContext: false));
		}
		return builder.ToImmutableAndFree();
	}

	public static async ValueTask<ImmutableArray<TResult>> SelectAsArrayAsync<TItem, TResult>(this IEnumerable<TItem> source, Func<TItem, CancellationToken, ValueTask<TResult>> selector, CancellationToken cancellationToken)
	{
		if (!TryGetBuilder(source, true, out ArrayBuilder<TResult> builder))
		{
			return ImmutableArray<TResult>.Empty;
		}
		foreach (TItem item in source)
		{
			ArrayBuilder<TResult> arrayBuilder = builder;
			arrayBuilder.Add(await selector(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
		}
		return builder.ToImmutableAndFree();
	}

	public static async ValueTask<ImmutableArray<TResult>> SelectAsArrayAsync<TItem, TArg, TResult>(this IEnumerable<TItem> source, Func<TItem, TArg, CancellationToken, ValueTask<TResult>> selector, TArg arg, CancellationToken cancellationToken)
	{
		if (!TryGetBuilder(source, true, out ArrayBuilder<TResult> builder))
		{
			return ImmutableArray<TResult>.Empty;
		}
		foreach (TItem item in source)
		{
			ArrayBuilder<TResult> arrayBuilder = builder;
			arrayBuilder.Add(await selector(item, arg, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
		}
		return builder.ToImmutableAndFree();
	}

	public static async ValueTask<ImmutableArray<TResult>> SelectManyAsArrayAsync<TItem, TArg, TResult>(this IEnumerable<TItem> source, Func<TItem, TArg, CancellationToken, ValueTask<IEnumerable<TResult>>> selector, TArg arg, CancellationToken cancellationToken)
	{
		if (!TryGetBuilder(source, false, out ArrayBuilder<TResult> builder))
		{
			return ImmutableArray<TResult>.Empty;
		}
		foreach (TItem item in source)
		{
			ArrayBuilder<TResult> arrayBuilder = builder;
			arrayBuilder.AddRange(await selector(item, arg, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
		}
		return builder.ToImmutableAndFree();
	}

	public static async ValueTask<IEnumerable<TResult>> SelectManyInParallelAsync<TItem, TResult>(this IEnumerable<TItem> sequence, Func<TItem, CancellationToken, Task<IEnumerable<TResult>>> selector, CancellationToken cancellationToken)
	{
		return (await Task.WhenAll(sequence.Select((TItem item) => selector(item, cancellationToken))).ConfigureAwait(continueOnCapturedContext: false)).Flatten();
	}

	public static int IndexOf<T>(this IEnumerable<T> sequence, T value)
	{
		if (!(sequence is IList<T> list))
		{
			if (sequence is IReadOnlyList<T> list2)
			{
				return list2.IndexOf(value, EqualityComparer<T>.Default);
			}
			return sequence.EnumeratingIndexOf(value, EqualityComparer<T>.Default);
		}
		return list.IndexOf(value);
	}

	public static int IndexOf<T>(this IEnumerable<T> sequence, T value, IEqualityComparer<T> comparer)
	{
		if (sequence is IReadOnlyList<T> list)
		{
			return list.IndexOf(value, comparer);
		}
		return sequence.EnumeratingIndexOf(value, comparer);
	}

	private static int EnumeratingIndexOf<T>(this IEnumerable<T> sequence, T value, IEqualityComparer<T> comparer)
	{
		int num = 0;
		foreach (T item in sequence)
		{
			if (comparer.Equals(item, value))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public static int IndexOf<T>(this IReadOnlyList<T> list, T value, IEqualityComparer<T> comparer)
	{
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (comparer.Equals(list[i], value))
			{
				return i;
			}
		}
		return -1;
	}

	public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> sequence)
	{
		if (sequence == null)
		{
			throw new ArgumentNullException("sequence");
		}
		return sequence.SelectMany((IEnumerable<T> s) => s);
	}

	public static bool IsSorted<T>(this IEnumerable<T> enumerable, IComparer<T>? comparer = null)
	{
		using IEnumerator<T> enumerator = enumerable.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return true;
		}
		if (comparer == null)
		{
			comparer = Comparer<T>.Default;
		}
		T current = enumerator.Current;
		while (enumerator.MoveNext())
		{
			if (comparer.Compare(current, enumerator.Current) > 0)
			{
				return false;
			}
			current = enumerator.Current;
		}
		return true;
	}

	public static ImmutableDictionary<K, V> ToImmutableDictionaryOrEmpty<K, V>(this IEnumerable<KeyValuePair<K, V>>? items) where K : notnull
	{
		if (items == null)
		{
			return ImmutableDictionary.Create<K, V>();
		}
		return ImmutableDictionary.CreateRange(items);
	}

	public static ImmutableDictionary<K, V> ToImmutableDictionaryOrEmpty<K, V>(this IEnumerable<KeyValuePair<K, V>>? items, IEqualityComparer<K>? keyComparer) where K : notnull
	{
		if (items == null)
		{
			return ImmutableDictionary.Create<K, V>(keyComparer);
		}
		return ImmutableDictionary.CreateRange(keyComparer, items);
	}

	internal static IList<IList<T>> Transpose<T>(this IEnumerable<IEnumerable<T>> data)
	{
		return data.TransposeInternal().ToArray();
	}

	private static IEnumerable<IList<T>> TransposeInternal<T>(this IEnumerable<IEnumerable<T>> data)
	{
		List<IEnumerator<T>> enumerators = new List<IEnumerator<T>>();
		int width = 0;
		foreach (IEnumerable<T> datum in data)
		{
			enumerators.Add(datum.GetEnumerator());
			width++;
		}
		try
		{
			while (true)
			{
				T[] array = null;
				for (int i = 0; i < width; i++)
				{
					IEnumerator<T> enumerator2 = enumerators[i];
					if (enumerator2.MoveNext())
					{
						if (array == null)
						{
							array = new T[width];
						}
						array[i] = enumerator2.Current;
						continue;
					}
					yield break;
				}
				yield return array;
			}
		}
		finally
		{
			foreach (IEnumerator<T> item in enumerators)
			{
				item.Dispose();
			}
		}
	}

	internal static Dictionary<K, ImmutableArray<T>> ToMultiDictionary<K, T>(this IEnumerable<T> data, Func<T, K> keySelector, IEqualityComparer<K>? comparer = null) where K : notnull
	{
		Dictionary<K, ImmutableArray<T>> dictionary = new Dictionary<K, ImmutableArray<T>>(comparer);
		foreach (IGrouping<K, T> item in data.GroupBy(keySelector, comparer))
		{
			dictionary.Add(item.Key, ImmutableCollectionsMarshal.AsImmutableArray(item.ToArray()));
		}
		return dictionary;
	}

	internal static TSource? AsSingleton<TSource>(this IEnumerable<TSource>? source)
	{
		if (source == null)
		{
			return default(TSource);
		}
		if (source is IList<TSource> list)
		{
			if (list.Count != 1)
			{
				return default(TSource);
			}
			return list[0];
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return default(TSource);
		}
		TSource current = enumerator.Current;
		if (enumerator.MoveNext())
		{
			return default(TSource);
		}
		return current;
	}
}
