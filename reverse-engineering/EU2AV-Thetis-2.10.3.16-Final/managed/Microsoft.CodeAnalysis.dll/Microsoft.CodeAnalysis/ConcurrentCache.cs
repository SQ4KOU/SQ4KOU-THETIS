using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis;

internal sealed class ConcurrentCache<TKey, TValue> : CachingBase<ConcurrentCache<TKey, TValue>.Entry> where TKey : notnull
{
	internal class Entry
	{
		internal readonly int hash;

		internal readonly TKey key;

		internal readonly TValue value;

		internal Entry(int hash, TKey key, TValue value)
		{
			this.hash = hash;
			this.key = key;
			this.value = value;
		}
	}

	private readonly IEqualityComparer<TKey> _keyComparer;

	public ConcurrentCache(int size, IEqualityComparer<TKey> keyComparer)
		: base(size, false)
	{
		_keyComparer = keyComparer;
	}

	public ConcurrentCache(int size)
		: this(size, (IEqualityComparer<TKey>)EqualityComparer<TKey>.Default)
	{
	}

	public bool TryAdd(TKey key, TValue value)
	{
		int hashCode = _keyComparer.GetHashCode(key);
		int num = hashCode & mask;
		Entry entry = base.Entries[num];
		if (entry != null && entry.hash == hashCode && _keyComparer.Equals(entry.key, key))
		{
			return false;
		}
		base.Entries[num] = new Entry(hashCode, key, value);
		return true;
	}

	public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
	{
		int hashCode = _keyComparer.GetHashCode(key);
		int num = hashCode & mask;
		Entry entry = base.Entries[num];
		if (entry != null && entry.hash == hashCode && _keyComparer.Equals(entry.key, key))
		{
			value = entry.value;
			return true;
		}
		value = default(TValue);
		return false;
	}
}
