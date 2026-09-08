using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Collections;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
[DebuggerTypeProxy(typeof(OneOrMany<>.DebuggerProxy))]
internal readonly struct OneOrMany<T>
{
	internal struct Enumerator
	{
		private readonly OneOrMany<T> _collection;

		private int _index;

		public T Current => _collection[_index];

		internal Enumerator(OneOrMany<T> collection)
		{
			_collection = collection;
			_index = -1;
		}

		public bool MoveNext()
		{
			_index++;
			return _index < _collection.Count;
		}
	}

	private sealed class DebuggerProxy(OneOrMany<T> instance)
	{
		private readonly OneOrMany<T> _instance = instance;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public T[] Items => _instance.ToArray();
	}

	public static readonly OneOrMany<T> Empty = new OneOrMany<T>(ImmutableArray<T>.Empty);

	private readonly T? _one;

	private readonly ImmutableArray<T> _many;

	[MemberNotNullWhen(true, "_one")]
	private bool HasOneItem
	{
		[MemberNotNullWhen(true, "_one")]
		get
		{
			return _many.IsDefault;
		}
	}

	public bool IsDefault
	{
		get
		{
			if (_one == null)
			{
				return _many.IsDefault;
			}
			return false;
		}
	}

	public T this[int index]
	{
		get
		{
			if (HasOneItem)
			{
				if (index != 0)
				{
					throw new IndexOutOfRangeException();
				}
				return _one;
			}
			return _many[index];
		}
	}

	public int Count
	{
		get
		{
			if (!HasOneItem)
			{
				return _many.Length;
			}
			return 1;
		}
	}

	public bool IsEmpty => Count == 0;

	public OneOrMany(T one)
	{
		_one = one;
		_many = default(ImmutableArray<T>);
	}

	public OneOrMany(ImmutableArray<T> many)
	{
		if (many.IsDefault)
		{
			throw new ArgumentNullException("many");
		}
		if (many.Length == 1)
		{
			T one = many[0];
			_one = one;
			_many = default(ImmutableArray<T>);
		}
		else
		{
			_one = default(T);
			_many = many;
		}
	}

	public OneOrMany<T> Add(T item)
	{
		if (!HasOneItem)
		{
			if (!IsEmpty)
			{
				return OneOrMany.Create(_many.Add(item));
			}
			return OneOrMany.Create(item);
		}
		return OneOrMany.Create<T>(_one, item);
	}

	public void AddRangeTo(ArrayBuilder<T> builder)
	{
		if (HasOneItem)
		{
			builder.Add(_one);
		}
		else
		{
			builder.AddRange(_many);
		}
	}

	public bool Contains(T item)
	{
		if (!HasOneItem)
		{
			return _many.Contains(item);
		}
		return EqualityComparer<T>.Default.Equals(item, _one);
	}

	public OneOrMany<T> RemoveAll(T item)
	{
		if (HasOneItem)
		{
			if (!EqualityComparer<T>.Default.Equals(item, _one))
			{
				return this;
			}
			return Empty;
		}
		return OneOrMany.Create(_many.WhereAsArray((T value, T y) => !EqualityComparer<T>.Default.Equals(value, y), item));
	}

	public OneOrMany<TResult> Select<TResult>(Func<T, TResult> selector)
	{
		if (!HasOneItem)
		{
			return OneOrMany.Create(_many.SelectAsArray(selector));
		}
		return OneOrMany.Create(selector(_one));
	}

	public OneOrMany<TResult> Select<TResult, TArg>(Func<T, TArg, TResult> selector, TArg arg)
	{
		if (!HasOneItem)
		{
			return OneOrMany.Create(_many.SelectAsArray(selector, arg));
		}
		return OneOrMany.Create(selector(_one, arg));
	}

	public T First()
	{
		return this[0];
	}

	public T? FirstOrDefault()
	{
		if (!HasOneItem)
		{
			return _many.FirstOrDefault();
		}
		return _one;
	}

	public T? FirstOrDefault(Func<T, bool> predicate)
	{
		if (HasOneItem)
		{
			if (!predicate(_one))
			{
				return default(T);
			}
			return _one;
		}
		return _many.FirstOrDefault(predicate);
	}

	public T? FirstOrDefault<TArg>(Func<T, TArg, bool> predicate, TArg arg)
	{
		if (HasOneItem)
		{
			if (!predicate(_one, arg))
			{
				return default(T);
			}
			return _one;
		}
		return _many.FirstOrDefault(predicate, arg);
	}

	public static OneOrMany<T> CastUp<TDerived>(OneOrMany<TDerived> from) where TDerived : class, T
	{
		if (!from.HasOneItem)
		{
			return new OneOrMany<T>(ImmutableArray<T>.CastUp<TDerived>(from._many));
		}
		return new OneOrMany<T>((T)(object)from._one);
	}

	public bool All(Func<T, bool> predicate)
	{
		if (!HasOneItem)
		{
			return _many.All(predicate);
		}
		return predicate(_one);
	}

	public bool All<TArg>(Func<T, TArg, bool> predicate, TArg arg)
	{
		if (!HasOneItem)
		{
			return _many.All(predicate, arg);
		}
		return predicate(_one, arg);
	}

	public bool Any()
	{
		return !IsEmpty;
	}

	public bool Any(Func<T, bool> predicate)
	{
		if (!HasOneItem)
		{
			return _many.Any(predicate);
		}
		return predicate(_one);
	}

	public bool Any<TArg>(Func<T, TArg, bool> predicate, TArg arg)
	{
		if (!HasOneItem)
		{
			return _many.Any(predicate, arg);
		}
		return predicate(_one, arg);
	}

	public ImmutableArray<T> ToImmutable()
	{
		if (!HasOneItem)
		{
			return _many;
		}
		return ImmutableArray.Create(_one);
	}

	public T[] ToArray()
	{
		if (!HasOneItem)
		{
			return _many.ToArray();
		}
		return new T[1] { _one };
	}

	public bool SequenceEqual(OneOrMany<T> other, IEqualityComparer<T>? comparer = null)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<T>.Default;
		}
		if (Count != other.Count)
		{
			return false;
		}
		if (!HasOneItem)
		{
			return _many.SequenceEqual(other._many, comparer);
		}
		return comparer.Equals(_one, other._one);
	}

	public bool SequenceEqual(ImmutableArray<T> other, IEqualityComparer<T>? comparer = null)
	{
		return SequenceEqual(OneOrMany.Create(other), comparer);
	}

	public bool SequenceEqual(IEnumerable<T> other, IEqualityComparer<T>? comparer = null)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<T>.Default;
		}
		if (!HasOneItem)
		{
			return _many.SequenceEqual(other, comparer);
		}
		bool flag = true;
		foreach (T item in other)
		{
			if (!flag || !comparer.Equals(_one, item))
			{
				return false;
			}
			flag = false;
		}
		return true;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	private string GetDebuggerDisplay()
	{
		return "Count = " + Count;
	}
}
internal static class OneOrMany
{
	public static OneOrMany<T> Create<T>(T one)
	{
		return new OneOrMany<T>(one);
	}

	public static OneOrMany<T> Create<T>(T one, T two)
	{
		return new OneOrMany<T>(ImmutableArray.Create(one, two));
	}

	public static OneOrMany<T> OneOrNone<T>(T? one)
	{
		if (one != null)
		{
			return new OneOrMany<T>(one);
		}
		return OneOrMany<T>.Empty;
	}

	public static OneOrMany<T> Create<T>(ImmutableArray<T> many)
	{
		return new OneOrMany<T>(many);
	}

	public static bool SequenceEqual<T>(this ImmutableArray<T> array, OneOrMany<T> other, IEqualityComparer<T>? comparer = null)
	{
		return Create(array).SequenceEqual(other, comparer);
	}

	public static bool SequenceEqual<T>(this IEnumerable<T> array, OneOrMany<T> other, IEqualityComparer<T>? comparer = null)
	{
		return other.SequenceEqual(array, comparer);
	}
}
