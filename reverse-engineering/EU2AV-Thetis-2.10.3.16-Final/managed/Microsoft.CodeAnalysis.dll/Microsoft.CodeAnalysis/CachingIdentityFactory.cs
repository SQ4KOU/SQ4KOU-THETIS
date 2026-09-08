using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal class CachingIdentityFactory<TKey, TValue> : CachingBase<CachingIdentityFactory<TKey, TValue>.Entry> where TKey : class
{
	internal struct Entry
	{
		internal TKey key;

		internal TValue value;
	}

	private readonly Func<TKey, TValue> _valueFactory;

	private readonly ObjectPool<CachingIdentityFactory<TKey, TValue>>? _pool;

	public CachingIdentityFactory(int size, Func<TKey, TValue> valueFactory)
		: base(size, true)
	{
		_valueFactory = valueFactory;
	}

	public CachingIdentityFactory(int size, Func<TKey, TValue> valueFactory, ObjectPool<CachingIdentityFactory<TKey, TValue>> pool)
		: this(size, valueFactory)
	{
		_pool = pool;
	}

	public void Add(TKey key, TValue value)
	{
		int num = RuntimeHelpers.GetHashCode(key) & mask;
		base.Entries[num].key = key;
		base.Entries[num].value = value;
	}

	public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
	{
		int num = RuntimeHelpers.GetHashCode(key) & mask;
		Entry[] entries = base.Entries;
		if (entries[num].key == key)
		{
			value = entries[num].value;
			return true;
		}
		value = default(TValue);
		return false;
	}

	public TValue GetOrMakeValue(TKey key)
	{
		int num = RuntimeHelpers.GetHashCode(key) & mask;
		Entry[] entries = base.Entries;
		if (entries[num].key == key)
		{
			return entries[num].value;
		}
		TValue val = _valueFactory(key);
		entries[num].key = key;
		entries[num].value = val;
		return val;
	}

	public static ObjectPool<CachingIdentityFactory<TKey, TValue>> CreatePool(int size, Func<TKey, TValue> valueFactory)
	{
		return new ObjectPool<CachingIdentityFactory<TKey, TValue>>((ObjectPool<CachingIdentityFactory<TKey, TValue>> pool) => new CachingIdentityFactory<TKey, TValue>(size, valueFactory, pool), Environment.ProcessorCount * 2);
	}

	public void Free()
	{
		_pool?.Free(this);
	}
}
