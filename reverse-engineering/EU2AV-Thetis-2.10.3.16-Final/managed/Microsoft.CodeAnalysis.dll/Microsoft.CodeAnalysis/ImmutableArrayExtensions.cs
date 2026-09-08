using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal static class ImmutableArrayExtensions
{
	public static ImmutableArray<T> AsImmutable<T>(this IEnumerable<T> items)
	{
		return ImmutableArray.CreateRange(items);
	}

	public static ImmutableArray<T> AsImmutableOrEmpty<T>(this IEnumerable<T>? items)
	{
		if (items == null)
		{
			return ImmutableArray<T>.Empty;
		}
		return ImmutableArray.CreateRange(items);
	}

	public static ImmutableArray<T> AsImmutableOrNull<T>(this IEnumerable<T>? items)
	{
		if (items == null)
		{
			return default(ImmutableArray<T>);
		}
		return ImmutableArray.CreateRange(items);
	}

	public static ImmutableArray<T> AsImmutable<T>(this T[] items)
	{
		return ImmutableArray.Create(items);
	}

	public static ImmutableArray<T> AsImmutableOrNull<T>(this T[]? items)
	{
		if (items == null)
		{
			return default(ImmutableArray<T>);
		}
		return ImmutableArray.Create(items);
	}

	public static ImmutableArray<T> AsImmutableOrEmpty<T>(this T[]? items)
	{
		if (items == null)
		{
			return ImmutableArray<T>.Empty;
		}
		return ImmutableArray.Create(items);
	}

	public static ImmutableArray<byte> ToImmutable(this MemoryStream stream)
	{
		return ImmutableArray.Create(stream.ToArray());
	}

	public static ImmutableArray<TResult> SelectAsArray<TItem, TResult>(this ImmutableArray<TItem> items, Func<TItem, TResult> map)
	{
		return ImmutableArray.CreateRange(items, map);
	}

	public static ImmutableArray<TResult> SelectAsArray<TItem, TArg, TResult>(this ImmutableArray<TItem> items, Func<TItem, TArg, TResult> map, TArg arg)
	{
		return ImmutableArray.CreateRange(items, map, arg);
	}

	public static ImmutableArray<TResult> SelectAsArray<TItem, TArg, TResult>(this ImmutableArray<TItem> items, Func<TItem, int, TArg, TResult> map, TArg arg)
	{
		switch (items.Length)
		{
		case 0:
			return ImmutableArray<TResult>.Empty;
		case 1:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[1] { map(items[0], 0, arg) });
		case 2:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[2]
			{
				map(items[0], 0, arg),
				map(items[1], 1, arg)
			});
		case 3:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[3]
			{
				map(items[0], 0, arg),
				map(items[1], 1, arg),
				map(items[2], 2, arg)
			});
		case 4:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[4]
			{
				map(items[0], 0, arg),
				map(items[1], 1, arg),
				map(items[2], 2, arg),
				map(items[3], 3, arg)
			});
		default:
		{
			FixedSizeArrayBuilder<TResult> fixedSizeArrayBuilder = new FixedSizeArrayBuilder<TResult>(items.Length);
			for (int i = 0; i < items.Length; i++)
			{
				fixedSizeArrayBuilder.Add(map(items[i], i, arg));
			}
			return fixedSizeArrayBuilder.MoveToImmutable();
		}
		}
	}

	public static ImmutableArray<TResult> SelectAsArray<TItem, TResult>(this ImmutableArray<TItem> array, Func<TItem, bool> predicate, Func<TItem, TResult> selector)
	{
		if (array.Length == 0)
		{
			return ImmutableArray<TResult>.Empty;
		}
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance();
		foreach (TItem item in array)
		{
			if (predicate(item))
			{
				instance.Add(selector(item));
			}
		}
		return instance.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> SelectAsArray<TItem, TArg, TResult>(this ImmutableArray<TItem> array, Func<TItem, TArg, bool> predicate, Func<TItem, TArg, TResult> selector, TArg arg)
	{
		if (array.Length == 0)
		{
			return ImmutableArray<TResult>.Empty;
		}
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance();
		foreach (TItem item in array)
		{
			if (predicate(item, arg))
			{
				instance.Add(selector(item, arg));
			}
		}
		return instance.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> SelectManyAsArray<TItem, TResult>(this ImmutableArray<TItem> array, Func<TItem, IEnumerable<TResult>> selector)
	{
		if (array.Length == 0)
		{
			return ImmutableArray<TResult>.Empty;
		}
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance();
		foreach (TItem item in array)
		{
			instance.AddRange(selector(item));
		}
		return instance.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> SelectManyAsArray<TItem, TResult>(this ImmutableArray<TItem> array, Func<TItem, ImmutableArray<TResult>> selector)
	{
		if (array.Length == 0)
		{
			return ImmutableArray<TResult>.Empty;
		}
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance();
		foreach (TItem item in array)
		{
			instance.AddRange(selector(item));
		}
		return instance.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> SelectManyAsArray<TItem, TResult>(this ImmutableArray<TItem> array, Func<TItem, OneOrMany<TResult>> selector)
	{
		if (array.Length == 0)
		{
			return ImmutableArray<TResult>.Empty;
		}
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance();
		foreach (TItem item in array)
		{
			selector(item).AddRangeTo(instance);
		}
		return instance.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> SelectManyAsArray<TItem, TResult>(this ImmutableArray<TItem> array, Func<TItem, bool> predicate, Func<TItem, IEnumerable<TResult>> selector)
	{
		if (array.Length == 0)
		{
			return ImmutableArray<TResult>.Empty;
		}
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance();
		foreach (TItem item in array)
		{
			if (predicate(item))
			{
				instance.AddRange(selector(item));
			}
		}
		return instance.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> SelectManyAsArray<TItem, TResult>(this ImmutableArray<TItem> array, Func<TItem, bool> predicate, Func<TItem, ImmutableArray<TResult>> selector)
	{
		if (array.Length == 0)
		{
			return ImmutableArray<TResult>.Empty;
		}
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance();
		foreach (TItem item in array)
		{
			if (predicate(item))
			{
				instance.AddRange(selector(item));
			}
		}
		return instance.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> SelectManyAsArray<TItem, TResult>(this ImmutableArray<TItem> array, Func<TItem, bool> predicate, Func<TItem, OneOrMany<TResult>> selector)
	{
		if (array.Length == 0)
		{
			return ImmutableArray<TResult>.Empty;
		}
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance();
		foreach (TItem item in array)
		{
			if (predicate(item))
			{
				selector(item).AddRangeTo(instance);
			}
		}
		return instance.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> SelectManyAsArray<TItem, TArg, TResult>(this ImmutableArray<TItem> array, Func<TItem, TArg, bool> predicate, Func<TItem, TArg, OneOrMany<TResult>> selector, TArg arg)
	{
		if (array.Length == 0)
		{
			return ImmutableArray<TResult>.Empty;
		}
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance();
		foreach (TItem item in array)
		{
			if (predicate(item, arg))
			{
				selector(item, arg).AddRangeTo(instance);
			}
		}
		return instance.ToImmutableAndFree();
	}

	public static async ValueTask<ImmutableArray<TResult>> SelectAsArrayAsync<TItem, TResult>(this ImmutableArray<TItem> array, Func<TItem, CancellationToken, ValueTask<TResult>> selector, CancellationToken cancellationToken)
	{
		if (array.IsEmpty)
		{
			return ImmutableArray<TResult>.Empty;
		}
		FixedSizeArrayBuilder<TResult> builder = new FixedSizeArrayBuilder<TResult>(array.Length);
		foreach (TItem item in array)
		{
			builder.Add(await selector(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
		}
		return builder.MoveToImmutable();
	}

	public static async ValueTask<ImmutableArray<TResult>> SelectAsArrayAsync<TItem, TArg, TResult>(this ImmutableArray<TItem> array, Func<TItem, TArg, CancellationToken, ValueTask<TResult>> selector, TArg arg, CancellationToken cancellationToken)
	{
		if (array.IsEmpty)
		{
			return ImmutableArray<TResult>.Empty;
		}
		FixedSizeArrayBuilder<TResult> builder = new FixedSizeArrayBuilder<TResult>(array.Length);
		foreach (TItem item in array)
		{
			builder.Add(await selector(item, arg, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
		}
		return builder.MoveToImmutable();
	}

	public static ValueTask<ImmutableArray<TResult>> SelectManyAsArrayAsync<TItem, TArg, TResult>(this ImmutableArray<TItem> source, Func<TItem, TArg, CancellationToken, ValueTask<ImmutableArray<TResult>>> selector, TArg arg, CancellationToken cancellationToken)
	{
		if (source.Length == 0)
		{
			return new ValueTask<ImmutableArray<TResult>>(ImmutableArray<TResult>.Empty);
		}
		if (source.Length == 1)
		{
			return selector(source[0], arg, cancellationToken);
		}
		return CreateTaskAsync();
		async ValueTask<ImmutableArray<TResult>> CreateTaskAsync()
		{
			ArrayBuilder<TResult> builder = ArrayBuilder<TResult>.GetInstance();
			foreach (TItem item in source)
			{
				ArrayBuilder<TResult> arrayBuilder = builder;
				arrayBuilder.AddRange(await selector(item, arg, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
			}
			return builder.ToImmutableAndFree();
		}
	}

	public static ImmutableArray<TResult> ZipAsArray<T1, T2, TResult>(this ImmutableArray<T1> self, ImmutableArray<T2> other, Func<T1, T2, TResult> map)
	{
		switch (self.Length)
		{
		case 0:
			return ImmutableArray<TResult>.Empty;
		case 1:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[1] { map(self[0], other[0]) });
		case 2:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[2]
			{
				map(self[0], other[0]),
				map(self[1], other[1])
			});
		case 3:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[3]
			{
				map(self[0], other[0]),
				map(self[1], other[1]),
				map(self[2], other[2])
			});
		case 4:
			return ImmutableCollectionsMarshal.AsImmutableArray(new TResult[4]
			{
				map(self[0], other[0]),
				map(self[1], other[1]),
				map(self[2], other[2]),
				map(self[3], other[3])
			});
		default:
		{
			TResult[] array = new TResult[self.Length];
			for (int i = 0; i < self.Length; i++)
			{
				array[i] = map(self[i], other[i]);
			}
			return ImmutableCollectionsMarshal.AsImmutableArray(array);
		}
		}
	}

	public static ImmutableArray<TResult> ZipAsArray<T1, T2, TArg, TResult>(this ImmutableArray<T1> self, ImmutableArray<T2> other, TArg arg, Func<T1, T2, int, TArg, TResult> map)
	{
		if (self.IsEmpty)
		{
			return ImmutableArray<TResult>.Empty;
		}
		FixedSizeArrayBuilder<TResult> fixedSizeArrayBuilder = new FixedSizeArrayBuilder<TResult>(self.Length);
		for (int i = 0; i < self.Length; i++)
		{
			fixedSizeArrayBuilder.Add(map(self[i], other[i], i, arg));
		}
		return fixedSizeArrayBuilder.MoveToImmutable();
	}

	public static ImmutableArray<T> WhereAsArray<T>(this ImmutableArray<T> array, Func<T, bool> predicate)
	{
		return WhereAsArrayImpl<T, object>(array, predicate, null, null);
	}

	public static ImmutableArray<T> WhereAsArray<T, TArg>(this ImmutableArray<T> array, Func<T, TArg, bool> predicate, TArg arg)
	{
		return WhereAsArrayImpl(array, null, predicate, arg);
	}

	private static ImmutableArray<T> WhereAsArrayImpl<T, TArg>(ImmutableArray<T> array, Func<T, bool>? predicateWithoutArg, Func<T, TArg, bool>? predicateWithArg, TArg arg)
	{
		ArrayBuilder<T> arrayBuilder = null;
		bool flag = true;
		bool flag2 = true;
		int length = array.Length;
		for (int i = 0; i < length; i++)
		{
			T val = array[i];
			if (predicateWithoutArg?.Invoke(val) ?? predicateWithArg(val, arg))
			{
				flag = false;
				if (!flag2)
				{
					if (arrayBuilder == null)
					{
						arrayBuilder = ArrayBuilder<T>.GetInstance();
					}
					arrayBuilder.Add(val);
				}
			}
			else if (flag)
			{
				flag2 = false;
			}
			else if (flag2)
			{
				flag2 = false;
				arrayBuilder = ArrayBuilder<T>.GetInstance();
				for (int j = 0; j < i; j++)
				{
					arrayBuilder.Add(array[j]);
				}
			}
		}
		if (arrayBuilder != null)
		{
			return arrayBuilder.ToImmutableAndFree();
		}
		if (flag2)
		{
			return array;
		}
		return ImmutableArray<T>.Empty;
	}

	public static async Task<bool> AnyAsync<T>(this ImmutableArray<T> array, Func<T, Task<bool>> predicateAsync)
	{
		foreach (T item in array)
		{
			if (await predicateAsync(item).ConfigureAwait(continueOnCapturedContext: false))
			{
				return true;
			}
		}
		return false;
	}

	public static async Task<bool> AnyAsync<T, TArg>(this ImmutableArray<T> array, Func<T, TArg, Task<bool>> predicateAsync, TArg arg)
	{
		foreach (T item in array)
		{
			if (await predicateAsync(item, arg).ConfigureAwait(continueOnCapturedContext: false))
			{
				return true;
			}
		}
		return false;
	}

	public static async ValueTask<T?> FirstOrDefaultAsync<T>(this ImmutableArray<T> array, Func<T, Task<bool>> predicateAsync)
	{
		foreach (T item in array)
		{
			if (await predicateAsync(item).ConfigureAwait(continueOnCapturedContext: false))
			{
				return item;
			}
		}
		return default(T);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ImmutableArray<TBase> Cast<TDerived, TBase>(this ImmutableArray<TDerived> items) where TDerived : class, TBase
	{
		return ImmutableArray<TBase>.CastUp<TDerived>(items);
	}

	public static bool SetEquals<T>(this ImmutableArray<T> array1, ImmutableArray<T> array2, IEqualityComparer<T> comparer)
	{
		if (array1.IsDefault)
		{
			return array2.IsDefault;
		}
		if (array2.IsDefault)
		{
			return false;
		}
		int length = array1.Length;
		int length2 = array2.Length;
		if (length == 0)
		{
			return length2 == 0;
		}
		if (length2 == 0)
		{
			return false;
		}
		if (length == 1 && length2 == 1)
		{
			T x = array1[0];
			T y = array2[0];
			return comparer.Equals(x, y);
		}
		HashSet<T> hashSet = new HashSet<T>(array1, comparer);
		HashSet<T> hashSet2 = new HashSet<T>(array2, comparer);
		return hashSet.SetEquals(hashSet2);
	}

	public static ImmutableArray<T> NullToEmpty<T>(this ImmutableArray<T> array)
	{
		if (!array.IsDefault)
		{
			return array;
		}
		return ImmutableArray<T>.Empty;
	}

	public static ImmutableArray<T> NullToEmpty<T>(this ImmutableArray<T>? array)
	{
		if (array.HasValue)
		{
			ImmutableArray<T> valueOrDefault = array.GetValueOrDefault();
			if (!valueOrDefault.IsDefault)
			{
				return valueOrDefault;
			}
		}
		return ImmutableArray<T>.Empty;
	}

	internal static ImmutableArray<T> ConditionallyDeOrder<T>(this ImmutableArray<T> array)
	{
		return array;
	}

	internal static ImmutableArray<TValue> Flatten<TKey, TValue>(this Dictionary<TKey, ImmutableArray<TValue>> dictionary, IComparer<TValue>? comparer = null) where TKey : notnull
	{
		if (dictionary.Count == 0)
		{
			return ImmutableArray<TValue>.Empty;
		}
		ArrayBuilder<TValue> instance = ArrayBuilder<TValue>.GetInstance();
		foreach (KeyValuePair<TKey, ImmutableArray<TValue>> item in dictionary)
		{
			instance.AddRange(item.Value);
		}
		if (comparer != null && instance.Count > 1)
		{
			instance.Sort(comparer);
		}
		return instance.ToImmutableAndFree();
	}

	internal static ImmutableArray<T> AddRange<T>(this ImmutableArray<T> self, in TemporaryArray<T> items)
	{
		if (items.Count == 0)
		{
			return self;
		}
		if (items.Count == 1)
		{
			return self.Add(items[0]);
		}
		T[] array = new T[self.Length + items.Count];
		int num = 0;
		foreach (T item in self)
		{
			array[num++] = item;
		}
		foreach (T item2 in items)
		{
			array[num++] = item2;
		}
		return ImmutableCollectionsMarshal.AsImmutableArray(array);
	}

	internal static bool HasDuplicates<T>(this ImmutableArray<T> array)
	{
		return array.HasDuplicates(EqualityComparer<T>.Default);
	}

	internal static bool HasDuplicates<T>(this ImmutableArray<T> array, IEqualityComparer<T> comparer)
	{
		return array.HasDuplicates((T x) => x, comparer);
	}

	public static bool HasDuplicates<TItem, TValue>(this ImmutableArray<TItem> array, Func<TItem, TValue> selector)
	{
		return array.HasDuplicates(selector, EqualityComparer<TValue>.Default);
	}

	internal static bool HasDuplicates<TItem, TValue>(this ImmutableArray<TItem> array, Func<TItem, TValue> selector, IEqualityComparer<TValue> comparer)
	{
		switch (array.Length)
		{
		case 0:
		case 1:
			return false;
		case 2:
			return comparer.Equals(selector(array[0]), selector(array[1]));
		default:
		{
			HashSet<TValue> hashSet = ((comparer == EqualityComparer<TValue>.Default) ? PooledHashSet<TValue>.GetInstance() : new HashSet<TValue>(comparer));
			bool result = false;
			foreach (TItem item in array)
			{
				if (!hashSet.Add(selector(item)))
				{
					result = true;
					break;
				}
			}
			(hashSet as PooledHashSet<TValue>)?.Free();
			return result;
		}
		}
	}

	internal static void AddToMultiValueDictionaryBuilder<K, T>(Dictionary<K, object> accumulator, K key, T item) where K : notnull where T : notnull
	{
		if (accumulator.TryGetValue(key, out object value))
		{
			ArrayBuilder<T> arrayBuilder = value as ArrayBuilder<T>;
			if (arrayBuilder == null)
			{
				arrayBuilder = ArrayBuilder<T>.GetInstance(2);
				arrayBuilder.Add((T)value);
				accumulator[key] = arrayBuilder;
			}
			arrayBuilder.Add(item);
		}
		else
		{
			accumulator.Add(key, item);
		}
	}

	internal static void CreateNameToMembersMap<TKey, TNamespaceOrTypeSymbol, TNamedTypeSymbol, TNamespaceSymbol>(Dictionary<TKey, object> dictionary, Dictionary<TKey, ImmutableArray<TNamespaceOrTypeSymbol>> result) where TKey : notnull where TNamespaceOrTypeSymbol : class where TNamedTypeSymbol : class, TNamespaceOrTypeSymbol where TNamespaceSymbol : class, TNamespaceOrTypeSymbol
	{
		foreach (KeyValuePair<TKey, object> item in dictionary)
		{
			result.Add(item.Key, createMembers(item.Value));
		}
		static ImmutableArray<TNamespaceOrTypeSymbol> createMembers(object value)
		{
			if (value is ArrayBuilder<TNamespaceOrTypeSymbol> arrayBuilder)
			{
				foreach (TNamespaceOrTypeSymbol item2 in arrayBuilder)
				{
					if (item2 is TNamespaceSymbol)
					{
						return arrayBuilder.ToImmutableAndFree();
					}
				}
				return ImmutableArray<TNamespaceOrTypeSymbol>.CastUp<TNamedTypeSymbol>(arrayBuilder.ToDowncastedImmutableAndFree<TNamedTypeSymbol>());
			}
			TNamespaceOrTypeSymbol val = (TNamespaceOrTypeSymbol)value;
			if (!(val is TNamespaceSymbol))
			{
				return ImmutableArray<TNamespaceOrTypeSymbol>.CastUp<TNamedTypeSymbol>(ImmutableArray.Create((TNamedTypeSymbol)(object)val));
			}
			return ImmutableArray.Create(val);
		}
	}

	internal static Dictionary<TKey, ImmutableArray<TNamedTypeSymbol>> GetTypesFromMemberMap<TKey, TNamespaceOrTypeSymbol, TNamedTypeSymbol>(Dictionary<TKey, ImmutableArray<TNamespaceOrTypeSymbol>> map, IEqualityComparer<TKey> comparer) where TKey : notnull where TNamespaceOrTypeSymbol : class where TNamedTypeSymbol : class, TNamespaceOrTypeSymbol
	{
		Dictionary<TKey, ImmutableArray<TNamedTypeSymbol>> dictionary = new Dictionary<TKey, ImmutableArray<TNamedTypeSymbol>>((map.Count > 3) ? map.Count : 0, comparer);
		foreach (KeyValuePair<TKey, ImmutableArray<TNamespaceOrTypeSymbol>> item2 in map)
		{
			ImmutableArray<TNamedTypeSymbol> value = getOrCreateNamedTypes(item2.Value);
			if (value.Length > 0)
			{
				dictionary.Add(item2.Key, value);
			}
		}
		return dictionary;
		static ImmutableArray<TNamedTypeSymbol> getOrCreateNamedTypes(ImmutableArray<TNamespaceOrTypeSymbol> members)
		{
			ImmutableArray<TNamedTypeSymbol> result = members.As<TNamedTypeSymbol>();
			if (!result.IsDefault)
			{
				return result;
			}
			int num = members.Count((TNamespaceOrTypeSymbol s) => s is TNamedTypeSymbol);
			if (num == 0)
			{
				return ImmutableArray<TNamedTypeSymbol>.Empty;
			}
			ArrayBuilder<TNamedTypeSymbol> instance = ArrayBuilder<TNamedTypeSymbol>.GetInstance(num);
			foreach (TNamespaceOrTypeSymbol item3 in members)
			{
				if (item3 is TNamedTypeSymbol item)
				{
					instance.Add(item);
				}
			}
			return instance.ToImmutableAndFree();
		}
	}

	internal static int IndexOf<T>(this ImmutableArray<T> array, T item, IEqualityComparer<T> comparer)
	{
		return array.IndexOf(item, 0, comparer);
	}

	internal static bool IsSorted<T>(this ImmutableArray<T> array, Comparison<T> comparison)
	{
		return array.IsSorted(Comparer<T>.Create(comparison));
	}

	internal static bool IsSorted<T>(this ImmutableArray<T> array, IComparer<T>? comparer = null)
	{
		if (comparer == null)
		{
			comparer = Comparer<T>.Default;
		}
		for (int i = 1; i < array.Length; i++)
		{
			if (comparer.Compare(array[i - 1], array[i]) > 0)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsSubsetOf<TElement>(this ImmutableArray<TElement> array, ImmutableArray<TElement> other)
	{
		if (other.Length == 0)
		{
			return array.Length == 0;
		}
		switch (array.Length)
		{
		case 0:
			return true;
		case 1:
			return other.Contains(array[0]);
		case 2:
			if (other.Contains(array[0]))
			{
				return other.Contains(array[1]);
			}
			return false;
		case 3:
			if (other.Contains(array[0]) && other.Contains(array[1]))
			{
				return other.Contains(array[2]);
			}
			return false;
		default:
		{
			PooledHashSet<TElement> instance = PooledHashSet<TElement>.GetInstance();
			foreach (TElement item in other)
			{
				instance.Add(item);
			}
			foreach (TElement item2 in array)
			{
				if (!instance.Contains(item2))
				{
					instance.Free();
					return false;
				}
			}
			instance.Free();
			return true;
		}
		}
	}
}
