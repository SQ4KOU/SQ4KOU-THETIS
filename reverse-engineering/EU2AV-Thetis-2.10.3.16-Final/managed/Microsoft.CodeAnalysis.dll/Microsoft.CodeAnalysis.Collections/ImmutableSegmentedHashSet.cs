using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis.Collections.Internal;

namespace Microsoft.CodeAnalysis.Collections;

internal static class ImmutableSegmentedHashSet
{
	public static ImmutableSegmentedHashSet<T> Create<T>()
	{
		return ImmutableSegmentedHashSet<T>.Empty;
	}

	public static ImmutableSegmentedHashSet<T> Create<T>(T item)
	{
		return ImmutableSegmentedHashSet<T>.Empty.Add(item);
	}

	public static ImmutableSegmentedHashSet<T> Create<T>(params T[] items)
	{
		return ImmutableSegmentedHashSet<T>.Empty.Union(items);
	}

	public static ImmutableSegmentedHashSet<T> Create<T>(IEqualityComparer<T>? equalityComparer)
	{
		return ImmutableSegmentedHashSet<T>.Empty.WithComparer(equalityComparer);
	}

	public static ImmutableSegmentedHashSet<T> Create<T>(IEqualityComparer<T>? equalityComparer, T item)
	{
		return ImmutableSegmentedHashSet<T>.Empty.WithComparer(equalityComparer).Add(item);
	}

	public static ImmutableSegmentedHashSet<T> Create<T>(IEqualityComparer<T>? equalityComparer, params T[] items)
	{
		return ImmutableSegmentedHashSet<T>.Empty.WithComparer(equalityComparer).Union(items);
	}

	public static ImmutableSegmentedHashSet<T>.Builder CreateBuilder<T>()
	{
		return ImmutableSegmentedHashSet<T>.Empty.ToBuilder();
	}

	public static ImmutableSegmentedHashSet<T>.Builder CreateBuilder<T>(IEqualityComparer<T>? equalityComparer)
	{
		return ImmutableSegmentedHashSet<T>.Empty.WithComparer(equalityComparer).ToBuilder();
	}

	public static ImmutableSegmentedHashSet<T> CreateRange<T>(IEnumerable<T> items)
	{
		if (items is ImmutableSegmentedHashSet<T> immutableSegmentedHashSet)
		{
			return immutableSegmentedHashSet.WithComparer(null);
		}
		return ImmutableSegmentedHashSet<T>.Empty.Union(items);
	}

	public static ImmutableSegmentedHashSet<T> CreateRange<T>(IEqualityComparer<T>? equalityComparer, IEnumerable<T> items)
	{
		if (items is ImmutableSegmentedHashSet<T> immutableSegmentedHashSet)
		{
			return immutableSegmentedHashSet.WithComparer(equalityComparer);
		}
		return ImmutableSegmentedHashSet<T>.Empty.WithComparer(equalityComparer).Union(items);
	}

	public static ImmutableSegmentedHashSet<TSource> ToImmutableSegmentedHashSet<TSource>(this IEnumerable<TSource> source)
	{
		if (source is ImmutableSegmentedHashSet<TSource> immutableSegmentedHashSet)
		{
			return immutableSegmentedHashSet.WithComparer(null);
		}
		return ImmutableSegmentedHashSet<TSource>.Empty.Union(source);
	}

	public static ImmutableSegmentedHashSet<TSource> ToImmutableSegmentedHashSet<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource>? equalityComparer)
	{
		if (source is ImmutableSegmentedHashSet<TSource> immutableSegmentedHashSet)
		{
			return immutableSegmentedHashSet.WithComparer(equalityComparer);
		}
		return ImmutableSegmentedHashSet<TSource>.Empty.WithComparer(equalityComparer).Union(source);
	}

	public static ImmutableSegmentedHashSet<TSource> ToImmutableSegmentedHashSet<TSource>(this ImmutableSegmentedHashSet<TSource>.Builder builder)
	{
		if (builder == null)
		{
			throw new ArgumentNullException("builder");
		}
		return builder.ToImmutable();
	}
}
internal readonly struct ImmutableSegmentedHashSet<T> : IImmutableSet<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, ISet<T>, ICollection<T>, ICollection, IEquatable<ImmutableSegmentedHashSet<T>>
{
	public sealed class Builder : ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
	{
		private ValueBuilder _builder;

		public IEqualityComparer<T> KeyComparer
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

		bool ICollection<T>.IsReadOnly => ICollectionCalls<T>.IsReadOnly(ref _builder);

		internal Builder(ImmutableSegmentedHashSet<T> set)
		{
			_builder = new ValueBuilder(set);
		}

		public bool Add(T item)
		{
			return _builder.Add(item);
		}

		public void Clear()
		{
			_builder.Clear();
		}

		public bool Contains(T item)
		{
			return _builder.Contains(item);
		}

		public void ExceptWith(IEnumerable<T> other)
		{
			if (other == this)
			{
				_builder.ExceptWith(_builder.ReadOnlySet);
			}
			else
			{
				_builder.ExceptWith(other);
			}
		}

		public Enumerator GetEnumerator()
		{
			return _builder.GetEnumerator();
		}

		public void IntersectWith(IEnumerable<T> other)
		{
			_builder.IntersectWith(other);
		}

		public bool IsProperSubsetOf(IEnumerable<T> other)
		{
			return _builder.IsProperSubsetOf(other);
		}

		public bool IsProperSupersetOf(IEnumerable<T> other)
		{
			return _builder.IsProperSupersetOf(other);
		}

		public bool IsSubsetOf(IEnumerable<T> other)
		{
			return _builder.IsSubsetOf(other);
		}

		public bool IsSupersetOf(IEnumerable<T> other)
		{
			return _builder.IsSupersetOf(other);
		}

		public bool Overlaps(IEnumerable<T> other)
		{
			return _builder.Overlaps(other);
		}

		public bool Remove(T item)
		{
			return _builder.Remove(item);
		}

		public bool SetEquals(IEnumerable<T> other)
		{
			return _builder.SetEquals(other);
		}

		public void SymmetricExceptWith(IEnumerable<T> other)
		{
			_builder.SymmetricExceptWith(other);
		}

		public bool TryGetValue(T equalValue, out T actualValue)
		{
			return _builder.TryGetValue(equalValue, out actualValue);
		}

		public void UnionWith(IEnumerable<T> other)
		{
			_builder.UnionWith(other);
		}

		public ImmutableSegmentedHashSet<T> ToImmutable()
		{
			return _builder.ToImmutable();
		}

		void ICollection<T>.Add(T item)
		{
			ICollectionCalls<T>.Add(ref _builder, item);
		}

		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			ICollectionCalls<T>.CopyTo(ref _builder, array, arrayIndex);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return IEnumerableCalls<T>.GetEnumerator(ref _builder);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return IEnumerableCalls.GetEnumerator(ref _builder);
		}
	}

	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly SegmentedHashSet<T> _set;

		private SegmentedHashSet<T>.Enumerator _enumerator;

		public readonly T Current => _enumerator.Current;

		readonly object? IEnumerator.Current => ((IEnumerator)_enumerator).Current;

		internal Enumerator(SegmentedHashSet<T> set)
		{
			_set = set;
			_enumerator = set.GetEnumerator();
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
			_enumerator = _set.GetEnumerator();
		}
	}

	internal static class PrivateMarshal
	{
		internal static ImmutableSegmentedHashSet<T> VolatileRead(in ImmutableSegmentedHashSet<T> location)
		{
			SegmentedHashSet<T> segmentedHashSet = Volatile.Read(in Unsafe.AsRef(in location._set));
			if (segmentedHashSet == null)
			{
				return default(ImmutableSegmentedHashSet<T>);
			}
			return new ImmutableSegmentedHashSet<T>(segmentedHashSet);
		}

		internal static ImmutableSegmentedHashSet<T> InterlockedExchange(ref ImmutableSegmentedHashSet<T> location, ImmutableSegmentedHashSet<T> value)
		{
			SegmentedHashSet<T> segmentedHashSet = Interlocked.Exchange(ref Unsafe.AsRef(in location._set), value._set);
			if (segmentedHashSet == null)
			{
				return default(ImmutableSegmentedHashSet<T>);
			}
			return new ImmutableSegmentedHashSet<T>(segmentedHashSet);
		}

		internal static ImmutableSegmentedHashSet<T> InterlockedCompareExchange(ref ImmutableSegmentedHashSet<T> location, ImmutableSegmentedHashSet<T> value, ImmutableSegmentedHashSet<T> comparand)
		{
			SegmentedHashSet<T> segmentedHashSet = Interlocked.CompareExchange(ref Unsafe.AsRef(in location._set), value._set, comparand._set);
			if (segmentedHashSet == null)
			{
				return default(ImmutableSegmentedHashSet<T>);
			}
			return new ImmutableSegmentedHashSet<T>(segmentedHashSet);
		}

		internal static ImmutableSegmentedHashSet<T> AsImmutableSegmentedHashSet(SegmentedHashSet<T>? set)
		{
			if (set == null)
			{
				return default(ImmutableSegmentedHashSet<T>);
			}
			return new ImmutableSegmentedHashSet<T>(set);
		}

		internal static SegmentedHashSet<T>? AsSegmentedHashSet(ImmutableSegmentedHashSet<T> set)
		{
			return set._set;
		}
	}

	private struct ValueBuilder : ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
	{
		private ImmutableSegmentedHashSet<T> _set;

		private SegmentedHashSet<T>? _mutableSet;

		public IEqualityComparer<T> KeyComparer
		{
			readonly get
			{
				return ReadOnlySet.Comparer;
			}
			set
			{
				if (!object.Equals(KeyComparer, value ?? EqualityComparer<T>.Default))
				{
					_mutableSet = new SegmentedHashSet<T>(ReadOnlySet, value ?? EqualityComparer<T>.Default);
					_set = default(ImmutableSegmentedHashSet<T>);
				}
			}
		}

		public readonly int Count => ReadOnlySet.Count;

		internal readonly SegmentedHashSet<T> ReadOnlySet => _mutableSet ?? _set._set;

		readonly bool ICollection<T>.IsReadOnly => false;

		internal ValueBuilder(ImmutableSegmentedHashSet<T> set)
		{
			_set = set;
			_mutableSet = null;
		}

		private SegmentedHashSet<T> GetOrCreateMutableSet()
		{
			if (_mutableSet == null)
			{
				ImmutableSegmentedHashSet<T> immutableSegmentedHashSet = RoslynImmutableInterlocked.InterlockedExchange(ref _set, default(ImmutableSegmentedHashSet<T>));
				if (immutableSegmentedHashSet.IsDefault)
				{
					throw new InvalidOperationException($"Unexpected concurrent access to {GetType()}");
				}
				_mutableSet = new SegmentedHashSet<T>(immutableSegmentedHashSet._set, immutableSegmentedHashSet.KeyComparer);
			}
			return _mutableSet;
		}

		public bool Add(T item)
		{
			if (_mutableSet == null && Contains(item))
			{
				return false;
			}
			return GetOrCreateMutableSet().Add(item);
		}

		public void Clear()
		{
			if (ReadOnlySet.Count != 0)
			{
				if (_mutableSet == null)
				{
					_mutableSet = new SegmentedHashSet<T>(KeyComparer);
					_set = default(ImmutableSegmentedHashSet<T>);
				}
				else
				{
					_mutableSet.Clear();
				}
			}
		}

		public readonly bool Contains(T item)
		{
			return ReadOnlySet.Contains(item);
		}

		public void ExceptWith(IEnumerable<T> other)
		{
			if (other == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other);
			}
			if (_mutableSet != null)
			{
				_mutableSet.ExceptWith(other);
				return;
			}
			if (other == ReadOnlySet)
			{
				Clear();
				return;
			}
			if (other is ImmutableSegmentedHashSet<T> immutableSegmentedHashSet)
			{
				if (immutableSegmentedHashSet == _set)
				{
					Clear();
				}
				else if (!immutableSegmentedHashSet.IsEmpty)
				{
					GetOrCreateMutableSet().ExceptWith(immutableSegmentedHashSet._set);
				}
				return;
			}
			SegmentedHashSet<T> segmentedHashSet = null;
			foreach (T item in other)
			{
				if (segmentedHashSet == null)
				{
					if (!ReadOnlySet.Contains(item))
					{
						continue;
					}
					segmentedHashSet = GetOrCreateMutableSet();
				}
				segmentedHashSet.Remove(item);
			}
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(GetOrCreateMutableSet());
		}

		public void IntersectWith(IEnumerable<T> other)
		{
			GetOrCreateMutableSet().IntersectWith(other);
		}

		public readonly bool IsProperSubsetOf(IEnumerable<T> other)
		{
			return ReadOnlySet.IsProperSubsetOf(other);
		}

		public readonly bool IsProperSupersetOf(IEnumerable<T> other)
		{
			return ReadOnlySet.IsProperSupersetOf(other);
		}

		public readonly bool IsSubsetOf(IEnumerable<T> other)
		{
			return ReadOnlySet.IsSubsetOf(other);
		}

		public readonly bool IsSupersetOf(IEnumerable<T> other)
		{
			return ReadOnlySet.IsSupersetOf(other);
		}

		public readonly bool Overlaps(IEnumerable<T> other)
		{
			return ReadOnlySet.Overlaps(other);
		}

		public bool Remove(T item)
		{
			if (_mutableSet == null && !Contains(item))
			{
				return false;
			}
			return GetOrCreateMutableSet().Remove(item);
		}

		public readonly bool SetEquals(IEnumerable<T> other)
		{
			return ReadOnlySet.SetEquals(other);
		}

		public void SymmetricExceptWith(IEnumerable<T> other)
		{
			GetOrCreateMutableSet().SymmetricExceptWith(other);
		}

		public readonly bool TryGetValue(T equalValue, out T actualValue)
		{
			if (ReadOnlySet.TryGetValue(equalValue, out var actualValue2))
			{
				actualValue = actualValue2;
				return true;
			}
			actualValue = equalValue;
			return false;
		}

		public void UnionWith(IEnumerable<T> other)
		{
			if (other == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other);
			}
			if (_mutableSet != null)
			{
				_mutableSet.UnionWith(other);
			}
			else
			{
				if (other is ImmutableSegmentedHashSet<T> { IsEmpty: not false })
				{
					return;
				}
				SegmentedHashSet<T> segmentedHashSet = null;
				foreach (T item in other)
				{
					if (segmentedHashSet == null)
					{
						if (ReadOnlySet.Contains(item))
						{
							continue;
						}
						segmentedHashSet = GetOrCreateMutableSet();
					}
					segmentedHashSet.Add(item);
				}
			}
		}

		public ImmutableSegmentedHashSet<T> ToImmutable()
		{
			_set = new ImmutableSegmentedHashSet<T>(ReadOnlySet);
			_mutableSet = null;
			return _set;
		}

		void ICollection<T>.Add(T item)
		{
			((ICollection<T>)GetOrCreateMutableSet()).Add(item);
		}

		readonly void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			((ICollection<T>)ReadOnlySet).CopyTo(array, arrayIndex);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public static readonly ImmutableSegmentedHashSet<T> Empty = new ImmutableSegmentedHashSet<T>(new SegmentedHashSet<T>());

	private readonly SegmentedHashSet<T> _set;

	public IEqualityComparer<T> KeyComparer => _set.Comparer;

	public int Count => _set.Count;

	public bool IsDefault => _set == null;

	public bool IsEmpty => _set.Count == 0;

	bool ICollection<T>.IsReadOnly => true;

	bool ICollection.IsSynchronized => true;

	object ICollection.SyncRoot => _set;

	private ImmutableSegmentedHashSet(SegmentedHashSet<T> set)
	{
		_set = set;
	}

	public static bool operator ==(ImmutableSegmentedHashSet<T> left, ImmutableSegmentedHashSet<T> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ImmutableSegmentedHashSet<T> left, ImmutableSegmentedHashSet<T> right)
	{
		return !left.Equals(right);
	}

	public static bool operator ==(ImmutableSegmentedHashSet<T>? left, ImmutableSegmentedHashSet<T>? right)
	{
		return left.GetValueOrDefault().Equals(right.GetValueOrDefault());
	}

	public static bool operator !=(ImmutableSegmentedHashSet<T>? left, ImmutableSegmentedHashSet<T>? right)
	{
		return !left.GetValueOrDefault().Equals(right.GetValueOrDefault());
	}

	public ImmutableSegmentedHashSet<T> Add(T value)
	{
		ImmutableSegmentedHashSet<T> result = this;
		if (result.IsEmpty)
		{
			return new ImmutableSegmentedHashSet<T>(new SegmentedHashSet<T>(result.KeyComparer) { value });
		}
		if (result.Contains(value))
		{
			return result;
		}
		ValueBuilder valueBuilder = result.ToValueBuilder();
		valueBuilder.Add(value);
		return valueBuilder.ToImmutable();
	}

	public ImmutableSegmentedHashSet<T> Clear()
	{
		ImmutableSegmentedHashSet<T> result = this;
		if (result.IsEmpty)
		{
			return result;
		}
		return Empty.WithComparer(result.KeyComparer);
	}

	public bool Contains(T value)
	{
		return _set.Contains(value);
	}

	public ImmutableSegmentedHashSet<T> Except(IEnumerable<T> other)
	{
		ImmutableSegmentedHashSet<T> result = this;
		if (other is ImmutableSegmentedHashSet<T> { IsEmpty: not false })
		{
			return result;
		}
		if (result.IsEmpty)
		{
			foreach (T item in other)
			{
				_ = item;
			}
			return result;
		}
		ValueBuilder valueBuilder = result.ToValueBuilder();
		valueBuilder.ExceptWith(other);
		return valueBuilder.ToImmutable();
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(_set);
	}

	public ImmutableSegmentedHashSet<T> Intersect(IEnumerable<T> other)
	{
		ImmutableSegmentedHashSet<T> immutableSegmentedHashSet = this;
		if (immutableSegmentedHashSet.IsEmpty || other is ImmutableSegmentedHashSet<T> { IsEmpty: not false })
		{
			return immutableSegmentedHashSet.Clear();
		}
		ValueBuilder valueBuilder = immutableSegmentedHashSet.ToValueBuilder();
		valueBuilder.IntersectWith(other);
		return valueBuilder.ToImmutable();
	}

	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		return _set.IsProperSubsetOf(other);
	}

	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		return _set.IsProperSupersetOf(other);
	}

	public bool IsSubsetOf(IEnumerable<T> other)
	{
		return _set.IsSubsetOf(other);
	}

	public bool IsSupersetOf(IEnumerable<T> other)
	{
		return _set.IsSupersetOf(other);
	}

	public bool Overlaps(IEnumerable<T> other)
	{
		return _set.Overlaps(other);
	}

	public ImmutableSegmentedHashSet<T> Remove(T value)
	{
		ImmutableSegmentedHashSet<T> result = this;
		if (!result.Contains(value))
		{
			return result;
		}
		ValueBuilder valueBuilder = result.ToValueBuilder();
		valueBuilder.Remove(value);
		return valueBuilder.ToImmutable();
	}

	public bool SetEquals(IEnumerable<T> other)
	{
		return _set.SetEquals(other);
	}

	public ImmutableSegmentedHashSet<T> SymmetricExcept(IEnumerable<T> other)
	{
		ImmutableSegmentedHashSet<T> result = this;
		if (other is ImmutableSegmentedHashSet<T> immutableSegmentedHashSet)
		{
			if (immutableSegmentedHashSet.IsEmpty)
			{
				return result;
			}
			if (result.IsEmpty)
			{
				return immutableSegmentedHashSet.WithComparer(result.KeyComparer);
			}
		}
		if (result.IsEmpty)
		{
			return ImmutableSegmentedHashSet.CreateRange(result.KeyComparer, other);
		}
		ValueBuilder valueBuilder = result.ToValueBuilder();
		valueBuilder.SymmetricExceptWith(other);
		return valueBuilder.ToImmutable();
	}

	public bool TryGetValue(T equalValue, out T actualValue)
	{
		if (_set.TryGetValue(equalValue, out var actualValue2))
		{
			actualValue = actualValue2;
			return true;
		}
		actualValue = equalValue;
		return false;
	}

	public ImmutableSegmentedHashSet<T> Union(IEnumerable<T> other)
	{
		ImmutableSegmentedHashSet<T> result = this;
		if (other is ImmutableSegmentedHashSet<T> immutableSegmentedHashSet)
		{
			if (immutableSegmentedHashSet.IsEmpty)
			{
				return result;
			}
			if (result.IsEmpty)
			{
				return immutableSegmentedHashSet.WithComparer(result.KeyComparer);
			}
		}
		ValueBuilder valueBuilder = result.ToValueBuilder();
		valueBuilder.UnionWith(other);
		return valueBuilder.ToImmutable();
	}

	public Builder ToBuilder()
	{
		return new Builder(this);
	}

	private ValueBuilder ToValueBuilder()
	{
		return new ValueBuilder(this);
	}

	public ImmutableSegmentedHashSet<T> WithComparer(IEqualityComparer<T>? equalityComparer)
	{
		ImmutableSegmentedHashSet<T> result = this;
		if (equalityComparer == null)
		{
			equalityComparer = EqualityComparer<T>.Default;
		}
		if (object.Equals(result.KeyComparer, equalityComparer))
		{
			return result;
		}
		return new ImmutableSegmentedHashSet<T>(new SegmentedHashSet<T>(result._set, equalityComparer));
	}

	public override int GetHashCode()
	{
		return _set?.GetHashCode() ?? 0;
	}

	public override bool Equals(object? obj)
	{
		if (obj is ImmutableSegmentedHashSet<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(ImmutableSegmentedHashSet<T> other)
	{
		return _set == other._set;
	}

	IImmutableSet<T> IImmutableSet<T>.Clear()
	{
		return Clear();
	}

	IImmutableSet<T> IImmutableSet<T>.Add(T value)
	{
		return Add(value);
	}

	IImmutableSet<T> IImmutableSet<T>.Remove(T value)
	{
		return Remove(value);
	}

	IImmutableSet<T> IImmutableSet<T>.Intersect(IEnumerable<T> other)
	{
		return Intersect(other);
	}

	IImmutableSet<T> IImmutableSet<T>.Except(IEnumerable<T> other)
	{
		return Except(other);
	}

	IImmutableSet<T> IImmutableSet<T>.SymmetricExcept(IEnumerable<T> other)
	{
		return SymmetricExcept(other);
	}

	IImmutableSet<T> IImmutableSet<T>.Union(IEnumerable<T> other)
	{
		return Union(other);
	}

	void ICollection<T>.CopyTo(T[] array, int arrayIndex)
	{
		_set.CopyTo(array, arrayIndex);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
		}
		if (index < 0)
		{
			ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
		}
		if (array.Length < index + Count)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index);
		}
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			array.SetValue(current, index++);
		}
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	bool ISet<T>.Add(T item)
	{
		throw new NotSupportedException();
	}

	void ISet<T>.UnionWith(IEnumerable<T> other)
	{
		throw new NotSupportedException();
	}

	void ISet<T>.IntersectWith(IEnumerable<T> other)
	{
		throw new NotSupportedException();
	}

	void ISet<T>.ExceptWith(IEnumerable<T> other)
	{
		throw new NotSupportedException();
	}

	void ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
	{
		throw new NotSupportedException();
	}

	void ICollection<T>.Add(T item)
	{
		throw new NotSupportedException();
	}

	void ICollection<T>.Clear()
	{
		throw new NotSupportedException();
	}

	bool ICollection<T>.Remove(T item)
	{
		throw new NotSupportedException();
	}
}
