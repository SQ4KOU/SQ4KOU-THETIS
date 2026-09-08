using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis.Collections.Internal;

namespace Microsoft.CodeAnalysis.Collections;

internal static class ImmutableSegmentedDictionary
{
	public static ImmutableSegmentedDictionary<TKey, TValue> Create<TKey, TValue>() where TKey : notnull
	{
		return ImmutableSegmentedDictionary<TKey, TValue>.Empty;
	}

	public static ImmutableSegmentedDictionary<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey>? keyComparer) where TKey : notnull
	{
		return ImmutableSegmentedDictionary<TKey, TValue>.Empty.WithComparer(keyComparer);
	}

	public static ImmutableSegmentedDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>() where TKey : notnull
	{
		return Create<TKey, TValue>().ToBuilder();
	}

	public static ImmutableSegmentedDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(IEqualityComparer<TKey>? keyComparer) where TKey : notnull
	{
		return Create<TKey, TValue>(keyComparer).ToBuilder();
	}

	public static ImmutableSegmentedDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> items) where TKey : notnull
	{
		return ImmutableSegmentedDictionary<TKey, TValue>.Empty.AddRange(items);
	}

	public static ImmutableSegmentedDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEqualityComparer<TKey>? keyComparer, IEnumerable<KeyValuePair<TKey, TValue>> items) where TKey : notnull
	{
		return ImmutableSegmentedDictionary<TKey, TValue>.Empty.WithComparer(keyComparer).AddRange(items);
	}

	public static ImmutableSegmentedDictionary<TKey, TValue> ToImmutableSegmentedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> items) where TKey : notnull
	{
		return items.ToImmutableSegmentedDictionary(null);
	}

	public static ImmutableSegmentedDictionary<TKey, TValue> ToImmutableSegmentedDictionary<TKey, TValue>(this ImmutableSegmentedDictionary<TKey, TValue>.Builder builder) where TKey : notnull
	{
		if (builder == null)
		{
			throw new ArgumentNullException("builder");
		}
		return builder.ToImmutable();
	}

	public static ImmutableSegmentedDictionary<TKey, TValue> ToImmutableSegmentedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> items, IEqualityComparer<TKey>? keyComparer) where TKey : notnull
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		if (items is ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary)
		{
			return immutableSegmentedDictionary.WithComparer(keyComparer);
		}
		return ImmutableSegmentedDictionary<TKey, TValue>.Empty.WithComparer(keyComparer).AddRange(items);
	}

	public static ImmutableSegmentedDictionary<TKey, TValue> ToImmutableSegmentedDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector) where TKey : notnull
	{
		return source.ToImmutableSegmentedDictionary(keySelector, elementSelector, null);
	}

	public static ImmutableSegmentedDictionary<TKey, TValue> ToImmutableSegmentedDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IEqualityComparer<TKey>? keyComparer) where TKey : notnull
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (elementSelector == null)
		{
			throw new ArgumentNullException("elementSelector");
		}
		return ImmutableSegmentedDictionary<TKey, TValue>.Empty.WithComparer(keyComparer).AddRange(source.Select((TSource element) => new KeyValuePair<TKey, TValue>(keySelector(element), elementSelector(element))));
	}

	public static ImmutableSegmentedDictionary<TKey, TSource> ToImmutableSegmentedDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : notnull
	{
		return source.ToImmutableSegmentedDictionary(keySelector, (TSource x) => x, null);
	}

	public static ImmutableSegmentedDictionary<TKey, TSource> ToImmutableSegmentedDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? keyComparer) where TKey : notnull
	{
		return source.ToImmutableSegmentedDictionary(keySelector, (TSource x) => x, keyComparer);
	}
}
internal readonly struct ImmutableSegmentedDictionary<TKey, TValue> : IImmutableDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEquatable<ImmutableSegmentedDictionary<TKey, TValue>> where TKey : notnull
{
	public sealed class Builder : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IDictionary, ICollection
	{
		public readonly struct KeyCollection : ICollection<TKey>, IEnumerable<TKey>, IEnumerable, IReadOnlyCollection<TKey>, ICollection
		{
			private readonly ImmutableSegmentedDictionary<TKey, TValue>.Builder _dictionary;

			public int Count => _dictionary.Count;

			bool ICollection<TKey>.IsReadOnly => false;

			bool ICollection.IsSynchronized => false;

			object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

			internal KeyCollection(ImmutableSegmentedDictionary<TKey, TValue>.Builder dictionary)
			{
				_dictionary = dictionary;
			}

			void ICollection<TKey>.Add(TKey item)
			{
				throw new NotSupportedException();
			}

			public void Clear()
			{
				_dictionary.Clear();
			}

			public bool Contains(TKey item)
			{
				return _dictionary.ContainsKey(item);
			}

			public void CopyTo(TKey[] array, int arrayIndex)
			{
				_dictionary.ReadOnlyDictionary.Keys.CopyTo(array, arrayIndex);
			}

			public ImmutableSegmentedDictionary<TKey, TValue>.KeyCollection.Enumerator GetEnumerator()
			{
				return new ImmutableSegmentedDictionary<TKey, TValue>.KeyCollection.Enumerator(_dictionary.GetEnumerator());
			}

			public bool Remove(TKey item)
			{
				return _dictionary.Remove(item);
			}

			void ICollection.CopyTo(Array array, int index)
			{
				((ICollection)_dictionary.ReadOnlyDictionary.Keys).CopyTo(array, index);
			}

			IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		internal static class PrivateMarshal
		{
			public static ref TValue FindValue(Builder dictionary, TKey key)
			{
				return ref SegmentedCollectionsMarshal.GetValueRefOrNullRef(dictionary._builder.GetOrCreateMutableDictionary(), key);
			}
		}

		public readonly struct ValueCollection : ICollection<TValue>, IEnumerable<TValue>, IEnumerable, IReadOnlyCollection<TValue>, ICollection
		{
			private readonly ImmutableSegmentedDictionary<TKey, TValue>.Builder _dictionary;

			public int Count => _dictionary.Count;

			bool ICollection<TValue>.IsReadOnly => false;

			bool ICollection.IsSynchronized => false;

			object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

			internal ValueCollection(ImmutableSegmentedDictionary<TKey, TValue>.Builder dictionary)
			{
				_dictionary = dictionary;
			}

			void ICollection<TValue>.Add(TValue item)
			{
				throw new NotSupportedException();
			}

			public void Clear()
			{
				_dictionary.Clear();
			}

			public bool Contains(TValue item)
			{
				return _dictionary.ContainsValue(item);
			}

			public void CopyTo(TValue[] array, int arrayIndex)
			{
				_dictionary.ReadOnlyDictionary.Values.CopyTo(array, arrayIndex);
			}

			public ImmutableSegmentedDictionary<TKey, TValue>.ValueCollection.Enumerator GetEnumerator()
			{
				return new ImmutableSegmentedDictionary<TKey, TValue>.ValueCollection.Enumerator(_dictionary.GetEnumerator());
			}

			bool ICollection<TValue>.Remove(TValue item)
			{
				throw new NotSupportedException();
			}

			void ICollection.CopyTo(Array array, int index)
			{
				((ICollection)_dictionary.ReadOnlyDictionary.Values).CopyTo(array, index);
			}

			IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		internal readonly struct TestAccessor(Builder instance)
		{
			internal SegmentedDictionary<TKey, TValue> GetOrCreateMutableDictionary()
			{
				return instance._builder.GetOrCreateMutableDictionary();
			}
		}

		private ValueBuilder _builder;

		public IEqualityComparer<TKey> KeyComparer
		{
			get
			{
				return _builder.KeyComparer;
			}
			set
			{
				_builder.KeyComparer = value;
			}
		}

		public int Count => _builder.Count;

		public KeyCollection Keys => new KeyCollection(this);

		public ValueCollection Values => new ValueCollection(this);

		private SegmentedDictionary<TKey, TValue> ReadOnlyDictionary => _builder.ReadOnlyDictionary;

		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

		ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

		ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => ICollectionCalls<KeyValuePair<TKey, TValue>>.IsReadOnly(ref _builder);

		ICollection IDictionary.Keys => Keys;

		ICollection IDictionary.Values => Values;

		bool IDictionary.IsReadOnly => IDictionaryCalls.IsReadOnly(ref _builder);

		bool IDictionary.IsFixedSize => IDictionaryCalls.IsFixedSize(ref _builder);

		object ICollection.SyncRoot => this;

		bool ICollection.IsSynchronized => ICollectionCalls.IsSynchronized(ref _builder);

		public TValue this[TKey key]
		{
			get
			{
				return _builder[key];
			}
			set
			{
				_builder[key] = value;
			}
		}

		object? IDictionary.this[object key]
		{
			get
			{
				return IDictionaryCalls.GetItem(ref _builder, key);
			}
			set
			{
				IDictionaryCalls.SetItem(ref _builder, key, value);
			}
		}

		internal Builder(ImmutableSegmentedDictionary<TKey, TValue> dictionary)
		{
			_builder = new ValueBuilder(dictionary);
		}

		public void Add(TKey key, TValue value)
		{
			_builder.Add(key, value);
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			_builder.Add(item);
		}

		public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
		{
			_builder.AddRange(items);
		}

		public void Clear()
		{
			_builder.Clear();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return _builder.Contains(item);
		}

		public bool ContainsKey(TKey key)
		{
			return _builder.ContainsKey(key);
		}

		public bool ContainsValue(TValue value)
		{
			return _builder.ContainsValue(value);
		}

		public Enumerator GetEnumerator()
		{
			return _builder.GetEnumerator();
		}

		public TValue? GetValueOrDefault(TKey key)
		{
			return _builder.GetValueOrDefault(key);
		}

		public TValue GetValueOrDefault(TKey key, TValue defaultValue)
		{
			return _builder.GetValueOrDefault(key, defaultValue);
		}

		public bool Remove(TKey key)
		{
			return _builder.Remove(key);
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			return _builder.Remove(item);
		}

		public void RemoveRange(IEnumerable<TKey> keys)
		{
			_builder.RemoveRange(keys);
		}

		public bool TryGetKey(TKey equalKey, out TKey actualKey)
		{
			return _builder.TryGetKey(equalKey, out actualKey);
		}

		public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
		{
			return _builder.TryGetValue(key, out value);
		}

		public ImmutableSegmentedDictionary<TKey, TValue> ToImmutable()
		{
			return _builder.ToImmutable();
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			ICollectionCalls<KeyValuePair<TKey, TValue>>.CopyTo(ref _builder, array, arrayIndex);
		}

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return IEnumerableCalls<KeyValuePair<TKey, TValue>>.GetEnumerator(ref _builder);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return IEnumerableCalls.GetEnumerator(ref _builder);
		}

		bool IDictionary.Contains(object key)
		{
			return IDictionaryCalls.Contains(ref _builder, key);
		}

		void IDictionary.Add(object key, object? value)
		{
			IDictionaryCalls.Add(ref _builder, key, value);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return IDictionaryCalls.GetEnumerator(ref _builder);
		}

		void IDictionary.Remove(object key)
		{
			IDictionaryCalls.Remove(ref _builder, key);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			ICollectionCalls.CopyTo(ref _builder, array, index);
		}

		internal TestAccessor GetTestAccessor()
		{
			return new TestAccessor(this);
		}
	}

	public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, IDisposable, IDictionaryEnumerator
	{
		internal enum ReturnType
		{
			KeyValuePair,
			DictionaryEntry
		}

		private readonly SegmentedDictionary<TKey, TValue> _dictionary;

		private readonly ReturnType _returnType;

		private SegmentedDictionary<TKey, TValue>.Enumerator _enumerator;

		public readonly KeyValuePair<TKey, TValue> Current => _enumerator.Current;

		readonly object IEnumerator.Current
		{
			get
			{
				if (_returnType != ReturnType.DictionaryEntry)
				{
					return Current;
				}
				return ((IDictionaryEnumerator)this).Entry;
			}
		}

		readonly DictionaryEntry IDictionaryEnumerator.Entry => new DictionaryEntry(Current.Key, Current.Value);

		readonly object IDictionaryEnumerator.Key => Current.Key;

		readonly object? IDictionaryEnumerator.Value => Current.Value;

		internal Enumerator(SegmentedDictionary<TKey, TValue> dictionary, ReturnType returnType)
		{
			_dictionary = dictionary;
			_returnType = returnType;
			_enumerator = dictionary.GetEnumerator();
		}

		public readonly void Dispose()
		{
			_enumerator.Dispose();
		}

		public bool MoveNext()
		{
			return _enumerator.MoveNext();
		}

		public void Reset()
		{
			_enumerator = _dictionary.GetEnumerator();
		}
	}

	public readonly struct KeyCollection : IReadOnlyCollection<TKey>, IEnumerable<TKey>, IEnumerable, ICollection<TKey>, ICollection
	{
		public struct Enumerator : IEnumerator<TKey>, IEnumerator, IDisposable
		{
			private ImmutableSegmentedDictionary<TKey, TValue>.Enumerator _enumerator;

			public readonly TKey Current => _enumerator.Current.Key;

			readonly object IEnumerator.Current => Current;

			internal Enumerator(ImmutableSegmentedDictionary<TKey, TValue>.Enumerator enumerator)
			{
				_enumerator = enumerator;
			}

			public readonly void Dispose()
			{
				_enumerator.Dispose();
			}

			public bool MoveNext()
			{
				return _enumerator.MoveNext();
			}

			public void Reset()
			{
				_enumerator.Reset();
			}
		}

		private readonly ImmutableSegmentedDictionary<TKey, TValue> _dictionary;

		public int Count => _dictionary.Count;

		bool ICollection<TKey>.IsReadOnly => true;

		bool ICollection.IsSynchronized => true;

		object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

		internal KeyCollection(ImmutableSegmentedDictionary<TKey, TValue> dictionary)
		{
			_dictionary = dictionary;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(_dictionary.GetEnumerator());
		}

		public bool Contains(TKey item)
		{
			return _dictionary.ContainsKey(item);
		}

		IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		void ICollection<TKey>.CopyTo(TKey[] array, int arrayIndex)
		{
			_dictionary._dictionary.Keys.CopyTo(array, arrayIndex);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)_dictionary._dictionary.Keys).CopyTo(array, index);
		}

		void ICollection<TKey>.Add(TKey item)
		{
			throw new NotSupportedException();
		}

		void ICollection<TKey>.Clear()
		{
			throw new NotSupportedException();
		}

		bool ICollection<TKey>.Remove(TKey item)
		{
			throw new NotSupportedException();
		}

		public bool All<TArg>(Func<TKey, TArg, bool> predicate, TArg arg)
		{
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					TKey current = enumerator.Current;
					if (!predicate(current, arg))
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	internal static class PrivateMarshal
	{
		internal static ImmutableSegmentedDictionary<TKey, TValue> VolatileRead(in ImmutableSegmentedDictionary<TKey, TValue> location)
		{
			SegmentedDictionary<TKey, TValue> segmentedDictionary = Volatile.Read(in Unsafe.AsRef(in location._dictionary));
			if (segmentedDictionary == null)
			{
				return default(ImmutableSegmentedDictionary<TKey, TValue>);
			}
			return new ImmutableSegmentedDictionary<TKey, TValue>(segmentedDictionary);
		}

		internal static ImmutableSegmentedDictionary<TKey, TValue> InterlockedExchange(ref ImmutableSegmentedDictionary<TKey, TValue> location, ImmutableSegmentedDictionary<TKey, TValue> value)
		{
			SegmentedDictionary<TKey, TValue> segmentedDictionary = Interlocked.Exchange(ref Unsafe.AsRef(in location._dictionary), value._dictionary);
			if (segmentedDictionary == null)
			{
				return default(ImmutableSegmentedDictionary<TKey, TValue>);
			}
			return new ImmutableSegmentedDictionary<TKey, TValue>(segmentedDictionary);
		}

		internal static ImmutableSegmentedDictionary<TKey, TValue> InterlockedCompareExchange(ref ImmutableSegmentedDictionary<TKey, TValue> location, ImmutableSegmentedDictionary<TKey, TValue> value, ImmutableSegmentedDictionary<TKey, TValue> comparand)
		{
			SegmentedDictionary<TKey, TValue> segmentedDictionary = Interlocked.CompareExchange(ref Unsafe.AsRef(in location._dictionary), value._dictionary, comparand._dictionary);
			if (segmentedDictionary == null)
			{
				return default(ImmutableSegmentedDictionary<TKey, TValue>);
			}
			return new ImmutableSegmentedDictionary<TKey, TValue>(segmentedDictionary);
		}

		public static ref readonly TValue FindValue(ImmutableSegmentedDictionary<TKey, TValue> dictionary, TKey key)
		{
			return ref SegmentedCollectionsMarshal.GetValueRefOrNullRef(dictionary._dictionary, key);
		}

		internal static ImmutableSegmentedDictionary<TKey, TValue> AsImmutableSegmentedDictionary(SegmentedDictionary<TKey, TValue>? dictionary)
		{
			if (dictionary == null)
			{
				return default(ImmutableSegmentedDictionary<TKey, TValue>);
			}
			return new ImmutableSegmentedDictionary<TKey, TValue>(dictionary);
		}

		internal static SegmentedDictionary<TKey, TValue>? AsSegmentedDictionary(ImmutableSegmentedDictionary<TKey, TValue> dictionary)
		{
			return dictionary._dictionary;
		}
	}

	private struct ValueBuilder : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IDictionary, ICollection
	{
		private ImmutableSegmentedDictionary<TKey, TValue> _dictionary;

		private SegmentedDictionary<TKey, TValue>? _mutableDictionary;

		public IEqualityComparer<TKey> KeyComparer
		{
			readonly get
			{
				return ReadOnlyDictionary.Comparer;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				if (value != KeyComparer)
				{
					SegmentedDictionary<TKey, TValue> readOnlyDictionary = ReadOnlyDictionary;
					_mutableDictionary = new SegmentedDictionary<TKey, TValue>(value);
					_dictionary = default(ImmutableSegmentedDictionary<TKey, TValue>);
					AddRange(readOnlyDictionary);
				}
			}
		}

		public readonly int Count => ReadOnlyDictionary.Count;

		internal readonly SegmentedDictionary<TKey, TValue> ReadOnlyDictionary => _mutableDictionary ?? _dictionary._dictionary;

		readonly IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		readonly IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		readonly ICollection<TKey> IDictionary<TKey, TValue>.Keys
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		readonly ICollection<TValue> IDictionary<TKey, TValue>.Values
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		readonly bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

		readonly ICollection IDictionary.Keys
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		readonly ICollection IDictionary.Values
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		readonly bool IDictionary.IsReadOnly => false;

		readonly bool IDictionary.IsFixedSize => false;

		readonly object ICollection.SyncRoot
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		readonly bool ICollection.IsSynchronized => false;

		public TValue this[TKey key]
		{
			readonly get
			{
				return ReadOnlyDictionary[key];
			}
			set
			{
				GetOrCreateMutableDictionary()[key] = value;
			}
		}

		object? IDictionary.this[object key]
		{
			readonly get
			{
				return ((IDictionary)ReadOnlyDictionary)[key];
			}
			set
			{
				((IDictionary)GetOrCreateMutableDictionary())[key] = value;
			}
		}

		internal ValueBuilder(ImmutableSegmentedDictionary<TKey, TValue> dictionary)
		{
			_dictionary = dictionary;
			_mutableDictionary = null;
		}

		internal SegmentedDictionary<TKey, TValue> GetOrCreateMutableDictionary()
		{
			if (_mutableDictionary == null)
			{
				ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary = RoslynImmutableInterlocked.InterlockedExchange(ref _dictionary, default(ImmutableSegmentedDictionary<TKey, TValue>));
				if (immutableSegmentedDictionary.IsDefault)
				{
					throw new InvalidOperationException($"Unexpected concurrent access to {GetType()}");
				}
				_mutableDictionary = new SegmentedDictionary<TKey, TValue>(immutableSegmentedDictionary._dictionary, immutableSegmentedDictionary.KeyComparer);
			}
			return _mutableDictionary;
		}

		public void Add(TKey key, TValue value)
		{
			if (!Contains(new KeyValuePair<TKey, TValue>(key, value)))
			{
				GetOrCreateMutableDictionary().Add(key, value);
			}
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
		{
			if (items == null)
			{
				throw new ArgumentNullException("items");
			}
			foreach (KeyValuePair<TKey, TValue> item in items)
			{
				Add(item.Key, item.Value);
			}
		}

		public void Clear()
		{
			if (ReadOnlyDictionary.Count != 0)
			{
				if (_mutableDictionary == null)
				{
					_mutableDictionary = new SegmentedDictionary<TKey, TValue>(KeyComparer);
					_dictionary = default(ImmutableSegmentedDictionary<TKey, TValue>);
				}
				else
				{
					_mutableDictionary.Clear();
				}
			}
		}

		public readonly bool Contains(KeyValuePair<TKey, TValue> item)
		{
			if (TryGetValue(item.Key, out var value))
			{
				return EqualityComparer<TValue>.Default.Equals(value, item.Value);
			}
			return false;
		}

		public readonly bool ContainsKey(TKey key)
		{
			return ReadOnlyDictionary.ContainsKey(key);
		}

		public readonly bool ContainsValue(TValue value)
		{
			return ReadOnlyDictionary.ContainsValue(value);
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(GetOrCreateMutableDictionary(), Enumerator.ReturnType.KeyValuePair);
		}

		public readonly TValue? GetValueOrDefault(TKey key)
		{
			if (TryGetValue(key, out var value))
			{
				return value;
			}
			return default(TValue);
		}

		public readonly TValue GetValueOrDefault(TKey key, TValue defaultValue)
		{
			if (TryGetValue(key, out var value))
			{
				return value;
			}
			return defaultValue;
		}

		public bool Remove(TKey key)
		{
			if (_mutableDictionary == null && !ContainsKey(key))
			{
				return false;
			}
			return GetOrCreateMutableDictionary().Remove(key);
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			if (!Contains(item))
			{
				return false;
			}
			GetOrCreateMutableDictionary().Remove(item.Key);
			return true;
		}

		public void RemoveRange(IEnumerable<TKey> keys)
		{
			if (keys == null)
			{
				throw new ArgumentNullException("keys");
			}
			foreach (TKey key in keys)
			{
				Remove(key);
			}
		}

		public bool TryGetKey(TKey equalKey, out TKey actualKey)
		{
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<TKey, TValue> current = enumerator.Current;
					if (KeyComparer.Equals(current.Key, equalKey))
					{
						actualKey = current.Key;
						return true;
					}
				}
			}
			actualKey = equalKey;
			return false;
		}

		public readonly bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
		{
			return ReadOnlyDictionary.TryGetValue(key, out value);
		}

		public ImmutableSegmentedDictionary<TKey, TValue> ToImmutable()
		{
			_dictionary = new ImmutableSegmentedDictionary<TKey, TValue>(ReadOnlyDictionary);
			_mutableDictionary = null;
			return _dictionary;
		}

		readonly void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<TKey, TValue>>)ReadOnlyDictionary).CopyTo(array, arrayIndex);
		}

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return new Enumerator(GetOrCreateMutableDictionary(), Enumerator.ReturnType.KeyValuePair);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(GetOrCreateMutableDictionary(), Enumerator.ReturnType.KeyValuePair);
		}

		readonly bool IDictionary.Contains(object key)
		{
			return ((IDictionary)ReadOnlyDictionary).Contains(key);
		}

		void IDictionary.Add(object key, object? value)
		{
			((IDictionary)GetOrCreateMutableDictionary()).Add(key, value);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return new Enumerator(GetOrCreateMutableDictionary(), Enumerator.ReturnType.DictionaryEntry);
		}

		void IDictionary.Remove(object key)
		{
			((IDictionary)GetOrCreateMutableDictionary()).Remove(key);
		}

		readonly void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)ReadOnlyDictionary).CopyTo(array, index);
		}
	}

	public readonly struct ValueCollection : IReadOnlyCollection<TValue>, IEnumerable<TValue>, IEnumerable, ICollection<TValue>, ICollection
	{
		public struct Enumerator : IEnumerator<TValue>, IEnumerator, IDisposable
		{
			private ImmutableSegmentedDictionary<TKey, TValue>.Enumerator _enumerator;

			public readonly TValue Current => _enumerator.Current.Value;

			readonly object? IEnumerator.Current => Current;

			internal Enumerator(ImmutableSegmentedDictionary<TKey, TValue>.Enumerator enumerator)
			{
				_enumerator = enumerator;
			}

			public readonly void Dispose()
			{
				_enumerator.Dispose();
			}

			public bool MoveNext()
			{
				return _enumerator.MoveNext();
			}

			public void Reset()
			{
				_enumerator.Reset();
			}
		}

		private readonly ImmutableSegmentedDictionary<TKey, TValue> _dictionary;

		public int Count => _dictionary.Count;

		bool ICollection<TValue>.IsReadOnly => true;

		bool ICollection.IsSynchronized => true;

		object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

		internal ValueCollection(ImmutableSegmentedDictionary<TKey, TValue> dictionary)
		{
			_dictionary = dictionary;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(_dictionary.GetEnumerator());
		}

		public bool Contains(TValue item)
		{
			return _dictionary.ContainsValue(item);
		}

		IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex)
		{
			_dictionary._dictionary.Values.CopyTo(array, arrayIndex);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)_dictionary._dictionary.Values).CopyTo(array, index);
		}

		void ICollection<TValue>.Add(TValue item)
		{
			throw new NotSupportedException();
		}

		void ICollection<TValue>.Clear()
		{
			throw new NotSupportedException();
		}

		bool ICollection<TValue>.Remove(TValue item)
		{
			throw new NotSupportedException();
		}

		public bool All<TArg>(Func<TValue, TArg, bool> predicate, TArg arg)
		{
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					TValue current = enumerator.Current;
					if (!predicate(current, arg))
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public static readonly ImmutableSegmentedDictionary<TKey, TValue> Empty = new ImmutableSegmentedDictionary<TKey, TValue>(new SegmentedDictionary<TKey, TValue>());

	private readonly SegmentedDictionary<TKey, TValue> _dictionary;

	public IEqualityComparer<TKey> KeyComparer => _dictionary.Comparer;

	public int Count => _dictionary.Count;

	public bool IsEmpty => _dictionary.Count == 0;

	public bool IsDefault => _dictionary == null;

	public bool IsDefaultOrEmpty
	{
		get
		{
			int? num = _dictionary?.Count;
			if (!num.HasValue || num.GetValueOrDefault() == 0)
			{
				return true;
			}
			return false;
		}
	}

	public KeyCollection Keys => new KeyCollection(this);

	public ValueCollection Values => new ValueCollection(this);

	ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

	ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

	IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

	bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => true;

	ICollection IDictionary.Keys => Keys;

	ICollection IDictionary.Values => Values;

	bool IDictionary.IsReadOnly => true;

	bool IDictionary.IsFixedSize => true;

	object ICollection.SyncRoot => _dictionary;

	bool ICollection.IsSynchronized => true;

	public TValue this[TKey key] => _dictionary[key];

	TValue IDictionary<TKey, TValue>.this[TKey key]
	{
		get
		{
			return this[key];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	object? IDictionary.this[object key]
	{
		get
		{
			return ((IDictionary)_dictionary)[key];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	private ImmutableSegmentedDictionary(SegmentedDictionary<TKey, TValue> dictionary)
	{
		_dictionary = dictionary ?? throw new ArgumentNullException("dictionary");
	}

	public static bool operator ==(ImmutableSegmentedDictionary<TKey, TValue> left, ImmutableSegmentedDictionary<TKey, TValue> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ImmutableSegmentedDictionary<TKey, TValue> left, ImmutableSegmentedDictionary<TKey, TValue> right)
	{
		return !left.Equals(right);
	}

	public static bool operator ==(ImmutableSegmentedDictionary<TKey, TValue>? left, ImmutableSegmentedDictionary<TKey, TValue>? right)
	{
		return left.GetValueOrDefault().Equals(right.GetValueOrDefault());
	}

	public static bool operator !=(ImmutableSegmentedDictionary<TKey, TValue>? left, ImmutableSegmentedDictionary<TKey, TValue>? right)
	{
		return !left.GetValueOrDefault().Equals(right.GetValueOrDefault());
	}

	public ImmutableSegmentedDictionary<TKey, TValue> Add(TKey key, TValue value)
	{
		ImmutableSegmentedDictionary<TKey, TValue> result = this;
		if (result.Contains(new KeyValuePair<TKey, TValue>(key, value)))
		{
			return result;
		}
		ValueBuilder valueBuilder = ToValueBuilder();
		valueBuilder.Add(key, value);
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
	{
		ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary = this;
		if (immutableSegmentedDictionary.IsEmpty && TryCastToImmutableSegmentedDictionary(pairs, out var other) && immutableSegmentedDictionary.KeyComparer == other.KeyComparer)
		{
			return other;
		}
		ValueBuilder valueBuilder = ToValueBuilder();
		valueBuilder.AddRange(pairs);
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedDictionary<TKey, TValue> Clear()
	{
		ImmutableSegmentedDictionary<TKey, TValue> result = this;
		if (result.IsEmpty)
		{
			return result;
		}
		return Empty.WithComparer(result.KeyComparer);
	}

	public bool Contains(KeyValuePair<TKey, TValue> pair)
	{
		if (TryGetValue(pair.Key, out var value))
		{
			return EqualityComparer<TValue>.Default.Equals(value, pair.Value);
		}
		return false;
	}

	public bool ContainsKey(TKey key)
	{
		return _dictionary.ContainsKey(key);
	}

	public bool ContainsValue(TValue value)
	{
		return _dictionary.ContainsValue(value);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(_dictionary, Enumerator.ReturnType.KeyValuePair);
	}

	public ImmutableSegmentedDictionary<TKey, TValue> Remove(TKey key)
	{
		ImmutableSegmentedDictionary<TKey, TValue> result = this;
		if (!result._dictionary.ContainsKey(key))
		{
			return result;
		}
		ValueBuilder valueBuilder = ToValueBuilder();
		valueBuilder.Remove(key);
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
	{
		if (keys == null)
		{
			throw new ArgumentNullException("keys");
		}
		ValueBuilder valueBuilder = ToValueBuilder();
		valueBuilder.RemoveRange(keys);
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedDictionary<TKey, TValue> SetItem(TKey key, TValue value)
	{
		ImmutableSegmentedDictionary<TKey, TValue> result = this;
		if (result.Contains(new KeyValuePair<TKey, TValue>(key, value)))
		{
			return result;
		}
		ValueBuilder valueBuilder = ToValueBuilder();
		valueBuilder[key] = value;
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedDictionary<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		ValueBuilder valueBuilder = ToValueBuilder();
		foreach (KeyValuePair<TKey, TValue> item in items)
		{
			valueBuilder[item.Key] = item.Value;
		}
		return valueBuilder.ToImmutable();
	}

	public bool TryGetKey(TKey equalKey, out TKey actualKey)
	{
		ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary = this;
		foreach (TKey key in immutableSegmentedDictionary.Keys)
		{
			if (immutableSegmentedDictionary.KeyComparer.Equals(key, equalKey))
			{
				actualKey = key;
				return true;
			}
		}
		actualKey = equalKey;
		return false;
	}

	public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
	{
		return _dictionary.TryGetValue(key, out value);
	}

	public ImmutableSegmentedDictionary<TKey, TValue> WithComparer(IEqualityComparer<TKey>? keyComparer)
	{
		if (keyComparer == null)
		{
			keyComparer = EqualityComparer<TKey>.Default;
		}
		ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary = this;
		if (immutableSegmentedDictionary.KeyComparer == keyComparer)
		{
			return immutableSegmentedDictionary;
		}
		if (immutableSegmentedDictionary.IsEmpty)
		{
			if (keyComparer == Empty.KeyComparer)
			{
				return Empty;
			}
			return new ImmutableSegmentedDictionary<TKey, TValue>(new SegmentedDictionary<TKey, TValue>(keyComparer));
		}
		return ImmutableSegmentedDictionary.CreateRange(keyComparer, immutableSegmentedDictionary);
	}

	public Builder ToBuilder()
	{
		return new Builder(this);
	}

	private ValueBuilder ToValueBuilder()
	{
		return new ValueBuilder(this);
	}

	public override int GetHashCode()
	{
		return _dictionary?.GetHashCode() ?? 0;
	}

	public override bool Equals(object? obj)
	{
		if (obj is ImmutableSegmentedDictionary<TKey, TValue> other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(ImmutableSegmentedDictionary<TKey, TValue> other)
	{
		return _dictionary == other._dictionary;
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Clear()
	{
		return Clear();
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Add(TKey key, TValue value)
	{
		return Add(key, value);
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
	{
		return AddRange(pairs);
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItem(TKey key, TValue value)
	{
		return SetItem(key, value);
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
	{
		return SetItems(items);
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.RemoveRange(IEnumerable<TKey> keys)
	{
		return RemoveRange(keys);
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Remove(TKey key)
	{
		return Remove(key);
	}

	IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
	{
		return new Enumerator(_dictionary, Enumerator.ReturnType.KeyValuePair);
	}

	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return new Enumerator(_dictionary, Enumerator.ReturnType.DictionaryEntry);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(_dictionary, Enumerator.ReturnType.KeyValuePair);
	}

	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
	}

	bool IDictionary.Contains(object key)
	{
		return ((IDictionary)_dictionary).Contains(key);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		((ICollection)_dictionary).CopyTo(array, index);
	}

	void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
	{
		throw new NotSupportedException();
	}

	bool IDictionary<TKey, TValue>.Remove(TKey key)
	{
		throw new NotSupportedException();
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
	{
		throw new NotSupportedException();
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Clear()
	{
		throw new NotSupportedException();
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
	{
		throw new NotSupportedException();
	}

	void IDictionary.Add(object key, object? value)
	{
		throw new NotSupportedException();
	}

	void IDictionary.Clear()
	{
		throw new NotSupportedException();
	}

	void IDictionary.Remove(object key)
	{
		throw new NotSupportedException();
	}

	private static bool TryCastToImmutableSegmentedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> pairs, out ImmutableSegmentedDictionary<TKey, TValue> other)
	{
		if (pairs is ImmutableSegmentedDictionary<TKey, TValue> immutableSegmentedDictionary)
		{
			other = immutableSegmentedDictionary;
			return true;
		}
		if (pairs is Builder builder)
		{
			other = builder.ToImmutable();
			return true;
		}
		other = default(ImmutableSegmentedDictionary<TKey, TValue>);
		return false;
	}
}
