using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.CodeAnalysis.Collections;

internal static class RoslynImmutableInterlocked
{
	public static bool Update<T>(ref ImmutableSegmentedList<T> location, Func<ImmutableSegmentedList<T>, ImmutableSegmentedList<T>> transformer)
	{
		if (transformer == null)
		{
			throw new ArgumentNullException("transformer");
		}
		ImmutableSegmentedList<T> immutableSegmentedList = ImmutableSegmentedList<T>.PrivateMarshal.VolatileRead(in location);
		while (true)
		{
			ImmutableSegmentedList<T> immutableSegmentedList2 = transformer(immutableSegmentedList);
			if (immutableSegmentedList == immutableSegmentedList2)
			{
				return false;
			}
			ImmutableSegmentedList<T> immutableSegmentedList3 = InterlockedCompareExchange(ref location, immutableSegmentedList2, immutableSegmentedList);
			if (immutableSegmentedList == immutableSegmentedList3)
			{
				break;
			}
			immutableSegmentedList = immutableSegmentedList3;
		}
		return true;
	}

	public static bool Update<T, TArg>(ref ImmutableSegmentedList<T> location, Func<ImmutableSegmentedList<T>, TArg, ImmutableSegmentedList<T>> transformer, TArg transformerArgument)
	{
		if (transformer == null)
		{
			throw new ArgumentNullException("transformer");
		}
		ImmutableSegmentedList<T> immutableSegmentedList = ImmutableSegmentedList<T>.PrivateMarshal.VolatileRead(in location);
		while (true)
		{
			ImmutableSegmentedList<T> immutableSegmentedList2 = transformer(immutableSegmentedList, transformerArgument);
			if (immutableSegmentedList == immutableSegmentedList2)
			{
				return false;
			}
			ImmutableSegmentedList<T> immutableSegmentedList3 = InterlockedCompareExchange(ref location, immutableSegmentedList2, immutableSegmentedList);
			if (immutableSegmentedList == immutableSegmentedList3)
			{
				break;
			}
			immutableSegmentedList = immutableSegmentedList3;
		}
		return true;
	}

	public static ImmutableSegmentedList<T> InterlockedExchange<T>(ref ImmutableSegmentedList<T> location, ImmutableSegmentedList<T> value)
	{
		return ImmutableSegmentedList<T>.PrivateMarshal.InterlockedExchange(ref location, value);
	}

	public static ImmutableSegmentedList<T> InterlockedCompareExchange<T>(ref ImmutableSegmentedList<T> location, ImmutableSegmentedList<T> value, ImmutableSegmentedList<T> comparand)
	{
		return ImmutableSegmentedList<T>.PrivateMarshal.InterlockedCompareExchange(ref location, value, comparand);
	}

	public static bool InterlockedInitialize<T>(ref ImmutableSegmentedList<T> location, ImmutableSegmentedList<T> value)
	{
		return InterlockedCompareExchange(ref location, value, default(ImmutableSegmentedList<T>)).IsDefault;
	}

	public static bool Update<T>(ref ImmutableSegmentedHashSet<T> location, Func<ImmutableSegmentedHashSet<T>, ImmutableSegmentedHashSet<T>> transformer)
	{
		if (transformer == null)
		{
			throw new ArgumentNullException("transformer");
		}
		ImmutableSegmentedHashSet<T> immutableSegmentedHashSet = ImmutableSegmentedHashSet<T>.PrivateMarshal.VolatileRead(in location);
		while (true)
		{
			ImmutableSegmentedHashSet<T> immutableSegmentedHashSet2 = transformer(immutableSegmentedHashSet);
			if (immutableSegmentedHashSet == immutableSegmentedHashSet2)
			{
				return false;
			}
			ImmutableSegmentedHashSet<T> immutableSegmentedHashSet3 = InterlockedCompareExchange(ref location, immutableSegmentedHashSet2, immutableSegmentedHashSet);
			if (immutableSegmentedHashSet == immutableSegmentedHashSet3)
			{
				break;
			}
			immutableSegmentedHashSet = immutableSegmentedHashSet3;
		}
		return true;
	}

	public static bool Update<T, TArg>(ref ImmutableSegmentedHashSet<T> location, Func<ImmutableSegmentedHashSet<T>, TArg, ImmutableSegmentedHashSet<T>> transformer, TArg transformerArgument)
	{
		if (transformer == null)
		{
			throw new ArgumentNullException("transformer");
		}
		ImmutableSegmentedHashSet<T> immutableSegmentedHashSet = ImmutableSegmentedHashSet<T>.PrivateMarshal.VolatileRead(in location);
		while (true)
		{
			ImmutableSegmentedHashSet<T> immutableSegmentedHashSet2 = transformer(immutableSegmentedHashSet, transformerArgument);
			if (immutableSegmentedHashSet == immutableSegmentedHashSet2)
			{
				return false;
			}
			ImmutableSegmentedHashSet<T> immutableSegmentedHashSet3 = InterlockedCompareExchange(ref location, immutableSegmentedHashSet2, immutableSegmentedHashSet);
			if (immutableSegmentedHashSet == immutableSegmentedHashSet3)
			{
				break;
			}
			immutableSegmentedHashSet = immutableSegmentedHashSet3;
		}
		return true;
	}

	public static ImmutableSegmentedHashSet<T> InterlockedExchange<T>(ref ImmutableSegmentedHashSet<T> location, ImmutableSegmentedHashSet<T> value)
	{
		return ImmutableSegmentedHashSet<T>.PrivateMarshal.InterlockedExchange(ref location, value);
	}

	public static ImmutableSegmentedHashSet<T> InterlockedCompareExchange<T>(ref ImmutableSegmentedHashSet<T> location, ImmutableSegmentedHashSet<T> value, ImmutableSegmentedHashSet<T> comparand)
	{
		return ImmutableSegmentedHashSet<T>.PrivateMarshal.InterlockedCompareExchange(ref location, value, comparand);
	}

	public static bool InterlockedInitialize<T>(ref ImmutableSegmentedHashSet<T> location, ImmutableSegmentedHashSet<T> value)
	{
		return InterlockedCompareExchange(ref location, value, default(ImmutableSegmentedHashSet<T>)).IsDefault;
	}

	public static bool Update<TKey, TValue>(ref ImmutableSegmentedDictionary<TKey, TValue> location, Func<ImmutableSegmentedDictionary<TKey, TValue>, ImmutableSegmentedDictionary<TKey, TValue>> transformer) where TKey : notnull
	{
		if (transformer == null)
		{
			throw new ArgumentNullException("transformer");
		}
		ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary = ImmutableSegmentedDictionary<TKey, TValue>.PrivateMarshal.VolatileRead(in location);
		while (true)
		{
			ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary2 = transformer(immutableSegmentedDictionary);
			if (immutableSegmentedDictionary == immutableSegmentedDictionary2)
			{
				return false;
			}
			ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary3 = InterlockedCompareExchange(ref location, immutableSegmentedDictionary2, immutableSegmentedDictionary);
			if (immutableSegmentedDictionary == immutableSegmentedDictionary3)
			{
				break;
			}
			immutableSegmentedDictionary = immutableSegmentedDictionary3;
		}
		return true;
	}

	public static bool Update<TKey, TValue, TArg>(ref ImmutableSegmentedDictionary<TKey, TValue> location, Func<ImmutableSegmentedDictionary<TKey, TValue>, TArg, ImmutableSegmentedDictionary<TKey, TValue>> transformer, TArg transformerArgument) where TKey : notnull
	{
		if (transformer == null)
		{
			throw new ArgumentNullException("transformer");
		}
		ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary = ImmutableSegmentedDictionary<TKey, TValue>.PrivateMarshal.VolatileRead(in location);
		while (true)
		{
			ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary2 = transformer(immutableSegmentedDictionary, transformerArgument);
			if (immutableSegmentedDictionary == immutableSegmentedDictionary2)
			{
				return false;
			}
			ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary3 = InterlockedCompareExchange(ref location, immutableSegmentedDictionary2, immutableSegmentedDictionary);
			if (immutableSegmentedDictionary == immutableSegmentedDictionary3)
			{
				break;
			}
			immutableSegmentedDictionary = immutableSegmentedDictionary3;
		}
		return true;
	}

	public static ImmutableSegmentedDictionary<TKey, TValue> InterlockedExchange<TKey, TValue>(ref ImmutableSegmentedDictionary<TKey, TValue> location, ImmutableSegmentedDictionary<TKey, TValue> value) where TKey : notnull
	{
		return ImmutableSegmentedDictionary<TKey, TValue>.PrivateMarshal.InterlockedExchange(ref location, value);
	}

	public static ImmutableSegmentedDictionary<TKey, TValue> InterlockedCompareExchange<TKey, TValue>(ref ImmutableSegmentedDictionary<TKey, TValue> location, ImmutableSegmentedDictionary<TKey, TValue> value, ImmutableSegmentedDictionary<TKey, TValue> comparand) where TKey : notnull
	{
		return ImmutableSegmentedDictionary<TKey, TValue>.PrivateMarshal.InterlockedCompareExchange(ref location, value, comparand);
	}

	public static bool InterlockedInitialize<TKey, TValue>(ref ImmutableSegmentedDictionary<TKey, TValue> location, ImmutableSegmentedDictionary<TKey, TValue> value) where TKey : notnull
	{
		return InterlockedCompareExchange(ref location, value, default(ImmutableSegmentedDictionary<TKey, TValue>)).IsDefault;
	}

	public static TValue GetOrAdd<TKey, TValue, TArg>(ref ImmutableSegmentedDictionary<TKey, TValue> location, TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument) where TKey : notnull
	{
		if (valueFactory == null)
		{
			throw new ArgumentNullException("valueFactory");
		}
		ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary = ImmutableSegmentedDictionary<TKey, TValue>.PrivateMarshal.VolatileRead(in location);
		if (immutableSegmentedDictionary.IsDefault)
		{
			throw new ArgumentNullException("location");
		}
		if (immutableSegmentedDictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		value = valueFactory(key, factoryArgument);
		return GetOrAdd(ref location, key, value);
	}

	public static TValue GetOrAdd<TKey, TValue>(ref ImmutableSegmentedDictionary<TKey, TValue> location, TKey key, Func<TKey, TValue> valueFactory) where TKey : notnull
	{
		if (valueFactory == null)
		{
			throw new ArgumentNullException("valueFactory");
		}
		ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary = ImmutableSegmentedDictionary<TKey, TValue>.PrivateMarshal.VolatileRead(in location);
		if (immutableSegmentedDictionary.IsDefault)
		{
			throw new ArgumentNullException("location");
		}
		if (immutableSegmentedDictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		value = valueFactory(key);
		return GetOrAdd(ref location, key, value);
	}

	public static TValue GetOrAdd<TKey, TValue>(ref ImmutableSegmentedDictionary<TKey, TValue> location, TKey key, TValue value) where TKey : notnull
	{
		ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary = ImmutableSegmentedDictionary<TKey, TValue>.PrivateMarshal.VolatileRead(in location);
		while (true)
		{
			if (immutableSegmentedDictionary.IsDefault)
			{
				throw new ArgumentNullException("location");
			}
			if (immutableSegmentedDictionary.TryGetValue(key, out var value2))
			{
				return value2;
			}
			ImmutableSegmentedDictionary<TKey, TValue> value3 = immutableSegmentedDictionary.Add(key, value);
			ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary2 = InterlockedCompareExchange(ref location, value3, immutableSegmentedDictionary);
			if (immutableSegmentedDictionary == immutableSegmentedDictionary2)
			{
				break;
			}
			immutableSegmentedDictionary = immutableSegmentedDictionary2;
		}
		return value;
	}

	public static TValue AddOrUpdate<TKey, TValue>(ref ImmutableSegmentedDictionary<TKey, TValue> location, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory) where TKey : notnull
	{
		if (addValueFactory == null)
		{
			throw new ArgumentNullException("addValueFactory");
		}
		if (updateValueFactory == null)
		{
			throw new ArgumentNullException("updateValueFactory");
		}
		ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary = ImmutableSegmentedDictionary<TKey, TValue>.PrivateMarshal.VolatileRead(in location);
		TValue val;
		while (true)
		{
			if (immutableSegmentedDictionary.IsDefault)
			{
				throw new ArgumentNullException("location");
			}
			val = ((!immutableSegmentedDictionary.TryGetValue(key, out var value)) ? addValueFactory(key) : updateValueFactory(key, value));
			ImmutableSegmentedDictionary<TKey, TValue> value2 = immutableSegmentedDictionary.SetItem(key, val);
			ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary2 = InterlockedCompareExchange(ref location, value2, immutableSegmentedDictionary);
			if (immutableSegmentedDictionary == immutableSegmentedDictionary2)
			{
				break;
			}
			immutableSegmentedDictionary = immutableSegmentedDictionary2;
		}
		return val;
	}

	public static TValue AddOrUpdate<TKey, TValue>(ref ImmutableSegmentedDictionary<TKey, TValue> location, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory) where TKey : notnull
	{
		if (updateValueFactory == null)
		{
			throw new ArgumentNullException("updateValueFactory");
		}
		ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary = ImmutableSegmentedDictionary<TKey, TValue>.PrivateMarshal.VolatileRead(in location);
		TValue val;
		while (true)
		{
			if (immutableSegmentedDictionary.IsDefault)
			{
				throw new ArgumentNullException("location");
			}
			val = (TValue)((!immutableSegmentedDictionary.TryGetValue(key, out var value)) ? ((object)addValue) : ((object)updateValueFactory(key, value)));
			ImmutableSegmentedDictionary<TKey, TValue> value2 = immutableSegmentedDictionary.SetItem(key, val);
			ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary2 = InterlockedCompareExchange(ref location, value2, immutableSegmentedDictionary);
			if (immutableSegmentedDictionary == immutableSegmentedDictionary2)
			{
				break;
			}
			immutableSegmentedDictionary = immutableSegmentedDictionary2;
		}
		return val;
	}

	public static bool TryAdd<TKey, TValue>(ref ImmutableSegmentedDictionary<TKey, TValue> location, TKey key, TValue value) where TKey : notnull
	{
		ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary = ImmutableSegmentedDictionary<TKey, TValue>.PrivateMarshal.VolatileRead(in location);
		while (true)
		{
			if (immutableSegmentedDictionary.IsDefault)
			{
				throw new ArgumentNullException("location");
			}
			if (immutableSegmentedDictionary.ContainsKey(key))
			{
				return false;
			}
			ImmutableSegmentedDictionary<TKey, TValue> value2 = immutableSegmentedDictionary.Add(key, value);
			ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary2 = InterlockedCompareExchange(ref location, value2, immutableSegmentedDictionary);
			if (immutableSegmentedDictionary == immutableSegmentedDictionary2)
			{
				break;
			}
			immutableSegmentedDictionary = immutableSegmentedDictionary2;
		}
		return true;
	}

	public static bool TryUpdate<TKey, TValue>(ref ImmutableSegmentedDictionary<TKey, TValue> location, TKey key, TValue newValue, TValue comparisonValue) where TKey : notnull
	{
		EqualityComparer<TValue> equalityComparer = EqualityComparer<TValue>.Default;
		ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary = ImmutableSegmentedDictionary<TKey, TValue>.PrivateMarshal.VolatileRead(in location);
		while (true)
		{
			if (immutableSegmentedDictionary.IsDefault)
			{
				throw new ArgumentNullException("location");
			}
			if (!immutableSegmentedDictionary.TryGetValue(key, out var value) || !equalityComparer.Equals(value, comparisonValue))
			{
				return false;
			}
			ImmutableSegmentedDictionary<TKey, TValue> value2 = immutableSegmentedDictionary.SetItem(key, newValue);
			ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary2 = InterlockedCompareExchange(ref location, value2, immutableSegmentedDictionary);
			if (immutableSegmentedDictionary == immutableSegmentedDictionary2)
			{
				break;
			}
			immutableSegmentedDictionary = immutableSegmentedDictionary2;
		}
		return true;
	}

	public static bool TryRemove<TKey, TValue>(ref ImmutableSegmentedDictionary<TKey, TValue> location, TKey key, [MaybeNullWhen(false)] out TValue value) where TKey : notnull
	{
		ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary = ImmutableSegmentedDictionary<TKey, TValue>.PrivateMarshal.VolatileRead(in location);
		while (true)
		{
			if (immutableSegmentedDictionary.IsDefault)
			{
				throw new ArgumentNullException("location");
			}
			if (!immutableSegmentedDictionary.TryGetValue(key, out value))
			{
				return false;
			}
			ImmutableSegmentedDictionary<TKey, TValue> value2 = immutableSegmentedDictionary.Remove(key);
			ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary2 = InterlockedCompareExchange(ref location, value2, immutableSegmentedDictionary);
			if (immutableSegmentedDictionary == immutableSegmentedDictionary2)
			{
				break;
			}
			immutableSegmentedDictionary = immutableSegmentedDictionary2;
		}
		return true;
	}

	public static ImmutableArray<T> VolatileRead<T>(ref readonly ImmutableArray<T> location)
	{
		ImmutableArray<T> result = location;
		Interlocked.MemoryBarrier();
		return result;
	}

	public static void VolatileWrite<T>(ref ImmutableArray<T> location, ImmutableArray<T> value)
	{
		Interlocked.MemoryBarrier();
		location = value;
	}
}
