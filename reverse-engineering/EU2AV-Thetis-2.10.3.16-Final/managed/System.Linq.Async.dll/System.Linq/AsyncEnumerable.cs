using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Internal;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

public static class AsyncEnumerable
{
	private sealed class AnonymousAsyncEnumerable<T> : IAsyncEnumerable<T>
	{
		private readonly Func<CancellationToken, IAsyncEnumerator<T>> _getEnumerator;

		public AnonymousAsyncEnumerable(Func<CancellationToken, IAsyncEnumerator<T>> getEnumerator)
		{
			_getEnumerator = getEnumerator;
		}

		public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return _getEnumerator(cancellationToken);
		}
	}

	private abstract class AppendPrependAsyncIterator<TSource> : System.Linq.AsyncIterator<TSource>, IAsyncIListProvider<TSource>, IAsyncEnumerable<TSource>
	{
		protected readonly IAsyncEnumerable<TSource> _source;

		protected IAsyncEnumerator<TSource>? _enumerator;

		protected AppendPrependAsyncIterator(IAsyncEnumerable<TSource> source)
		{
			_source = source;
		}

		protected void GetSourceEnumerator(CancellationToken cancellationToken)
		{
			_enumerator = _source.GetAsyncEnumerator(cancellationToken);
		}

		public abstract AppendPrependAsyncIterator<TSource> Append(TSource item);

		public abstract AppendPrependAsyncIterator<TSource> Prepend(TSource item);

		protected async Task<bool> LoadFromEnumeratorAsync()
		{
			if (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				_current = _enumerator.Current;
				return true;
			}
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
			}
			return false;
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		public abstract ValueTask<TSource[]> ToArrayAsync(CancellationToken cancellationToken);

		public abstract ValueTask<List<TSource>> ToListAsync(CancellationToken cancellationToken);

		public abstract ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken);
	}

	private sealed class AppendPrepend1AsyncIterator<TSource> : AppendPrependAsyncIterator<TSource>
	{
		private readonly TSource _item;

		private readonly bool _appending;

		private bool _hasEnumerator;

		public AppendPrepend1AsyncIterator(IAsyncEnumerable<TSource> source, TSource item, bool appending)
			: base(source)
		{
			_item = item;
			_appending = appending;
		}

		public override System.Linq.AsyncIteratorBase<TSource> Clone()
		{
			return new AppendPrepend1AsyncIterator<TSource>(_source, _item, _appending);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_010a;
				}
			}
			else
			{
				_hasEnumerator = false;
				_state = System.Linq.AsyncIteratorState.Iterating;
				if (!_appending)
				{
					_current = _item;
					return true;
				}
			}
			if (!_hasEnumerator)
			{
				GetSourceEnumerator(_cancellationToken);
				_hasEnumerator = true;
			}
			if (_enumerator != null)
			{
				if (await LoadFromEnumeratorAsync().ConfigureAwait(continueOnCapturedContext: false))
				{
					return true;
				}
				if (_appending)
				{
					_current = _item;
					return true;
				}
			}
			goto IL_010a;
			IL_010a:
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
		}

		public override AppendPrependAsyncIterator<TSource> Append(TSource element)
		{
			if (_appending)
			{
				return new AppendPrependNAsyncIterator<TSource>(_source, null, new System.Linq.SingleLinkedNode<TSource>(_item).Add(element), 0, 2);
			}
			return new AppendPrependNAsyncIterator<TSource>(_source, new System.Linq.SingleLinkedNode<TSource>(_item), new System.Linq.SingleLinkedNode<TSource>(element), 1, 1);
		}

		public override AppendPrependAsyncIterator<TSource> Prepend(TSource element)
		{
			if (_appending)
			{
				return new AppendPrependNAsyncIterator<TSource>(_source, new System.Linq.SingleLinkedNode<TSource>(element), new System.Linq.SingleLinkedNode<TSource>(_item), 1, 1);
			}
			return new AppendPrependNAsyncIterator<TSource>(_source, new System.Linq.SingleLinkedNode<TSource>(_item).Add(element), null, 2, 0);
		}

		public override async ValueTask<TSource[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			int num = await GetCountAsync(onlyIfCheap: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (num == -1)
			{
				return await AsyncEnumerableHelpers.ToArray(this, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			cancellationToken.ThrowIfCancellationRequested();
			TSource[] array = new TSource[num];
			int index;
			if (_appending)
			{
				index = 0;
			}
			else
			{
				array[0] = _item;
				index = 1;
			}
			if (_source is ICollection<TSource> collection)
			{
				collection.CopyTo(array, index);
			}
			else
			{
				await foreach (TSource item in _source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					array[index] = item;
					int num2 = index + 1;
					index = num2;
				}
			}
			if (_appending)
			{
				array[array.Length - 1] = _item;
			}
			return array;
		}

		public override async ValueTask<List<TSource>> ToListAsync(CancellationToken cancellationToken)
		{
			int num = await GetCountAsync(onlyIfCheap: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			cancellationToken.ThrowIfCancellationRequested();
			List<TSource> list = ((num == -1) ? new List<TSource>() : new List<TSource>(num));
			if (!_appending)
			{
				list.Add(_item);
			}
			await foreach (TSource item in _source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				list.Add(item);
			}
			if (_appending)
			{
				list.Add(_item);
			}
			return list;
		}

		public override async ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (_source is IAsyncIListProvider<TSource> asyncIListProvider)
			{
				int num = await asyncIListProvider.GetCountAsync(onlyIfCheap, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return (num == -1) ? (-1) : (num + 1);
			}
			return (onlyIfCheap && !(_source is ICollection<TSource>) && !(_source is ICollection)) ? (-1) : (await CountAsync(_source, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) + 1);
		}
	}

	private sealed class AppendPrependNAsyncIterator<TSource> : AppendPrependAsyncIterator<TSource>
	{
		private readonly System.Linq.SingleLinkedNode<TSource>? _prepended;

		private readonly System.Linq.SingleLinkedNode<TSource>? _appended;

		private readonly int _prependCount;

		private readonly int _appendCount;

		private System.Linq.SingleLinkedNode<TSource>? _node;

		private int _mode;

		private IEnumerator<TSource>? _appendedEnumerator;

		public AppendPrependNAsyncIterator(IAsyncEnumerable<TSource> source, System.Linq.SingleLinkedNode<TSource>? prepended, System.Linq.SingleLinkedNode<TSource>? appended, int prependCount, int appendCount)
			: base(source)
		{
			_prepended = prepended;
			_appended = appended;
			_prependCount = prependCount;
			_appendCount = appendCount;
		}

		public override System.Linq.AsyncIteratorBase<TSource> Clone()
		{
			return new AppendPrependNAsyncIterator<TSource>(_source, _prepended, _appended, _prependCount, _appendCount);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_appendedEnumerator != null)
			{
				_appendedEnumerator.Dispose();
				_appendedEnumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_0176;
				}
			}
			else
			{
				_mode = 1;
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			switch (_mode)
			{
			case 1:
				_node = _prepended;
				_mode = 2;
				goto case 2;
			case 2:
				if (_node != null)
				{
					_current = _node.Item;
					_node = _node.Linked;
					return true;
				}
				GetSourceEnumerator(_cancellationToken);
				_mode = 3;
				goto case 3;
			case 3:
				if (await LoadFromEnumeratorAsync().ConfigureAwait(continueOnCapturedContext: false))
				{
					return true;
				}
				if (_appended == null)
				{
					break;
				}
				_appendedEnumerator = _appended.GetEnumerator(_appendCount);
				_mode = 4;
				goto case 4;
			case 4:
				if (_appendedEnumerator.MoveNext())
				{
					_current = _appendedEnumerator.Current;
					return true;
				}
				break;
			}
			goto IL_0176;
			IL_0176:
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
		}

		public override AppendPrependAsyncIterator<TSource> Append(TSource item)
		{
			System.Linq.SingleLinkedNode<TSource> appended = ((_appended != null) ? _appended.Add(item) : new System.Linq.SingleLinkedNode<TSource>(item));
			return new AppendPrependNAsyncIterator<TSource>(_source, _prepended, appended, _prependCount, _appendCount + 1);
		}

		public override AppendPrependAsyncIterator<TSource> Prepend(TSource item)
		{
			System.Linq.SingleLinkedNode<TSource> prepended = ((_prepended != null) ? _prepended.Add(item) : new System.Linq.SingleLinkedNode<TSource>(item));
			return new AppendPrependNAsyncIterator<TSource>(_source, prepended, _appended, _prependCount + 1, _appendCount);
		}

		public override async ValueTask<TSource[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			int num = await GetCountAsync(onlyIfCheap: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (num == -1)
			{
				return await AsyncEnumerableHelpers.ToArray(this, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			TSource[] array = new TSource[num];
			int index = 0;
			for (System.Linq.SingleLinkedNode<TSource> singleLinkedNode = _prepended; singleLinkedNode != null; singleLinkedNode = singleLinkedNode.Linked)
			{
				array[index] = singleLinkedNode.Item;
				int num2 = index + 1;
				index = num2;
			}
			if (_source is ICollection<TSource> collection)
			{
				collection.CopyTo(array, index);
			}
			else
			{
				await foreach (TSource item in _source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					array[index] = item;
					int num2 = index + 1;
					index = num2;
				}
			}
			index = array.Length;
			for (System.Linq.SingleLinkedNode<TSource> singleLinkedNode2 = _appended; singleLinkedNode2 != null; singleLinkedNode2 = singleLinkedNode2.Linked)
			{
				int num2 = index - 1;
				index = num2;
				array[index] = singleLinkedNode2.Item;
			}
			return array;
		}

		public override async ValueTask<List<TSource>> ToListAsync(CancellationToken cancellationToken)
		{
			int num = await GetCountAsync(onlyIfCheap: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			List<TSource> list = ((num == -1) ? new List<TSource>() : new List<TSource>(num));
			for (System.Linq.SingleLinkedNode<TSource> singleLinkedNode = _prepended; singleLinkedNode != null; singleLinkedNode = singleLinkedNode.Linked)
			{
				list.Add(singleLinkedNode.Item);
			}
			await foreach (TSource item in _source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				list.Add(item);
			}
			if (_appended != null)
			{
				using IEnumerator<TSource> enumerator2 = _appended.GetEnumerator(_appendCount);
				while (enumerator2.MoveNext())
				{
					list.Add(enumerator2.Current);
				}
			}
			return list;
		}

		public override async ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (_source is IAsyncIListProvider<TSource> asyncIListProvider)
			{
				int num = await asyncIListProvider.GetCountAsync(onlyIfCheap, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return (num == -1) ? (-1) : (num + _appendCount + _prependCount);
			}
			return (onlyIfCheap && !(_source is ICollection<TSource>) && !(_source is ICollection)) ? (-1) : (await CountAsync(_source, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) + _appendCount + _prependCount);
		}
	}

	private sealed class Concat2AsyncIterator<TSource> : ConcatAsyncIterator<TSource>
	{
		private readonly IAsyncEnumerable<TSource> _first;

		private readonly IAsyncEnumerable<TSource> _second;

		internal Concat2AsyncIterator(IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second)
		{
			_first = first;
			_second = second;
		}

		public override System.Linq.AsyncIteratorBase<TSource> Clone()
		{
			return new Concat2AsyncIterator<TSource>(_first, _second);
		}

		internal override ConcatAsyncIterator<TSource> Concat(IAsyncEnumerable<TSource> next)
		{
			return new ConcatNAsyncIterator<TSource>(this, next, 2);
		}

		internal override IAsyncEnumerable<TSource>? GetAsyncEnumerable(int index)
		{
			return index switch
			{
				0 => _first, 
				1 => _second, 
				_ => null, 
			};
		}
	}

	private abstract class ConcatAsyncIterator<TSource> : System.Linq.AsyncIterator<TSource>, IAsyncIListProvider<TSource>, IAsyncEnumerable<TSource>
	{
		private int _counter;

		private IAsyncEnumerator<TSource>? _enumerator;

		public ValueTask<TSource[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return AsyncEnumerableHelpers.ToArray(this, cancellationToken);
		}

		public async ValueTask<List<TSource>> ToListAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			List<TSource> list = new List<TSource>();
			int i = 0;
			while (true)
			{
				IAsyncEnumerable<TSource> asyncEnumerable = GetAsyncEnumerable(i);
				if (asyncEnumerable == null)
				{
					break;
				}
				await foreach (TSource item in asyncEnumerable.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					list.Add(item);
				}
				i++;
			}
			return list;
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core();
			async ValueTask<int> Core()
			{
				cancellationToken.ThrowIfCancellationRequested();
				int num = 0;
				int i = 0;
				while (true)
				{
					IAsyncEnumerable<TSource> asyncEnumerable = GetAsyncEnumerable(i);
					if (asyncEnumerable == null)
					{
						break;
					}
					int num2 = num;
					num = checked(num2 + await CountAsync(asyncEnumerable, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
					i++;
				}
				return num;
			}
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			if (_state == System.Linq.AsyncIteratorState.Allocated)
			{
				_enumerator = GetAsyncEnumerable(0).GetAsyncEnumerator(_cancellationToken);
				_state = System.Linq.AsyncIteratorState.Iterating;
				_counter = 2;
			}
			if (_state == System.Linq.AsyncIteratorState.Iterating)
			{
				while (true)
				{
					if (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
					{
						_current = _enumerator.Current;
						return true;
					}
					IAsyncEnumerable<TSource> next = GetAsyncEnumerable(_counter++ - 1);
					if (next == null)
					{
						break;
					}
					await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
					_enumerator = next.GetAsyncEnumerator(_cancellationToken);
				}
				await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			return false;
		}

		internal abstract ConcatAsyncIterator<TSource> Concat(IAsyncEnumerable<TSource> next);

		internal abstract IAsyncEnumerable<TSource>? GetAsyncEnumerable(int index);
	}

	private sealed class ConcatNAsyncIterator<TSource> : ConcatAsyncIterator<TSource>
	{
		private readonly IAsyncEnumerable<TSource> _next;

		private readonly int _nextIndex;

		private readonly ConcatAsyncIterator<TSource> _previousConcat;

		internal ConcatNAsyncIterator(ConcatAsyncIterator<TSource> previousConcat, IAsyncEnumerable<TSource> next, int nextIndex)
		{
			_previousConcat = previousConcat;
			_next = next;
			_nextIndex = nextIndex;
		}

		public override System.Linq.AsyncIteratorBase<TSource> Clone()
		{
			return new ConcatNAsyncIterator<TSource>(_previousConcat, _next, _nextIndex);
		}

		internal override ConcatAsyncIterator<TSource> Concat(IAsyncEnumerable<TSource> next)
		{
			if (_nextIndex == 2147483645)
			{
				return new Concat2AsyncIterator<TSource>(this, next);
			}
			return new ConcatNAsyncIterator<TSource>(this, next, _nextIndex + 1);
		}

		internal override IAsyncEnumerable<TSource>? GetAsyncEnumerable(int index)
		{
			if (index > _nextIndex)
			{
				return null;
			}
			ConcatNAsyncIterator<TSource> concatNAsyncIterator = this;
			while (true)
			{
				if (index == concatNAsyncIterator._nextIndex)
				{
					return concatNAsyncIterator._next;
				}
				if (!(concatNAsyncIterator._previousConcat is ConcatNAsyncIterator<TSource> concatNAsyncIterator2))
				{
					break;
				}
				concatNAsyncIterator = concatNAsyncIterator2;
			}
			return concatNAsyncIterator._previousConcat.GetAsyncEnumerable(index);
		}
	}

	private sealed class DefaultIfEmptyAsyncIterator<TSource> : System.Linq.AsyncIterator<TSource>, IAsyncIListProvider<TSource>, IAsyncEnumerable<TSource>
	{
		private readonly IAsyncEnumerable<TSource> _source;

		private readonly TSource _defaultValue;

		private IAsyncEnumerator<TSource>? _enumerator;

		public DefaultIfEmptyAsyncIterator(IAsyncEnumerable<TSource> source, TSource defaultValue)
		{
			_source = source;
			_defaultValue = defaultValue;
		}

		public override System.Linq.AsyncIteratorBase<TSource> Clone()
		{
			return new DefaultIfEmptyAsyncIterator<TSource>(_source, _defaultValue);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			switch (_state)
			{
			case System.Linq.AsyncIteratorState.Allocated:
				_enumerator = _source.GetAsyncEnumerator(_cancellationToken);
				if (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
				{
					_current = _enumerator.Current;
					_state = System.Linq.AsyncIteratorState.Iterating;
				}
				else
				{
					_current = _defaultValue;
					await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
					_enumerator = null;
					_state = System.Linq.AsyncIteratorState.Disposed;
				}
				return true;
			case System.Linq.AsyncIteratorState.Iterating:
				if (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
				{
					_current = _enumerator.Current;
					return true;
				}
				break;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
		}

		public async ValueTask<TSource[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			TSource[] array = await AsyncEnumerable.ToArrayAsync(_source, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return (array.Length != 0) ? array : new TSource[1] { _defaultValue };
		}

		public async ValueTask<List<TSource>> ToListAsync(CancellationToken cancellationToken)
		{
			List<TSource> list = await AsyncEnumerable.ToListAsync(_source, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (list.Count == 0)
			{
				list.Add(_defaultValue);
			}
			return list;
		}

		public async ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			int num = ((onlyIfCheap && !(_source is ICollection<TSource>) && !(_source is ICollection)) ? ((!(_source is IAsyncIListProvider<TSource> asyncIListProvider)) ? (-1) : (await asyncIListProvider.GetCountAsync(onlyIfCheap: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))) : (await CountAsync(_source, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)));
			return (num == 0) ? 1 : num;
		}
	}

	private sealed class DistinctAsyncIterator<TSource> : System.Linq.AsyncIterator<TSource>, IAsyncIListProvider<TSource>, IAsyncEnumerable<TSource>
	{
		private readonly IEqualityComparer<TSource>? _comparer;

		private readonly IAsyncEnumerable<TSource> _source;

		private IAsyncEnumerator<TSource>? _enumerator;

		private System.Linq.Set<TSource>? _set;

		public DistinctAsyncIterator(IAsyncEnumerable<TSource> source, IEqualityComparer<TSource>? comparer)
		{
			_source = source;
			_comparer = comparer;
		}

		public async ValueTask<TSource[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return (await FillSetAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToArray();
		}

		public async ValueTask<List<TSource>> ToListAsync(CancellationToken cancellationToken)
		{
			return (await FillSetAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToList();
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core();
			async ValueTask<int> Core()
			{
				return (await FillSetAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Count;
			}
		}

		public override System.Linq.AsyncIteratorBase<TSource> Clone()
		{
			return new DistinctAsyncIterator<TSource>(_source, _comparer);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
				_set = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			TSource current;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state == System.Linq.AsyncIteratorState.Iterating)
				{
					while (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
					{
						current = _enumerator.Current;
						if (_set.Add(current))
						{
							_current = current;
							return true;
						}
					}
				}
				await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				return false;
			}
			_enumerator = _source.GetAsyncEnumerator(_cancellationToken);
			if (!(await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false)))
			{
				await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				return false;
			}
			current = _enumerator.Current;
			_set = new System.Linq.Set<TSource>(_comparer);
			_set.Add(current);
			_current = current;
			_state = System.Linq.AsyncIteratorState.Iterating;
			return true;
		}

		private Task<System.Linq.Set<TSource>> FillSetAsync(CancellationToken cancellationToken)
		{
			return AsyncEnumerableHelpers.ToSet(_source, _comparer, cancellationToken);
		}
	}

	internal sealed class EmptyAsyncIterator<TValue> : IAsyncPartition<TValue>, IAsyncIListProvider<TValue>, IAsyncEnumerable<TValue>, IAsyncEnumerator<TValue>, IAsyncDisposable
	{
		public static readonly EmptyAsyncIterator<TValue> Instance = new EmptyAsyncIterator<TValue>();

		public TValue Current => default(TValue);

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			return new ValueTask<int>(0);
		}

		public IAsyncPartition<TValue> Skip(int count)
		{
			return this;
		}

		public IAsyncPartition<TValue> Take(int count)
		{
			return this;
		}

		public ValueTask<TValue[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return new ValueTask<TValue[]>(Array.Empty<TValue>());
		}

		public ValueTask<List<TValue>> ToListAsync(CancellationToken cancellationToken)
		{
			return new ValueTask<List<TValue>>(new List<TValue>());
		}

		public ValueTask<Maybe<TValue>> TryGetElementAtAsync(int index, CancellationToken cancellationToken)
		{
			return new ValueTask<Maybe<TValue>>(default(Maybe<TValue>));
		}

		public ValueTask<Maybe<TValue>> TryGetFirstAsync(CancellationToken cancellationToken)
		{
			return new ValueTask<Maybe<TValue>>(default(Maybe<TValue>));
		}

		public ValueTask<Maybe<TValue>> TryGetLastAsync(CancellationToken cancellationToken)
		{
			return new ValueTask<Maybe<TValue>>(default(Maybe<TValue>));
		}

		public ValueTask<bool> MoveNextAsync()
		{
			return new ValueTask<bool>(result: false);
		}

		public IAsyncEnumerator<TValue> GetAsyncEnumerator(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return this;
		}

		public ValueTask DisposeAsync()
		{
			return default(ValueTask);
		}
	}

	private sealed class GroupedResultAsyncEnumerable<TSource, TKey, TResult> : System.Linq.AsyncIterator<TResult>, IAsyncIListProvider<TResult>, IAsyncEnumerable<TResult>
	{
		private readonly IAsyncEnumerable<TSource> _source;

		private readonly Func<TSource, TKey> _keySelector;

		private readonly Func<TKey, IAsyncEnumerable<TSource>, TResult> _resultSelector;

		private readonly IEqualityComparer<TKey>? _comparer;

		private System.Linq.Internal.Lookup<TKey, TSource>? _lookup;

		private IEnumerator<TResult>? _enumerator;

		public GroupedResultAsyncEnumerable(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IAsyncEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
		{
			_source = source ?? throw System.Error.ArgumentNull("source");
			_keySelector = keySelector ?? throw System.Error.ArgumentNull("keySelector");
			_resultSelector = resultSelector ?? throw System.Error.ArgumentNull("resultSelector");
			_comparer = comparer;
		}

		public override System.Linq.AsyncIteratorBase<TResult> Clone()
		{
			return new GroupedResultAsyncEnumerable<TSource, TKey, TResult>(_source, _keySelector, _resultSelector, _comparer);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
				_lookup = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_0169;
				}
			}
			else
			{
				_lookup = await System.Linq.Internal.Lookup<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = _lookup.ApplyResultSelector(_resultSelector).GetEnumerator();
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (_enumerator.MoveNext())
			{
				_current = _enumerator.Current;
				return true;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_0169;
			IL_0169:
			return false;
		}

		public async ValueTask<TResult[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return (await System.Linq.Internal.Lookup<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToArray(_resultSelector);
		}

		public async ValueTask<List<TResult>> ToListAsync(CancellationToken cancellationToken)
		{
			return (await System.Linq.Internal.Lookup<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToList(_resultSelector);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core();
			async ValueTask<int> Core()
			{
				return (await System.Linq.Internal.Lookup<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Count;
			}
		}
	}

	private sealed class GroupedResultAsyncEnumerableWithTask<TSource, TKey, TResult> : System.Linq.AsyncIterator<TResult>, IAsyncIListProvider<TResult>, IAsyncEnumerable<TResult>
	{
		private readonly IAsyncEnumerable<TSource> _source;

		private readonly Func<TSource, ValueTask<TKey>> _keySelector;

		private readonly Func<TKey, IAsyncEnumerable<TSource>, ValueTask<TResult>> _resultSelector;

		private readonly IEqualityComparer<TKey>? _comparer;

		private LookupWithTask<TKey, TSource>? _lookup;

		private IAsyncEnumerator<TResult>? _enumerator;

		public GroupedResultAsyncEnumerableWithTask(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TKey, IAsyncEnumerable<TSource>, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
		{
			_source = source ?? throw System.Error.ArgumentNull("source");
			_keySelector = keySelector ?? throw System.Error.ArgumentNull("keySelector");
			_resultSelector = resultSelector ?? throw System.Error.ArgumentNull("resultSelector");
			_comparer = comparer;
		}

		public override System.Linq.AsyncIteratorBase<TResult> Clone()
		{
			return new GroupedResultAsyncEnumerableWithTask<TSource, TKey, TResult>(_source, _keySelector, _resultSelector, _comparer);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
				_lookup = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_01e3;
				}
			}
			else
			{
				_lookup = await LookupWithTask<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = _lookup.SelectAwaitCore<IAsyncGrouping<TKey, TSource>, TResult>(async (IAsyncGrouping<TKey, TSource> g) => await _resultSelector(g.Key, g).ConfigureAwait(continueOnCapturedContext: false)).GetAsyncEnumerator(_cancellationToken);
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				_current = _enumerator.Current;
				return true;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_01e3;
			IL_01e3:
			return false;
		}

		public async ValueTask<TResult[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return await (await LookupWithTask<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToArray(_resultSelector).ConfigureAwait(continueOnCapturedContext: false);
		}

		public async ValueTask<List<TResult>> ToListAsync(CancellationToken cancellationToken)
		{
			return await (await LookupWithTask<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToList(_resultSelector).ConfigureAwait(continueOnCapturedContext: false);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core();
			async ValueTask<int> Core()
			{
				return (await LookupWithTask<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Count;
			}
		}
	}

	private sealed class GroupedResultAsyncEnumerableWithTaskAndCancellation<TSource, TKey, TResult> : System.Linq.AsyncIterator<TResult>, IAsyncIListProvider<TResult>, IAsyncEnumerable<TResult>
	{
		private readonly IAsyncEnumerable<TSource> _source;

		private readonly Func<TSource, CancellationToken, ValueTask<TKey>> _keySelector;

		private readonly Func<TKey, IAsyncEnumerable<TSource>, CancellationToken, ValueTask<TResult>> _resultSelector;

		private readonly IEqualityComparer<TKey>? _comparer;

		private LookupWithTask<TKey, TSource>? _lookup;

		private IAsyncEnumerator<TResult>? _enumerator;

		public GroupedResultAsyncEnumerableWithTaskAndCancellation(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TKey, IAsyncEnumerable<TSource>, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
		{
			_source = source ?? throw System.Error.ArgumentNull("source");
			_keySelector = keySelector ?? throw System.Error.ArgumentNull("keySelector");
			_resultSelector = resultSelector ?? throw System.Error.ArgumentNull("resultSelector");
			_comparer = comparer;
		}

		public override System.Linq.AsyncIteratorBase<TResult> Clone()
		{
			return new GroupedResultAsyncEnumerableWithTaskAndCancellation<TSource, TKey, TResult>(_source, _keySelector, _resultSelector, _comparer);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
				_lookup = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_01e3;
				}
			}
			else
			{
				_lookup = await LookupWithTask<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = _lookup.SelectAwaitCore<IAsyncGrouping<TKey, TSource>, TResult>(async (IAsyncGrouping<TKey, TSource> g) => await _resultSelector(g.Key, g, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetAsyncEnumerator(_cancellationToken);
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				_current = _enumerator.Current;
				return true;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_01e3;
			IL_01e3:
			return false;
		}

		public async ValueTask<TResult[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return await (await LookupWithTask<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToArray(_resultSelector, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public async ValueTask<List<TResult>> ToListAsync(CancellationToken cancellationToken)
		{
			return await (await LookupWithTask<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToList(_resultSelector, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core();
			async ValueTask<int> Core()
			{
				return (await LookupWithTask<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Count;
			}
		}
	}

	private sealed class GroupedResultAsyncEnumerable<TSource, TKey, TElement, TResult> : System.Linq.AsyncIterator<TResult>, IAsyncIListProvider<TResult>, IAsyncEnumerable<TResult>
	{
		private readonly IAsyncEnumerable<TSource> _source;

		private readonly Func<TSource, TKey> _keySelector;

		private readonly Func<TSource, TElement> _elementSelector;

		private readonly Func<TKey, IAsyncEnumerable<TElement>, TResult> _resultSelector;

		private readonly IEqualityComparer<TKey>? _comparer;

		private System.Linq.Internal.Lookup<TKey, TElement>? _lookup;

		private IEnumerator<TResult>? _enumerator;

		public GroupedResultAsyncEnumerable(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IAsyncEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
		{
			_source = source ?? throw System.Error.ArgumentNull("source");
			_keySelector = keySelector ?? throw System.Error.ArgumentNull("keySelector");
			_elementSelector = elementSelector ?? throw System.Error.ArgumentNull("elementSelector");
			_resultSelector = resultSelector ?? throw System.Error.ArgumentNull("resultSelector");
			_comparer = comparer;
		}

		public override System.Linq.AsyncIteratorBase<TResult> Clone()
		{
			return new GroupedResultAsyncEnumerable<TSource, TKey, TElement, TResult>(_source, _keySelector, _elementSelector, _resultSelector, _comparer);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
				_lookup = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_016f;
				}
			}
			else
			{
				_lookup = await System.Linq.Internal.Lookup<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = _lookup.ApplyResultSelector(_resultSelector).GetEnumerator();
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (_enumerator.MoveNext())
			{
				_current = _enumerator.Current;
				return true;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_016f;
			IL_016f:
			return false;
		}

		public async ValueTask<TResult[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return (await System.Linq.Internal.Lookup<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToArray(_resultSelector);
		}

		public async ValueTask<List<TResult>> ToListAsync(CancellationToken cancellationToken)
		{
			return (await System.Linq.Internal.Lookup<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToList(_resultSelector);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core();
			async ValueTask<int> Core()
			{
				return (await System.Linq.Internal.Lookup<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Count;
			}
		}
	}

	private sealed class GroupedResultAsyncEnumerableWithTask<TSource, TKey, TElement, TResult> : System.Linq.AsyncIterator<TResult>, IAsyncIListProvider<TResult>, IAsyncEnumerable<TResult>
	{
		private readonly IAsyncEnumerable<TSource> _source;

		private readonly Func<TSource, ValueTask<TKey>> _keySelector;

		private readonly Func<TSource, ValueTask<TElement>> _elementSelector;

		private readonly Func<TKey, IAsyncEnumerable<TElement>, ValueTask<TResult>> _resultSelector;

		private readonly IEqualityComparer<TKey>? _comparer;

		private LookupWithTask<TKey, TElement>? _lookup;

		private IAsyncEnumerator<TResult>? _enumerator;

		public GroupedResultAsyncEnumerableWithTask(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, Func<TKey, IAsyncEnumerable<TElement>, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
		{
			_source = source ?? throw System.Error.ArgumentNull("source");
			_keySelector = keySelector ?? throw System.Error.ArgumentNull("keySelector");
			_elementSelector = elementSelector ?? throw System.Error.ArgumentNull("elementSelector");
			_resultSelector = resultSelector ?? throw System.Error.ArgumentNull("resultSelector");
			_comparer = comparer;
		}

		public override System.Linq.AsyncIteratorBase<TResult> Clone()
		{
			return new GroupedResultAsyncEnumerableWithTask<TSource, TKey, TElement, TResult>(_source, _keySelector, _elementSelector, _resultSelector, _comparer);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
				_lookup = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_01e9;
				}
			}
			else
			{
				_lookup = await LookupWithTask<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = _lookup.SelectAwaitCore<IAsyncGrouping<TKey, TElement>, TResult>(async (IAsyncGrouping<TKey, TElement> g) => await _resultSelector(g.Key, g).ConfigureAwait(continueOnCapturedContext: false)).GetAsyncEnumerator(_cancellationToken);
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				_current = _enumerator.Current;
				return true;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_01e9;
			IL_01e9:
			return false;
		}

		public async ValueTask<TResult[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return await (await LookupWithTask<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToArray(_resultSelector).ConfigureAwait(continueOnCapturedContext: false);
		}

		public async ValueTask<List<TResult>> ToListAsync(CancellationToken cancellationToken)
		{
			return await (await LookupWithTask<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToList(_resultSelector).ConfigureAwait(continueOnCapturedContext: false);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core();
			async ValueTask<int> Core()
			{
				return (await LookupWithTask<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Count;
			}
		}
	}

	private sealed class GroupedResultAsyncEnumerableWithTaskAndCancellation<TSource, TKey, TElement, TResult> : System.Linq.AsyncIterator<TResult>, IAsyncIListProvider<TResult>, IAsyncEnumerable<TResult>
	{
		private readonly IAsyncEnumerable<TSource> _source;

		private readonly Func<TSource, CancellationToken, ValueTask<TKey>> _keySelector;

		private readonly Func<TSource, CancellationToken, ValueTask<TElement>> _elementSelector;

		private readonly Func<TKey, IAsyncEnumerable<TElement>, CancellationToken, ValueTask<TResult>> _resultSelector;

		private readonly IEqualityComparer<TKey>? _comparer;

		private LookupWithTask<TKey, TElement>? _lookup;

		private IAsyncEnumerator<TResult>? _enumerator;

		public GroupedResultAsyncEnumerableWithTaskAndCancellation(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, Func<TKey, IAsyncEnumerable<TElement>, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
		{
			_source = source ?? throw System.Error.ArgumentNull("source");
			_keySelector = keySelector ?? throw System.Error.ArgumentNull("keySelector");
			_elementSelector = elementSelector ?? throw System.Error.ArgumentNull("elementSelector");
			_resultSelector = resultSelector ?? throw System.Error.ArgumentNull("resultSelector");
			_comparer = comparer;
		}

		public override System.Linq.AsyncIteratorBase<TResult> Clone()
		{
			return new GroupedResultAsyncEnumerableWithTaskAndCancellation<TSource, TKey, TElement, TResult>(_source, _keySelector, _elementSelector, _resultSelector, _comparer);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
				_lookup = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_01e9;
				}
			}
			else
			{
				_lookup = await LookupWithTask<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = _lookup.SelectAwaitCore<IAsyncGrouping<TKey, TElement>, TResult>(async (IAsyncGrouping<TKey, TElement> g) => await _resultSelector(g.Key, g, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetAsyncEnumerator(_cancellationToken);
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				_current = _enumerator.Current;
				return true;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_01e9;
			IL_01e9:
			return false;
		}

		public async ValueTask<TResult[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return await (await LookupWithTask<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToArray(_resultSelector, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public async ValueTask<List<TResult>> ToListAsync(CancellationToken cancellationToken)
		{
			return await (await LookupWithTask<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToList(_resultSelector, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core();
			async ValueTask<int> Core()
			{
				return (await LookupWithTask<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Count;
			}
		}
	}

	private sealed class GroupedAsyncEnumerable<TSource, TKey, TElement> : System.Linq.AsyncIterator<IAsyncGrouping<TKey, TElement>>, IAsyncIListProvider<IAsyncGrouping<TKey, TElement>>, IAsyncEnumerable<IAsyncGrouping<TKey, TElement>>
	{
		private readonly IAsyncEnumerable<TSource> _source;

		private readonly Func<TSource, TKey> _keySelector;

		private readonly Func<TSource, TElement> _elementSelector;

		private readonly IEqualityComparer<TKey>? _comparer;

		private System.Linq.Internal.Lookup<TKey, TElement>? _lookup;

		private IEnumerator<IGrouping<TKey, TElement>>? _enumerator;

		public GroupedAsyncEnumerable(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer)
		{
			_source = source ?? throw System.Error.ArgumentNull("source");
			_keySelector = keySelector ?? throw System.Error.ArgumentNull("keySelector");
			_elementSelector = elementSelector ?? throw System.Error.ArgumentNull("elementSelector");
			_comparer = comparer;
		}

		public override System.Linq.AsyncIteratorBase<IAsyncGrouping<TKey, TElement>> Clone()
		{
			return new GroupedAsyncEnumerable<TSource, TKey, TElement>(_source, _keySelector, _elementSelector, _comparer);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
				_lookup = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_0169;
				}
			}
			else
			{
				_lookup = await System.Linq.Internal.Lookup<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = _lookup.GetEnumerator();
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (_enumerator.MoveNext())
			{
				_current = (IAsyncGrouping<TKey, TElement>)_enumerator.Current;
				return true;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_0169;
			IL_0169:
			return false;
		}

		public async ValueTask<IAsyncGrouping<TKey, TElement>[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return await ((IAsyncIListProvider<IAsyncGrouping<TKey, TElement>>)(await System.Linq.Internal.Lookup<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))).ToArrayAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public async ValueTask<List<IAsyncGrouping<TKey, TElement>>> ToListAsync(CancellationToken cancellationToken)
		{
			return await ((IAsyncIListProvider<IAsyncGrouping<TKey, TElement>>)(await System.Linq.Internal.Lookup<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))).ToListAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core();
			async ValueTask<int> Core()
			{
				return (await System.Linq.Internal.Lookup<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Count;
			}
		}
	}

	private sealed class GroupedAsyncEnumerableWithTask<TSource, TKey, TElement> : System.Linq.AsyncIterator<IAsyncGrouping<TKey, TElement>>, IAsyncIListProvider<IAsyncGrouping<TKey, TElement>>, IAsyncEnumerable<IAsyncGrouping<TKey, TElement>>
	{
		private readonly IAsyncEnumerable<TSource> _source;

		private readonly Func<TSource, ValueTask<TKey>> _keySelector;

		private readonly Func<TSource, ValueTask<TElement>> _elementSelector;

		private readonly IEqualityComparer<TKey>? _comparer;

		private LookupWithTask<TKey, TElement>? _lookup;

		private IEnumerator<IGrouping<TKey, TElement>>? _enumerator;

		public GroupedAsyncEnumerableWithTask(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer)
		{
			_source = source ?? throw System.Error.ArgumentNull("source");
			_keySelector = keySelector ?? throw System.Error.ArgumentNull("keySelector");
			_elementSelector = elementSelector ?? throw System.Error.ArgumentNull("elementSelector");
			_comparer = comparer;
		}

		public override System.Linq.AsyncIteratorBase<IAsyncGrouping<TKey, TElement>> Clone()
		{
			return new GroupedAsyncEnumerableWithTask<TSource, TKey, TElement>(_source, _keySelector, _elementSelector, _comparer);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
				_lookup = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_0169;
				}
			}
			else
			{
				_lookup = await LookupWithTask<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = _lookup.GetEnumerator();
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (_enumerator.MoveNext())
			{
				_current = (IAsyncGrouping<TKey, TElement>)_enumerator.Current;
				return true;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_0169;
			IL_0169:
			return false;
		}

		public async ValueTask<IAsyncGrouping<TKey, TElement>[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return await ((IAsyncIListProvider<IAsyncGrouping<TKey, TElement>>)(await LookupWithTask<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))).ToArrayAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public async ValueTask<List<IAsyncGrouping<TKey, TElement>>> ToListAsync(CancellationToken cancellationToken)
		{
			return await ((IAsyncIListProvider<IAsyncGrouping<TKey, TElement>>)(await LookupWithTask<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))).ToListAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core();
			async ValueTask<int> Core()
			{
				return (await LookupWithTask<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Count;
			}
		}
	}

	private sealed class GroupedAsyncEnumerableWithTaskAndCancellation<TSource, TKey, TElement> : System.Linq.AsyncIterator<IAsyncGrouping<TKey, TElement>>, IAsyncIListProvider<IAsyncGrouping<TKey, TElement>>, IAsyncEnumerable<IAsyncGrouping<TKey, TElement>>
	{
		private readonly IAsyncEnumerable<TSource> _source;

		private readonly Func<TSource, CancellationToken, ValueTask<TKey>> _keySelector;

		private readonly Func<TSource, CancellationToken, ValueTask<TElement>> _elementSelector;

		private readonly IEqualityComparer<TKey>? _comparer;

		private LookupWithTask<TKey, TElement>? _lookup;

		private IEnumerator<IGrouping<TKey, TElement>>? _enumerator;

		public GroupedAsyncEnumerableWithTaskAndCancellation(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer)
		{
			_source = source ?? throw System.Error.ArgumentNull("source");
			_keySelector = keySelector ?? throw System.Error.ArgumentNull("keySelector");
			_elementSelector = elementSelector ?? throw System.Error.ArgumentNull("elementSelector");
			_comparer = comparer;
		}

		public override System.Linq.AsyncIteratorBase<IAsyncGrouping<TKey, TElement>> Clone()
		{
			return new GroupedAsyncEnumerableWithTaskAndCancellation<TSource, TKey, TElement>(_source, _keySelector, _elementSelector, _comparer);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
				_lookup = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_0169;
				}
			}
			else
			{
				_lookup = await LookupWithTask<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = _lookup.GetEnumerator();
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (_enumerator.MoveNext())
			{
				_current = (IAsyncGrouping<TKey, TElement>)_enumerator.Current;
				return true;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_0169;
			IL_0169:
			return false;
		}

		public async ValueTask<IAsyncGrouping<TKey, TElement>[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return await ((IAsyncIListProvider<IAsyncGrouping<TKey, TElement>>)(await LookupWithTask<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))).ToArrayAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public async ValueTask<List<IAsyncGrouping<TKey, TElement>>> ToListAsync(CancellationToken cancellationToken)
		{
			return await ((IAsyncIListProvider<IAsyncGrouping<TKey, TElement>>)(await LookupWithTask<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))).ToListAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core();
			async ValueTask<int> Core()
			{
				return (await LookupWithTask<TKey, TElement>.CreateAsync(_source, _keySelector, _elementSelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Count;
			}
		}
	}

	private sealed class GroupedAsyncEnumerable<TSource, TKey> : System.Linq.AsyncIterator<IAsyncGrouping<TKey, TSource>>, IAsyncIListProvider<IAsyncGrouping<TKey, TSource>>, IAsyncEnumerable<IAsyncGrouping<TKey, TSource>>
	{
		private readonly IAsyncEnumerable<TSource> _source;

		private readonly Func<TSource, TKey> _keySelector;

		private readonly IEqualityComparer<TKey>? _comparer;

		private System.Linq.Internal.Lookup<TKey, TSource>? _lookup;

		private IEnumerator<IGrouping<TKey, TSource>>? _enumerator;

		public GroupedAsyncEnumerable(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
		{
			_source = source ?? throw System.Error.ArgumentNull("source");
			_keySelector = keySelector ?? throw System.Error.ArgumentNull("keySelector");
			_comparer = comparer;
		}

		public override System.Linq.AsyncIteratorBase<IAsyncGrouping<TKey, TSource>> Clone()
		{
			return new GroupedAsyncEnumerable<TSource, TKey>(_source, _keySelector, _comparer);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
				_lookup = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_0163;
				}
			}
			else
			{
				_lookup = await System.Linq.Internal.Lookup<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = _lookup.GetEnumerator();
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (_enumerator.MoveNext())
			{
				_current = (IAsyncGrouping<TKey, TSource>)_enumerator.Current;
				return true;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_0163;
			IL_0163:
			return false;
		}

		public async ValueTask<IAsyncGrouping<TKey, TSource>[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return await ((IAsyncIListProvider<IAsyncGrouping<TKey, TSource>>)(await System.Linq.Internal.Lookup<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))).ToArrayAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public async ValueTask<List<IAsyncGrouping<TKey, TSource>>> ToListAsync(CancellationToken cancellationToken)
		{
			return await ((IAsyncIListProvider<IAsyncGrouping<TKey, TSource>>)(await System.Linq.Internal.Lookup<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))).ToListAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core();
			async ValueTask<int> Core()
			{
				return (await System.Linq.Internal.Lookup<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Count;
			}
		}
	}

	private sealed class GroupedAsyncEnumerableWithTask<TSource, TKey> : System.Linq.AsyncIterator<IAsyncGrouping<TKey, TSource>>, IAsyncIListProvider<IAsyncGrouping<TKey, TSource>>, IAsyncEnumerable<IAsyncGrouping<TKey, TSource>>
	{
		private readonly IAsyncEnumerable<TSource> _source;

		private readonly Func<TSource, ValueTask<TKey>> _keySelector;

		private readonly IEqualityComparer<TKey>? _comparer;

		private LookupWithTask<TKey, TSource>? _lookup;

		private IEnumerator<IGrouping<TKey, TSource>>? _enumerator;

		public GroupedAsyncEnumerableWithTask(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer)
		{
			_source = source ?? throw System.Error.ArgumentNull("source");
			_keySelector = keySelector ?? throw System.Error.ArgumentNull("keySelector");
			_comparer = comparer;
		}

		public override System.Linq.AsyncIteratorBase<IAsyncGrouping<TKey, TSource>> Clone()
		{
			return new GroupedAsyncEnumerableWithTask<TSource, TKey>(_source, _keySelector, _comparer);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
				_lookup = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_0163;
				}
			}
			else
			{
				_lookup = await LookupWithTask<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = _lookup.GetEnumerator();
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (_enumerator.MoveNext())
			{
				_current = (IAsyncGrouping<TKey, TSource>)_enumerator.Current;
				return true;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_0163;
			IL_0163:
			return false;
		}

		public async ValueTask<IAsyncGrouping<TKey, TSource>[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return await ((IAsyncIListProvider<IAsyncGrouping<TKey, TSource>>)(await LookupWithTask<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))).ToArrayAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public async ValueTask<List<IAsyncGrouping<TKey, TSource>>> ToListAsync(CancellationToken cancellationToken)
		{
			return await ((IAsyncIListProvider<IAsyncGrouping<TKey, TSource>>)(await LookupWithTask<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))).ToListAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core();
			async ValueTask<int> Core()
			{
				return (await LookupWithTask<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Count;
			}
		}
	}

	private sealed class GroupedAsyncEnumerableWithTaskAndCancellation<TSource, TKey> : System.Linq.AsyncIterator<IAsyncGrouping<TKey, TSource>>, IAsyncIListProvider<IAsyncGrouping<TKey, TSource>>, IAsyncEnumerable<IAsyncGrouping<TKey, TSource>>
	{
		private readonly IAsyncEnumerable<TSource> _source;

		private readonly Func<TSource, CancellationToken, ValueTask<TKey>> _keySelector;

		private readonly IEqualityComparer<TKey>? _comparer;

		private LookupWithTask<TKey, TSource>? _lookup;

		private IEnumerator<IGrouping<TKey, TSource>>? _enumerator;

		public GroupedAsyncEnumerableWithTaskAndCancellation(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer)
		{
			_source = source ?? throw System.Error.ArgumentNull("source");
			_keySelector = keySelector ?? throw System.Error.ArgumentNull("keySelector");
			_comparer = comparer;
		}

		public override System.Linq.AsyncIteratorBase<IAsyncGrouping<TKey, TSource>> Clone()
		{
			return new GroupedAsyncEnumerableWithTaskAndCancellation<TSource, TKey>(_source, _keySelector, _comparer);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
				_lookup = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_0163;
				}
			}
			else
			{
				_lookup = await LookupWithTask<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = _lookup.GetEnumerator();
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (_enumerator.MoveNext())
			{
				_current = (IAsyncGrouping<TKey, TSource>)_enumerator.Current;
				return true;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_0163;
			IL_0163:
			return false;
		}

		public async ValueTask<IAsyncGrouping<TKey, TSource>[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return await ((IAsyncIListProvider<IAsyncGrouping<TKey, TSource>>)(await LookupWithTask<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))).ToArrayAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public async ValueTask<List<IAsyncGrouping<TKey, TSource>>> ToListAsync(CancellationToken cancellationToken)
		{
			return await ((IAsyncIListProvider<IAsyncGrouping<TKey, TSource>>)(await LookupWithTask<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))).ToListAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core();
			async ValueTask<int> Core()
			{
				return (await LookupWithTask<TKey, TSource>.CreateAsync(_source, _keySelector, _comparer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Count;
			}
		}
	}

	private sealed class RangeAsyncIterator : System.Linq.AsyncIterator<int>, IAsyncPartition<int>, IAsyncIListProvider<int>, IAsyncEnumerable<int>
	{
		private readonly int _start;

		private readonly int _end;

		public RangeAsyncIterator(int start, int count)
		{
			_start = start;
			_end = start + count;
		}

		public override System.Linq.AsyncIteratorBase<int> Clone()
		{
			return new RangeAsyncIterator(_start, _end - _start);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			return new ValueTask<int>(_end - _start);
		}

		public IAsyncPartition<int> Skip(int count)
		{
			int num = _end - _start;
			if (count >= num)
			{
				return EmptyAsyncIterator<int>.Instance;
			}
			return new RangeAsyncIterator(_start + count, num - count);
		}

		public IAsyncPartition<int> Take(int count)
		{
			int num = _end - _start;
			if (count >= num)
			{
				return this;
			}
			return new RangeAsyncIterator(_start, count);
		}

		public ValueTask<int[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			int[] array = new int[_end - _start];
			int start = _start;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = start++;
			}
			return new ValueTask<int[]>(array);
		}

		public ValueTask<List<int>> ToListAsync(CancellationToken cancellationToken)
		{
			List<int> list = new List<int>(_end - _start);
			for (int i = _start; i < _end; i++)
			{
				list.Add(i);
			}
			return new ValueTask<List<int>>(list);
		}

		public ValueTask<Maybe<int>> TryGetElementAtAsync(int index, CancellationToken cancellationToken)
		{
			if ((uint)index < (uint)(_end - _start))
			{
				return new ValueTask<Maybe<int>>(new Maybe<int>(_start + index));
			}
			return new ValueTask<Maybe<int>>(default(Maybe<int>));
		}

		public ValueTask<Maybe<int>> TryGetFirstAsync(CancellationToken cancellationToken)
		{
			return new ValueTask<Maybe<int>>(new Maybe<int>(_start));
		}

		public ValueTask<Maybe<int>> TryGetLastAsync(CancellationToken cancellationToken)
		{
			return new ValueTask<Maybe<int>>(new Maybe<int>(_end - 1));
		}

		protected override ValueTask<bool> MoveNextCore()
		{
			switch (_state)
			{
			case System.Linq.AsyncIteratorState.Allocated:
				_current = _start;
				_state = System.Linq.AsyncIteratorState.Iterating;
				return new ValueTask<bool>(result: true);
			case System.Linq.AsyncIteratorState.Iterating:
				_current++;
				if (_current != _end)
				{
					return new ValueTask<bool>(result: true);
				}
				break;
			}
			return Core();
			async ValueTask<bool> Core()
			{
				await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				return false;
			}
		}
	}

	private sealed class RepeatAsyncIterator<TResult> : System.Linq.AsyncIterator<TResult>, IAsyncIListProvider<TResult>, IAsyncEnumerable<TResult>
	{
		private readonly TResult _element;

		private readonly int _count;

		private int _remaining;

		public RepeatAsyncIterator(TResult element, int count)
		{
			_element = element;
			_count = count;
		}

		public override System.Linq.AsyncIteratorBase<TResult> Clone()
		{
			return new RepeatAsyncIterator<TResult>(_element, _count);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			return new ValueTask<int>(_count);
		}

		public ValueTask<TResult[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			TResult[] array = new TResult[_count];
			for (int i = 0; i < _count; i++)
			{
				array[i] = _element;
			}
			return new ValueTask<TResult[]>(array);
		}

		public ValueTask<List<TResult>> ToListAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			List<TResult> list = new List<TResult>(_count);
			for (int i = 0; i < _count; i++)
			{
				list.Add(_element);
			}
			return new ValueTask<List<TResult>>(list);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_006a;
				}
			}
			else
			{
				_remaining = _count;
				if (_remaining > 0)
				{
					_current = _element;
				}
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (_remaining-- != 0)
			{
				return true;
			}
			goto IL_006a;
			IL_006a:
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
		}
	}

	private sealed class ReverseAsyncIterator<TSource> : System.Linq.AsyncIterator<TSource>, IAsyncIListProvider<TSource>, IAsyncEnumerable<TSource>
	{
		private readonly IAsyncEnumerable<TSource> _source;

		private int _index;

		private TSource[]? _items;

		public ReverseAsyncIterator(IAsyncEnumerable<TSource> source)
		{
			_source = source;
		}

		public async ValueTask<TSource[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			TSource[] array = await AsyncEnumerable.ToArrayAsync(_source, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			int num = 0;
			int num2 = array.Length - 1;
			while (num < num2)
			{
				TSource[] array2 = array;
				int num3 = num2;
				TSource[] array3 = array;
				int num4 = num;
				TSource val = array[num];
				TSource val2 = array[num2];
				array2[num3] = val;
				array3[num4] = val2;
				num++;
				num2--;
			}
			return array;
		}

		public async ValueTask<List<TSource>> ToListAsync(CancellationToken cancellationToken)
		{
			List<TSource> obj = await AsyncEnumerable.ToListAsync(_source, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			obj.Reverse();
			return obj;
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				if (_source is IAsyncIListProvider<TSource> asyncIListProvider)
				{
					return asyncIListProvider.GetCountAsync(onlyIfCheap: true, cancellationToken);
				}
				if (!(_source is ICollection<TSource>) && !(_source is ICollection))
				{
					return new ValueTask<int>(-1);
				}
			}
			return CountAsync(_source, cancellationToken);
		}

		public override System.Linq.AsyncIteratorBase<TSource> Clone()
		{
			return new ReverseAsyncIterator<TSource>(_source);
		}

		public override async ValueTask DisposeAsync()
		{
			_items = null;
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_00fc;
				}
			}
			else
			{
				_items = await AsyncEnumerable.ToArrayAsync(_source, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_index = _items.Length - 1;
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (_index != -1)
			{
				_current = _items[_index];
				_index--;
				return true;
			}
			goto IL_00fc;
			IL_00fc:
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
		}
	}

	internal sealed class SelectEnumerableAsyncIterator<TSource, TResult> : System.Linq.AsyncIterator<TResult>
	{
		private readonly Func<TSource, TResult> _selector;

		private readonly IAsyncEnumerable<TSource> _source;

		private IAsyncEnumerator<TSource>? _enumerator;

		public SelectEnumerableAsyncIterator(IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			_source = source;
			_selector = selector;
		}

		public override System.Linq.AsyncIteratorBase<TResult> Clone()
		{
			return new SelectEnumerableAsyncIterator<TSource, TResult>(_source, _selector);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		public override IAsyncEnumerable<TResult1> Select<TResult1>(Func<TResult, TResult1> selector)
		{
			return new SelectEnumerableAsyncIterator<TSource, TResult1>(_source, CombineSelectors(_selector, selector));
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_00e3;
				}
			}
			else
			{
				_enumerator = _source.GetAsyncEnumerator(_cancellationToken);
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				_current = _selector(_enumerator.Current);
				return true;
			}
			goto IL_00e3;
			IL_00e3:
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
		}
	}

	internal sealed class SelectIListIterator<TSource, TResult> : System.Linq.AsyncIterator<TResult>, IAsyncIListProvider<TResult>, IAsyncEnumerable<TResult>
	{
		private readonly Func<TSource, TResult> _selector;

		private readonly IList<TSource> _source;

		private IEnumerator<TSource>? _enumerator;

		public SelectIListIterator(IList<TSource> source, Func<TSource, TResult> selector)
		{
			_source = source;
			_selector = selector;
		}

		public override System.Linq.AsyncIteratorBase<TResult> Clone()
		{
			return new SelectIListIterator<TSource, TResult>(_source, _selector);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			cancellationToken.ThrowIfCancellationRequested();
			int num = 0;
			foreach (TSource item in _source)
			{
				_selector(item);
				num = checked(num + 1);
			}
			return new ValueTask<int>(num);
		}

		public override IAsyncEnumerable<TResult1> Select<TResult1>(Func<TResult, TResult1> selector)
		{
			return new SelectIListIterator<TSource, TResult1>(_source, CombineSelectors(_selector, selector));
		}

		public ValueTask<TResult[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			int count = _source.Count;
			TResult[] array = new TResult[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = _selector(_source[i]);
			}
			return new ValueTask<TResult[]>(array);
		}

		public ValueTask<List<TResult>> ToListAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			int count = _source.Count;
			List<TResult> list = new List<TResult>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(_selector(_source[i]));
			}
			return new ValueTask<List<TResult>>(list);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_00d9;
				}
			}
			else
			{
				_enumerator = _source.GetEnumerator();
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (_enumerator.MoveNext())
			{
				_current = _selector(_enumerator.Current);
				return true;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_00d9;
			IL_00d9:
			return false;
		}
	}

	internal sealed class SelectEnumerableAsyncIteratorWithTask<TSource, TResult> : System.Linq.AsyncIterator<TResult>
	{
		private readonly Func<TSource, ValueTask<TResult>> _selector;

		private readonly IAsyncEnumerable<TSource> _source;

		private IAsyncEnumerator<TSource>? _enumerator;

		public SelectEnumerableAsyncIteratorWithTask(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TResult>> selector)
		{
			_source = source;
			_selector = selector;
		}

		public override System.Linq.AsyncIteratorBase<TResult> Clone()
		{
			return new SelectEnumerableAsyncIteratorWithTask<TSource, TResult>(_source, _selector);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		public override IAsyncEnumerable<TResult1> Select<TResult1>(Func<TResult, ValueTask<TResult1>> selector)
		{
			return new SelectEnumerableAsyncIteratorWithTask<TSource, TResult1>(_source, CombineSelectors(_selector, selector));
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_0155;
				}
			}
			else
			{
				_enumerator = _source.GetAsyncEnumerator(_cancellationToken);
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				_current = await _selector(_enumerator.Current).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			}
			goto IL_0155;
			IL_0155:
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
		}
	}

	internal sealed class SelectEnumerableAsyncIteratorWithTaskAndCancellation<TSource, TResult> : System.Linq.AsyncIterator<TResult>
	{
		private readonly Func<TSource, CancellationToken, ValueTask<TResult>> _selector;

		private readonly IAsyncEnumerable<TSource> _source;

		private IAsyncEnumerator<TSource>? _enumerator;

		public SelectEnumerableAsyncIteratorWithTaskAndCancellation(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TResult>> selector)
		{
			_source = source;
			_selector = selector;
		}

		public override System.Linq.AsyncIteratorBase<TResult> Clone()
		{
			return new SelectEnumerableAsyncIteratorWithTaskAndCancellation<TSource, TResult>(_source, _selector);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		public override IAsyncEnumerable<TResult1> Select<TResult1>(Func<TResult, CancellationToken, ValueTask<TResult1>> selector)
		{
			return new SelectEnumerableAsyncIteratorWithTaskAndCancellation<TSource, TResult1>(_source, CombineSelectors(_selector, selector));
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_015b;
				}
			}
			else
			{
				_enumerator = _source.GetAsyncEnumerator(_cancellationToken);
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				_current = await _selector(_enumerator.Current, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			}
			goto IL_015b;
			IL_015b:
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
		}
	}

	private sealed class SelectIListIteratorWithTask<TSource, TResult> : System.Linq.AsyncIterator<TResult>, IAsyncIListProvider<TResult>, IAsyncEnumerable<TResult>
	{
		private readonly Func<TSource, ValueTask<TResult>> _selector;

		private readonly IList<TSource> _source;

		private IEnumerator<TSource>? _enumerator;

		public SelectIListIteratorWithTask(IList<TSource> source, Func<TSource, ValueTask<TResult>> selector)
		{
			_source = source;
			_selector = selector;
		}

		public override System.Linq.AsyncIteratorBase<TResult> Clone()
		{
			return new SelectIListIteratorWithTask<TSource, TResult>(_source, _selector);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core();
			async ValueTask<int> Core()
			{
				cancellationToken.ThrowIfCancellationRequested();
				int count = 0;
				foreach (TSource item in _source)
				{
					await _selector(item).ConfigureAwait(continueOnCapturedContext: false);
					count = checked(count + 1);
				}
				return count;
			}
		}

		public override IAsyncEnumerable<TResult1> Select<TResult1>(Func<TResult, ValueTask<TResult1>> selector)
		{
			return new SelectIListIteratorWithTask<TSource, TResult1>(_source, CombineSelectors(_selector, selector));
		}

		public async ValueTask<TResult[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			int n = _source.Count;
			TResult[] res = new TResult[n];
			for (int i = 0; i < n; i++)
			{
				TResult[] array = res;
				int num = i;
				array[num] = await _selector(_source[i]).ConfigureAwait(continueOnCapturedContext: false);
			}
			return res;
		}

		public async ValueTask<List<TResult>> ToListAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			int n = _source.Count;
			List<TResult> res = new List<TResult>(n);
			for (int i = 0; i < n; i++)
			{
				List<TResult> list = res;
				list.Add(await _selector(_source[i]).ConfigureAwait(continueOnCapturedContext: false));
			}
			return res;
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_00e4;
				}
			}
			else
			{
				_enumerator = _source.GetEnumerator();
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (_enumerator.MoveNext())
			{
				_current = await _selector(_enumerator.Current).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			}
			goto IL_00e4;
			IL_00e4:
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
		}
	}

	private sealed class SelectIListIteratorWithTaskAndCancellation<TSource, TResult> : System.Linq.AsyncIterator<TResult>, IAsyncIListProvider<TResult>, IAsyncEnumerable<TResult>
	{
		private readonly Func<TSource, CancellationToken, ValueTask<TResult>> _selector;

		private readonly IList<TSource> _source;

		private IEnumerator<TSource>? _enumerator;

		public SelectIListIteratorWithTaskAndCancellation(IList<TSource> source, Func<TSource, CancellationToken, ValueTask<TResult>> selector)
		{
			_source = source;
			_selector = selector;
		}

		public override System.Linq.AsyncIteratorBase<TResult> Clone()
		{
			return new SelectIListIteratorWithTaskAndCancellation<TSource, TResult>(_source, _selector);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core();
			async ValueTask<int> Core()
			{
				cancellationToken.ThrowIfCancellationRequested();
				int count = 0;
				foreach (TSource item in _source)
				{
					await _selector(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					count = checked(count + 1);
				}
				return count;
			}
		}

		public override IAsyncEnumerable<TResult1> Select<TResult1>(Func<TResult, CancellationToken, ValueTask<TResult1>> selector)
		{
			return new SelectIListIteratorWithTaskAndCancellation<TSource, TResult1>(_source, CombineSelectors(_selector, selector));
		}

		public async ValueTask<TResult[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			int n = _source.Count;
			TResult[] res = new TResult[n];
			for (int i = 0; i < n; i++)
			{
				TResult[] array = res;
				int num = i;
				array[num] = await _selector(_source[i], cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			return res;
		}

		public async ValueTask<List<TResult>> ToListAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			int n = _source.Count;
			List<TResult> res = new List<TResult>(n);
			for (int i = 0; i < n; i++)
			{
				List<TResult> list = res;
				list.Add(await _selector(_source[i], cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
			}
			return res;
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_00ea;
				}
			}
			else
			{
				_enumerator = _source.GetEnumerator();
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (_enumerator.MoveNext())
			{
				_current = await _selector(_enumerator.Current, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			}
			goto IL_00ea;
			IL_00ea:
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
		}
	}

	private interface ICombinedSelectors<TSource, TResult>
	{
		ICombinedSelectors<TSource, TNewResult> Combine<TNewResult>(Func<TResult, TNewResult> selector);

		TResult Invoke(TSource x);
	}

	private interface ICombinedAsyncSelectors<TSource, TResult>
	{
		ICombinedAsyncSelectors<TSource, TNewResult> Combine<TNewResult>(Func<TResult, ValueTask<TNewResult>> selector);

		ValueTask<TResult> Invoke(TSource x);
	}

	private interface ICombinedAsyncSelectorsWithCancellation<TSource, TResult>
	{
		ICombinedAsyncSelectorsWithCancellation<TSource, TNewResult> Combine<TNewResult>(Func<TResult, CancellationToken, ValueTask<TNewResult>> selector);

		ValueTask<TResult> Invoke(TSource x, CancellationToken ct);
	}

	private sealed class CombinedSelectors2<TSource, TMiddle1, TResult> : ICombinedSelectors<TSource, TResult>
	{
		private readonly Func<TSource, TMiddle1> _selector1;

		private readonly Func<TMiddle1, TResult> _selector2;

		public CombinedSelectors2(Func<TSource, TMiddle1> selector1, Func<TMiddle1, TResult> selector2)
		{
			_selector1 = selector1;
			_selector2 = selector2;
		}

		public ICombinedSelectors<TSource, TNewResult> Combine<TNewResult>(Func<TResult, TNewResult> selector)
		{
			return new CombinedSelectors3<TSource, TMiddle1, TResult, TNewResult>(_selector1, _selector2, selector);
		}

		public TResult Invoke(TSource x)
		{
			return _selector2(_selector1(x));
		}
	}

	private sealed class CombinedSelectors3<TSource, TMiddle1, TMiddle2, TResult> : ICombinedSelectors<TSource, TResult>
	{
		private readonly Func<TSource, TMiddle1> _selector1;

		private readonly Func<TMiddle1, TMiddle2> _selector2;

		private readonly Func<TMiddle2, TResult> _selector3;

		public CombinedSelectors3(Func<TSource, TMiddle1> selector1, Func<TMiddle1, TMiddle2> selector2, Func<TMiddle2, TResult> selector3)
		{
			_selector1 = selector1;
			_selector2 = selector2;
			_selector3 = selector3;
		}

		public ICombinedSelectors<TSource, TNewResult> Combine<TNewResult>(Func<TResult, TNewResult> selector)
		{
			return new CombinedSelectors4<TSource, TMiddle1, TMiddle2, TResult, TNewResult>(_selector1, _selector2, _selector3, selector);
		}

		public TResult Invoke(TSource x)
		{
			return _selector3(_selector2(_selector1(x)));
		}
	}

	private sealed class CombinedSelectors4<TSource, TMiddle1, TMiddle2, TMiddle3, TResult> : ICombinedSelectors<TSource, TResult>
	{
		private readonly Func<TSource, TMiddle1> _selector1;

		private readonly Func<TMiddle1, TMiddle2> _selector2;

		private readonly Func<TMiddle2, TMiddle3> _selector3;

		private readonly Func<TMiddle3, TResult> _selector4;

		public CombinedSelectors4(Func<TSource, TMiddle1> selector1, Func<TMiddle1, TMiddle2> selector2, Func<TMiddle2, TMiddle3> selector3, Func<TMiddle3, TResult> selector4)
		{
			_selector1 = selector1;
			_selector2 = selector2;
			_selector3 = selector3;
			_selector4 = selector4;
		}

		public ICombinedSelectors<TSource, TNewResult> Combine<TNewResult>(Func<TResult, TNewResult> selector)
		{
			return new CombinedSelectors2<TSource, TResult, TNewResult>(Invoke, selector);
		}

		public TResult Invoke(TSource x)
		{
			return _selector4(_selector3(_selector2(_selector1(x))));
		}
	}

	private sealed class CombinedAsyncSelectors2<TSource, TMiddle1, TResult> : ICombinedAsyncSelectors<TSource, TResult>
	{
		private readonly Func<TSource, ValueTask<TMiddle1>> _selector1;

		private readonly Func<TMiddle1, ValueTask<TResult>> _selector2;

		public CombinedAsyncSelectors2(Func<TSource, ValueTask<TMiddle1>> selector1, Func<TMiddle1, ValueTask<TResult>> selector2)
		{
			_selector1 = selector1;
			_selector2 = selector2;
		}

		public ICombinedAsyncSelectors<TSource, TNewResult> Combine<TNewResult>(Func<TResult, ValueTask<TNewResult>> selector)
		{
			return new CombinedAsyncSelectors3<TSource, TMiddle1, TResult, TNewResult>(_selector1, _selector2, selector);
		}

		public async ValueTask<TResult> Invoke(TSource x)
		{
			Func<TMiddle1, ValueTask<TResult>> selector = _selector2;
			return await selector(await _selector1(x).ConfigureAwait(continueOnCapturedContext: false)).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private sealed class CombinedAsyncSelectors3<TSource, TMiddle1, TMiddle2, TResult> : ICombinedAsyncSelectors<TSource, TResult>
	{
		private readonly Func<TSource, ValueTask<TMiddle1>> _selector1;

		private readonly Func<TMiddle1, ValueTask<TMiddle2>> _selector2;

		private readonly Func<TMiddle2, ValueTask<TResult>> _selector3;

		public CombinedAsyncSelectors3(Func<TSource, ValueTask<TMiddle1>> selector1, Func<TMiddle1, ValueTask<TMiddle2>> selector2, Func<TMiddle2, ValueTask<TResult>> selector3)
		{
			_selector1 = selector1;
			_selector2 = selector2;
			_selector3 = selector3;
		}

		public ICombinedAsyncSelectors<TSource, TNewResult> Combine<TNewResult>(Func<TResult, ValueTask<TNewResult>> selector)
		{
			return new CombinedAsyncSelectors4<TSource, TMiddle1, TMiddle2, TResult, TNewResult>(_selector1, _selector2, _selector3, selector);
		}

		public async ValueTask<TResult> Invoke(TSource x)
		{
			Func<TMiddle2, ValueTask<TResult>> selector = _selector3;
			Func<TMiddle1, ValueTask<TMiddle2>> selector2 = _selector2;
			return await selector(await selector2(await _selector1(x).ConfigureAwait(continueOnCapturedContext: false)).ConfigureAwait(continueOnCapturedContext: false)).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private sealed class CombinedAsyncSelectors4<TSource, TMiddle1, TMiddle2, TMiddle3, TResult> : ICombinedAsyncSelectors<TSource, TResult>
	{
		private readonly Func<TSource, ValueTask<TMiddle1>> _selector1;

		private readonly Func<TMiddle1, ValueTask<TMiddle2>> _selector2;

		private readonly Func<TMiddle2, ValueTask<TMiddle3>> _selector3;

		private readonly Func<TMiddle3, ValueTask<TResult>> _selector4;

		public CombinedAsyncSelectors4(Func<TSource, ValueTask<TMiddle1>> selector1, Func<TMiddle1, ValueTask<TMiddle2>> selector2, Func<TMiddle2, ValueTask<TMiddle3>> selector3, Func<TMiddle3, ValueTask<TResult>> selector4)
		{
			_selector1 = selector1;
			_selector2 = selector2;
			_selector3 = selector3;
			_selector4 = selector4;
		}

		public ICombinedAsyncSelectors<TSource, TNewResult> Combine<TNewResult>(Func<TResult, ValueTask<TNewResult>> selector)
		{
			return new CombinedAsyncSelectors2<TSource, TResult, TNewResult>(Invoke, selector);
		}

		public async ValueTask<TResult> Invoke(TSource x)
		{
			Func<TMiddle3, ValueTask<TResult>> selector = _selector4;
			Func<TMiddle2, ValueTask<TMiddle3>> selector2 = _selector3;
			Func<TMiddle1, ValueTask<TMiddle2>> selector3 = _selector2;
			return await selector(await selector2(await selector3(await _selector1(x).ConfigureAwait(continueOnCapturedContext: false)).ConfigureAwait(continueOnCapturedContext: false)).ConfigureAwait(continueOnCapturedContext: false)).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private sealed class CombinedAsyncSelectorsWithCancellation2<TSource, TMiddle1, TResult> : ICombinedAsyncSelectorsWithCancellation<TSource, TResult>
	{
		private readonly Func<TSource, CancellationToken, ValueTask<TMiddle1>> _selector1;

		private readonly Func<TMiddle1, CancellationToken, ValueTask<TResult>> _selector2;

		public CombinedAsyncSelectorsWithCancellation2(Func<TSource, CancellationToken, ValueTask<TMiddle1>> selector1, Func<TMiddle1, CancellationToken, ValueTask<TResult>> selector2)
		{
			_selector1 = selector1;
			_selector2 = selector2;
		}

		public ICombinedAsyncSelectorsWithCancellation<TSource, TNewResult> Combine<TNewResult>(Func<TResult, CancellationToken, ValueTask<TNewResult>> selector)
		{
			return new CombinedAsyncSelectorsWithCancellation3<TSource, TMiddle1, TResult, TNewResult>(_selector1, _selector2, selector);
		}

		public async ValueTask<TResult> Invoke(TSource x, CancellationToken ct)
		{
			Func<TMiddle1, CancellationToken, ValueTask<TResult>> selector = _selector2;
			return await selector(await _selector1(x, ct).ConfigureAwait(continueOnCapturedContext: false), ct).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private sealed class CombinedAsyncSelectorsWithCancellation3<TSource, TMiddle1, TMiddle2, TResult> : ICombinedAsyncSelectorsWithCancellation<TSource, TResult>
	{
		private readonly Func<TSource, CancellationToken, ValueTask<TMiddle1>> _selector1;

		private readonly Func<TMiddle1, CancellationToken, ValueTask<TMiddle2>> _selector2;

		private readonly Func<TMiddle2, CancellationToken, ValueTask<TResult>> _selector3;

		public CombinedAsyncSelectorsWithCancellation3(Func<TSource, CancellationToken, ValueTask<TMiddle1>> selector1, Func<TMiddle1, CancellationToken, ValueTask<TMiddle2>> selector2, Func<TMiddle2, CancellationToken, ValueTask<TResult>> selector3)
		{
			_selector1 = selector1;
			_selector2 = selector2;
			_selector3 = selector3;
		}

		public ICombinedAsyncSelectorsWithCancellation<TSource, TNewResult> Combine<TNewResult>(Func<TResult, CancellationToken, ValueTask<TNewResult>> selector)
		{
			return new CombinedAsyncSelectorsWithCancellation4<TSource, TMiddle1, TMiddle2, TResult, TNewResult>(_selector1, _selector2, _selector3, selector);
		}

		public async ValueTask<TResult> Invoke(TSource x, CancellationToken ct)
		{
			Func<TMiddle2, CancellationToken, ValueTask<TResult>> selector = _selector3;
			Func<TMiddle1, CancellationToken, ValueTask<TMiddle2>> selector2 = _selector2;
			return await selector(await selector2(await _selector1(x, ct).ConfigureAwait(continueOnCapturedContext: false), ct).ConfigureAwait(continueOnCapturedContext: false), ct).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private sealed class CombinedAsyncSelectorsWithCancellation4<TSource, TMiddle1, TMiddle2, TMiddle3, TResult> : ICombinedAsyncSelectorsWithCancellation<TSource, TResult>
	{
		private readonly Func<TSource, CancellationToken, ValueTask<TMiddle1>> _selector1;

		private readonly Func<TMiddle1, CancellationToken, ValueTask<TMiddle2>> _selector2;

		private readonly Func<TMiddle2, CancellationToken, ValueTask<TMiddle3>> _selector3;

		private readonly Func<TMiddle3, CancellationToken, ValueTask<TResult>> _selector4;

		public CombinedAsyncSelectorsWithCancellation4(Func<TSource, CancellationToken, ValueTask<TMiddle1>> selector1, Func<TMiddle1, CancellationToken, ValueTask<TMiddle2>> selector2, Func<TMiddle2, CancellationToken, ValueTask<TMiddle3>> selector3, Func<TMiddle3, CancellationToken, ValueTask<TResult>> selector4)
		{
			_selector1 = selector1;
			_selector2 = selector2;
			_selector3 = selector3;
			_selector4 = selector4;
		}

		public ICombinedAsyncSelectorsWithCancellation<TSource, TNewResult> Combine<TNewResult>(Func<TResult, CancellationToken, ValueTask<TNewResult>> selector)
		{
			return new CombinedAsyncSelectorsWithCancellation2<TSource, TResult, TNewResult>(Invoke, selector);
		}

		public async ValueTask<TResult> Invoke(TSource x, CancellationToken ct)
		{
			Func<TMiddle3, CancellationToken, ValueTask<TResult>> selector = _selector4;
			Func<TMiddle2, CancellationToken, ValueTask<TMiddle3>> selector2 = _selector3;
			Func<TMiddle1, CancellationToken, ValueTask<TMiddle2>> selector3 = _selector2;
			return await selector(await selector2(await selector3(await _selector1(x, ct).ConfigureAwait(continueOnCapturedContext: false), ct).ConfigureAwait(continueOnCapturedContext: false), ct).ConfigureAwait(continueOnCapturedContext: false), ct).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private sealed class SelectManyAsyncIterator<TSource, TResult> : System.Linq.AsyncIterator<TResult>, IAsyncIListProvider<TResult>, IAsyncEnumerable<TResult>
	{
		private const int State_Source = 1;

		private const int State_Result = 2;

		private readonly Func<TSource, IAsyncEnumerable<TResult>> _selector;

		private readonly IAsyncEnumerable<TSource> _source;

		private int _mode;

		private IAsyncEnumerator<TResult>? _resultEnumerator;

		private IAsyncEnumerator<TSource>? _sourceEnumerator;

		public SelectManyAsyncIterator(IAsyncEnumerable<TSource> source, Func<TSource, IAsyncEnumerable<TResult>> selector)
		{
			_source = source;
			_selector = selector;
		}

		public override System.Linq.AsyncIteratorBase<TResult> Clone()
		{
			return new SelectManyAsyncIterator<TSource, TResult>(_source, _selector);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_resultEnumerator != null)
			{
				await _resultEnumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_resultEnumerator = null;
			}
			if (_sourceEnumerator != null)
			{
				await _sourceEnumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_sourceEnumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core(cancellationToken);
			async ValueTask<int> Core(CancellationToken cancellationToken2)
			{
				int count = 0;
				await foreach (TSource item in _source.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
				{
					int num = count;
					count = checked(num + await CountAsync(_selector(item)).ConfigureAwait(continueOnCapturedContext: false));
				}
				return count;
			}
		}

		public async ValueTask<TResult[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return (await ToListAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToArray();
		}

		public async ValueTask<List<TResult>> ToListAsync(CancellationToken cancellationToken)
		{
			List<TResult> list = new List<TResult>();
			await foreach (TSource item in _source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				IAsyncEnumerable<TResult> collection = _selector(item);
				await list.AddRangeAsync(collection, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			return list;
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_022f;
				}
			}
			else
			{
				_sourceEnumerator = _source.GetAsyncEnumerator(_cancellationToken);
				_mode = 1;
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			int mode = _mode;
			if (mode == 1)
			{
				goto IL_0077;
			}
			if (mode == 2)
			{
				goto IL_0198;
			}
			goto IL_022f;
			IL_022f:
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
			IL_0198:
			if (await _resultEnumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				_current = _resultEnumerator.Current;
				return true;
			}
			_mode = 1;
			goto IL_0077;
			IL_0077:
			if (await _sourceEnumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				if (_resultEnumerator != null)
				{
					await _resultEnumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				}
				IAsyncEnumerable<TResult> asyncEnumerable = _selector(_sourceEnumerator.Current);
				_resultEnumerator = asyncEnumerable.GetAsyncEnumerator(_cancellationToken);
				_mode = 2;
				goto IL_0198;
			}
			goto IL_022f;
		}
	}

	private sealed class SelectManyAsyncIteratorWithTask<TSource, TResult> : System.Linq.AsyncIterator<TResult>, IAsyncIListProvider<TResult>, IAsyncEnumerable<TResult>
	{
		private const int State_Source = 1;

		private const int State_Result = 2;

		private readonly Func<TSource, ValueTask<IAsyncEnumerable<TResult>>> _selector;

		private readonly IAsyncEnumerable<TSource> _source;

		private int _mode;

		private IAsyncEnumerator<TResult>? _resultEnumerator;

		private IAsyncEnumerator<TSource>? _sourceEnumerator;

		public SelectManyAsyncIteratorWithTask(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<IAsyncEnumerable<TResult>>> selector)
		{
			_source = source;
			_selector = selector;
		}

		public override System.Linq.AsyncIteratorBase<TResult> Clone()
		{
			return new SelectManyAsyncIteratorWithTask<TSource, TResult>(_source, _selector);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_resultEnumerator != null)
			{
				await _resultEnumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_resultEnumerator = null;
			}
			if (_sourceEnumerator != null)
			{
				await _sourceEnumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_sourceEnumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core(cancellationToken);
			async ValueTask<int> Core(CancellationToken cancellationToken2)
			{
				int count = 0;
				await foreach (TSource item in _source.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
				{
					IAsyncEnumerable<TResult> source = await _selector(item).ConfigureAwait(continueOnCapturedContext: false);
					int num = count;
					count = checked(num + await CountAsync(source).ConfigureAwait(continueOnCapturedContext: false));
				}
				return count;
			}
		}

		public async ValueTask<TResult[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return (await ToListAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToArray();
		}

		public async ValueTask<List<TResult>> ToListAsync(CancellationToken cancellationToken)
		{
			List<TResult> list = new List<TResult>();
			await foreach (TSource item in _source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				await list.AddRangeAsync(await _selector(item).ConfigureAwait(continueOnCapturedContext: false), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			return list;
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_0299;
				}
			}
			else
			{
				_sourceEnumerator = _source.GetAsyncEnumerator(_cancellationToken);
				_mode = 1;
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			int mode = _mode;
			if (mode == 1)
			{
				goto IL_007b;
			}
			if (mode == 2)
			{
				goto IL_0202;
			}
			goto IL_0299;
			IL_0299:
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
			IL_0202:
			if (await _resultEnumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				_current = _resultEnumerator.Current;
				return true;
			}
			_mode = 1;
			goto IL_007b;
			IL_007b:
			if (await _sourceEnumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				if (_resultEnumerator != null)
				{
					await _resultEnumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				}
				_resultEnumerator = (await _selector(_sourceEnumerator.Current).ConfigureAwait(continueOnCapturedContext: false)).GetAsyncEnumerator(_cancellationToken);
				_mode = 2;
				goto IL_0202;
			}
			goto IL_0299;
		}
	}

	private sealed class SelectManyAsyncIteratorWithTaskAndCancellation<TSource, TResult> : System.Linq.AsyncIterator<TResult>, IAsyncIListProvider<TResult>, IAsyncEnumerable<TResult>
	{
		private const int State_Source = 1;

		private const int State_Result = 2;

		private readonly Func<TSource, CancellationToken, ValueTask<IAsyncEnumerable<TResult>>> _selector;

		private readonly IAsyncEnumerable<TSource> _source;

		private int _mode;

		private IAsyncEnumerator<TResult>? _resultEnumerator;

		private IAsyncEnumerator<TSource>? _sourceEnumerator;

		public SelectManyAsyncIteratorWithTaskAndCancellation(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<IAsyncEnumerable<TResult>>> selector)
		{
			_source = source;
			_selector = selector;
		}

		public override System.Linq.AsyncIteratorBase<TResult> Clone()
		{
			return new SelectManyAsyncIteratorWithTaskAndCancellation<TSource, TResult>(_source, _selector);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_resultEnumerator != null)
			{
				await _resultEnumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_resultEnumerator = null;
			}
			if (_sourceEnumerator != null)
			{
				await _sourceEnumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_sourceEnumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core(cancellationToken);
			async ValueTask<int> Core(CancellationToken cancellationToken2)
			{
				int count = 0;
				await foreach (TSource item in _source.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
				{
					IAsyncEnumerable<TResult> source = await _selector(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					int num = count;
					count = checked(num + await CountAsync(source).ConfigureAwait(continueOnCapturedContext: false));
				}
				return count;
			}
		}

		public async ValueTask<TResult[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return (await ToListAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToArray();
		}

		public async ValueTask<List<TResult>> ToListAsync(CancellationToken cancellationToken)
		{
			List<TResult> list = new List<TResult>();
			await foreach (TSource item in _source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				await list.AddRangeAsync(await _selector(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			return list;
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_029f;
				}
			}
			else
			{
				_sourceEnumerator = _source.GetAsyncEnumerator(_cancellationToken);
				_mode = 1;
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			int mode = _mode;
			if (mode == 1)
			{
				goto IL_007b;
			}
			if (mode == 2)
			{
				goto IL_0208;
			}
			goto IL_029f;
			IL_029f:
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
			IL_0208:
			if (await _resultEnumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				_current = _resultEnumerator.Current;
				return true;
			}
			_mode = 1;
			goto IL_007b;
			IL_007b:
			if (await _sourceEnumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				if (_resultEnumerator != null)
				{
					await _resultEnumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				}
				_resultEnumerator = (await _selector(_sourceEnumerator.Current, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetAsyncEnumerator(_cancellationToken);
				_mode = 2;
				goto IL_0208;
			}
			goto IL_029f;
		}
	}

	private sealed class AsyncEnumerableAdapter<T> : System.Linq.AsyncIterator<T>, IAsyncIListProvider<T>, IAsyncEnumerable<T>
	{
		private readonly IEnumerable<T> _source;

		private IEnumerator<T>? _enumerator;

		public AsyncEnumerableAdapter(IEnumerable<T> source)
		{
			_source = source;
		}

		public override System.Linq.AsyncIteratorBase<T> Clone()
		{
			return new AsyncEnumerableAdapter<T>(_source);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_00ce;
				}
			}
			else
			{
				_enumerator = _source.GetEnumerator();
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (_enumerator.MoveNext())
			{
				_current = _enumerator.Current;
				return true;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_00ce;
			IL_00ce:
			return false;
		}

		public ValueTask<T[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return new ValueTask<T[]>(_source.ToArray());
		}

		public ValueTask<List<T>> ToListAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return new ValueTask<List<T>>(_source.ToList());
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return new ValueTask<int>(_source.Count());
		}
	}

	private sealed class AsyncIListEnumerableAdapter<T> : System.Linq.AsyncIterator<T>, IAsyncIListProvider<T>, IAsyncEnumerable<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
	{
		private readonly IList<T> _source;

		private IEnumerator<T>? _enumerator;

		int ICollection<T>.Count => _source.Count;

		bool ICollection<T>.IsReadOnly => _source.IsReadOnly;

		T IList<T>.this[int index]
		{
			get
			{
				return _source[index];
			}
			set
			{
				_source[index] = value;
			}
		}

		public AsyncIListEnumerableAdapter(IList<T> source)
		{
			_source = source;
		}

		public override System.Linq.AsyncIteratorBase<T> Clone()
		{
			return new AsyncIListEnumerableAdapter<T>(_source);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_00ce;
				}
			}
			else
			{
				_enumerator = _source.GetEnumerator();
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (_enumerator.MoveNext())
			{
				_current = _enumerator.Current;
				return true;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_00ce;
			IL_00ce:
			return false;
		}

		public override IAsyncEnumerable<TResult> Select<TResult>(Func<T, TResult> selector)
		{
			return new SelectIListIterator<T, TResult>(_source, selector);
		}

		public ValueTask<T[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return new ValueTask<T[]>(_source.ToArray());
		}

		public ValueTask<List<T>> ToListAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return new ValueTask<List<T>>(_source.ToList());
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return new ValueTask<int>(_source.Count);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return _source.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _source.GetEnumerator();
		}

		void ICollection<T>.Add(T item)
		{
			_source.Add(item);
		}

		void ICollection<T>.Clear()
		{
			_source.Clear();
		}

		bool ICollection<T>.Contains(T item)
		{
			return _source.Contains(item);
		}

		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			_source.CopyTo(array, arrayIndex);
		}

		bool ICollection<T>.Remove(T item)
		{
			return _source.Remove(item);
		}

		int IList<T>.IndexOf(T item)
		{
			return _source.IndexOf(item);
		}

		void IList<T>.Insert(int index, T item)
		{
			_source.Insert(index, item);
		}

		void IList<T>.RemoveAt(int index)
		{
			_source.RemoveAt(index);
		}
	}

	private sealed class AsyncICollectionEnumerableAdapter<T> : System.Linq.AsyncIterator<T>, IAsyncIListProvider<T>, IAsyncEnumerable<T>, ICollection<T>, IEnumerable<T>, IEnumerable
	{
		private readonly ICollection<T> _source;

		private IEnumerator<T>? _enumerator;

		int ICollection<T>.Count => _source.Count;

		bool ICollection<T>.IsReadOnly => _source.IsReadOnly;

		public AsyncICollectionEnumerableAdapter(ICollection<T> source)
		{
			_source = source;
		}

		public override System.Linq.AsyncIteratorBase<T> Clone()
		{
			return new AsyncICollectionEnumerableAdapter<T>(_source);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				_enumerator.Dispose();
				_enumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_00ce;
				}
			}
			else
			{
				_enumerator = _source.GetEnumerator();
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			if (_enumerator.MoveNext())
			{
				_current = _enumerator.Current;
				return true;
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_00ce;
			IL_00ce:
			return false;
		}

		public ValueTask<T[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return new ValueTask<T[]>(_source.ToArray());
		}

		public ValueTask<List<T>> ToListAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return new ValueTask<List<T>>(_source.ToList());
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return new ValueTask<int>(_source.Count);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return _source.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _source.GetEnumerator();
		}

		void ICollection<T>.Add(T item)
		{
			_source.Add(item);
		}

		void ICollection<T>.Clear()
		{
			_source.Clear();
		}

		bool ICollection<T>.Contains(T item)
		{
			return _source.Contains(item);
		}

		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			_source.CopyTo(array, arrayIndex);
		}

		bool ICollection<T>.Remove(T item)
		{
			return _source.Remove(item);
		}
	}

	private sealed class ObservableAsyncEnumerable<TSource> : System.Linq.AsyncIterator<TSource>, IObserver<TSource>
	{
		private readonly IObservable<TSource> _source;

		private ConcurrentQueue<TSource>? _values = new ConcurrentQueue<TSource>();

		private Exception? _error;

		private bool _completed;

		private TaskCompletionSource<bool>? _signal;

		private IDisposable? _subscription;

		private CancellationTokenRegistration _ctr;

		public ObservableAsyncEnumerable(IObservable<TSource> source)
		{
			_source = source;
		}

		public override System.Linq.AsyncIteratorBase<TSource> Clone()
		{
			return new ObservableAsyncEnumerable<TSource>(_source);
		}

		public override ValueTask DisposeAsync()
		{
			Dispose();
			return base.DisposeAsync();
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			_cancellationToken.ThrowIfCancellationRequested();
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
					return false;
				}
			}
			else
			{
				_subscription = _source.Subscribe(this);
				_ctr = _cancellationToken.Register(OnCanceled, null);
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			while (true)
			{
				bool flag = Volatile.Read(ref _completed);
				if (_values.TryDequeue(out _current))
				{
					return true;
				}
				if (flag)
				{
					break;
				}
				await Resume().ConfigureAwait(continueOnCapturedContext: false);
				Volatile.Write(ref _signal, null);
			}
			Exception error = _error;
			if (error != null)
			{
				throw error;
			}
			return false;
		}

		public void OnCompleted()
		{
			Volatile.Write(ref _completed, value: true);
			DisposeSubscription();
			OnNotification();
		}

		public void OnError(Exception error)
		{
			_error = error;
			Volatile.Write(ref _completed, value: true);
			DisposeSubscription();
			OnNotification();
		}

		public void OnNext(TSource value)
		{
			_values?.Enqueue(value);
			OnNotification();
		}

		private void OnNotification()
		{
			do
			{
				TaskCompletionSource<bool> taskCompletionSource = Volatile.Read(ref _signal);
				if (taskCompletionSource == System.Threading.Tasks.TaskExt.True)
				{
					break;
				}
				if (taskCompletionSource != null)
				{
					taskCompletionSource.TrySetResult(result: true);
					break;
				}
			}
			while (Interlocked.CompareExchange(ref _signal, System.Threading.Tasks.TaskExt.True, null) != null);
		}

		private void Dispose()
		{
			_ctr.Dispose();
			DisposeSubscription();
			_values = null;
			_error = null;
		}

		private void DisposeSubscription()
		{
			Interlocked.Exchange(ref _subscription, null)?.Dispose();
		}

		private void OnCanceled(object? state)
		{
			TaskCompletionSource<bool> taskCompletionSource = null;
			Dispose();
			TaskCompletionSource<bool> taskCompletionSource2;
			do
			{
				taskCompletionSource2 = Volatile.Read(ref _signal);
				if (taskCompletionSource2 != null && taskCompletionSource2.TrySetCanceled(_cancellationToken))
				{
					break;
				}
				if (taskCompletionSource == null)
				{
					taskCompletionSource = new TaskCompletionSource<bool>();
					taskCompletionSource.TrySetCanceled(_cancellationToken);
				}
			}
			while (Interlocked.CompareExchange(ref _signal, taskCompletionSource, taskCompletionSource2) != taskCompletionSource2);
		}

		private Task Resume()
		{
			TaskCompletionSource<bool> taskCompletionSource = null;
			do
			{
				TaskCompletionSource<bool> taskCompletionSource2 = Volatile.Read(ref _signal);
				if (taskCompletionSource2 != null)
				{
					return taskCompletionSource2.Task;
				}
				if (taskCompletionSource == null)
				{
					taskCompletionSource = new TaskCompletionSource<bool>();
				}
			}
			while (Interlocked.CompareExchange(ref _signal, taskCompletionSource, null) != null);
			return taskCompletionSource.Task;
		}
	}

	private sealed class TaskToAsyncEnumerable<T> : System.Linq.AsyncIterator<T>
	{
		private readonly Task<T> _task;

		public TaskToAsyncEnumerable(Task<T> task)
		{
			_task = task;
		}

		public override System.Linq.AsyncIteratorBase<T> Clone()
		{
			return new TaskToAsyncEnumerable<T>(_task);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			if (_state == System.Linq.AsyncIteratorState.Allocated)
			{
				_state = System.Linq.AsyncIteratorState.Iterating;
				_current = await _task.ConfigureAwait(continueOnCapturedContext: false);
				return true;
			}
			return false;
		}
	}

	private sealed class ToObservableObservable<T> : IObservable<T>
	{
		private readonly IAsyncEnumerable<T> _source;

		public ToObservableObservable(IAsyncEnumerable<T> source)
		{
			_source = source;
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			System.Linq.CancellationTokenDisposable ctd = new System.Linq.CancellationTokenDisposable();
			Core();
			return ctd;
			async void Core()
			{
				await using IAsyncEnumerator<T> e = _source.GetAsyncEnumerator(ctd.Token);
				do
				{
					T value = default(T);
					bool flag;
					try
					{
						flag = await e.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false);
						if (flag)
						{
							value = e.Current;
						}
					}
					catch (Exception error)
					{
						if (!ctd.Token.IsCancellationRequested)
						{
							observer.OnError(error);
						}
						break;
					}
					if (!flag)
					{
						observer.OnCompleted();
						break;
					}
					observer.OnNext(value);
				}
				while (!ctd.Token.IsCancellationRequested);
			}
		}
	}

	private abstract class UnionAsyncIterator<TSource> : System.Linq.AsyncIterator<TSource>, IAsyncIListProvider<TSource>, IAsyncEnumerable<TSource>
	{
		internal readonly IEqualityComparer<TSource>? _comparer;

		private IAsyncEnumerator<TSource>? _enumerator;

		private System.Linq.Set<TSource>? _set;

		private int _index;

		protected UnionAsyncIterator(IEqualityComparer<TSource>? comparer)
		{
			_comparer = comparer;
		}

		public sealed override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
				_set = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		internal abstract IAsyncEnumerable<TSource>? GetEnumerable(int index);

		internal abstract UnionAsyncIterator<TSource> Union(IAsyncEnumerable<TSource> next);

		private async Task SetEnumeratorAsync(IAsyncEnumerator<TSource> enumerator)
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			_enumerator = enumerator;
		}

		private void StoreFirst()
		{
			System.Linq.Set<TSource> set = new System.Linq.Set<TSource>(_comparer);
			TSource current = _enumerator.Current;
			set.Add(current);
			_current = current;
			_set = set;
		}

		private async ValueTask<bool> GetNextAsync()
		{
			System.Linq.Set<TSource> set = _set;
			while (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				TSource current = _enumerator.Current;
				if (set.Add(current))
				{
					_current = current;
					return true;
				}
			}
			return false;
		}

		protected sealed override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state == System.Linq.AsyncIteratorState.Iterating)
				{
					while (true)
					{
						if (await GetNextAsync().ConfigureAwait(continueOnCapturedContext: false))
						{
							return true;
						}
						IAsyncEnumerable<TSource> enumerable = GetEnumerable(_index);
						if (enumerable == null)
						{
							break;
						}
						await SetEnumeratorAsync(enumerable.GetAsyncEnumerator(_cancellationToken)).ConfigureAwait(continueOnCapturedContext: false);
						_index++;
					}
				}
			}
			else
			{
				_index = 0;
				for (IAsyncEnumerable<TSource> enumerable2 = GetEnumerable(0); enumerable2 != null; enumerable2 = GetEnumerable(_index))
				{
					_index++;
					IAsyncEnumerator<TSource> enumerator = enumerable2.GetAsyncEnumerator(_cancellationToken);
					await SetEnumeratorAsync(enumerator).ConfigureAwait(continueOnCapturedContext: false);
					if (await enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
					{
						StoreFirst();
						_state = System.Linq.AsyncIteratorState.Iterating;
						return true;
					}
				}
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
		}

		private async Task<System.Linq.Set<TSource>> FillSetAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			System.Linq.Set<TSource> set = new System.Linq.Set<TSource>(_comparer);
			int index = 0;
			while (true)
			{
				IAsyncEnumerable<TSource> enumerable = GetEnumerable(index);
				if (enumerable == null)
				{
					break;
				}
				await foreach (TSource item in enumerable.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					set.Add(item);
				}
				int num = index + 1;
				index = num;
			}
			return set;
		}

		public async ValueTask<TSource[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return (await FillSetAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToArray();
		}

		public async ValueTask<List<TSource>> ToListAsync(CancellationToken cancellationToken)
		{
			return (await FillSetAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToList();
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return new ValueTask<int>(-1);
			}
			return Core();
			async ValueTask<int> Core()
			{
				return (await FillSetAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Count;
			}
		}
	}

	private sealed class UnionAsyncIterator2<TSource> : UnionAsyncIterator<TSource>
	{
		private readonly IAsyncEnumerable<TSource> _first;

		private readonly IAsyncEnumerable<TSource> _second;

		public UnionAsyncIterator2(IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second, IEqualityComparer<TSource>? comparer)
			: base(comparer)
		{
			_first = first;
			_second = second;
		}

		public override System.Linq.AsyncIteratorBase<TSource> Clone()
		{
			return new UnionAsyncIterator2<TSource>(_first, _second, _comparer);
		}

		internal override IAsyncEnumerable<TSource>? GetEnumerable(int index)
		{
			return index switch
			{
				0 => _first, 
				1 => _second, 
				_ => null, 
			};
		}

		internal override UnionAsyncIterator<TSource> Union(IAsyncEnumerable<TSource> next)
		{
			return new UnionAsyncIteratorN<TSource>(new System.Linq.SingleLinkedNode<IAsyncEnumerable<TSource>>(_first).Add(_second).Add(next), 2, _comparer);
		}
	}

	private sealed class UnionAsyncIteratorN<TSource> : UnionAsyncIterator<TSource>
	{
		private readonly System.Linq.SingleLinkedNode<IAsyncEnumerable<TSource>> _sources;

		private readonly int _headIndex;

		public UnionAsyncIteratorN(System.Linq.SingleLinkedNode<IAsyncEnumerable<TSource>> sources, int headIndex, IEqualityComparer<TSource>? comparer)
			: base(comparer)
		{
			_sources = sources;
			_headIndex = headIndex;
		}

		public override System.Linq.AsyncIteratorBase<TSource> Clone()
		{
			return new UnionAsyncIteratorN<TSource>(_sources, _headIndex, _comparer);
		}

		internal override IAsyncEnumerable<TSource>? GetEnumerable(int index)
		{
			if (index <= _headIndex)
			{
				return _sources.GetNode(_headIndex - index).Item;
			}
			return null;
		}

		internal override UnionAsyncIterator<TSource> Union(IAsyncEnumerable<TSource> next)
		{
			if (_headIndex == 2147483645)
			{
				return new UnionAsyncIterator2<TSource>(this, next, _comparer);
			}
			return new UnionAsyncIteratorN<TSource>(_sources.Add(next), _headIndex + 1, _comparer);
		}
	}

	internal sealed class WhereEnumerableAsyncIterator<TSource> : System.Linq.AsyncIterator<TSource>
	{
		private readonly Func<TSource, bool> _predicate;

		private readonly IAsyncEnumerable<TSource> _source;

		private IAsyncEnumerator<TSource>? _enumerator;

		public WhereEnumerableAsyncIterator(IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			_source = source;
			_predicate = predicate;
		}

		public override System.Linq.AsyncIteratorBase<TSource> Clone()
		{
			return new WhereEnumerableAsyncIterator<TSource>(_source, _predicate);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		public override IAsyncEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
		{
			return new WhereSelectEnumerableAsyncIterator<TSource, TResult>(_source, _predicate, selector);
		}

		public override IAsyncEnumerable<TSource> Where(Func<TSource, bool> predicate)
		{
			return new WhereEnumerableAsyncIterator<TSource>(_source, CombinePredicates(_predicate, predicate));
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_0159;
				}
			}
			else
			{
				_enumerator = _source.GetAsyncEnumerator(_cancellationToken);
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			while (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				TSource current = _enumerator.Current;
				if (_predicate(current))
				{
					_current = current;
					return true;
				}
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_0159;
			IL_0159:
			return false;
		}
	}

	internal sealed class WhereEnumerableAsyncIteratorWithTask<TSource> : System.Linq.AsyncIterator<TSource>
	{
		private readonly Func<TSource, ValueTask<bool>> _predicate;

		private readonly IAsyncEnumerable<TSource> _source;

		private IAsyncEnumerator<TSource>? _enumerator;

		public WhereEnumerableAsyncIteratorWithTask(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate)
		{
			_source = source;
			_predicate = predicate;
		}

		public override System.Linq.AsyncIteratorBase<TSource> Clone()
		{
			return new WhereEnumerableAsyncIteratorWithTask<TSource>(_source, _predicate);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		public override IAsyncEnumerable<TSource> Where(Func<TSource, ValueTask<bool>> predicate)
		{
			return new WhereEnumerableAsyncIteratorWithTask<TSource>(_source, CombinePredicates(_predicate, predicate));
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_01e2;
				}
			}
			else
			{
				_enumerator = _source.GetAsyncEnumerator(_cancellationToken);
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			while (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				TSource item = _enumerator.Current;
				if (await _predicate(item).ConfigureAwait(continueOnCapturedContext: false))
				{
					_current = item;
					return true;
				}
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_01e2;
			IL_01e2:
			return false;
		}
	}

	internal sealed class WhereEnumerableAsyncIteratorWithTaskAndCancellation<TSource> : System.Linq.AsyncIterator<TSource>
	{
		private readonly Func<TSource, CancellationToken, ValueTask<bool>> _predicate;

		private readonly IAsyncEnumerable<TSource> _source;

		private IAsyncEnumerator<TSource>? _enumerator;

		public WhereEnumerableAsyncIteratorWithTaskAndCancellation(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate)
		{
			_source = source;
			_predicate = predicate;
		}

		public override System.Linq.AsyncIteratorBase<TSource> Clone()
		{
			return new WhereEnumerableAsyncIteratorWithTaskAndCancellation<TSource>(_source, _predicate);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		public override IAsyncEnumerable<TSource> Where(Func<TSource, CancellationToken, ValueTask<bool>> predicate)
		{
			return new WhereEnumerableAsyncIteratorWithTaskAndCancellation<TSource>(_source, CombinePredicates(_predicate, predicate));
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_01e8;
				}
			}
			else
			{
				_enumerator = _source.GetAsyncEnumerator(_cancellationToken);
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			while (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				TSource item = _enumerator.Current;
				if (await _predicate(item, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					_current = item;
					return true;
				}
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_01e8;
			IL_01e8:
			return false;
		}
	}

	private sealed class WhereSelectEnumerableAsyncIterator<TSource, TResult> : System.Linq.AsyncIterator<TResult>
	{
		private readonly Func<TSource, bool> _predicate;

		private readonly Func<TSource, TResult> _selector;

		private readonly IAsyncEnumerable<TSource> _source;

		private IAsyncEnumerator<TSource>? _enumerator;

		public WhereSelectEnumerableAsyncIterator(IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
		{
			_source = source;
			_predicate = predicate;
			_selector = selector;
		}

		public override System.Linq.AsyncIteratorBase<TResult> Clone()
		{
			return new WhereSelectEnumerableAsyncIterator<TSource, TResult>(_source, _predicate, _selector);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		public override IAsyncEnumerable<TResult1> Select<TResult1>(Func<TResult, TResult1> selector)
		{
			return new WhereSelectEnumerableAsyncIterator<TSource, TResult1>(_source, _predicate, CombineSelectors(_selector, selector));
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			System.Linq.AsyncIteratorState state = _state;
			if (state != System.Linq.AsyncIteratorState.Allocated)
			{
				if (state != System.Linq.AsyncIteratorState.Iterating)
				{
					goto IL_0164;
				}
			}
			else
			{
				_enumerator = _source.GetAsyncEnumerator(_cancellationToken);
				_state = System.Linq.AsyncIteratorState.Iterating;
			}
			while (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
			{
				TSource current = _enumerator.Current;
				if (_predicate(current))
				{
					_current = _selector(current);
					return true;
				}
			}
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			goto IL_0164;
			IL_0164:
			return false;
		}
	}

	private interface ICombinedPredicates<TSource>
	{
		ICombinedPredicates<TSource> And(Func<TSource, bool> predicate);

		bool Invoke(TSource x);
	}

	private sealed class CombinedPredicatesN<TSource> : ICombinedPredicates<TSource>
	{
		private readonly Func<TSource, bool>[] _predicates;

		public CombinedPredicatesN(params Func<TSource, bool>[] predicates)
		{
			_predicates = predicates;
		}

		public ICombinedPredicates<TSource> And(Func<TSource, bool> predicate)
		{
			Func<TSource, bool>[] array = new Func<TSource, bool>[_predicates.Length + 1];
			Array.Copy(_predicates, array, _predicates.Length);
			array[_predicates.Length] = predicate;
			return new CombinedPredicatesN<TSource>(array);
		}

		public bool Invoke(TSource x)
		{
			Func<TSource, bool>[] predicates = _predicates;
			for (int i = 0; i < predicates.Length; i++)
			{
				if (!predicates[i](x))
				{
					return false;
				}
			}
			return true;
		}
	}

	private interface ICombinedAsyncPredicates<TSource>
	{
		ICombinedAsyncPredicates<TSource> And(Func<TSource, ValueTask<bool>> predicate);

		ValueTask<bool> Invoke(TSource x);
	}

	private sealed class CombinedAsyncPredicatesN<TSource> : ICombinedAsyncPredicates<TSource>
	{
		private readonly Func<TSource, ValueTask<bool>>[] _predicates;

		public CombinedAsyncPredicatesN(params Func<TSource, ValueTask<bool>>[] predicates)
		{
			_predicates = predicates;
		}

		public ICombinedAsyncPredicates<TSource> And(Func<TSource, ValueTask<bool>> predicate)
		{
			Func<TSource, ValueTask<bool>>[] array = new Func<TSource, ValueTask<bool>>[_predicates.Length + 1];
			Array.Copy(_predicates, array, _predicates.Length);
			array[_predicates.Length] = predicate;
			return new CombinedAsyncPredicatesN<TSource>(array);
		}

		public async ValueTask<bool> Invoke(TSource x)
		{
			Func<TSource, ValueTask<bool>>[] predicates = _predicates;
			for (int i = 0; i < predicates.Length; i++)
			{
				if (!(await predicates[i](x).ConfigureAwait(continueOnCapturedContext: false)))
				{
					return false;
				}
			}
			return true;
		}
	}

	private interface ICombinedAsyncPredicatesWithCancellation<TSource>
	{
		ICombinedAsyncPredicatesWithCancellation<TSource> And(Func<TSource, CancellationToken, ValueTask<bool>> predicate);

		ValueTask<bool> Invoke(TSource x, CancellationToken ct);
	}

	private sealed class CombinedAsyncPredicatesWithCancellationN<TSource> : ICombinedAsyncPredicatesWithCancellation<TSource>
	{
		private readonly Func<TSource, CancellationToken, ValueTask<bool>>[] _predicates;

		public CombinedAsyncPredicatesWithCancellationN(params Func<TSource, CancellationToken, ValueTask<bool>>[] predicates)
		{
			_predicates = predicates;
		}

		public ICombinedAsyncPredicatesWithCancellation<TSource> And(Func<TSource, CancellationToken, ValueTask<bool>> predicate)
		{
			Func<TSource, CancellationToken, ValueTask<bool>>[] array = new Func<TSource, CancellationToken, ValueTask<bool>>[_predicates.Length + 1];
			Array.Copy(_predicates, array, _predicates.Length);
			array[_predicates.Length] = predicate;
			return new CombinedAsyncPredicatesWithCancellationN<TSource>(array);
		}

		public async ValueTask<bool> Invoke(TSource x, CancellationToken ct)
		{
			Func<TSource, CancellationToken, ValueTask<bool>>[] predicates = _predicates;
			for (int i = 0; i < predicates.Length; i++)
			{
				if (!(await predicates[i](x, ct).ConfigureAwait(continueOnCapturedContext: false)))
				{
					return false;
				}
			}
			return true;
		}
	}

	private sealed class CombinedPredicates2<TSource> : ICombinedPredicates<TSource>
	{
		private readonly Func<TSource, bool> _predicate1;

		private readonly Func<TSource, bool> _predicate2;

		public CombinedPredicates2(Func<TSource, bool> predicate1, Func<TSource, bool> predicate2)
		{
			_predicate1 = predicate1;
			_predicate2 = predicate2;
		}

		public ICombinedPredicates<TSource> And(Func<TSource, bool> predicate)
		{
			return new CombinedPredicates3<TSource>(_predicate1, _predicate2, predicate);
		}

		public bool Invoke(TSource x)
		{
			if (_predicate1(x))
			{
				return _predicate2(x);
			}
			return false;
		}
	}

	private sealed class CombinedPredicates3<TSource> : ICombinedPredicates<TSource>
	{
		private readonly Func<TSource, bool> _predicate1;

		private readonly Func<TSource, bool> _predicate2;

		private readonly Func<TSource, bool> _predicate3;

		public CombinedPredicates3(Func<TSource, bool> predicate1, Func<TSource, bool> predicate2, Func<TSource, bool> predicate3)
		{
			_predicate1 = predicate1;
			_predicate2 = predicate2;
			_predicate3 = predicate3;
		}

		public ICombinedPredicates<TSource> And(Func<TSource, bool> predicate)
		{
			return new CombinedPredicates4<TSource>(_predicate1, _predicate2, _predicate3, predicate);
		}

		public bool Invoke(TSource x)
		{
			if (_predicate1(x) && _predicate2(x))
			{
				return _predicate3(x);
			}
			return false;
		}
	}

	private sealed class CombinedPredicates4<TSource> : ICombinedPredicates<TSource>
	{
		private readonly Func<TSource, bool> _predicate1;

		private readonly Func<TSource, bool> _predicate2;

		private readonly Func<TSource, bool> _predicate3;

		private readonly Func<TSource, bool> _predicate4;

		public CombinedPredicates4(Func<TSource, bool> predicate1, Func<TSource, bool> predicate2, Func<TSource, bool> predicate3, Func<TSource, bool> predicate4)
		{
			_predicate1 = predicate1;
			_predicate2 = predicate2;
			_predicate3 = predicate3;
			_predicate4 = predicate4;
		}

		public ICombinedPredicates<TSource> And(Func<TSource, bool> predicate)
		{
			return new CombinedPredicatesN<TSource>(_predicate1, _predicate2, _predicate3, _predicate4, predicate);
		}

		public bool Invoke(TSource x)
		{
			if (_predicate1(x) && _predicate2(x) && _predicate3(x))
			{
				return _predicate4(x);
			}
			return false;
		}
	}

	private sealed class CombinedAsyncPredicates2<TSource> : ICombinedAsyncPredicates<TSource>
	{
		private readonly Func<TSource, ValueTask<bool>> _predicate1;

		private readonly Func<TSource, ValueTask<bool>> _predicate2;

		public CombinedAsyncPredicates2(Func<TSource, ValueTask<bool>> predicate1, Func<TSource, ValueTask<bool>> predicate2)
		{
			_predicate1 = predicate1;
			_predicate2 = predicate2;
		}

		public ICombinedAsyncPredicates<TSource> And(Func<TSource, ValueTask<bool>> predicate)
		{
			return new CombinedAsyncPredicates3<TSource>(_predicate1, _predicate2, predicate);
		}

		public async ValueTask<bool> Invoke(TSource x)
		{
			bool flag = await _predicate1(x).ConfigureAwait(continueOnCapturedContext: false);
			if (flag)
			{
				flag = await _predicate2(x).ConfigureAwait(continueOnCapturedContext: false);
			}
			return flag;
		}
	}

	private sealed class CombinedAsyncPredicates3<TSource> : ICombinedAsyncPredicates<TSource>
	{
		private readonly Func<TSource, ValueTask<bool>> _predicate1;

		private readonly Func<TSource, ValueTask<bool>> _predicate2;

		private readonly Func<TSource, ValueTask<bool>> _predicate3;

		public CombinedAsyncPredicates3(Func<TSource, ValueTask<bool>> predicate1, Func<TSource, ValueTask<bool>> predicate2, Func<TSource, ValueTask<bool>> predicate3)
		{
			_predicate1 = predicate1;
			_predicate2 = predicate2;
			_predicate3 = predicate3;
		}

		public ICombinedAsyncPredicates<TSource> And(Func<TSource, ValueTask<bool>> predicate)
		{
			return new CombinedAsyncPredicates4<TSource>(_predicate1, _predicate2, _predicate3, predicate);
		}

		public async ValueTask<bool> Invoke(TSource x)
		{
			bool flag = await _predicate1(x).ConfigureAwait(continueOnCapturedContext: false);
			if (flag)
			{
				flag = await _predicate2(x).ConfigureAwait(continueOnCapturedContext: false);
			}
			bool flag2 = flag;
			if (flag2)
			{
				flag2 = await _predicate3(x).ConfigureAwait(continueOnCapturedContext: false);
			}
			return flag2;
		}
	}

	private sealed class CombinedAsyncPredicates4<TSource> : ICombinedAsyncPredicates<TSource>
	{
		private readonly Func<TSource, ValueTask<bool>> _predicate1;

		private readonly Func<TSource, ValueTask<bool>> _predicate2;

		private readonly Func<TSource, ValueTask<bool>> _predicate3;

		private readonly Func<TSource, ValueTask<bool>> _predicate4;

		public CombinedAsyncPredicates4(Func<TSource, ValueTask<bool>> predicate1, Func<TSource, ValueTask<bool>> predicate2, Func<TSource, ValueTask<bool>> predicate3, Func<TSource, ValueTask<bool>> predicate4)
		{
			_predicate1 = predicate1;
			_predicate2 = predicate2;
			_predicate3 = predicate3;
			_predicate4 = predicate4;
		}

		public ICombinedAsyncPredicates<TSource> And(Func<TSource, ValueTask<bool>> predicate)
		{
			return new CombinedAsyncPredicatesN<TSource>(_predicate1, _predicate2, _predicate3, _predicate4, predicate);
		}

		public async ValueTask<bool> Invoke(TSource x)
		{
			bool flag = await _predicate1(x).ConfigureAwait(continueOnCapturedContext: false);
			if (flag)
			{
				flag = await _predicate2(x).ConfigureAwait(continueOnCapturedContext: false);
			}
			bool flag2 = flag;
			if (flag2)
			{
				flag2 = await _predicate3(x).ConfigureAwait(continueOnCapturedContext: false);
			}
			bool flag3 = flag2;
			if (flag3)
			{
				flag3 = await _predicate4(x).ConfigureAwait(continueOnCapturedContext: false);
			}
			return flag3;
		}
	}

	private sealed class CombinedAsyncPredicatesWithCancellation2<TSource> : ICombinedAsyncPredicatesWithCancellation<TSource>
	{
		private readonly Func<TSource, CancellationToken, ValueTask<bool>> _predicate1;

		private readonly Func<TSource, CancellationToken, ValueTask<bool>> _predicate2;

		public CombinedAsyncPredicatesWithCancellation2(Func<TSource, CancellationToken, ValueTask<bool>> predicate1, Func<TSource, CancellationToken, ValueTask<bool>> predicate2)
		{
			_predicate1 = predicate1;
			_predicate2 = predicate2;
		}

		public ICombinedAsyncPredicatesWithCancellation<TSource> And(Func<TSource, CancellationToken, ValueTask<bool>> predicate)
		{
			return new CombinedAsyncPredicatesWithCancellation3<TSource>(_predicate1, _predicate2, predicate);
		}

		public async ValueTask<bool> Invoke(TSource x, CancellationToken ct)
		{
			bool flag = await _predicate1(x, ct).ConfigureAwait(continueOnCapturedContext: false);
			if (flag)
			{
				flag = await _predicate2(x, ct).ConfigureAwait(continueOnCapturedContext: false);
			}
			return flag;
		}
	}

	private sealed class CombinedAsyncPredicatesWithCancellation3<TSource> : ICombinedAsyncPredicatesWithCancellation<TSource>
	{
		private readonly Func<TSource, CancellationToken, ValueTask<bool>> _predicate1;

		private readonly Func<TSource, CancellationToken, ValueTask<bool>> _predicate2;

		private readonly Func<TSource, CancellationToken, ValueTask<bool>> _predicate3;

		public CombinedAsyncPredicatesWithCancellation3(Func<TSource, CancellationToken, ValueTask<bool>> predicate1, Func<TSource, CancellationToken, ValueTask<bool>> predicate2, Func<TSource, CancellationToken, ValueTask<bool>> predicate3)
		{
			_predicate1 = predicate1;
			_predicate2 = predicate2;
			_predicate3 = predicate3;
		}

		public ICombinedAsyncPredicatesWithCancellation<TSource> And(Func<TSource, CancellationToken, ValueTask<bool>> predicate)
		{
			return new CombinedAsyncPredicatesWithCancellation4<TSource>(_predicate1, _predicate2, _predicate3, predicate);
		}

		public async ValueTask<bool> Invoke(TSource x, CancellationToken ct)
		{
			bool flag = await _predicate1(x, ct).ConfigureAwait(continueOnCapturedContext: false);
			if (flag)
			{
				flag = await _predicate2(x, ct).ConfigureAwait(continueOnCapturedContext: false);
			}
			bool flag2 = flag;
			if (flag2)
			{
				flag2 = await _predicate3(x, ct).ConfigureAwait(continueOnCapturedContext: false);
			}
			return flag2;
		}
	}

	private sealed class CombinedAsyncPredicatesWithCancellation4<TSource> : ICombinedAsyncPredicatesWithCancellation<TSource>
	{
		private readonly Func<TSource, CancellationToken, ValueTask<bool>> _predicate1;

		private readonly Func<TSource, CancellationToken, ValueTask<bool>> _predicate2;

		private readonly Func<TSource, CancellationToken, ValueTask<bool>> _predicate3;

		private readonly Func<TSource, CancellationToken, ValueTask<bool>> _predicate4;

		public CombinedAsyncPredicatesWithCancellation4(Func<TSource, CancellationToken, ValueTask<bool>> predicate1, Func<TSource, CancellationToken, ValueTask<bool>> predicate2, Func<TSource, CancellationToken, ValueTask<bool>> predicate3, Func<TSource, CancellationToken, ValueTask<bool>> predicate4)
		{
			_predicate1 = predicate1;
			_predicate2 = predicate2;
			_predicate3 = predicate3;
			_predicate4 = predicate4;
		}

		public ICombinedAsyncPredicatesWithCancellation<TSource> And(Func<TSource, CancellationToken, ValueTask<bool>> predicate)
		{
			return new CombinedAsyncPredicatesWithCancellationN<TSource>(_predicate1, _predicate2, _predicate3, _predicate4, predicate);
		}

		public async ValueTask<bool> Invoke(TSource x, CancellationToken ct)
		{
			bool flag = await _predicate1(x, ct).ConfigureAwait(continueOnCapturedContext: false);
			if (flag)
			{
				flag = await _predicate2(x, ct).ConfigureAwait(continueOnCapturedContext: false);
			}
			bool flag2 = flag;
			if (flag2)
			{
				flag2 = await _predicate3(x, ct).ConfigureAwait(continueOnCapturedContext: false);
			}
			bool flag3 = flag2;
			if (flag3)
			{
				flag3 = await _predicate4(x, ct).ConfigureAwait(continueOnCapturedContext: false);
			}
			return flag3;
		}
	}

	public static IAsyncEnumerable<T> Create<T>(Func<CancellationToken, IAsyncEnumerator<T>> getAsyncEnumerator)
	{
		if (getAsyncEnumerator == null)
		{
			throw System.Error.ArgumentNull("getAsyncEnumerator");
		}
		return new AnonymousAsyncEnumerable<T>(getAsyncEnumerator);
	}

	public static ValueTask<TSource> AggregateAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, TSource, TSource> accumulator, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw System.Error.ArgumentNull("accumulator");
		}
		return Core(source, accumulator, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, TSource, TSource> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TSource result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				TSource acc = e.Current;
				while (await e.MoveNextAsync())
				{
					acc = func(acc, e.Current);
				}
				result = acc;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	[Obsolete("Use AggregateAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AggregateAwaitAsync now exists as overloads of AggregateAsync.")]
	private static ValueTask<TSource> AggregateAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, TSource, ValueTask<TSource>> accumulator, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw System.Error.ArgumentNull("accumulator");
		}
		return Core(source, accumulator, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, TSource, ValueTask<TSource>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TSource result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				TSource acc = e.Current;
				while (await e.MoveNextAsync())
				{
					acc = await func(acc, e.Current).ConfigureAwait(continueOnCapturedContext: false);
				}
				result = acc;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	[Obsolete("Use AggregateAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AggregateAwaitWithCancellationAsync functionality now exists as overloads of AggregateAsync.")]
	private static ValueTask<TSource> AggregateAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, TSource, CancellationToken, ValueTask<TSource>> accumulator, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw System.Error.ArgumentNull("accumulator");
		}
		return Core(source, accumulator, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, TSource, CancellationToken, ValueTask<TSource>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TSource result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				TSource acc = e.Current;
				while (await e.MoveNextAsync())
				{
					acc = await func(acc, e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				}
				result = acc;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	public static ValueTask<TAccumulate> AggregateAsync<TSource, TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw System.Error.ArgumentNull("accumulator");
		}
		return Core(source, seed, accumulator, cancellationToken);
		static async ValueTask<TAccumulate> Core(IAsyncEnumerable<TSource> source2, TAccumulate val, Func<TAccumulate, TSource, TAccumulate> func, CancellationToken cancellationToken2)
		{
			TAccumulate acc = val;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				acc = func(acc, item);
			}
			return acc;
		}
	}

	[Obsolete("Use AggregateAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AggregateAwaitAsync functionality now exists as overloads of AggregateAsync.")]
	private static ValueTask<TAccumulate> AggregateAwaitAsyncCore<TSource, TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, ValueTask<TAccumulate>> accumulator, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw System.Error.ArgumentNull("accumulator");
		}
		return Core(source, seed, accumulator, cancellationToken);
		static async ValueTask<TAccumulate> Core(IAsyncEnumerable<TSource> source2, TAccumulate val, Func<TAccumulate, TSource, ValueTask<TAccumulate>> func, CancellationToken cancellationToken2)
		{
			TAccumulate acc = val;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				acc = await func(acc, item).ConfigureAwait(continueOnCapturedContext: false);
			}
			return acc;
		}
	}

	[Obsolete("Use AggregateAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AggregateAwaitWithCancellationAsync functionality now exists as overloads of AggregateAsync.")]
	private static ValueTask<TAccumulate> AggregateAwaitWithCancellationAsyncCore<TSource, TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> accumulator, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw System.Error.ArgumentNull("accumulator");
		}
		return Core(source, seed, accumulator, cancellationToken);
		static async ValueTask<TAccumulate> Core(IAsyncEnumerable<TSource> source2, TAccumulate val, Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> func, CancellationToken cancellationToken2)
		{
			TAccumulate acc = val;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				acc = await func(acc, item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			}
			return acc;
		}
	}

	public static ValueTask<TResult> AggregateAsync<TSource, TAccumulate, TResult>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator, Func<TAccumulate, TResult> resultSelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw System.Error.ArgumentNull("accumulator");
		}
		if (resultSelector == null)
		{
			throw System.Error.ArgumentNull("resultSelector");
		}
		return Core(source, seed, accumulator, resultSelector, cancellationToken);
		static async ValueTask<TResult> Core(IAsyncEnumerable<TSource> source2, TAccumulate val, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> func2, CancellationToken cancellationToken2)
		{
			TAccumulate acc = val;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				acc = func(acc, item);
			}
			return func2(acc);
		}
	}

	[Obsolete("Use AggregateAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AggregateAwaitAsync functionality now exists as overloads of AggregateAsync.")]
	private static ValueTask<TResult> AggregateAwaitAsyncCore<TSource, TAccumulate, TResult>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, ValueTask<TAccumulate>> accumulator, Func<TAccumulate, ValueTask<TResult>> resultSelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw System.Error.ArgumentNull("accumulator");
		}
		if (resultSelector == null)
		{
			throw System.Error.ArgumentNull("resultSelector");
		}
		return Core(source, seed, accumulator, resultSelector, cancellationToken);
		static async ValueTask<TResult> Core(IAsyncEnumerable<TSource> source2, TAccumulate val, Func<TAccumulate, TSource, ValueTask<TAccumulate>> func, Func<TAccumulate, ValueTask<TResult>> func2, CancellationToken cancellationToken2)
		{
			TAccumulate acc = val;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				acc = await func(acc, item).ConfigureAwait(continueOnCapturedContext: false);
			}
			return await func2(acc).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	[Obsolete("Use AggregateAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AggregateAwaitWithCancellationAsync functionality now exists as overloads of AggregateAsync.")]
	private static ValueTask<TResult> AggregateAwaitWithCancellationAsyncCore<TSource, TAccumulate, TResult>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> accumulator, Func<TAccumulate, CancellationToken, ValueTask<TResult>> resultSelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw System.Error.ArgumentNull("accumulator");
		}
		if (resultSelector == null)
		{
			throw System.Error.ArgumentNull("resultSelector");
		}
		return Core(source, seed, accumulator, resultSelector, cancellationToken);
		static async ValueTask<TResult> Core(IAsyncEnumerable<TSource> source2, TAccumulate val, Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> func, Func<TAccumulate, CancellationToken, ValueTask<TResult>> func2, CancellationToken cancellationToken2)
		{
			TAccumulate acc = val;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				acc = await func(acc, item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			}
			return await func2(acc, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public static ValueTask<bool> AllAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<bool> Core(IAsyncEnumerable<TSource> source2, Func<TSource, bool> func, CancellationToken cancellationToken2)
		{
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (!func(item))
				{
					return false;
				}
			}
			return true;
		}
	}

	[Obsolete("Use AllAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AllAwaitAsync functionality now exists as overloads of AllAsync.")]
	private static ValueTask<bool> AllAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<bool> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<bool>> func, CancellationToken cancellationToken2)
		{
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (!(await func(item).ConfigureAwait(continueOnCapturedContext: false)))
				{
					return false;
				}
			}
			return true;
		}
	}

	[Obsolete("Use AllAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AllAwaitWithCancellationAsync functionality now exists as overloads of AllAsync.")]
	internal static ValueTask<bool> AllAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<bool> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<bool>> func, CancellationToken cancellationToken2)
		{
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (!(await func(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false)))
				{
					return false;
				}
			}
			return true;
		}
	}

	public static ValueTask<bool> AnyAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<bool> Core(IAsyncEnumerable<TSource> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			bool result;
			try
			{
				result = await e.MoveNextAsync();
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	public static ValueTask<bool> AnyAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<bool> Core(IAsyncEnumerable<TSource> source2, Func<TSource, bool> func, CancellationToken cancellationToken2)
		{
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (func(item))
				{
					return true;
				}
			}
			return false;
		}
	}

	[Obsolete("Use AnyAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AnyAwaitAsync functionality now exists as overloads of AnyAsync.")]
	private static ValueTask<bool> AnyAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<bool> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<bool>> func, CancellationToken cancellationToken2)
		{
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (await func(item).ConfigureAwait(continueOnCapturedContext: false))
				{
					return true;
				}
			}
			return false;
		}
	}

	[Obsolete("Use AnyAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AnyAwaitWithCancellationAsync functionality now exists as overloads of AnyAsync.")]
	internal static ValueTask<bool> AnyAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<bool> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<bool>> func, CancellationToken cancellationToken2)
		{
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (await func(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
				{
					return true;
				}
			}
			return false;
		}
	}

	public static IAsyncEnumerable<TSource> Append<TSource>(this IAsyncEnumerable<TSource> source, TSource element)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (source is AppendPrependAsyncIterator<TSource> appendPrependAsyncIterator)
		{
			return appendPrependAsyncIterator.Append(element);
		}
		return new AppendPrepend1AsyncIterator<TSource>(source, element, appending: true);
	}

	public static IAsyncEnumerable<TSource> Prepend<TSource>(this IAsyncEnumerable<TSource> source, TSource element)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (source is AppendPrependAsyncIterator<TSource> appendPrependAsyncIterator)
		{
			return appendPrependAsyncIterator.Prepend(element);
		}
		return new AppendPrepend1AsyncIterator<TSource>(source, element, appending: false);
	}

	public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(this IAsyncEnumerable<TSource> source)
	{
		return source;
	}

	public static ValueTask<double> AverageAsync(this IAsyncEnumerable<int> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<int> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<int>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			checked
			{
				double result;
				try
				{
					if (!(await e.MoveNextAsync()))
					{
						throw System.Error.NoElements();
					}
					long sum = e.Current;
					long count = 1L;
					while (await e.MoveNextAsync())
					{
						sum += e.Current;
						long num = count + 1;
						count = num;
					}
					result = (double)sum / (double)count;
				}
				finally
				{
					IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
				return result;
			}
		}
	}

	public static ValueTask<double> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, int> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			checked
			{
				double result;
				try
				{
					if (!(await e.MoveNextAsync()))
					{
						throw System.Error.NoElements();
					}
					long sum = func(e.Current);
					long count = 1L;
					while (await e.MoveNextAsync())
					{
						sum += func(e.Current);
						long num = count + 1;
						count = num;
					}
					result = (double)sum / (double)count;
				}
				finally
				{
					IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
				return result;
			}
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<double> AverageAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<int>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			checked
			{
				double result;
				try
				{
					if (!(await e.MoveNextAsync()))
					{
						throw System.Error.NoElements();
					}
					long sum = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					long count = 1L;
					while (await e.MoveNextAsync())
					{
						long num = sum;
						sum = num + await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
						long num2 = count + 1;
						count = num2;
					}
					result = (double)sum / (double)count;
				}
				finally
				{
					IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
				return result;
			}
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	private static ValueTask<double> AverageAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<int>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			checked
			{
				double result;
				try
				{
					if (!(await e.MoveNextAsync()))
					{
						throw System.Error.NoElements();
					}
					long sum = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					long count = 1L;
					while (await e.MoveNextAsync())
					{
						long num = sum;
						sum = num + await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
						long num2 = count + 1;
						count = num2;
					}
					result = (double)sum / (double)count;
				}
				finally
				{
					IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
				return result;
			}
		}
	}

	public static ValueTask<double> AverageAsync(this IAsyncEnumerable<long> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<long> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<long>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			checked
			{
				double result;
				try
				{
					if (!(await e.MoveNextAsync()))
					{
						throw System.Error.NoElements();
					}
					long sum = e.Current;
					long count = 1L;
					while (await e.MoveNextAsync())
					{
						sum += e.Current;
						long num = count + 1;
						count = num;
					}
					result = (double)sum / (double)count;
				}
				finally
				{
					IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
				return result;
			}
		}
	}

	public static ValueTask<double> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, long> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, long> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			checked
			{
				double result;
				try
				{
					if (!(await e.MoveNextAsync()))
					{
						throw System.Error.NoElements();
					}
					long sum = func(e.Current);
					long count = 1L;
					while (await e.MoveNextAsync())
					{
						sum += func(e.Current);
						long num = count + 1;
						count = num;
					}
					result = (double)sum / (double)count;
				}
				finally
				{
					IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
				return result;
			}
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<double> AverageAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<long>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			checked
			{
				double result;
				try
				{
					if (!(await e.MoveNextAsync()))
					{
						throw System.Error.NoElements();
					}
					long sum = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					long count = 1L;
					while (await e.MoveNextAsync())
					{
						long num = sum;
						sum = num + await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
						long num2 = count + 1;
						count = num2;
					}
					result = (double)sum / (double)count;
				}
				finally
				{
					IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
				return result;
			}
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	private static ValueTask<double> AverageAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<long>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			checked
			{
				double result;
				try
				{
					if (!(await e.MoveNextAsync()))
					{
						throw System.Error.NoElements();
					}
					long sum = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					long count = 1L;
					while (await e.MoveNextAsync())
					{
						long num = sum;
						sum = num + await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
						long num2 = count + 1;
						count = num2;
					}
					result = (double)sum / (double)count;
				}
				finally
				{
					IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
				return result;
			}
		}
	}

	public static ValueTask<float> AverageAsync(this IAsyncEnumerable<float> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<float> Core(IAsyncEnumerable<float> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<float>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				double sum = e.Current;
				long count = 1L;
				while (await e.MoveNextAsync())
				{
					sum += (double)e.Current;
					long num = checked(count + 1);
					count = num;
				}
				result = (float)(sum / (double)count);
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	public static ValueTask<float> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, float> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, float> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				double sum = func(e.Current);
				long count = 1L;
				while (await e.MoveNextAsync())
				{
					sum += (double)func(e.Current);
					long num = checked(count + 1);
					count = num;
				}
				result = (float)(sum / (double)count);
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<float> AverageAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<float>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				double sum = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				long count = 1L;
				while (await e.MoveNextAsync())
				{
					double num = sum;
					sum = num + (double)(await func(e.Current).ConfigureAwait(continueOnCapturedContext: false));
					long num2 = checked(count + 1);
					count = num2;
				}
				result = (float)(sum / (double)count);
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	private static ValueTask<float> AverageAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<float>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				double sum = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				long count = 1L;
				while (await e.MoveNextAsync())
				{
					double num = sum;
					sum = num + (double)(await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false));
					long num2 = checked(count + 1);
					count = num2;
				}
				result = (float)(sum / (double)count);
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	public static ValueTask<double> AverageAsync(this IAsyncEnumerable<double> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<double> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<double>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				double sum = e.Current;
				long count = 1L;
				while (await e.MoveNextAsync())
				{
					sum += e.Current;
					long num = checked(count + 1);
					count = num;
				}
				result = sum / (double)count;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	public static ValueTask<double> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, double> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, double> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				double sum = func(e.Current);
				long count = 1L;
				while (await e.MoveNextAsync())
				{
					sum += func(e.Current);
					long num = checked(count + 1);
					count = num;
				}
				result = sum / (double)count;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<double> AverageAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<double>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				double sum = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				long count = 1L;
				while (await e.MoveNextAsync())
				{
					double num = sum;
					sum = num + await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					long num2 = checked(count + 1);
					count = num2;
				}
				result = sum / (double)count;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	private static ValueTask<double> AverageAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<double>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				double sum = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				long count = 1L;
				while (await e.MoveNextAsync())
				{
					double num = sum;
					sum = num + await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					long num2 = checked(count + 1);
					count = num2;
				}
				result = sum / (double)count;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	public static ValueTask<decimal> AverageAsync(this IAsyncEnumerable<decimal> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<decimal> Core(IAsyncEnumerable<decimal> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<decimal>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			decimal result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				decimal sum = e.Current;
				long count = 1L;
				while (await e.MoveNextAsync())
				{
					sum += e.Current;
					long num = checked(count + 1);
					count = num;
				}
				result = sum / (decimal)count;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	public static ValueTask<decimal> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, decimal> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, decimal> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			decimal result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				decimal sum = func(e.Current);
				long count = 1L;
				while (await e.MoveNextAsync())
				{
					sum += func(e.Current);
					long num = checked(count + 1);
					count = num;
				}
				result = sum / (decimal)count;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<decimal> AverageAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<decimal>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			decimal result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				decimal sum = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				long count = 1L;
				while (await e.MoveNextAsync())
				{
					decimal num = sum;
					sum = num + await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					long num2 = checked(count + 1);
					count = num2;
				}
				result = sum / (decimal)count;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	private static ValueTask<decimal> AverageAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<decimal>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			decimal result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				decimal sum = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				long count = 1L;
				while (await e.MoveNextAsync())
				{
					decimal num = sum;
					sum = num + await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					long num2 = checked(count + 1);
					count = num2;
				}
				result = sum / (decimal)count;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	public static ValueTask<double?> AverageAsync(this IAsyncEnumerable<int?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<int?> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<int?>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			checked
			{
				try
				{
					while (await e.MoveNextAsync())
					{
						int? current = e.Current;
						if (current.HasValue)
						{
							long sum = current.GetValueOrDefault();
							long count = 1L;
							while (await e.MoveNextAsync())
							{
								current = e.Current;
								if (current.HasValue)
								{
									sum += current.GetValueOrDefault();
									long num = count + 1;
									count = num;
								}
							}
							return (double)sum / (double)count;
						}
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
				return null;
			}
		}
	}

	public static ValueTask<double?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, int?> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			checked
			{
				try
				{
					while (await e.MoveNextAsync())
					{
						int? num = func(e.Current);
						if (num.HasValue)
						{
							long sum = num.GetValueOrDefault();
							long count = 1L;
							while (await e.MoveNextAsync())
							{
								num = func(e.Current);
								if (num.HasValue)
								{
									sum += num.GetValueOrDefault();
									long num2 = count + 1;
									count = num2;
								}
							}
							return (double)sum / (double)count;
						}
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
				return null;
			}
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<double?> AverageAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<int?>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			checked
			{
				try
				{
					while (await e.MoveNextAsync())
					{
						int? num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
						if (num.HasValue)
						{
							long sum = num.GetValueOrDefault();
							long count = 1L;
							while (await e.MoveNextAsync())
							{
								num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
								if (num.HasValue)
								{
									sum += num.GetValueOrDefault();
									long num2 = count + 1;
									count = num2;
								}
							}
							return (double)sum / (double)count;
						}
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
				return null;
			}
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	private static ValueTask<double?> AverageAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<int?>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			checked
			{
				try
				{
					while (await e.MoveNextAsync())
					{
						int? num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
						if (num.HasValue)
						{
							long sum = num.GetValueOrDefault();
							long count = 1L;
							while (await e.MoveNextAsync())
							{
								num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
								if (num.HasValue)
								{
									sum += num.GetValueOrDefault();
									long num2 = count + 1;
									count = num2;
								}
							}
							return (double)sum / (double)count;
						}
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
				return null;
			}
		}
	}

	public static ValueTask<double?> AverageAsync(this IAsyncEnumerable<long?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<long?> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<long?>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			checked
			{
				try
				{
					while (await e.MoveNextAsync())
					{
						long? current = e.Current;
						if (current.HasValue)
						{
							long sum = current.GetValueOrDefault();
							long count = 1L;
							while (await e.MoveNextAsync())
							{
								current = e.Current;
								if (current.HasValue)
								{
									sum += current.GetValueOrDefault();
									long num = count + 1;
									count = num;
								}
							}
							return (double)sum / (double)count;
						}
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
				return null;
			}
		}
	}

	public static ValueTask<double?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, long?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, long?> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			checked
			{
				try
				{
					while (await e.MoveNextAsync())
					{
						long? num = func(e.Current);
						if (num.HasValue)
						{
							long sum = num.GetValueOrDefault();
							long count = 1L;
							while (await e.MoveNextAsync())
							{
								num = func(e.Current);
								if (num.HasValue)
								{
									sum += num.GetValueOrDefault();
									long num2 = count + 1;
									count = num2;
								}
							}
							return (double)sum / (double)count;
						}
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
				return null;
			}
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<double?> AverageAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<long?>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			checked
			{
				try
				{
					while (await e.MoveNextAsync())
					{
						long? num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
						if (num.HasValue)
						{
							long sum = num.GetValueOrDefault();
							long count = 1L;
							while (await e.MoveNextAsync())
							{
								num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
								if (num.HasValue)
								{
									sum += num.GetValueOrDefault();
									long num2 = count + 1;
									count = num2;
								}
							}
							return (double)sum / (double)count;
						}
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
				return null;
			}
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	private static ValueTask<double?> AverageAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<long?>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			checked
			{
				try
				{
					while (await e.MoveNextAsync())
					{
						long? num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
						if (num.HasValue)
						{
							long sum = num.GetValueOrDefault();
							long count = 1L;
							while (await e.MoveNextAsync())
							{
								num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
								if (num.HasValue)
								{
									sum += num.GetValueOrDefault();
									long num2 = count + 1;
									count = num2;
								}
							}
							return (double)sum / (double)count;
						}
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
				return null;
			}
		}
	}

	public static ValueTask<float?> AverageAsync(this IAsyncEnumerable<float?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<float?> Core(IAsyncEnumerable<float?> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<float?>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					float? current = e.Current;
					if (current.HasValue)
					{
						double sum = current.GetValueOrDefault();
						long count = 1L;
						while (await e.MoveNextAsync())
						{
							current = e.Current;
							if (current.HasValue)
							{
								sum += (double)current.GetValueOrDefault();
								long num = checked(count + 1);
								count = num;
							}
						}
						return (float)(sum / (double)count);
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return null;
		}
	}

	public static ValueTask<float?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, float?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, float?> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					float? num = func(e.Current);
					if (num.HasValue)
					{
						double sum = num.GetValueOrDefault();
						long count = 1L;
						while (await e.MoveNextAsync())
						{
							num = func(e.Current);
							if (num.HasValue)
							{
								sum += (double)num.GetValueOrDefault();
								long num2 = checked(count + 1);
								count = num2;
							}
						}
						return (float)(sum / (double)count);
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return null;
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<float?> AverageAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<float?>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					float? num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (num.HasValue)
					{
						double sum = num.GetValueOrDefault();
						long count = 1L;
						while (await e.MoveNextAsync())
						{
							num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
							if (num.HasValue)
							{
								sum += (double)num.GetValueOrDefault();
								long num2 = checked(count + 1);
								count = num2;
							}
						}
						return (float)(sum / (double)count);
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return null;
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	private static ValueTask<float?> AverageAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<float?>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					float? num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (num.HasValue)
					{
						double sum = num.GetValueOrDefault();
						long count = 1L;
						while (await e.MoveNextAsync())
						{
							num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
							if (num.HasValue)
							{
								sum += (double)num.GetValueOrDefault();
								long num2 = checked(count + 1);
								count = num2;
							}
						}
						return (float)(sum / (double)count);
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return null;
		}
	}

	public static ValueTask<double?> AverageAsync(this IAsyncEnumerable<double?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<double?> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<double?>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					double? current = e.Current;
					if (current.HasValue)
					{
						double sum = current.GetValueOrDefault();
						long count = 1L;
						while (await e.MoveNextAsync())
						{
							current = e.Current;
							if (current.HasValue)
							{
								sum += current.GetValueOrDefault();
								long num = checked(count + 1);
								count = num;
							}
						}
						return sum / (double)count;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return null;
		}
	}

	public static ValueTask<double?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, double?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, double?> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					double? num = func(e.Current);
					if (num.HasValue)
					{
						double sum = num.GetValueOrDefault();
						long count = 1L;
						while (await e.MoveNextAsync())
						{
							num = func(e.Current);
							if (num.HasValue)
							{
								sum += num.GetValueOrDefault();
								long num2 = checked(count + 1);
								count = num2;
							}
						}
						return sum / (double)count;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return null;
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<double?> AverageAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<double?>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					double? num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (num.HasValue)
					{
						double sum = num.GetValueOrDefault();
						long count = 1L;
						while (await e.MoveNextAsync())
						{
							num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
							if (num.HasValue)
							{
								sum += num.GetValueOrDefault();
								long num2 = checked(count + 1);
								count = num2;
							}
						}
						return sum / (double)count;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return null;
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	private static ValueTask<double?> AverageAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<double?>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					double? num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (num.HasValue)
					{
						double sum = num.GetValueOrDefault();
						long count = 1L;
						while (await e.MoveNextAsync())
						{
							num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
							if (num.HasValue)
							{
								sum += num.GetValueOrDefault();
								long num2 = checked(count + 1);
								count = num2;
							}
						}
						return sum / (double)count;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return null;
		}
	}

	public static ValueTask<decimal?> AverageAsync(this IAsyncEnumerable<decimal?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<decimal?> Core(IAsyncEnumerable<decimal?> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<decimal?>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					decimal? current = e.Current;
					if (current.HasValue)
					{
						decimal sum = current.GetValueOrDefault();
						long count = 1L;
						while (await e.MoveNextAsync())
						{
							current = e.Current;
							if (current.HasValue)
							{
								sum += current.GetValueOrDefault();
								long num = checked(count + 1);
								count = num;
							}
						}
						return sum / (decimal)count;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return null;
		}
	}

	public static ValueTask<decimal?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, decimal?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, decimal?> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					decimal? num = func(e.Current);
					if (num.HasValue)
					{
						decimal sum = num.GetValueOrDefault();
						long count = 1L;
						while (await e.MoveNextAsync())
						{
							num = func(e.Current);
							if (num.HasValue)
							{
								sum += num.GetValueOrDefault();
								long num2 = checked(count + 1);
								count = num2;
							}
						}
						return sum / (decimal)count;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return null;
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<decimal?> AverageAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<decimal?>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					decimal? num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (num.HasValue)
					{
						decimal sum = num.GetValueOrDefault();
						long count = 1L;
						while (await e.MoveNextAsync())
						{
							num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
							if (num.HasValue)
							{
								sum += num.GetValueOrDefault();
								long num2 = checked(count + 1);
								count = num2;
							}
						}
						return sum / (decimal)count;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return null;
		}
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	private static ValueTask<decimal?> AverageAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<decimal?>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					decimal? num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (num.HasValue)
					{
						decimal sum = num.GetValueOrDefault();
						long count = 1L;
						while (await e.MoveNextAsync())
						{
							num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
							if (num.HasValue)
							{
								sum += num.GetValueOrDefault();
								long num2 = checked(count + 1);
								count = num2;
							}
						}
						return sum / (decimal)count;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return null;
		}
	}

	public static IAsyncEnumerable<TResult> Cast<TResult>(this IAsyncEnumerable<object> source)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (source is IAsyncEnumerable<TResult> result)
		{
			return result;
		}
		return Core(source);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<object> source2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			await foreach (object item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				yield return (TResult)item;
			}
		}
	}

	public static IAsyncEnumerable<TSource> Concat<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second)
	{
		if (first == null)
		{
			throw System.Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw System.Error.ArgumentNull("second");
		}
		if (!(first is ConcatAsyncIterator<TSource> concatAsyncIterator))
		{
			return new Concat2AsyncIterator<TSource>(first, second);
		}
		return concatAsyncIterator.Concat(second);
	}

	public static ValueTask<bool> ContainsAsync<TSource>(this IAsyncEnumerable<TSource> source, TSource value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!(source is ICollection<TSource> collection))
		{
			return ContainsAsync(source, value, null, cancellationToken);
		}
		return new ValueTask<bool>(collection.Contains(value));
	}

	public static ValueTask<bool> ContainsAsync<TSource>(this IAsyncEnumerable<TSource> source, TSource value, IEqualityComparer<TSource>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (comparer == null)
		{
			return Core(source, value, cancellationToken);
		}
		return Core2(source, value, comparer, cancellationToken);
		static async ValueTask<bool> Core(IAsyncEnumerable<TSource> source2, TSource y, CancellationToken cancellationToken2)
		{
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (EqualityComparer<TSource>.Default.Equals(item, y))
				{
					return true;
				}
			}
			return false;
		}
		static async ValueTask<bool> Core2(IAsyncEnumerable<TSource> source2, TSource y, IEqualityComparer<TSource> equalityComparer, CancellationToken cancellationToken2)
		{
			await foreach (TSource item2 in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (equalityComparer.Equals(item2, y))
				{
					return true;
				}
			}
			return false;
		}
	}

	public static ValueTask<int> CountAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (!(source is ICollection<TSource> collection))
		{
			if (!(source is IAsyncIListProvider<TSource> asyncIListProvider))
			{
				if (source is ICollection collection2)
				{
					return new ValueTask<int>(collection2.Count);
				}
				return Core(source, cancellationToken);
			}
			return asyncIListProvider.GetCountAsync(onlyIfCheap: false, cancellationToken);
		}
		return new ValueTask<int>(collection.Count);
		static async ValueTask<int> Core(IAsyncEnumerable<TSource> source2, CancellationToken cancellationToken2)
		{
			int count = 0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				_ = item;
				count = checked(count + 1);
			}
			return count;
		}
	}

	public static ValueTask<int> CountAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<int> Core(IAsyncEnumerable<TSource> source2, Func<TSource, bool> func, CancellationToken cancellationToken2)
		{
			int count = 0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (func(item))
				{
					count = checked(count + 1);
				}
			}
			return count;
		}
	}

	[Obsolete("Use CountAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the CountAwaitAsync functionality now exists as overloads of CountAsync.")]
	private static ValueTask<int> CountAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<int> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<bool>> func, CancellationToken cancellationToken2)
		{
			int count = 0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (await func(item).ConfigureAwait(continueOnCapturedContext: false))
				{
					count = checked(count + 1);
				}
			}
			return count;
		}
	}

	[Obsolete("Use CountAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the CountAwaitWithCancellationAsync functionality now exists as overloads of CountAsync.")]
	private static ValueTask<int> CountAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<int> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<bool>> func, CancellationToken cancellationToken2)
		{
			int count = 0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (await func(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
				{
					count = checked(count + 1);
				}
			}
			return count;
		}
	}

	public static IAsyncEnumerable<TSource> DefaultIfEmpty<TSource>(this IAsyncEnumerable<TSource> source)
	{
		return DefaultIfEmpty(source, default(TSource));
	}

	public static IAsyncEnumerable<TSource> DefaultIfEmpty<TSource>(this IAsyncEnumerable<TSource> source, TSource defaultValue)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return new DefaultIfEmptyAsyncIterator<TSource>(source, defaultValue);
	}

	public static IAsyncEnumerable<TSource> Distinct<TSource>(this IAsyncEnumerable<TSource> source)
	{
		return Distinct(source, null);
	}

	public static IAsyncEnumerable<TSource> Distinct<TSource>(this IAsyncEnumerable<TSource> source, IEqualityComparer<TSource>? comparer)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return new DistinctAsyncIterator<TSource>(source, comparer);
	}

	public static ValueTask<TSource> ElementAtAsync<TSource>(this IAsyncEnumerable<TSource> source, int index, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, index, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> asyncEnumerable, int num, CancellationToken cancellationToken2)
		{
			if (asyncEnumerable is IAsyncPartition<TSource> asyncPartition)
			{
				Maybe<TSource> maybe = await asyncPartition.TryGetElementAtAsync(num, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				if (maybe.HasValue)
				{
					return maybe.Value;
				}
			}
			else
			{
				if (asyncEnumerable is IList<TSource> list)
				{
					return list[num];
				}
				if (num >= 0)
				{
					await foreach (TSource item in asyncEnumerable.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
					{
						if (num == 0)
						{
							return item;
						}
						num--;
					}
				}
			}
			throw System.Error.ArgumentOutOfRange("index");
		}
	}

	public static ValueTask<TSource?> ElementAtOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, int index, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, index, cancellationToken);
		static async ValueTask<TSource?> Core(IAsyncEnumerable<TSource> asyncEnumerable, int num, CancellationToken cancellationToken2)
		{
			if (asyncEnumerable is IAsyncPartition<TSource> asyncPartition)
			{
				Maybe<TSource> maybe = await asyncPartition.TryGetElementAtAsync(num, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				if (maybe.HasValue)
				{
					return maybe.Value;
				}
			}
			if (num >= 0)
			{
				if (asyncEnumerable is IList<TSource> list)
				{
					if (num < list.Count)
					{
						return list[num];
					}
				}
				else
				{
					await foreach (TSource item in asyncEnumerable.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
					{
						if (num == 0)
						{
							return item;
						}
						num--;
					}
				}
			}
			return default(TSource);
		}
	}

	public static IAsyncEnumerable<TValue> Empty<TValue>()
	{
		return EmptyAsyncIterator<TValue>.Instance;
	}

	public static IAsyncEnumerable<TSource> Except<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second)
	{
		return Except(first, second, null);
	}

	public static IAsyncEnumerable<TSource> Except<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second, IEqualityComparer<TSource>? comparer)
	{
		if (first == null)
		{
			throw System.Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw System.Error.ArgumentNull("second");
		}
		return Core(first, second, comparer);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> source2, IAsyncEnumerable<TSource> source, IEqualityComparer<TSource>? comparer2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			System.Linq.Set<TSource> set = new System.Linq.Set<TSource>(comparer2);
			await foreach (TSource item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				set.Add(item);
			}
			await foreach (TSource item2 in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (set.Add(item2))
				{
					yield return item2;
				}
			}
		}
	}

	public static ValueTask<TSource> FirstAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> source2, CancellationToken cancellationToken2)
		{
			Maybe<TSource> maybe = await TryGetFirst(source2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			if (!maybe.HasValue)
			{
				throw System.Error.NoElements();
			}
			return maybe.Value;
		}
	}

	public static ValueTask<TSource> FirstAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> source2, Func<TSource, bool> predicate2, CancellationToken cancellationToken2)
		{
			Maybe<TSource> maybe = await TryGetFirst(source2, predicate2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			if (!maybe.HasValue)
			{
				throw System.Error.NoElements();
			}
			return maybe.Value;
		}
	}

	[Obsolete("Use FirstAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the FirstAwaitAsync functionality now exists as overloads of FirstAsync.")]
	private static ValueTask<TSource> FirstAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<bool>> predicate2, CancellationToken cancellationToken2)
		{
			Maybe<TSource> maybe = await TryGetFirst(source2, predicate2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			if (!maybe.HasValue)
			{
				throw System.Error.NoElements();
			}
			return maybe.Value;
		}
	}

	[Obsolete("Use FirstAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the FirstAwaitWithCancellationAsync functionality now exists as overloads of FirstAsync.")]
	internal static ValueTask<TSource> FirstAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<bool>> predicate2, CancellationToken cancellationToken2)
		{
			Maybe<TSource> maybe = await TryGetFirst(source2, predicate2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			if (!maybe.HasValue)
			{
				throw System.Error.NoElements();
			}
			return maybe.Value;
		}
	}

	public static ValueTask<TSource?> FirstOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<TSource?> Core(IAsyncEnumerable<TSource> source2, CancellationToken cancellationToken2)
		{
			Maybe<TSource> maybe = await TryGetFirst(source2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			return maybe.HasValue ? maybe.Value : default(TSource);
		}
	}

	public static ValueTask<TSource?> FirstOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<TSource?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, bool> predicate2, CancellationToken cancellationToken2)
		{
			Maybe<TSource> maybe = await TryGetFirst(source2, predicate2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			return maybe.HasValue ? maybe.Value : default(TSource);
		}
	}

	[Obsolete("Use FirstOrDefaultAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the FirstOrDefaultAwaitAsync functionality now exists as overloads of FirstOrDefaultAsync.")]
	private static ValueTask<TSource?> FirstOrDefaultAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<TSource?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<bool>> predicate2, CancellationToken cancellationToken2)
		{
			Maybe<TSource> maybe = await TryGetFirst(source2, predicate2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			return maybe.HasValue ? maybe.Value : default(TSource);
		}
	}

	[Obsolete("Use FirstOrDefaultAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the FirstOrDefaultAwaitWithCancellationAsync functionality now exists as overloads of FirstOrDefaultAsync.")]
	private static ValueTask<TSource?> FirstOrDefaultAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<TSource?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<bool>> predicate2, CancellationToken cancellationToken2)
		{
			Maybe<TSource> maybe = await TryGetFirst(source2, predicate2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			return maybe.HasValue ? maybe.Value : default(TSource);
		}
	}

	private static ValueTask<Maybe<TSource>> TryGetFirst<TSource>(IAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
	{
		if (source is IList<TSource> list)
		{
			if (list.Count > 0)
			{
				return new ValueTask<Maybe<TSource>>(new Maybe<TSource>(list[0]));
			}
			return new ValueTask<Maybe<TSource>>(default(Maybe<TSource>));
		}
		if (source is IAsyncPartition<TSource> asyncPartition)
		{
			return asyncPartition.TryGetFirstAsync(cancellationToken);
		}
		return Core(source, cancellationToken);
		static async ValueTask<Maybe<TSource>> Core(IAsyncEnumerable<TSource> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					return new Maybe<TSource>(e.Current);
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return default(Maybe<TSource>);
		}
	}

	private static async ValueTask<Maybe<TSource>> TryGetFirst<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
	{
		ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = source.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
		try
		{
			while (await e.MoveNextAsync())
			{
				TSource current = e.Current;
				if (predicate(current))
				{
					return new Maybe<TSource>(current);
				}
			}
		}
		finally
		{
			IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
			if (asyncDisposable != null)
			{
				await asyncDisposable.DisposeAsync();
			}
		}
		return default(Maybe<TSource>);
	}

	private static async ValueTask<Maybe<TSource>> TryGetFirst<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken)
	{
		ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = source.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
		try
		{
			while (await e.MoveNextAsync())
			{
				TSource value = e.Current;
				if (await predicate(value).ConfigureAwait(continueOnCapturedContext: false))
				{
					return new Maybe<TSource>(value);
				}
			}
		}
		finally
		{
			IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
			if (asyncDisposable != null)
			{
				await asyncDisposable.DisposeAsync();
			}
		}
		return default(Maybe<TSource>);
	}

	private static async ValueTask<Maybe<TSource>> TryGetFirst<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken)
	{
		ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = source.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
		try
		{
			while (await e.MoveNextAsync())
			{
				TSource value = e.Current;
				if (await predicate(value, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					return new Maybe<TSource>(value);
				}
			}
		}
		finally
		{
			IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
			if (asyncDisposable != null)
			{
				await asyncDisposable.DisposeAsync();
			}
		}
		return default(Maybe<TSource>);
	}

	[Obsolete("Use the language support for async foreach instead.")]
	public static Task ForEachAsync<TSource>(this IAsyncEnumerable<TSource> source, Action<TSource> action, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (action == null)
		{
			throw System.Error.ArgumentNull("action");
		}
		return Core(source, action, cancellationToken);
		static async Task Core(IAsyncEnumerable<TSource> source2, Action<TSource> action2, CancellationToken cancellationToken2)
		{
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				action2(item);
			}
		}
	}

	[Obsolete("Use the language support for async foreach instead.")]
	public static Task ForEachAsync<TSource>(this IAsyncEnumerable<TSource> source, Action<TSource, int> action, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (action == null)
		{
			throw System.Error.ArgumentNull("action");
		}
		return Core(source, action, cancellationToken);
		static async Task Core(IAsyncEnumerable<TSource> source2, Action<TSource, int> action2, CancellationToken cancellationToken2)
		{
			int index = 0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				action2(item, checked(index++));
			}
		}
	}

	[Obsolete("Use the language support for async foreach instead.")]
	private static Task ForEachAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, Task> action, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (action == null)
		{
			throw System.Error.ArgumentNull("action");
		}
		return Core(source, action, cancellationToken);
		static async Task Core(IAsyncEnumerable<TSource> source2, Func<TSource, Task> func, CancellationToken cancellationToken2)
		{
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				await func(item).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	[Obsolete("Use the language support for async foreach instead.")]
	internal static Task ForEachAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task> action, CancellationToken cancellationToken)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (action == null)
		{
			throw System.Error.ArgumentNull("action");
		}
		return Core(source, action, cancellationToken);
		static async Task Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, Task> func, CancellationToken cancellationToken2)
		{
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				await func(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	[Obsolete("Use the language support for async foreach instead.")]
	private static Task ForEachAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, Task> action, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (action == null)
		{
			throw System.Error.ArgumentNull("action");
		}
		return Core(source, action, cancellationToken);
		static async Task Core(IAsyncEnumerable<TSource> source2, Func<TSource, int, Task> func, CancellationToken cancellationToken2)
		{
			int index = 0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				await func(item, checked(index++)).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	[Obsolete("Use the language support for async foreach instead.")]
	internal static Task ForEachAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, Task> action, CancellationToken cancellationToken)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (action == null)
		{
			throw System.Error.ArgumentNull("action");
		}
		return Core(source, action, cancellationToken);
		static async Task Core(IAsyncEnumerable<TSource> source2, Func<TSource, int, CancellationToken, Task> func, CancellationToken cancellationToken2)
		{
			int index = 0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				await func(item, checked(index++), cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	public static IAsyncEnumerable<IAsyncGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return new GroupedAsyncEnumerable<TSource, TKey>(source, keySelector, null);
	}

	public static IAsyncEnumerable<IAsyncGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
	{
		return new GroupedAsyncEnumerable<TSource, TKey>(source, keySelector, comparer);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwait functionality now exists as overloads of GroupBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<IAsyncGrouping<TKey, TSource>> GroupByAwaitCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector)
	{
		return new GroupedAsyncEnumerableWithTask<TSource, TKey>(source, keySelector, null);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwait functionality now exists as overloads of GroupBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<IAsyncGrouping<TKey, TSource>> GroupByAwaitCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer)
	{
		return new GroupedAsyncEnumerableWithTask<TSource, TKey>(source, keySelector, comparer);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwaitWithCancellationAsync functionality now exists as overloads of GroupBy.")]
	private static IAsyncEnumerable<IAsyncGrouping<TKey, TSource>> GroupByAwaitWithCancellationCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		return new GroupedAsyncEnumerableWithTaskAndCancellation<TSource, TKey>(source, keySelector, null);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwaitWithCancellationAsync functionality now exists as overloads of GroupBy.")]
	private static IAsyncEnumerable<IAsyncGrouping<TKey, TSource>> GroupByAwaitWithCancellationCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer)
	{
		return new GroupedAsyncEnumerableWithTaskAndCancellation<TSource, TKey>(source, keySelector, comparer);
	}

	public static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
	{
		return new GroupedAsyncEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector, null);
	}

	public static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer)
	{
		return new GroupedAsyncEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector, comparer);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwait functionality now exists as overloads of GroupBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> GroupByAwaitCore<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector)
	{
		return new GroupedAsyncEnumerableWithTask<TSource, TKey, TElement>(source, keySelector, elementSelector, null);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwait functionality now exists as overloads of GroupBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> GroupByAwaitCore<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer)
	{
		return new GroupedAsyncEnumerableWithTask<TSource, TKey, TElement>(source, keySelector, elementSelector, comparer);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwaitWithCancellationAsync functionality now exists as overloads of GroupBy.")]
	private static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> GroupByAwaitWithCancellationCore<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector)
	{
		return new GroupedAsyncEnumerableWithTaskAndCancellation<TSource, TKey, TElement>(source, keySelector, elementSelector, null);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwaitWithCancellationAsync functionality now exists as overloads of GroupBy.")]
	private static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> GroupByAwaitWithCancellationCore<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer)
	{
		return new GroupedAsyncEnumerableWithTaskAndCancellation<TSource, TKey, TElement>(source, keySelector, elementSelector, comparer);
	}

	public static IAsyncEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IAsyncEnumerable<TSource>, TResult> resultSelector)
	{
		return new GroupedResultAsyncEnumerable<TSource, TKey, TResult>(source, keySelector, resultSelector, null);
	}

	public static IAsyncEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IAsyncEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return new GroupedResultAsyncEnumerable<TSource, TKey, TResult>(source, keySelector, resultSelector, comparer);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwait functionality now exists as overloads of GroupBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TResult> GroupByAwaitCore<TSource, TKey, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TKey, IAsyncEnumerable<TSource>, ValueTask<TResult>> resultSelector)
	{
		return new GroupedResultAsyncEnumerableWithTask<TSource, TKey, TResult>(source, keySelector, resultSelector, null);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwait functionality now exists as overloads of GroupBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TResult> GroupByAwaitCore<TSource, TKey, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TKey, IAsyncEnumerable<TSource>, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return new GroupedResultAsyncEnumerableWithTask<TSource, TKey, TResult>(source, keySelector, resultSelector, comparer);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwaitWithCancellationAsync functionality now exists as overloads of GroupBy.")]
	private static IAsyncEnumerable<TResult> GroupByAwaitWithCancellationCore<TSource, TKey, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TKey, IAsyncEnumerable<TSource>, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		return new GroupedResultAsyncEnumerableWithTaskAndCancellation<TSource, TKey, TResult>(source, keySelector, resultSelector, null);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwaitWithCancellationAsync functionality now exists as overloads of GroupBy.")]
	private static IAsyncEnumerable<TResult> GroupByAwaitWithCancellationCore<TSource, TKey, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TKey, IAsyncEnumerable<TSource>, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return new GroupedResultAsyncEnumerableWithTaskAndCancellation<TSource, TKey, TResult>(source, keySelector, resultSelector, comparer);
	}

	public static IAsyncEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IAsyncEnumerable<TElement>, TResult> resultSelector)
	{
		return new GroupedResultAsyncEnumerable<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, null);
	}

	public static IAsyncEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IAsyncEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return new GroupedResultAsyncEnumerable<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, comparer);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwait functionality now exists as overloads of GroupBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TResult> GroupByAwaitCore<TSource, TKey, TElement, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, Func<TKey, IAsyncEnumerable<TElement>, ValueTask<TResult>> resultSelector)
	{
		return new GroupedResultAsyncEnumerableWithTask<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, null);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwait functionality now exists as overloads of GroupBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TResult> GroupByAwaitCore<TSource, TKey, TElement, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, Func<TKey, IAsyncEnumerable<TElement>, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return new GroupedResultAsyncEnumerableWithTask<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, comparer);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwaitWithCancellationAsync functionality now exists as overloads of GroupBy.")]
	private static IAsyncEnumerable<TResult> GroupByAwaitWithCancellationCore<TSource, TKey, TElement, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, Func<TKey, IAsyncEnumerable<TElement>, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		return new GroupedResultAsyncEnumerableWithTaskAndCancellation<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, null);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwaitWithCancellationAsync functionality now exists as overloads of GroupBy.")]
	private static IAsyncEnumerable<TResult> GroupByAwaitWithCancellationCore<TSource, TKey, TElement, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, Func<TKey, IAsyncEnumerable<TElement>, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return new GroupedResultAsyncEnumerableWithTaskAndCancellation<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, comparer);
	}

	public static IAsyncEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IAsyncEnumerable<TInner>, TResult> resultSelector)
	{
		return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector, null);
	}

	public static IAsyncEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IAsyncEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		if (outer == null)
		{
			throw System.Error.ArgumentNull("outer");
		}
		if (inner == null)
		{
			throw System.Error.ArgumentNull("inner");
		}
		if (outerKeySelector == null)
		{
			throw System.Error.ArgumentNull("outerKeySelector");
		}
		if (innerKeySelector == null)
		{
			throw System.Error.ArgumentNull("innerKeySelector");
		}
		if (resultSelector == null)
		{
			throw System.Error.ArgumentNull("resultSelector");
		}
		return Core(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TOuter> enumerable, IAsyncEnumerable<TInner> source, Func<TOuter, TKey> func, Func<TInner, TKey> keySelector, Func<TOuter, IAsyncEnumerable<TInner>, TResult> func2, IEqualityComparer<TKey>? comparer2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TOuter>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					System.Linq.Internal.Lookup<TKey, TInner> lookup = await System.Linq.Internal.Lookup<TKey, TInner>.CreateForJoinAsync(source, keySelector, comparer2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					do
					{
						TOuter current = e.Current;
						TKey key = func(current);
						yield return func2(current, ToAsyncEnumerable(lookup[key]));
					}
					while (await e.MoveNextAsync());
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
		}
	}

	[Obsolete("Use GroupJoin. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupJoinAwait functionality now exists as an overload of GroupJoin. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TResult> GroupJoinAwaitCore<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, ValueTask<TKey>> outerKeySelector, Func<TInner, ValueTask<TKey>> innerKeySelector, Func<TOuter, IAsyncEnumerable<TInner>, ValueTask<TResult>> resultSelector)
	{
		return outer.GroupJoinAwaitCore(inner, outerKeySelector, innerKeySelector, resultSelector, null);
	}

	[Obsolete("Use GroupJoin. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupJoinAwait functionality now exists as an overload of GroupJoin. You will need to modify your callback to take an additional CancellationToken argument. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TResult> GroupJoinAwaitCore<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, ValueTask<TKey>> outerKeySelector, Func<TInner, ValueTask<TKey>> innerKeySelector, Func<TOuter, IAsyncEnumerable<TInner>, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		if (outer == null)
		{
			throw System.Error.ArgumentNull("outer");
		}
		if (inner == null)
		{
			throw System.Error.ArgumentNull("inner");
		}
		if (outerKeySelector == null)
		{
			throw System.Error.ArgumentNull("outerKeySelector");
		}
		if (innerKeySelector == null)
		{
			throw System.Error.ArgumentNull("innerKeySelector");
		}
		if (resultSelector == null)
		{
			throw System.Error.ArgumentNull("resultSelector");
		}
		return Core(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TOuter> enumerable, IAsyncEnumerable<TInner> source, Func<TOuter, ValueTask<TKey>> func2, Func<TInner, ValueTask<TKey>> keySelector, Func<TOuter, IAsyncEnumerable<TInner>, ValueTask<TResult>> func, IEqualityComparer<TKey>? comparer2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TOuter>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					LookupWithTask<TKey, TInner> lookup = await LookupWithTask<TKey, TInner>.CreateForJoinAsync(source, keySelector, comparer2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					do
					{
						TOuter item = e.Current;
						yield return await func(item, ToAsyncEnumerable(lookup[await func2(item).ConfigureAwait(continueOnCapturedContext: false)])).ConfigureAwait(continueOnCapturedContext: false);
					}
					while (await e.MoveNextAsync());
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
		}
	}

	[Obsolete("Use GroupJoin. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupJoinAwaitWithCancellation functionality now exists as an overload of GroupJoin.")]
	private static IAsyncEnumerable<TResult> GroupJoinAwaitWithCancellationCore<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, CancellationToken, ValueTask<TKey>> outerKeySelector, Func<TInner, CancellationToken, ValueTask<TKey>> innerKeySelector, Func<TOuter, IAsyncEnumerable<TInner>, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		return outer.GroupJoinAwaitWithCancellationCore(inner, outerKeySelector, innerKeySelector, resultSelector, null);
	}

	[Obsolete("Use GroupJoin. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupJoinAwaitWithCancellation functionality now exists as an overload of GroupJoin.")]
	private static IAsyncEnumerable<TResult> GroupJoinAwaitWithCancellationCore<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, CancellationToken, ValueTask<TKey>> outerKeySelector, Func<TInner, CancellationToken, ValueTask<TKey>> innerKeySelector, Func<TOuter, IAsyncEnumerable<TInner>, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		if (outer == null)
		{
			throw System.Error.ArgumentNull("outer");
		}
		if (inner == null)
		{
			throw System.Error.ArgumentNull("inner");
		}
		if (outerKeySelector == null)
		{
			throw System.Error.ArgumentNull("outerKeySelector");
		}
		if (innerKeySelector == null)
		{
			throw System.Error.ArgumentNull("innerKeySelector");
		}
		if (resultSelector == null)
		{
			throw System.Error.ArgumentNull("resultSelector");
		}
		return Core(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TOuter> enumerable, IAsyncEnumerable<TInner> source, Func<TOuter, CancellationToken, ValueTask<TKey>> func2, Func<TInner, CancellationToken, ValueTask<TKey>> keySelector, Func<TOuter, IAsyncEnumerable<TInner>, CancellationToken, ValueTask<TResult>> func, IEqualityComparer<TKey>? comparer2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TOuter>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					LookupWithTask<TKey, TInner> lookup = await LookupWithTask<TKey, TInner>.CreateForJoinAsync(source, keySelector, comparer2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					do
					{
						TOuter item = e.Current;
						yield return await func(item, ToAsyncEnumerable(lookup[await func2(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)]), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					}
					while (await e.MoveNextAsync());
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Intersect<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second)
	{
		return Intersect(first, second, null);
	}

	public static IAsyncEnumerable<TSource> Intersect<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second, IEqualityComparer<TSource>? comparer)
	{
		if (first == null)
		{
			throw System.Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw System.Error.ArgumentNull("second");
		}
		return Core(first, second, comparer);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> source2, IAsyncEnumerable<TSource> source, IEqualityComparer<TSource>? comparer2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			System.Linq.Set<TSource> set = new System.Linq.Set<TSource>(comparer2);
			await foreach (TSource item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				set.Add(item);
			}
			await foreach (TSource item2 in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (set.Remove(item2))
				{
					yield return item2;
				}
			}
		}
	}

	public static IAsyncEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector)
	{
		return Join(outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);
	}

	public static IAsyncEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		if (outer == null)
		{
			throw System.Error.ArgumentNull("outer");
		}
		if (inner == null)
		{
			throw System.Error.ArgumentNull("inner");
		}
		if (outerKeySelector == null)
		{
			throw System.Error.ArgumentNull("outerKeySelector");
		}
		if (innerKeySelector == null)
		{
			throw System.Error.ArgumentNull("innerKeySelector");
		}
		if (resultSelector == null)
		{
			throw System.Error.ArgumentNull("resultSelector");
		}
		return Core(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TOuter> enumerable, IAsyncEnumerable<TInner> source, Func<TOuter, TKey> func, Func<TInner, TKey> keySelector, Func<TOuter, TInner, TResult> func2, IEqualityComparer<TKey>? comparer2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TOuter>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					System.Linq.Internal.Lookup<TKey, TInner> lookup = await System.Linq.Internal.Lookup<TKey, TInner>.CreateForJoinAsync(source, keySelector, comparer2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					if (lookup.Count != 0)
					{
						do
						{
							TOuter item = e.Current;
							TKey key = func(item);
							Grouping<TKey, TInner> grouping = lookup.GetGrouping(key);
							if (grouping != null)
							{
								int count = grouping._count;
								TInner[] elements = grouping._elements;
								int i = 0;
								while (i != count)
								{
									yield return func2(item, elements[i]);
									int num = i + 1;
									i = num;
								}
							}
						}
						while (await e.MoveNextAsync());
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
		}
	}

	[Obsolete("Use Join. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the JoinAwait functionality now exists as an overload of Join. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TResult> JoinAwaitCore<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, ValueTask<TKey>> outerKeySelector, Func<TInner, ValueTask<TKey>> innerKeySelector, Func<TOuter, TInner, ValueTask<TResult>> resultSelector)
	{
		return outer.JoinAwaitCore(inner, outerKeySelector, innerKeySelector, resultSelector, null);
	}

	[Obsolete("Use Join. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the JoinAwait functionality now exists as an overload of Join. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TResult> JoinAwaitCore<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, ValueTask<TKey>> outerKeySelector, Func<TInner, ValueTask<TKey>> innerKeySelector, Func<TOuter, TInner, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		if (outer == null)
		{
			throw System.Error.ArgumentNull("outer");
		}
		if (inner == null)
		{
			throw System.Error.ArgumentNull("inner");
		}
		if (outerKeySelector == null)
		{
			throw System.Error.ArgumentNull("outerKeySelector");
		}
		if (innerKeySelector == null)
		{
			throw System.Error.ArgumentNull("innerKeySelector");
		}
		if (resultSelector == null)
		{
			throw System.Error.ArgumentNull("resultSelector");
		}
		return Core(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TOuter> enumerable, IAsyncEnumerable<TInner> source, Func<TOuter, ValueTask<TKey>> func, Func<TInner, ValueTask<TKey>> keySelector, Func<TOuter, TInner, ValueTask<TResult>> func2, IEqualityComparer<TKey>? comparer2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TOuter>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					LookupWithTask<TKey, TInner> lookup = await LookupWithTask<TKey, TInner>.CreateForJoinAsync(source, keySelector, comparer2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					if (lookup.Count != 0)
					{
						do
						{
							TOuter item = e.Current;
							Grouping<TKey, TInner> grouping = lookup.GetGrouping(await func(item).ConfigureAwait(continueOnCapturedContext: false));
							if (grouping != null)
							{
								int count = grouping._count;
								TInner[] elements = grouping._elements;
								int i = 0;
								while (i != count)
								{
									yield return await func2(item, elements[i]).ConfigureAwait(continueOnCapturedContext: false);
									int num = i + 1;
									i = num;
								}
							}
						}
						while (await e.MoveNextAsync());
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
		}
	}

	[Obsolete("Use Join. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the JoinAwaitWithCancellation functionality now exists as an overload of Join.")]
	private static IAsyncEnumerable<TResult> JoinAwaitWithCancellationCore<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, CancellationToken, ValueTask<TKey>> outerKeySelector, Func<TInner, CancellationToken, ValueTask<TKey>> innerKeySelector, Func<TOuter, TInner, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		return outer.JoinAwaitWithCancellationCore(inner, outerKeySelector, innerKeySelector, resultSelector, null);
	}

	[Obsolete("Use Join. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the JoinAwaitWithCancellation functionality now exists as an overload of Join.")]
	private static IAsyncEnumerable<TResult> JoinAwaitWithCancellationCore<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, CancellationToken, ValueTask<TKey>> outerKeySelector, Func<TInner, CancellationToken, ValueTask<TKey>> innerKeySelector, Func<TOuter, TInner, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		if (outer == null)
		{
			throw System.Error.ArgumentNull("outer");
		}
		if (inner == null)
		{
			throw System.Error.ArgumentNull("inner");
		}
		if (outerKeySelector == null)
		{
			throw System.Error.ArgumentNull("outerKeySelector");
		}
		if (innerKeySelector == null)
		{
			throw System.Error.ArgumentNull("innerKeySelector");
		}
		if (resultSelector == null)
		{
			throw System.Error.ArgumentNull("resultSelector");
		}
		return Core(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TOuter> enumerable, IAsyncEnumerable<TInner> source, Func<TOuter, CancellationToken, ValueTask<TKey>> func, Func<TInner, CancellationToken, ValueTask<TKey>> keySelector, Func<TOuter, TInner, CancellationToken, ValueTask<TResult>> func2, IEqualityComparer<TKey>? comparer2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TOuter>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					LookupWithTask<TKey, TInner> lookup = await LookupWithTask<TKey, TInner>.CreateForJoinAsync(source, keySelector, comparer2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					if (lookup.Count != 0)
					{
						do
						{
							TOuter item = e.Current;
							Grouping<TKey, TInner> grouping = lookup.GetGrouping(await func(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
							if (grouping != null)
							{
								int count = grouping._count;
								TInner[] elements = grouping._elements;
								int i = 0;
								while (i != count)
								{
									yield return await func2(item, elements[i], cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
									int num = i + 1;
									i = num;
								}
							}
						}
						while (await e.MoveNextAsync());
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
		}
	}

	public static ValueTask<TSource> LastAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> source2, CancellationToken cancellationToken2)
		{
			Maybe<TSource> maybe = await TryGetLast(source2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			if (!maybe.HasValue)
			{
				throw System.Error.NoElements();
			}
			return maybe.Value;
		}
	}

	public static ValueTask<TSource> LastAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> source2, Func<TSource, bool> predicate2, CancellationToken cancellationToken2)
		{
			Maybe<TSource> maybe = await TryGetLast(source2, predicate2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			if (!maybe.HasValue)
			{
				throw System.Error.NoElements();
			}
			return maybe.Value;
		}
	}

	[Obsolete("Use LastAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the LastAwaitAsync functionality now exists as overloads of LastAsync.")]
	private static ValueTask<TSource> LastAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<bool>> predicate2, CancellationToken cancellationToken2)
		{
			Maybe<TSource> maybe = await TryGetLast(source2, predicate2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			if (!maybe.HasValue)
			{
				throw System.Error.NoElements();
			}
			return maybe.Value;
		}
	}

	[Obsolete("Use LastAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the LastAwaitWithCancellationAsync functionality now exists as overloads of LastAsync.")]
	private static ValueTask<TSource> LastAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<bool>> predicate2, CancellationToken cancellationToken2)
		{
			Maybe<TSource> maybe = await TryGetLast(source2, predicate2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			if (!maybe.HasValue)
			{
				throw System.Error.NoElements();
			}
			return maybe.Value;
		}
	}

	public static ValueTask<TSource?> LastOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<TSource?> Core(IAsyncEnumerable<TSource> source2, CancellationToken cancellationToken2)
		{
			Maybe<TSource> maybe = await TryGetLast(source2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			return maybe.HasValue ? maybe.Value : default(TSource);
		}
	}

	public static ValueTask<TSource?> LastOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<TSource?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, bool> predicate2, CancellationToken cancellationToken2)
		{
			Maybe<TSource> maybe = await TryGetLast(source2, predicate2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			return maybe.HasValue ? maybe.Value : default(TSource);
		}
	}

	[Obsolete("Use LastOrDefaultAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the LastOrDefaultAwaitAsync functionality now exists as overloads of LastOrDefaultAsync.")]
	private static ValueTask<TSource?> LastOrDefaultAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<TSource?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<bool>> predicate2, CancellationToken cancellationToken2)
		{
			Maybe<TSource> maybe = await TryGetLast(source2, predicate2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			return maybe.HasValue ? maybe.Value : default(TSource);
		}
	}

	[Obsolete("Use LastOrDefaultAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the LastOrDefaultAwaitWithCancellationAsync functionality now exists as overloads of LastOrDefaultAsync.")]
	private static ValueTask<TSource?> LastOrDefaultAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<TSource?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<bool>> predicate2, CancellationToken cancellationToken2)
		{
			Maybe<TSource> maybe = await TryGetLast(source2, predicate2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			return maybe.HasValue ? maybe.Value : default(TSource);
		}
	}

	private static ValueTask<Maybe<TSource>> TryGetLast<TSource>(IAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
	{
		if (source is IList<TSource> { Count: var count } list)
		{
			if (count > 0)
			{
				return new ValueTask<Maybe<TSource>>(new Maybe<TSource>(list[count - 1]));
			}
			return new ValueTask<Maybe<TSource>>(default(Maybe<TSource>));
		}
		if (source is IAsyncPartition<TSource> asyncPartition)
		{
			return asyncPartition.TryGetLastAsync(cancellationToken);
		}
		return Core(source, cancellationToken);
		static async ValueTask<Maybe<TSource>> Core(IAsyncEnumerable<TSource> source2, CancellationToken cancellationToken2)
		{
			TSource last = default(TSource);
			bool hasLast = false;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				hasLast = true;
				last = item;
			}
			return hasLast ? new Maybe<TSource>(last) : default(Maybe<TSource>);
		}
	}

	private static async ValueTask<Maybe<TSource>> TryGetLast<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
	{
		TSource last = default(TSource);
		bool hasLast = false;
		await foreach (TSource item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			if (predicate(item))
			{
				hasLast = true;
				last = item;
			}
		}
		return hasLast ? new Maybe<TSource>(last) : default(Maybe<TSource>);
	}

	private static async ValueTask<Maybe<TSource>> TryGetLast<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken)
	{
		TSource last = default(TSource);
		bool hasLast = false;
		await foreach (TSource item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			if (await predicate(item).ConfigureAwait(continueOnCapturedContext: false))
			{
				hasLast = true;
				last = item;
			}
		}
		return hasLast ? new Maybe<TSource>(last) : default(Maybe<TSource>);
	}

	private static async ValueTask<Maybe<TSource>> TryGetLast<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken)
	{
		TSource last = default(TSource);
		bool hasLast = false;
		await foreach (TSource item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			if (await predicate(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				hasLast = true;
				last = item;
			}
		}
		return hasLast ? new Maybe<TSource>(last) : default(Maybe<TSource>);
	}

	public static ValueTask<long> LongCountAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<long> Core(IAsyncEnumerable<TSource> source2, CancellationToken cancellationToken2)
		{
			long count = 0L;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				_ = item;
				count = checked(count + 1);
			}
			return count;
		}
	}

	public static ValueTask<long> LongCountAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<long> Core(IAsyncEnumerable<TSource> source2, Func<TSource, bool> func, CancellationToken cancellationToken2)
		{
			long count = 0L;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (func(item))
				{
					count = checked(count + 1);
				}
			}
			return count;
		}
	}

	[Obsolete("Use LongCountAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the LongCountAwaitAsync functionality now exists as overloads of LongCountAsync.")]
	private static ValueTask<long> LongCountAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<long> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<bool>> func, CancellationToken cancellationToken2)
		{
			long count = 0L;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (await func(item).ConfigureAwait(continueOnCapturedContext: false))
				{
					count = checked(count + 1);
				}
			}
			return count;
		}
	}

	[Obsolete("Use LongCountAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the LongCountAwaitWithCancellationAsync functionality now exists as overloads of LongCountAsync.")]
	private static ValueTask<long> LongCountAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<long> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<bool>> func, CancellationToken cancellationToken2)
		{
			long count = 0L;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (await func(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
				{
					count = checked(count + 1);
				}
			}
			return count;
		}
	}

	public static ValueTask<TSource> MaxAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (default(TSource) == null)
		{
			return Core(source, cancellationToken);
		}
		return Core2(source, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> enumerable, CancellationToken cancellationToken2)
		{
			Comparer<TSource> comparer = Comparer<TSource>.Default;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TSource value;
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return default(TSource);
					}
					value = e.Current;
				}
				while (value == null);
				while (await e.MoveNextAsync())
				{
					TSource current = e.Current;
					if (current != null && comparer.Compare(current, value) > 0)
					{
						value = current;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
		static async ValueTask<TSource> Core2(IAsyncEnumerable<TSource> enumerable, CancellationToken cancellationToken2)
		{
			Comparer<TSource> comparer = Comparer<TSource>.Default;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TSource value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = e.Current;
				while (await e.MoveNextAsync())
				{
					TSource current = e.Current;
					if (comparer.Compare(current, value) > 0)
					{
						value = current;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable2 = e as IAsyncDisposable;
				if (asyncDisposable2 != null)
				{
					await asyncDisposable2.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by the MaxAsync overload that took a selector callback now exists as an overload of MaxByAsync.")]
	public static ValueTask<TResult> MaxAsync<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		if (default(TResult) == null)
		{
			return Core(source, selector, cancellationToken);
		}
		return Core2(source, selector, cancellationToken);
		static async ValueTask<TResult> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, TResult> func, CancellationToken cancellationToken2)
		{
			Comparer<TResult> comparer = Comparer<TResult>.Default;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TResult value;
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return default(TResult);
					}
					value = func(e.Current);
				}
				while (value == null);
				while (await e.MoveNextAsync())
				{
					TResult val = func(e.Current);
					if (val != null && comparer.Compare(val, value) > 0)
					{
						value = val;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
		static async ValueTask<TResult> Core2(IAsyncEnumerable<TSource> enumerable, Func<TSource, TResult> func, CancellationToken cancellationToken2)
		{
			Comparer<TResult> comparer = Comparer<TResult>.Default;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TResult value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = func(e.Current);
				while (await e.MoveNextAsync())
				{
					TResult val = func(e.Current);
					if (comparer.Compare(val, value) > 0)
					{
						value = val;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable2 = e as IAsyncDisposable;
				if (asyncDisposable2 != null)
				{
					await asyncDisposable2.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<TResult> MaxAwaitAsyncCore<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		if (default(TResult) == null)
		{
			return Core(source, selector, cancellationToken);
		}
		return Core2(source, selector, cancellationToken);
		static async ValueTask<TResult> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<TResult>> func, CancellationToken cancellationToken2)
		{
			Comparer<TResult> comparer = Comparer<TResult>.Default;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TResult value;
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return default(TResult);
					}
					value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				}
				while (value == null);
				while (await e.MoveNextAsync())
				{
					TResult val = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (val != null && comparer.Compare(val, value) > 0)
					{
						value = val;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
		static async ValueTask<TResult> Core2(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<TResult>> func, CancellationToken cancellationToken2)
		{
			Comparer<TResult> comparer = Comparer<TResult>.Default;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TResult value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					TResult val = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (comparer.Compare(val, value) > 0)
					{
						value = val;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable2 = e as IAsyncDisposable;
				if (asyncDisposable2 != null)
				{
					await asyncDisposable2.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<TResult> MaxAwaitWithCancellationAsyncCore<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		if (default(TResult) == null)
		{
			return Core(source, selector, cancellationToken);
		}
		return Core2(source, selector, cancellationToken);
		static async ValueTask<TResult> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<TResult>> func, CancellationToken cancellationToken2)
		{
			Comparer<TResult> comparer = Comparer<TResult>.Default;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TResult value;
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return default(TResult);
					}
					value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				}
				while (value == null);
				while (await e.MoveNextAsync())
				{
					TResult val = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (val != null && comparer.Compare(val, value) > 0)
					{
						value = val;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
		static async ValueTask<TResult> Core2(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<TResult>> func, CancellationToken cancellationToken2)
		{
			Comparer<TResult> comparer = Comparer<TResult>.Default;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TResult value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					TResult val = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (comparer.Compare(val, value) > 0)
					{
						value = val;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable2 = e as IAsyncDisposable;
				if (asyncDisposable2 != null)
				{
					await asyncDisposable2.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<TSource> MinAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (default(TSource) == null)
		{
			return Core(source, cancellationToken);
		}
		return Core2(source, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> enumerable, CancellationToken cancellationToken2)
		{
			Comparer<TSource> comparer = Comparer<TSource>.Default;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TSource value;
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return default(TSource);
					}
					value = e.Current;
				}
				while (value == null);
				while (await e.MoveNextAsync())
				{
					TSource current = e.Current;
					if (current != null && comparer.Compare(current, value) < 0)
					{
						value = current;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
		static async ValueTask<TSource> Core2(IAsyncEnumerable<TSource> enumerable, CancellationToken cancellationToken2)
		{
			Comparer<TSource> comparer = Comparer<TSource>.Default;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TSource value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = e.Current;
				while (await e.MoveNextAsync())
				{
					TSource current = e.Current;
					if (comparer.Compare(current, value) < 0)
					{
						value = current;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable2 = e as IAsyncDisposable;
				if (asyncDisposable2 != null)
				{
					await asyncDisposable2.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by the MinAsync overload that took a selector callback now exists as an overload of MinByAsync.")]
	public static ValueTask<TResult> MinAsync<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		if (default(TResult) == null)
		{
			return Core(source, selector, cancellationToken);
		}
		return Core2(source, selector, cancellationToken);
		static async ValueTask<TResult> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, TResult> func, CancellationToken cancellationToken2)
		{
			Comparer<TResult> comparer = Comparer<TResult>.Default;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TResult value;
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return default(TResult);
					}
					value = func(e.Current);
				}
				while (value == null);
				while (await e.MoveNextAsync())
				{
					TResult val = func(e.Current);
					if (val != null && comparer.Compare(val, value) < 0)
					{
						value = val;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
		static async ValueTask<TResult> Core2(IAsyncEnumerable<TSource> enumerable, Func<TSource, TResult> func, CancellationToken cancellationToken2)
		{
			Comparer<TResult> comparer = Comparer<TResult>.Default;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TResult value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = func(e.Current);
				while (await e.MoveNextAsync())
				{
					TResult val = func(e.Current);
					if (comparer.Compare(val, value) < 0)
					{
						value = val;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable2 = e as IAsyncDisposable;
				if (asyncDisposable2 != null)
				{
					await asyncDisposable2.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<TResult> MinAwaitAsyncCore<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		if (default(TResult) == null)
		{
			return Core(source, selector, cancellationToken);
		}
		return Core2(source, selector, cancellationToken);
		static async ValueTask<TResult> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<TResult>> func, CancellationToken cancellationToken2)
		{
			Comparer<TResult> comparer = Comparer<TResult>.Default;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TResult value;
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return default(TResult);
					}
					value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				}
				while (value == null);
				while (await e.MoveNextAsync())
				{
					TResult val = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (val != null && comparer.Compare(val, value) < 0)
					{
						value = val;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
		static async ValueTask<TResult> Core2(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<TResult>> func, CancellationToken cancellationToken2)
		{
			Comparer<TResult> comparer = Comparer<TResult>.Default;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TResult value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					TResult val = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (comparer.Compare(val, value) < 0)
					{
						value = val;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable2 = e as IAsyncDisposable;
				if (asyncDisposable2 != null)
				{
					await asyncDisposable2.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<TResult> MinAwaitWithCancellationAsyncCore<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		if (default(TResult) == null)
		{
			return Core(source, selector, cancellationToken);
		}
		return Core2(source, selector, cancellationToken);
		static async ValueTask<TResult> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<TResult>> func, CancellationToken cancellationToken2)
		{
			Comparer<TResult> comparer = Comparer<TResult>.Default;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TResult value;
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return default(TResult);
					}
					value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				}
				while (value == null);
				while (await e.MoveNextAsync())
				{
					TResult val = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (val != null && comparer.Compare(val, value) < 0)
					{
						value = val;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
		static async ValueTask<TResult> Core2(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<TResult>> func, CancellationToken cancellationToken2)
		{
			Comparer<TResult> comparer = Comparer<TResult>.Default;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TResult value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					TResult val = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (comparer.Compare(val, value) < 0)
					{
						value = val;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable2 = e as IAsyncDisposable;
				if (asyncDisposable2 != null)
				{
					await asyncDisposable2.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<int> MaxAsync(this IAsyncEnumerable<int> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<int> Core(IAsyncEnumerable<int> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<int>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			int value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = e.Current;
				while (await e.MoveNextAsync())
				{
					int current = e.Current;
					if (current > value)
					{
						value = current;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<int> MaxAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<int> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, int> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			int value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = func(e.Current);
				while (await e.MoveNextAsync())
				{
					int num = func(e.Current);
					if (num > value)
					{
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<int> MaxAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<int> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<int>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			int value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					int num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (num > value)
					{
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<int> MaxAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<int> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<int>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			int value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					int num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (num > value)
					{
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<int?> MaxAsync(this IAsyncEnumerable<int?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<int?> Core(IAsyncEnumerable<int?> enumerable, CancellationToken cancellationToken2)
		{
			int? value = null;
			ConfiguredCancelableAsyncEnumerable<int?>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = e.Current;
				}
				while (!value.HasValue);
				int valueVal = value.GetValueOrDefault();
				if (valueVal >= 0)
				{
					while (await e.MoveNextAsync())
					{
						int? current = e.Current;
						int valueOrDefault = current.GetValueOrDefault();
						if (valueOrDefault > valueVal)
						{
							valueVal = valueOrDefault;
							value = current;
						}
					}
				}
				else
				{
					while (await e.MoveNextAsync())
					{
						int? current2 = e.Current;
						int valueOrDefault2 = current2.GetValueOrDefault();
						if (current2.HasValue & (valueOrDefault2 > valueVal))
						{
							valueVal = valueOrDefault2;
							value = current2;
						}
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<int?> MaxAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<int?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, int?> func, CancellationToken cancellationToken2)
		{
			int? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = func(e.Current);
				}
				while (!value.HasValue);
				int valueVal = value.GetValueOrDefault();
				if (valueVal >= 0)
				{
					while (await e.MoveNextAsync())
					{
						int? num = func(e.Current);
						int valueOrDefault = num.GetValueOrDefault();
						if (valueOrDefault > valueVal)
						{
							valueVal = valueOrDefault;
							value = num;
						}
					}
				}
				else
				{
					while (await e.MoveNextAsync())
					{
						int? num2 = func(e.Current);
						int valueOrDefault2 = num2.GetValueOrDefault();
						if (num2.HasValue & (valueOrDefault2 > valueVal))
						{
							valueVal = valueOrDefault2;
							value = num2;
						}
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<int?> MaxAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<int?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<int?>> func, CancellationToken cancellationToken2)
		{
			int? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				}
				while (!value.HasValue);
				int valueVal = value.GetValueOrDefault();
				if (valueVal >= 0)
				{
					while (await e.MoveNextAsync())
					{
						int? num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
						int valueOrDefault = num.GetValueOrDefault();
						if (valueOrDefault > valueVal)
						{
							valueVal = valueOrDefault;
							value = num;
						}
					}
				}
				else
				{
					while (await e.MoveNextAsync())
					{
						int? num2 = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
						int valueOrDefault2 = num2.GetValueOrDefault();
						if (num2.HasValue & (valueOrDefault2 > valueVal))
						{
							valueVal = valueOrDefault2;
							value = num2;
						}
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<int?> MaxAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<int?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<int?>> func, CancellationToken cancellationToken2)
		{
			int? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				}
				while (!value.HasValue);
				int valueVal = value.GetValueOrDefault();
				if (valueVal >= 0)
				{
					while (await e.MoveNextAsync())
					{
						int? num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
						int valueOrDefault = num.GetValueOrDefault();
						if (valueOrDefault > valueVal)
						{
							valueVal = valueOrDefault;
							value = num;
						}
					}
				}
				else
				{
					while (await e.MoveNextAsync())
					{
						int? num2 = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
						int valueOrDefault2 = num2.GetValueOrDefault();
						if (num2.HasValue & (valueOrDefault2 > valueVal))
						{
							valueVal = valueOrDefault2;
							value = num2;
						}
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<long> MaxAsync(this IAsyncEnumerable<long> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<long> Core(IAsyncEnumerable<long> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<long>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			long value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = e.Current;
				while (await e.MoveNextAsync())
				{
					long current = e.Current;
					if (current > value)
					{
						value = current;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<long> MaxAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, long> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<long> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, long> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			long value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = func(e.Current);
				while (await e.MoveNextAsync())
				{
					long num = func(e.Current);
					if (num > value)
					{
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<long> MaxAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<long> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<long>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			long value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					long num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (num > value)
					{
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<long> MaxAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<long> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<long>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			long value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					long num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (num > value)
					{
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<long?> MaxAsync(this IAsyncEnumerable<long?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<long?> Core(IAsyncEnumerable<long?> enumerable, CancellationToken cancellationToken2)
		{
			long? value = null;
			ConfiguredCancelableAsyncEnumerable<long?>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = e.Current;
				}
				while (!value.HasValue);
				long valueVal = value.GetValueOrDefault();
				if (valueVal >= 0)
				{
					while (await e.MoveNextAsync())
					{
						long? current = e.Current;
						long valueOrDefault = current.GetValueOrDefault();
						if (valueOrDefault > valueVal)
						{
							valueVal = valueOrDefault;
							value = current;
						}
					}
				}
				else
				{
					while (await e.MoveNextAsync())
					{
						long? current2 = e.Current;
						long valueOrDefault2 = current2.GetValueOrDefault();
						if (current2.HasValue & (valueOrDefault2 > valueVal))
						{
							valueVal = valueOrDefault2;
							value = current2;
						}
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<long?> MaxAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, long?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<long?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, long?> func, CancellationToken cancellationToken2)
		{
			long? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = func(e.Current);
				}
				while (!value.HasValue);
				long valueVal = value.GetValueOrDefault();
				if (valueVal >= 0)
				{
					while (await e.MoveNextAsync())
					{
						long? num = func(e.Current);
						long valueOrDefault = num.GetValueOrDefault();
						if (valueOrDefault > valueVal)
						{
							valueVal = valueOrDefault;
							value = num;
						}
					}
				}
				else
				{
					while (await e.MoveNextAsync())
					{
						long? num2 = func(e.Current);
						long valueOrDefault2 = num2.GetValueOrDefault();
						if (num2.HasValue & (valueOrDefault2 > valueVal))
						{
							valueVal = valueOrDefault2;
							value = num2;
						}
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<long?> MaxAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<long?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<long?>> func, CancellationToken cancellationToken2)
		{
			long? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				}
				while (!value.HasValue);
				long valueVal = value.GetValueOrDefault();
				if (valueVal >= 0)
				{
					while (await e.MoveNextAsync())
					{
						long? num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
						long valueOrDefault = num.GetValueOrDefault();
						if (valueOrDefault > valueVal)
						{
							valueVal = valueOrDefault;
							value = num;
						}
					}
				}
				else
				{
					while (await e.MoveNextAsync())
					{
						long? num2 = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
						long valueOrDefault2 = num2.GetValueOrDefault();
						if (num2.HasValue & (valueOrDefault2 > valueVal))
						{
							valueVal = valueOrDefault2;
							value = num2;
						}
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<long?> MaxAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<long?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<long?>> func, CancellationToken cancellationToken2)
		{
			long? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				}
				while (!value.HasValue);
				long valueVal = value.GetValueOrDefault();
				if (valueVal >= 0)
				{
					while (await e.MoveNextAsync())
					{
						long? num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
						long valueOrDefault = num.GetValueOrDefault();
						if (valueOrDefault > valueVal)
						{
							valueVal = valueOrDefault;
							value = num;
						}
					}
				}
				else
				{
					while (await e.MoveNextAsync())
					{
						long? num2 = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
						long valueOrDefault2 = num2.GetValueOrDefault();
						if (num2.HasValue & (valueOrDefault2 > valueVal))
						{
							valueVal = valueOrDefault2;
							value = num2;
						}
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<float> MaxAsync(this IAsyncEnumerable<float> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<float> Core(IAsyncEnumerable<float> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<float>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				float value = e.Current;
				while (true)
				{
					if (!float.IsNaN(value))
					{
						while (await e.MoveNextAsync())
						{
							float current = e.Current;
							if (current > value)
							{
								value = current;
							}
						}
						return value;
					}
					if (!(await e.MoveNextAsync()))
					{
						break;
					}
					value = e.Current;
				}
				result = value;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	public static ValueTask<float> MaxAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, float> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, float> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				float value = func(e.Current);
				while (true)
				{
					if (!float.IsNaN(value))
					{
						while (await e.MoveNextAsync())
						{
							float num = func(e.Current);
							if (num > value)
							{
								value = num;
							}
						}
						return value;
					}
					if (!(await e.MoveNextAsync()))
					{
						break;
					}
					value = func(e.Current);
				}
				result = value;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<float> MaxAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<float>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				float value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				while (true)
				{
					if (!float.IsNaN(value))
					{
						while (await e.MoveNextAsync())
						{
							float num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
							if (num > value)
							{
								value = num;
							}
						}
						return value;
					}
					if (!(await e.MoveNextAsync()))
					{
						break;
					}
					value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				}
				result = value;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<float> MaxAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<float>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				float value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				while (true)
				{
					if (!float.IsNaN(value))
					{
						while (await e.MoveNextAsync())
						{
							float num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
							if (num > value)
							{
								value = num;
							}
						}
						return value;
					}
					if (!(await e.MoveNextAsync()))
					{
						break;
					}
					value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				}
				result = value;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	public static ValueTask<float?> MaxAsync(this IAsyncEnumerable<float?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<float?> Core(IAsyncEnumerable<float?> enumerable, CancellationToken cancellationToken2)
		{
			float? value = null;
			ConfiguredCancelableAsyncEnumerable<float?>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float? result;
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						result = value;
						break;
					}
					value = e.Current;
					if (!value.HasValue)
					{
						continue;
					}
					float valueVal = value.GetValueOrDefault();
					while (true)
					{
						if (!float.IsNaN(valueVal))
						{
							while (await e.MoveNextAsync())
							{
								float? current = e.Current;
								float valueOrDefault = current.GetValueOrDefault();
								if (current.HasValue & (valueOrDefault > valueVal))
								{
									valueVal = valueOrDefault;
									value = current;
								}
							}
							break;
						}
						if (!(await e.MoveNextAsync()))
						{
							result = value;
							goto end_IL_0057;
						}
						float? current2 = e.Current;
						if (current2.HasValue)
						{
							float? num;
							value = (num = current2);
							num = num;
							valueVal = num.GetValueOrDefault();
						}
					}
					goto end_IL_0000;
					continue;
					end_IL_0057:
					break;
				}
				goto IL_030e;
				end_IL_0000:;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
			IL_030e:
			return result;
		}
	}

	public static ValueTask<float?> MaxAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, float?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, float?> func, CancellationToken cancellationToken2)
		{
			float? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float? result;
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						result = value;
						break;
					}
					value = func(e.Current);
					if (!value.HasValue)
					{
						continue;
					}
					float valueVal = value.GetValueOrDefault();
					while (true)
					{
						if (!float.IsNaN(valueVal))
						{
							while (await e.MoveNextAsync())
							{
								float? num = func(e.Current);
								float valueOrDefault = num.GetValueOrDefault();
								if (num.HasValue & (valueOrDefault > valueVal))
								{
									valueVal = valueOrDefault;
									value = num;
								}
							}
							break;
						}
						if (!(await e.MoveNextAsync()))
						{
							result = value;
							goto end_IL_0057;
						}
						float? num2 = func(e.Current);
						if (num2.HasValue)
						{
							float? num3;
							value = (num3 = num2);
							num3 = num3;
							valueVal = num3.GetValueOrDefault();
						}
					}
					goto end_IL_0000;
					continue;
					end_IL_0057:
					break;
				}
				goto IL_032f;
				end_IL_0000:;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
			IL_032f:
			return result;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<float?> MaxAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<float?>> func, CancellationToken cancellationToken2)
		{
			float? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float? result;
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						result = value;
						break;
					}
					value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (!value.HasValue)
					{
						continue;
					}
					float valueVal = value.GetValueOrDefault();
					while (true)
					{
						if (!float.IsNaN(valueVal))
						{
							while (await e.MoveNextAsync())
							{
								float? num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
								float valueOrDefault = num.GetValueOrDefault();
								if (num.HasValue & (valueOrDefault > valueVal))
								{
									valueVal = valueOrDefault;
									value = num;
								}
							}
							break;
						}
						if (!(await e.MoveNextAsync()))
						{
							result = value;
							goto end_IL_0063;
						}
						float? num2 = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
						if (num2.HasValue)
						{
							float? num3;
							value = (num3 = num2);
							num3 = num3;
							valueVal = num3.GetValueOrDefault();
						}
					}
					goto end_IL_0000;
					continue;
					end_IL_0063:
					break;
				}
				goto IL_0474;
				end_IL_0000:;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
			IL_0474:
			return result;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<float?> MaxAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<float?>> func, CancellationToken cancellationToken2)
		{
			float? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float? result;
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						result = value;
						break;
					}
					value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (!value.HasValue)
					{
						continue;
					}
					float valueVal = value.GetValueOrDefault();
					while (true)
					{
						if (!float.IsNaN(valueVal))
						{
							while (await e.MoveNextAsync())
							{
								float? num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
								float valueOrDefault = num.GetValueOrDefault();
								if (num.HasValue & (valueOrDefault > valueVal))
								{
									valueVal = valueOrDefault;
									value = num;
								}
							}
							break;
						}
						if (!(await e.MoveNextAsync()))
						{
							result = value;
							goto end_IL_0063;
						}
						float? num2 = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
						if (num2.HasValue)
						{
							float? num3;
							value = (num3 = num2);
							num3 = num3;
							valueVal = num3.GetValueOrDefault();
						}
					}
					goto end_IL_0000;
					continue;
					end_IL_0063:
					break;
				}
				goto IL_0486;
				end_IL_0000:;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
			IL_0486:
			return result;
		}
	}

	public static ValueTask<double> MaxAsync(this IAsyncEnumerable<double> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<double> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<double>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				double value = e.Current;
				while (true)
				{
					if (!double.IsNaN(value))
					{
						while (await e.MoveNextAsync())
						{
							double current = e.Current;
							if (current > value)
							{
								value = current;
							}
						}
						return value;
					}
					if (!(await e.MoveNextAsync()))
					{
						break;
					}
					value = e.Current;
				}
				result = value;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	public static ValueTask<double> MaxAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, double> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, double> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				double value = func(e.Current);
				while (true)
				{
					if (!double.IsNaN(value))
					{
						while (await e.MoveNextAsync())
						{
							double num = func(e.Current);
							if (num > value)
							{
								value = num;
							}
						}
						return value;
					}
					if (!(await e.MoveNextAsync()))
					{
						break;
					}
					value = func(e.Current);
				}
				result = value;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<double> MaxAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<double>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				double value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				while (true)
				{
					if (!double.IsNaN(value))
					{
						while (await e.MoveNextAsync())
						{
							double num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
							if (num > value)
							{
								value = num;
							}
						}
						return value;
					}
					if (!(await e.MoveNextAsync()))
					{
						break;
					}
					value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				}
				result = value;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<double> MaxAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<double>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				double value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				while (true)
				{
					if (!double.IsNaN(value))
					{
						while (await e.MoveNextAsync())
						{
							double num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
							if (num > value)
							{
								value = num;
							}
						}
						return value;
					}
					if (!(await e.MoveNextAsync()))
					{
						break;
					}
					value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				}
				result = value;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	public static ValueTask<double?> MaxAsync(this IAsyncEnumerable<double?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<double?> enumerable, CancellationToken cancellationToken2)
		{
			double? value = null;
			ConfiguredCancelableAsyncEnumerable<double?>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double? result;
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						result = value;
						break;
					}
					value = e.Current;
					if (!value.HasValue)
					{
						continue;
					}
					double valueVal = value.GetValueOrDefault();
					while (true)
					{
						if (!double.IsNaN(valueVal))
						{
							while (await e.MoveNextAsync())
							{
								double? current = e.Current;
								double valueOrDefault = current.GetValueOrDefault();
								if (current.HasValue & (valueOrDefault > valueVal))
								{
									valueVal = valueOrDefault;
									value = current;
								}
							}
							break;
						}
						if (!(await e.MoveNextAsync()))
						{
							result = value;
							goto end_IL_0057;
						}
						double? current2 = e.Current;
						if (current2.HasValue)
						{
							double? num;
							value = (num = current2);
							num = num;
							valueVal = num.GetValueOrDefault();
						}
					}
					goto end_IL_0000;
					continue;
					end_IL_0057:
					break;
				}
				goto IL_030e;
				end_IL_0000:;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
			IL_030e:
			return result;
		}
	}

	public static ValueTask<double?> MaxAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, double?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, double?> func, CancellationToken cancellationToken2)
		{
			double? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double? result;
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						result = value;
						break;
					}
					value = func(e.Current);
					if (!value.HasValue)
					{
						continue;
					}
					double valueVal = value.GetValueOrDefault();
					while (true)
					{
						if (!double.IsNaN(valueVal))
						{
							while (await e.MoveNextAsync())
							{
								double? num = func(e.Current);
								double valueOrDefault = num.GetValueOrDefault();
								if (num.HasValue & (valueOrDefault > valueVal))
								{
									valueVal = valueOrDefault;
									value = num;
								}
							}
							break;
						}
						if (!(await e.MoveNextAsync()))
						{
							result = value;
							goto end_IL_0057;
						}
						double? num2 = func(e.Current);
						if (num2.HasValue)
						{
							double? num3;
							value = (num3 = num2);
							num3 = num3;
							valueVal = num3.GetValueOrDefault();
						}
					}
					goto end_IL_0000;
					continue;
					end_IL_0057:
					break;
				}
				goto IL_032f;
				end_IL_0000:;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
			IL_032f:
			return result;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<double?> MaxAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<double?>> func, CancellationToken cancellationToken2)
		{
			double? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double? result;
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						result = value;
						break;
					}
					value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (!value.HasValue)
					{
						continue;
					}
					double valueVal = value.GetValueOrDefault();
					while (true)
					{
						if (!double.IsNaN(valueVal))
						{
							while (await e.MoveNextAsync())
							{
								double? num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
								double valueOrDefault = num.GetValueOrDefault();
								if (num.HasValue & (valueOrDefault > valueVal))
								{
									valueVal = valueOrDefault;
									value = num;
								}
							}
							break;
						}
						if (!(await e.MoveNextAsync()))
						{
							result = value;
							goto end_IL_0063;
						}
						double? num2 = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
						if (num2.HasValue)
						{
							double? num3;
							value = (num3 = num2);
							num3 = num3;
							valueVal = num3.GetValueOrDefault();
						}
					}
					goto end_IL_0000;
					continue;
					end_IL_0063:
					break;
				}
				goto IL_0474;
				end_IL_0000:;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
			IL_0474:
			return result;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<double?> MaxAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<double?>> func, CancellationToken cancellationToken2)
		{
			double? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double? result;
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						result = value;
						break;
					}
					value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (!value.HasValue)
					{
						continue;
					}
					double valueVal = value.GetValueOrDefault();
					while (true)
					{
						if (!double.IsNaN(valueVal))
						{
							while (await e.MoveNextAsync())
							{
								double? num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
								double valueOrDefault = num.GetValueOrDefault();
								if (num.HasValue & (valueOrDefault > valueVal))
								{
									valueVal = valueOrDefault;
									value = num;
								}
							}
							break;
						}
						if (!(await e.MoveNextAsync()))
						{
							result = value;
							goto end_IL_0063;
						}
						double? num2 = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
						if (num2.HasValue)
						{
							double? num3;
							value = (num3 = num2);
							num3 = num3;
							valueVal = num3.GetValueOrDefault();
						}
					}
					goto end_IL_0000;
					continue;
					end_IL_0063:
					break;
				}
				goto IL_0486;
				end_IL_0000:;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
			IL_0486:
			return result;
		}
	}

	public static ValueTask<decimal> MaxAsync(this IAsyncEnumerable<decimal> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<decimal> Core(IAsyncEnumerable<decimal> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<decimal>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			decimal value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = e.Current;
				while (await e.MoveNextAsync())
				{
					decimal current = e.Current;
					if (current > value)
					{
						value = current;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<decimal> MaxAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, decimal> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, decimal> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			decimal value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = func(e.Current);
				while (await e.MoveNextAsync())
				{
					decimal num = func(e.Current);
					if (num > value)
					{
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<decimal> MaxAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<decimal>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			decimal value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					decimal num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (num > value)
					{
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<decimal> MaxAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<decimal>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			decimal value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					decimal num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (num > value)
					{
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<decimal?> MaxAsync(this IAsyncEnumerable<decimal?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<decimal?> Core(IAsyncEnumerable<decimal?> enumerable, CancellationToken cancellationToken2)
		{
			decimal? value = null;
			ConfiguredCancelableAsyncEnumerable<decimal?>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = e.Current;
				}
				while (!value.HasValue);
				decimal valueVal = value.GetValueOrDefault();
				while (await e.MoveNextAsync())
				{
					decimal? current = e.Current;
					decimal valueOrDefault = current.GetValueOrDefault();
					if (current.HasValue && valueOrDefault > valueVal)
					{
						valueVal = valueOrDefault;
						value = current;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<decimal?> MaxAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, decimal?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, decimal?> func, CancellationToken cancellationToken2)
		{
			decimal? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = func(e.Current);
				}
				while (!value.HasValue);
				decimal valueVal = value.GetValueOrDefault();
				while (await e.MoveNextAsync())
				{
					decimal? num = func(e.Current);
					decimal valueOrDefault = num.GetValueOrDefault();
					if (num.HasValue && valueOrDefault > valueVal)
					{
						valueVal = valueOrDefault;
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<decimal?> MaxAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<decimal?>> func, CancellationToken cancellationToken2)
		{
			decimal? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				}
				while (!value.HasValue);
				decimal valueVal = value.GetValueOrDefault();
				while (await e.MoveNextAsync())
				{
					decimal? num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					decimal valueOrDefault = num.GetValueOrDefault();
					if (num.HasValue && valueOrDefault > valueVal)
					{
						valueVal = valueOrDefault;
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	private static ValueTask<decimal?> MaxAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<decimal?>> func, CancellationToken cancellationToken2)
		{
			decimal? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				}
				while (!value.HasValue);
				decimal valueVal = value.GetValueOrDefault();
				while (await e.MoveNextAsync())
				{
					decimal? num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					decimal valueOrDefault = num.GetValueOrDefault();
					if (num.HasValue && valueOrDefault > valueVal)
					{
						valueVal = valueOrDefault;
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<int> MinAsync(this IAsyncEnumerable<int> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<int> Core(IAsyncEnumerable<int> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<int>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			int value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = e.Current;
				while (await e.MoveNextAsync())
				{
					int current = e.Current;
					if (current < value)
					{
						value = current;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<int> MinAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<int> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, int> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			int value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = func(e.Current);
				while (await e.MoveNextAsync())
				{
					int num = func(e.Current);
					if (num < value)
					{
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<int> MinAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<int> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<int>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			int value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					int num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (num < value)
					{
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<int> MinAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<int> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<int>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			int value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					int num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (num < value)
					{
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<int?> MinAsync(this IAsyncEnumerable<int?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<int?> Core(IAsyncEnumerable<int?> enumerable, CancellationToken cancellationToken2)
		{
			int? value = null;
			ConfiguredCancelableAsyncEnumerable<int?>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = e.Current;
				}
				while (!value.HasValue);
				int valueVal = value.GetValueOrDefault();
				while (await e.MoveNextAsync())
				{
					int? current = e.Current;
					int valueOrDefault = current.GetValueOrDefault();
					if (current.HasValue & (valueOrDefault < valueVal))
					{
						valueVal = valueOrDefault;
						value = current;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<int?> MinAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<int?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, int?> func, CancellationToken cancellationToken2)
		{
			int? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = func(e.Current);
				}
				while (!value.HasValue);
				int valueVal = value.GetValueOrDefault();
				while (await e.MoveNextAsync())
				{
					int? num = func(e.Current);
					int valueOrDefault = num.GetValueOrDefault();
					if (num.HasValue & (valueOrDefault < valueVal))
					{
						valueVal = valueOrDefault;
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<int?> MinAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<int?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<int?>> func, CancellationToken cancellationToken2)
		{
			int? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				}
				while (!value.HasValue);
				int valueVal = value.GetValueOrDefault();
				while (await e.MoveNextAsync())
				{
					int? num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					int valueOrDefault = num.GetValueOrDefault();
					if (num.HasValue & (valueOrDefault < valueVal))
					{
						valueVal = valueOrDefault;
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<int?> MinAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<int?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<int?>> func, CancellationToken cancellationToken2)
		{
			int? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				}
				while (!value.HasValue);
				int valueVal = value.GetValueOrDefault();
				while (await e.MoveNextAsync())
				{
					int? num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					int valueOrDefault = num.GetValueOrDefault();
					if (num.HasValue & (valueOrDefault < valueVal))
					{
						valueVal = valueOrDefault;
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<long> MinAsync(this IAsyncEnumerable<long> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<long> Core(IAsyncEnumerable<long> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<long>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			long value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = e.Current;
				while (await e.MoveNextAsync())
				{
					long current = e.Current;
					if (current < value)
					{
						value = current;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<long> MinAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, long> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<long> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, long> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			long value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = func(e.Current);
				while (await e.MoveNextAsync())
				{
					long num = func(e.Current);
					if (num < value)
					{
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<long> MinAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<long> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<long>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			long value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					long num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (num < value)
					{
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<long> MinAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<long> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<long>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			long value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					long num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (num < value)
					{
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<long?> MinAsync(this IAsyncEnumerable<long?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<long?> Core(IAsyncEnumerable<long?> enumerable, CancellationToken cancellationToken2)
		{
			long? value = null;
			ConfiguredCancelableAsyncEnumerable<long?>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = e.Current;
				}
				while (!value.HasValue);
				long valueVal = value.GetValueOrDefault();
				while (await e.MoveNextAsync())
				{
					long? current = e.Current;
					long valueOrDefault = current.GetValueOrDefault();
					if (current.HasValue & (valueOrDefault < valueVal))
					{
						valueVal = valueOrDefault;
						value = current;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<long?> MinAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, long?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<long?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, long?> func, CancellationToken cancellationToken2)
		{
			long? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = func(e.Current);
				}
				while (!value.HasValue);
				long valueVal = value.GetValueOrDefault();
				while (await e.MoveNextAsync())
				{
					long? num = func(e.Current);
					long valueOrDefault = num.GetValueOrDefault();
					if (num.HasValue & (valueOrDefault < valueVal))
					{
						valueVal = valueOrDefault;
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<long?> MinAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<long?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<long?>> func, CancellationToken cancellationToken2)
		{
			long? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				}
				while (!value.HasValue);
				long valueVal = value.GetValueOrDefault();
				while (await e.MoveNextAsync())
				{
					long? num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					long valueOrDefault = num.GetValueOrDefault();
					if (num.HasValue & (valueOrDefault < valueVal))
					{
						valueVal = valueOrDefault;
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<long?> MinAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<long?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<long?>> func, CancellationToken cancellationToken2)
		{
			long? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				}
				while (!value.HasValue);
				long valueVal = value.GetValueOrDefault();
				while (await e.MoveNextAsync())
				{
					long? num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					long valueOrDefault = num.GetValueOrDefault();
					if (num.HasValue & (valueOrDefault < valueVal))
					{
						valueVal = valueOrDefault;
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<float> MinAsync(this IAsyncEnumerable<float> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<float> Core(IAsyncEnumerable<float> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<float>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = e.Current;
				while (await e.MoveNextAsync())
				{
					float current = e.Current;
					if (current < value)
					{
						value = current;
					}
					else if (float.IsNaN(current))
					{
						return current;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<float> MinAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, float> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, float> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = func(e.Current);
				while (await e.MoveNextAsync())
				{
					float num = func(e.Current);
					if (num < value)
					{
						value = num;
					}
					else if (float.IsNaN(num))
					{
						return num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<float> MinAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<float>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					float num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (num < value)
					{
						value = num;
					}
					else if (float.IsNaN(num))
					{
						return num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<float> MinAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<float>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					float num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (num < value)
					{
						value = num;
					}
					else if (float.IsNaN(num))
					{
						return num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<float?> MinAsync(this IAsyncEnumerable<float?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<float?> Core(IAsyncEnumerable<float?> enumerable, CancellationToken cancellationToken2)
		{
			float? value = null;
			ConfiguredCancelableAsyncEnumerable<float?>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float? result;
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						result = value;
						break;
					}
					value = e.Current;
					if (!value.HasValue)
					{
						continue;
					}
					float valueVal = value.GetValueOrDefault();
					while (await e.MoveNextAsync())
					{
						float? current = e.Current;
						if (current.HasValue)
						{
							float valueOrDefault = current.GetValueOrDefault();
							if (valueOrDefault < valueVal)
							{
								valueVal = valueOrDefault;
								value = current;
							}
							else if (float.IsNaN(valueOrDefault))
							{
								result = current;
								goto end_IL_004f;
							}
						}
					}
					goto end_IL_0000;
					continue;
					end_IL_004f:
					break;
				}
				goto IL_025d;
				end_IL_0000:;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
			IL_025d:
			return result;
		}
	}

	public static ValueTask<float?> MinAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, float?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, float?> func, CancellationToken cancellationToken2)
		{
			float? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float? result;
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						result = value;
						break;
					}
					value = func(e.Current);
					if (!value.HasValue)
					{
						continue;
					}
					float valueVal = value.GetValueOrDefault();
					while (await e.MoveNextAsync())
					{
						float? num = func(e.Current);
						if (num.HasValue)
						{
							float valueOrDefault = num.GetValueOrDefault();
							if (valueOrDefault < valueVal)
							{
								valueVal = valueOrDefault;
								value = num;
							}
							else if (float.IsNaN(valueOrDefault))
							{
								result = num;
								goto end_IL_004f;
							}
						}
					}
					goto end_IL_0000;
					continue;
					end_IL_004f:
					break;
				}
				goto IL_0273;
				end_IL_0000:;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
			IL_0273:
			return result;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<float?> MinAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<float?>> func, CancellationToken cancellationToken2)
		{
			float? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float? result;
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						result = value;
						break;
					}
					value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (!value.HasValue)
					{
						continue;
					}
					float valueVal = value.GetValueOrDefault();
					while (await e.MoveNextAsync())
					{
						float? num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
						if (num.HasValue)
						{
							float valueOrDefault = num.GetValueOrDefault();
							if (valueOrDefault < valueVal)
							{
								valueVal = valueOrDefault;
								value = num;
							}
							else if (float.IsNaN(valueOrDefault))
							{
								result = num;
								goto end_IL_005b;
							}
						}
					}
					goto end_IL_0000;
					continue;
					end_IL_005b:
					break;
				}
				goto IL_0352;
				end_IL_0000:;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
			IL_0352:
			return result;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<float?> MinAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<float?>> func, CancellationToken cancellationToken2)
		{
			float? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			float? result;
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						result = value;
						break;
					}
					value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (!value.HasValue)
					{
						continue;
					}
					float valueVal = value.GetValueOrDefault();
					while (await e.MoveNextAsync())
					{
						float? num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
						if (num.HasValue)
						{
							float valueOrDefault = num.GetValueOrDefault();
							if (valueOrDefault < valueVal)
							{
								valueVal = valueOrDefault;
								value = num;
							}
							else if (float.IsNaN(valueOrDefault))
							{
								result = num;
								goto end_IL_005b;
							}
						}
					}
					goto end_IL_0000;
					continue;
					end_IL_005b:
					break;
				}
				goto IL_035e;
				end_IL_0000:;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
			IL_035e:
			return result;
		}
	}

	public static ValueTask<double> MinAsync(this IAsyncEnumerable<double> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<double> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<double>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = e.Current;
				while (await e.MoveNextAsync())
				{
					double current = e.Current;
					if (current < value)
					{
						value = current;
					}
					else if (double.IsNaN(current))
					{
						return current;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<double> MinAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, double> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, double> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = func(e.Current);
				while (await e.MoveNextAsync())
				{
					double num = func(e.Current);
					if (num < value)
					{
						value = num;
					}
					else if (double.IsNaN(num))
					{
						return num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<double> MinAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<double>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					double num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (num < value)
					{
						value = num;
					}
					else if (double.IsNaN(num))
					{
						return num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<double> MinAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<double>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					double num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (num < value)
					{
						value = num;
					}
					else if (double.IsNaN(num))
					{
						return num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<double?> MinAsync(this IAsyncEnumerable<double?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<double?> enumerable, CancellationToken cancellationToken2)
		{
			double? value = null;
			ConfiguredCancelableAsyncEnumerable<double?>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double? result;
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						result = value;
						break;
					}
					value = e.Current;
					if (!value.HasValue)
					{
						continue;
					}
					double valueVal = value.GetValueOrDefault();
					while (await e.MoveNextAsync())
					{
						double? current = e.Current;
						if (current.HasValue)
						{
							double valueOrDefault = current.GetValueOrDefault();
							if (valueOrDefault < valueVal)
							{
								valueVal = valueOrDefault;
								value = current;
							}
							else if (double.IsNaN(valueOrDefault))
							{
								result = current;
								goto end_IL_004f;
							}
						}
					}
					goto end_IL_0000;
					continue;
					end_IL_004f:
					break;
				}
				goto IL_025d;
				end_IL_0000:;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
			IL_025d:
			return result;
		}
	}

	public static ValueTask<double?> MinAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, double?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, double?> func, CancellationToken cancellationToken2)
		{
			double? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double? result;
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						result = value;
						break;
					}
					value = func(e.Current);
					if (!value.HasValue)
					{
						continue;
					}
					double valueVal = value.GetValueOrDefault();
					while (await e.MoveNextAsync())
					{
						double? num = func(e.Current);
						if (num.HasValue)
						{
							double valueOrDefault = num.GetValueOrDefault();
							if (valueOrDefault < valueVal)
							{
								valueVal = valueOrDefault;
								value = num;
							}
							else if (double.IsNaN(valueOrDefault))
							{
								result = num;
								goto end_IL_004f;
							}
						}
					}
					goto end_IL_0000;
					continue;
					end_IL_004f:
					break;
				}
				goto IL_0273;
				end_IL_0000:;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
			IL_0273:
			return result;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<double?> MinAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<double?>> func, CancellationToken cancellationToken2)
		{
			double? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double? result;
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						result = value;
						break;
					}
					value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (!value.HasValue)
					{
						continue;
					}
					double valueVal = value.GetValueOrDefault();
					while (await e.MoveNextAsync())
					{
						double? num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
						if (num.HasValue)
						{
							double valueOrDefault = num.GetValueOrDefault();
							if (valueOrDefault < valueVal)
							{
								valueVal = valueOrDefault;
								value = num;
							}
							else if (double.IsNaN(valueOrDefault))
							{
								result = num;
								goto end_IL_005b;
							}
						}
					}
					goto end_IL_0000;
					continue;
					end_IL_005b:
					break;
				}
				goto IL_0352;
				end_IL_0000:;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
			IL_0352:
			return result;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<double?> MinAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<double?>> func, CancellationToken cancellationToken2)
		{
			double? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			double? result;
			try
			{
				while (true)
				{
					if (!(await e.MoveNextAsync()))
					{
						result = value;
						break;
					}
					value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (!value.HasValue)
					{
						continue;
					}
					double valueVal = value.GetValueOrDefault();
					while (await e.MoveNextAsync())
					{
						double? num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
						if (num.HasValue)
						{
							double valueOrDefault = num.GetValueOrDefault();
							if (valueOrDefault < valueVal)
							{
								valueVal = valueOrDefault;
								value = num;
							}
							else if (double.IsNaN(valueOrDefault))
							{
								result = num;
								goto end_IL_005b;
							}
						}
					}
					goto end_IL_0000;
					continue;
					end_IL_005b:
					break;
				}
				goto IL_035e;
				end_IL_0000:;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
			IL_035e:
			return result;
		}
	}

	public static ValueTask<decimal> MinAsync(this IAsyncEnumerable<decimal> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<decimal> Core(IAsyncEnumerable<decimal> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<decimal>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			decimal value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = e.Current;
				while (await e.MoveNextAsync())
				{
					decimal current = e.Current;
					if (current < value)
					{
						value = current;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<decimal> MinAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, decimal> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, decimal> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			decimal value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = func(e.Current);
				while (await e.MoveNextAsync())
				{
					decimal num = func(e.Current);
					if (num < value)
					{
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<decimal> MinAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<decimal>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			decimal value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					decimal num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					if (num < value)
					{
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<decimal> MinAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<decimal>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			decimal value;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				while (await e.MoveNextAsync())
				{
					decimal num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					if (num < value)
					{
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<decimal?> MinAsync(this IAsyncEnumerable<decimal?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<decimal?> Core(IAsyncEnumerable<decimal?> enumerable, CancellationToken cancellationToken2)
		{
			decimal? value = null;
			ConfiguredCancelableAsyncEnumerable<decimal?>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = e.Current;
				}
				while (!value.HasValue);
				decimal valueVal = value.GetValueOrDefault();
				while (await e.MoveNextAsync())
				{
					decimal? current = e.Current;
					decimal valueOrDefault = current.GetValueOrDefault();
					if (current.HasValue && valueOrDefault < valueVal)
					{
						valueVal = valueOrDefault;
						value = current;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static ValueTask<decimal?> MinAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, decimal?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, decimal?> func, CancellationToken cancellationToken2)
		{
			decimal? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = func(e.Current);
				}
				while (!value.HasValue);
				decimal valueVal = value.GetValueOrDefault();
				while (await e.MoveNextAsync())
				{
					decimal? num = func(e.Current);
					decimal valueOrDefault = num.GetValueOrDefault();
					if (num.HasValue && valueOrDefault < valueVal)
					{
						valueVal = valueOrDefault;
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<decimal?> MinAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<decimal?>> func, CancellationToken cancellationToken2)
		{
			decimal? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
				}
				while (!value.HasValue);
				decimal valueVal = value.GetValueOrDefault();
				while (await e.MoveNextAsync())
				{
					decimal? num = await func(e.Current).ConfigureAwait(continueOnCapturedContext: false);
					decimal valueOrDefault = num.GetValueOrDefault();
					if (num.HasValue && valueOrDefault < valueVal)
					{
						valueVal = valueOrDefault;
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	private static ValueTask<decimal?> MinAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<decimal?>> func, CancellationToken cancellationToken2)
		{
			decimal? value = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						return value;
					}
					value = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				}
				while (!value.HasValue);
				decimal valueVal = value.GetValueOrDefault();
				while (await e.MoveNextAsync())
				{
					decimal? num = await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
					decimal valueOrDefault = num.GetValueOrDefault();
					if (num.HasValue && valueOrDefault < valueVal)
					{
						valueVal = valueOrDefault;
						value = num;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return value;
		}
	}

	public static IAsyncEnumerable<TResult> OfType<TResult>(this IAsyncEnumerable<object?> source)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<object?> source2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			await foreach (object item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (item is TResult)
				{
					yield return (TResult)item;
				}
			}
		}
	}

	public static IOrderedAsyncEnumerable<TSource> OrderBy<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return new OrderedAsyncEnumerable<TSource, TKey>(source, keySelector, null, descending: false, null);
	}

	[Obsolete("Use OrderBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the OrderByAwait functionality now exists as an overload of OrderBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IOrderedAsyncEnumerable<TSource> OrderByAwaitCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector)
	{
		return new OrderedAsyncEnumerableWithTask<TSource, TKey>(source, keySelector, null, descending: false, null);
	}

	[Obsolete("Use OrderBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the OrderByAwaitWithCancellation functionality now exists as an overload of OrderBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IOrderedAsyncEnumerable<TSource> OrderByAwaitWithCancellationCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		return new OrderedAsyncEnumerableWithTaskAndCancellation<TSource, TKey>(source, keySelector, null, descending: false, null);
	}

	public static IOrderedAsyncEnumerable<TSource> OrderBy<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
	{
		return new OrderedAsyncEnumerable<TSource, TKey>(source, keySelector, comparer, descending: false, null);
	}

	[Obsolete("Use OrderBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the OrderByAwait functionality now exists as an overload of OrderBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IOrderedAsyncEnumerable<TSource> OrderByAwaitCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return new OrderedAsyncEnumerableWithTask<TSource, TKey>(source, keySelector, comparer, descending: false, null);
	}

	[Obsolete("Use OrderBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the OrderByAwaitWithCancellation functionality now exists as an overload of OrderBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IOrderedAsyncEnumerable<TSource> OrderByAwaitWithCancellationCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return new OrderedAsyncEnumerableWithTaskAndCancellation<TSource, TKey>(source, keySelector, comparer, descending: false, null);
	}

	public static IOrderedAsyncEnumerable<TSource> OrderByDescending<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return new OrderedAsyncEnumerable<TSource, TKey>(source, keySelector, null, descending: true, null);
	}

	[Obsolete("Use OrderByDescending. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the OrderByDescendingAwait functionality now exists as an overload of OrderByDescending. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IOrderedAsyncEnumerable<TSource> OrderByDescendingAwaitCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector)
	{
		return new OrderedAsyncEnumerableWithTask<TSource, TKey>(source, keySelector, null, descending: true, null);
	}

	[Obsolete("Use OrderByDescending. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the OrderByDescendingAwaitWithCancellation functionality now exists as an overload of OrderByDescending.")]
	private static IOrderedAsyncEnumerable<TSource> OrderByDescendingAwaitWithCancellationCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		return new OrderedAsyncEnumerableWithTaskAndCancellation<TSource, TKey>(source, keySelector, null, descending: true, null);
	}

	public static IOrderedAsyncEnumerable<TSource> OrderByDescending<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
	{
		return new OrderedAsyncEnumerable<TSource, TKey>(source, keySelector, comparer, descending: true, null);
	}

	[Obsolete("Use OrderByDescending. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the OrderByDescendingAwait functionality now exists as an overload of OrderByDescending. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IOrderedAsyncEnumerable<TSource> OrderByDescendingAwaitCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return new OrderedAsyncEnumerableWithTask<TSource, TKey>(source, keySelector, comparer, descending: true, null);
	}

	[Obsolete("Use OrderByDescending. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the OrderByDescendingAwaitWithCancellation functionality now exists as an overload of OrderByDescending.")]
	private static IOrderedAsyncEnumerable<TSource> OrderByDescendingAwaitWithCancellationCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return new OrderedAsyncEnumerableWithTaskAndCancellation<TSource, TKey>(source, keySelector, comparer, descending: true, null);
	}

	public static IOrderedAsyncEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return source.CreateOrderedEnumerable(keySelector, null, descending: false);
	}

	[Obsolete("Use ThenBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ThenByAwait functionality now exists as an overload of ThenBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IOrderedAsyncEnumerable<TSource> ThenByAwaitCore<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return source.CreateOrderedEnumerable(keySelector, (IComparer<TKey>?)null, false);
	}

	[Obsolete("Use ThenBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ThenByAwaitWithCancellation functionality now exists as an overload of ThenBy.")]
	private static IOrderedAsyncEnumerable<TSource> ThenByAwaitWithCancellationCore<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return source.CreateOrderedEnumerable(keySelector, null, descending: false);
	}

	public static IOrderedAsyncEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return source.CreateOrderedEnumerable(keySelector, comparer, descending: false);
	}

	[Obsolete("Use ThenBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ThenByAwait functionality now exists as an overload of ThenBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IOrderedAsyncEnumerable<TSource> ThenByAwaitCore<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return source.CreateOrderedEnumerable(keySelector, comparer, descending: false);
	}

	[Obsolete("Use ThenBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ThenByAwaitWithCancellation functionality now exists as an overload of ThenBy.")]
	private static IOrderedAsyncEnumerable<TSource> ThenByAwaitWithCancellationCore<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return source.CreateOrderedEnumerable(keySelector, comparer, descending: false);
	}

	public static IOrderedAsyncEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return source.CreateOrderedEnumerable(keySelector, null, descending: true);
	}

	[Obsolete("Use ThenByDescending. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ThenByDescendingAwait functionality now exists as an overload of ThenByDescending. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IOrderedAsyncEnumerable<TSource> ThenByDescendingAwaitCore<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return source.CreateOrderedEnumerable(keySelector, (IComparer<TKey>?)null, true);
	}

	[Obsolete("Use ThenByDescending. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ThenByDescendingAwaitWithCancellation functionality now exists as an overload of ThenByDescending.")]
	private static IOrderedAsyncEnumerable<TSource> ThenByDescendingAwaitWithCancellationCore<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return source.CreateOrderedEnumerable(keySelector, null, descending: true);
	}

	public static IOrderedAsyncEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return source.CreateOrderedEnumerable(keySelector, comparer, descending: true);
	}

	[Obsolete("Use ThenByDescending. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ThenByDescendingAwait functionality now exists as an overload of ThenByDescending. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IOrderedAsyncEnumerable<TSource> ThenByDescendingAwaitCore<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return source.CreateOrderedEnumerable(keySelector, comparer, descending: true);
	}

	[Obsolete("Use ThenByDescending. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ThenByDescendingAwaitWithCancellation functionality now exists as an overload of ThenByDescending.")]
	private static IOrderedAsyncEnumerable<TSource> ThenByDescendingAwaitWithCancellationCore<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return source.CreateOrderedEnumerable(keySelector, comparer, descending: true);
	}

	public static IAsyncEnumerable<int> Range(int start, int count)
	{
		if (count < 0)
		{
			throw System.Error.ArgumentOutOfRange("count");
		}
		if ((long)start + (long)count - 1 > int.MaxValue)
		{
			throw System.Error.ArgumentOutOfRange("count");
		}
		if (count == 0)
		{
			return Empty<int>();
		}
		return new RangeAsyncIterator(start, count);
	}

	public static IAsyncEnumerable<TResult> Repeat<TResult>(TResult element, int count)
	{
		if (count < 0)
		{
			throw System.Error.ArgumentOutOfRange("count");
		}
		return new RepeatAsyncIterator<TResult>(element, count);
	}

	public static IAsyncEnumerable<TSource> Reverse<TSource>(this IAsyncEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return new ReverseAsyncIterator<TSource>(source);
	}

	public static IAsyncEnumerable<TResult> Select<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		if (!(source is System.Linq.AsyncIterator<TSource> asyncIterator))
		{
			if (source is IList<TSource> source2)
			{
				return new SelectIListIterator<TSource, TResult>(source2, selector);
			}
			return new SelectEnumerableAsyncIterator<TSource, TResult>(source, selector);
		}
		return asyncIterator.Select(selector);
	}

	public static IAsyncEnumerable<TResult> Select<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, TResult> selector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TSource> source2, Func<TSource, int, TResult> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			int index = -1;
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				index = checked(index + 1);
				yield return func(item, index);
			}
		}
	}

	[Obsolete("Use Select. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectAwait functionality now exists as overloads of Select. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TResult> SelectAwaitCore<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TResult>> selector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		if (!(source is System.Linq.AsyncIterator<TSource> asyncIterator))
		{
			if (source is IList<TSource> source2)
			{
				return new SelectIListIteratorWithTask<TSource, TResult>(source2, selector);
			}
			return new SelectEnumerableAsyncIteratorWithTask<TSource, TResult>(source, selector);
		}
		return asyncIterator.Select(selector);
	}

	[Obsolete("Use Select. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectAwaitWithCancellation functionality now exists as overloads of Select.")]
	private static IAsyncEnumerable<TResult> SelectAwaitWithCancellationCore<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TResult>> selector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		if (!(source is System.Linq.AsyncIterator<TSource> asyncIterator))
		{
			if (source is IList<TSource> source2)
			{
				return new SelectIListIteratorWithTaskAndCancellation<TSource, TResult>(source2, selector);
			}
			return new SelectEnumerableAsyncIteratorWithTaskAndCancellation<TSource, TResult>(source, selector);
		}
		return asyncIterator.Select(selector);
	}

	[Obsolete("Use Select. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectAwait functionality now exists as overloads of Select. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TResult> SelectAwaitCore<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, ValueTask<TResult>> selector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TSource> source2, Func<TSource, int, ValueTask<TResult>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			int index = -1;
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				index = checked(index + 1);
				yield return await func(item, index).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	[Obsolete("Use Select. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectAwaitWithCancellation functionality now exists as overloads of Select.")]
	private static IAsyncEnumerable<TResult> SelectAwaitWithCancellationCore<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<TResult>> selector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TSource> source2, Func<TSource, int, CancellationToken, ValueTask<TResult>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			int index = -1;
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				index = checked(index + 1);
				yield return await func(item, index, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	private static Func<TSource, TResult> CombineSelectors<TSource, TMiddle, TResult>(Func<TSource, TMiddle> selector1, Func<TMiddle, TResult> selector2)
	{
		if (selector1.Target is ICombinedSelectors<TSource, TMiddle> combinedSelectors)
		{
			return combinedSelectors.Combine(selector2).Invoke;
		}
		return new CombinedSelectors2<TSource, TMiddle, TResult>(selector1, selector2).Invoke;
	}

	private static Func<TSource, ValueTask<TResult>> CombineSelectors<TSource, TMiddle, TResult>(Func<TSource, ValueTask<TMiddle>> selector1, Func<TMiddle, ValueTask<TResult>> selector2)
	{
		if (selector1.Target is ICombinedAsyncSelectors<TSource, TMiddle> combinedAsyncSelectors)
		{
			return combinedAsyncSelectors.Combine(selector2).Invoke;
		}
		return new CombinedAsyncSelectors2<TSource, TMiddle, TResult>(selector1, selector2).Invoke;
	}

	private static Func<TSource, CancellationToken, ValueTask<TResult>> CombineSelectors<TSource, TMiddle, TResult>(Func<TSource, CancellationToken, ValueTask<TMiddle>> selector1, Func<TMiddle, CancellationToken, ValueTask<TResult>> selector2)
	{
		if (selector1.Target is ICombinedAsyncSelectorsWithCancellation<TSource, TMiddle> combinedAsyncSelectorsWithCancellation)
		{
			return combinedAsyncSelectorsWithCancellation.Combine(selector2).Invoke;
		}
		return new CombinedAsyncSelectorsWithCancellation2<TSource, TMiddle, TResult>(selector1, selector2).Invoke;
	}

	public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, IAsyncEnumerable<TResult>> selector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return new SelectManyAsyncIterator<TSource, TResult>(source, selector);
	}

	[Obsolete("Use SelectMany. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectManyAwait functionality now exists as overloads of SelectMany. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TResult> SelectManyAwaitCore<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<IAsyncEnumerable<TResult>>> selector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return new SelectManyAsyncIteratorWithTask<TSource, TResult>(source, selector);
	}

	[Obsolete("Use SelectMany. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectManyAwaitWithCancellation functionality now exists as overloads of SelectMany.")]
	private static IAsyncEnumerable<TResult> SelectManyAwaitWithCancellationCore<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<IAsyncEnumerable<TResult>>> selector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return new SelectManyAsyncIteratorWithTaskAndCancellation<TSource, TResult>(source, selector);
	}

	public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, IAsyncEnumerable<TResult>> selector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TSource> source2, Func<TSource, int, IAsyncEnumerable<TResult>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			int index = -1;
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				index = checked(index + 1);
				IAsyncEnumerable<TResult> source3 = func(item, index);
				await foreach (TResult item2 in source3.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item2;
				}
			}
		}
	}

	[Obsolete("Use SelectMany. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectManyAwait functionality now exists as overloads of SelectMany. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TResult> SelectManyAwaitCore<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, ValueTask<IAsyncEnumerable<TResult>>> selector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TSource> source2, Func<TSource, int, ValueTask<IAsyncEnumerable<TResult>>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			int index = -1;
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				index = checked(index + 1);
				await foreach (TResult item2 in (await func(item, index).ConfigureAwait(continueOnCapturedContext: false)).WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item2;
				}
			}
		}
	}

	[Obsolete("Use SelectMany. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectManyAwaitWithCancellation functionality now exists as overloads of SelectMany.")]
	private static IAsyncEnumerable<TResult> SelectManyAwaitWithCancellationCore<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<IAsyncEnumerable<TResult>>> selector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TSource> source2, Func<TSource, int, CancellationToken, ValueTask<IAsyncEnumerable<TResult>>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			int index = -1;
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				index = checked(index + 1);
				await foreach (TResult item2 in (await func(item, index, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item2;
				}
			}
		}
	}

	public static IAsyncEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, IAsyncEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (collectionSelector == null)
		{
			throw System.Error.ArgumentNull("collectionSelector");
		}
		if (resultSelector == null)
		{
			throw System.Error.ArgumentNull("resultSelector");
		}
		return Core(source, collectionSelector, resultSelector);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TSource> source2, Func<TSource, IAsyncEnumerable<TCollection>> func, Func<TSource, TCollection, TResult> func2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			await foreach (TSource element in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				IAsyncEnumerable<TCollection> source3 = func(element);
				await foreach (TCollection item in source3.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return func2(element, item);
				}
			}
		}
	}

	[Obsolete("Use SelectMany. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectManyAwait functionality now exists as overloads of SelectMany.")]
	private static IAsyncEnumerable<TResult> SelectManyAwaitCore<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<IAsyncEnumerable<TCollection>>> collectionSelector, Func<TSource, TCollection, ValueTask<TResult>> resultSelector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (collectionSelector == null)
		{
			throw System.Error.ArgumentNull("collectionSelector");
		}
		if (resultSelector == null)
		{
			throw System.Error.ArgumentNull("resultSelector");
		}
		return Core(source, collectionSelector, resultSelector);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<IAsyncEnumerable<TCollection>>> func, Func<TSource, TCollection, ValueTask<TResult>> func2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			await foreach (TSource element in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				await foreach (TCollection item in (await func(element).ConfigureAwait(continueOnCapturedContext: false)).WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return await func2(element, item).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
		}
	}

	[Obsolete("Use SelectMany. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectManyAwaitWithCancellation functionality now exists as overloads of SelectMany.")]
	private static IAsyncEnumerable<TResult> SelectManyAwaitWithCancellationCore<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<IAsyncEnumerable<TCollection>>> collectionSelector, Func<TSource, TCollection, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (collectionSelector == null)
		{
			throw System.Error.ArgumentNull("collectionSelector");
		}
		if (resultSelector == null)
		{
			throw System.Error.ArgumentNull("resultSelector");
		}
		return Core(source, collectionSelector, resultSelector);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<IAsyncEnumerable<TCollection>>> func, Func<TSource, TCollection, CancellationToken, ValueTask<TResult>> func2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			await foreach (TSource element in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				await foreach (TCollection item in (await func(element, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return await func2(element, item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
		}
	}

	public static IAsyncEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, IAsyncEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (collectionSelector == null)
		{
			throw System.Error.ArgumentNull("collectionSelector");
		}
		if (resultSelector == null)
		{
			throw System.Error.ArgumentNull("resultSelector");
		}
		return Core(source, collectionSelector, resultSelector);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TSource> source2, Func<TSource, int, IAsyncEnumerable<TCollection>> func, Func<TSource, TCollection, TResult> func2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			int index = -1;
			await foreach (TSource element in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				index = checked(index + 1);
				IAsyncEnumerable<TCollection> source3 = func(element, index);
				await foreach (TCollection item in source3.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return func2(element, item);
				}
			}
		}
	}

	[Obsolete("Use SelectMany. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectManyAwait functionality now exists as overloads of SelectMany. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TResult> SelectManyAwaitCore<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, ValueTask<IAsyncEnumerable<TCollection>>> collectionSelector, Func<TSource, TCollection, ValueTask<TResult>> resultSelector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (collectionSelector == null)
		{
			throw System.Error.ArgumentNull("collectionSelector");
		}
		if (resultSelector == null)
		{
			throw System.Error.ArgumentNull("resultSelector");
		}
		return Core(source, collectionSelector, resultSelector);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TSource> source2, Func<TSource, int, ValueTask<IAsyncEnumerable<TCollection>>> func, Func<TSource, TCollection, ValueTask<TResult>> func2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			int index = -1;
			await foreach (TSource element in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				index = checked(index + 1);
				await foreach (TCollection item in (await func(element, index).ConfigureAwait(continueOnCapturedContext: false)).WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return await func2(element, item).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
		}
	}

	[Obsolete("Use SelectMany. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectManyAwaitWithCancellation functionality now exists as overloads of SelectMany.")]
	private static IAsyncEnumerable<TResult> SelectManyAwaitWithCancellationCore<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<IAsyncEnumerable<TCollection>>> collectionSelector, Func<TSource, TCollection, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (collectionSelector == null)
		{
			throw System.Error.ArgumentNull("collectionSelector");
		}
		if (resultSelector == null)
		{
			throw System.Error.ArgumentNull("resultSelector");
		}
		return Core(source, collectionSelector, resultSelector);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TSource> source2, Func<TSource, int, CancellationToken, ValueTask<IAsyncEnumerable<TCollection>>> func, Func<TSource, TCollection, CancellationToken, ValueTask<TResult>> func2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			int index = -1;
			await foreach (TSource element in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				index = checked(index + 1);
				await foreach (TCollection item in (await func(element, index, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return await func2(element, item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
		}
	}

	public static ValueTask<bool> SequenceEqualAsync<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second, CancellationToken cancellationToken = default(CancellationToken))
	{
		return SequenceEqualAsync(first, second, null, cancellationToken);
	}

	public static ValueTask<bool> SequenceEqualAsync<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second, IEqualityComparer<TSource>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (first == null)
		{
			throw System.Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw System.Error.ArgumentNull("second");
		}
		if (comparer == null)
		{
			comparer = EqualityComparer<TSource>.Default;
		}
		if (first is ICollection<TSource> collection && second is ICollection<TSource> collection2)
		{
			if (collection.Count != collection2.Count)
			{
				return new ValueTask<bool>(result: false);
			}
			if (collection is IList<TSource> list && collection2 is IList<TSource> list2)
			{
				int count = collection.Count;
				for (int i = 0; i < count; i++)
				{
					if (!comparer.Equals(list[i], list2[i]))
					{
						return new ValueTask<bool>(result: false);
					}
				}
				return new ValueTask<bool>(result: true);
			}
		}
		return Core(first, second, comparer, cancellationToken);
		static async ValueTask<bool> Core(IAsyncEnumerable<TSource> enumerable, IAsyncEnumerable<TSource> enumerable2, IEqualityComparer<TSource> equalityComparer, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e1 = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			bool result;
			try
			{
				ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e2 = enumerable2.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
				bool flag;
				try
				{
					while (true)
					{
						if (!(await e1.MoveNextAsync()))
						{
							flag = !(await e2.MoveNextAsync());
							break;
						}
						if (!(await e2.MoveNextAsync()) || !equalityComparer.Equals(e1.Current, e2.Current))
						{
							flag = false;
							break;
						}
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable2 = e2 as IAsyncDisposable;
					if (asyncDisposable2 != null)
					{
						await asyncDisposable2.DisposeAsync();
					}
				}
				result = flag;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e1 as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result;
		}
	}

	public static ValueTask<TSource> SingleAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> asyncEnumerable, CancellationToken cancellationToken2)
		{
			if (asyncEnumerable is IList<TSource> { Count: var count } list)
			{
				return count switch
				{
					0 => throw System.Error.NoElements(), 
					1 => list[0], 
					_ => throw System.Error.MoreThanOneElement(), 
				};
			}
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = asyncEnumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TSource result2;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw System.Error.NoElements();
				}
				TSource result = e.Current;
				if (await e.MoveNextAsync())
				{
					throw System.Error.MoreThanOneElement();
				}
				result2 = result;
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return result2;
		}
	}

	public static ValueTask<TSource> SingleAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, bool> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					TSource result = e.Current;
					if (func(result))
					{
						while (await e.MoveNextAsync())
						{
							if (func(e.Current))
							{
								throw System.Error.MoreThanOneElement();
							}
						}
						return result;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			throw System.Error.NoElements();
		}
	}

	[Obsolete("Use SingleAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SingleAwaitAsync functionality now exists as overloads of SingleAsync.")]
	private static ValueTask<TSource> SingleAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<bool>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					TSource result = e.Current;
					if (await func(result).ConfigureAwait(continueOnCapturedContext: false))
					{
						while (await e.MoveNextAsync())
						{
							if (await func(e.Current).ConfigureAwait(continueOnCapturedContext: false))
							{
								throw System.Error.MoreThanOneElement();
							}
						}
						return result;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			throw System.Error.NoElements();
		}
	}

	[Obsolete("Use SingleAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SingleAwaitWithCancellationAsync functionality now exists as overloads of SingleAsync.")]
	private static ValueTask<TSource> SingleAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<bool>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					TSource result = e.Current;
					if (await func(result, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
					{
						while (await e.MoveNextAsync())
						{
							if (await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
							{
								throw System.Error.MoreThanOneElement();
							}
						}
						return result;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			throw System.Error.NoElements();
		}
	}

	public static ValueTask<TSource?> SingleOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<TSource?> Core(IAsyncEnumerable<TSource> asyncEnumerable, CancellationToken cancellationToken2)
		{
			if (asyncEnumerable is IList<TSource> { Count: var count } list)
			{
				return count switch
				{
					0 => default(TSource), 
					1 => list[0], 
					_ => throw System.Error.MoreThanOneElement(), 
				};
			}
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = asyncEnumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TSource result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					result = default(TSource);
					goto IL_0245;
				}
				TSource result2 = e.Current;
				if (!(await e.MoveNextAsync()))
				{
					result = result2;
					goto IL_0245;
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			throw System.Error.MoreThanOneElement();
			IL_0245:
			return result;
		}
	}

	public static ValueTask<TSource?> SingleOrDefaultAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<TSource?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, bool> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					TSource result = e.Current;
					if (func(result))
					{
						while (await e.MoveNextAsync())
						{
							if (func(e.Current))
							{
								throw System.Error.MoreThanOneElement();
							}
						}
						return result;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return default(TSource);
		}
	}

	[Obsolete("Use SingleOrDefaultAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SingleOrDefaultAwaitAsync functionality now exists as overloads of SingleOrDefaultAsync.")]
	private static ValueTask<TSource?> SingleOrDefaultAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<TSource?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<bool>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					TSource result = e.Current;
					if (await func(result).ConfigureAwait(continueOnCapturedContext: false))
					{
						while (await e.MoveNextAsync())
						{
							if (await func(e.Current).ConfigureAwait(continueOnCapturedContext: false))
							{
								throw System.Error.MoreThanOneElement();
							}
						}
						return result;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return default(TSource);
		}
	}

	[Obsolete("Use SingleOrDefaultAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SingleOrDefaultAwaitWithCancellationAsync functionality now exists as overloads of SingleOrDefaultAsync.")]
	private static ValueTask<TSource?> SingleOrDefaultAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate, cancellationToken);
		static async ValueTask<TSource?> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<bool>> func, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					TSource result = e.Current;
					if (await func(result, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
					{
						while (await e.MoveNextAsync())
						{
							if (await func(e.Current, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
							{
								throw System.Error.MoreThanOneElement();
							}
						}
						return result;
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			return default(TSource);
		}
	}

	public static IAsyncEnumerable<TSource> Skip<TSource>(this IAsyncEnumerable<TSource> source, int count)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (count <= 0)
		{
			if (source is System.Linq.AsyncIteratorBase<TSource> || source is IAsyncPartition<TSource>)
			{
				return source;
			}
			count = 0;
		}
		else
		{
			if (source is IAsyncPartition<TSource> asyncPartition)
			{
				return asyncPartition.Skip(count);
			}
			if (source is IList<TSource> source2)
			{
				return new AsyncListPartition<TSource>(source2, count, int.MaxValue);
			}
		}
		return new AsyncEnumerablePartition<TSource>(source, count, -1);
	}

	public static IAsyncEnumerable<TSource> SkipLast<TSource>(this IAsyncEnumerable<TSource> source, int count)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (count <= 0)
		{
			if (source is System.Linq.AsyncIteratorBase<TSource>)
			{
				return source;
			}
			count = 0;
		}
		return Core(source, count);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> enumerable, int num, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			Queue<TSource> queue = new Queue<TSource>();
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				while (await e.MoveNextAsync())
				{
					if (queue.Count == num)
					{
						do
						{
							yield return queue.Dequeue();
							queue.Enqueue(e.Current);
						}
						while (await e.MoveNextAsync());
						break;
					}
					queue.Enqueue(e.Current);
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> SkipWhile<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, bool> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				TSource current;
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						yield break;
					}
					current = e.Current;
				}
				while (func(current));
				yield return current;
				while (await e.MoveNextAsync())
				{
					yield return e.Current;
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			throw null;
		}
	}

	public static IAsyncEnumerable<TSource> SkipWhile<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, bool> predicate)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, int, bool> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				int index = -1;
				TSource current;
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						yield break;
					}
					index = checked(index + 1);
					current = e.Current;
				}
				while (func(current, index));
				yield return current;
				while (await e.MoveNextAsync())
				{
					yield return e.Current;
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			throw null;
		}
	}

	[Obsolete("Use SkipWhile. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SkipWhileAwait functionality now exists as overloads of SkipWhile. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TSource> SkipWhileAwaitCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<bool>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				TSource element;
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						yield break;
					}
					element = e.Current;
				}
				while (await func(element).ConfigureAwait(continueOnCapturedContext: false));
				yield return element;
				while (await e.MoveNextAsync())
				{
					yield return e.Current;
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			throw null;
		}
	}

	[Obsolete("Use SkipWhile. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SkipWhileAwaitWithCancellation functionality now exists as overloads of SkipWhile.")]
	private static IAsyncEnumerable<TSource> SkipWhileAwaitWithCancellationCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<bool>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				TSource element;
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						yield break;
					}
					element = e.Current;
				}
				while (await func(element, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
				yield return element;
				while (await e.MoveNextAsync())
				{
					yield return e.Current;
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			throw null;
		}
	}

	[Obsolete("Use SkipWhile. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SkipWhileAwait functionality now exists as overloads of SkipWhile. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TSource> SkipWhileAwaitCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, ValueTask<bool>> predicate)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, int, ValueTask<bool>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				int index = -1;
				TSource element;
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						yield break;
					}
					index = checked(index + 1);
					element = e.Current;
				}
				while (await func(element, index).ConfigureAwait(continueOnCapturedContext: false));
				yield return element;
				while (await e.MoveNextAsync())
				{
					yield return e.Current;
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			throw null;
		}
	}

	[Obsolete("Use SkipWhile. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SkipWhileAwaitWithCancellation functionality now exists as overloads of SkipWhile.")]
	private static IAsyncEnumerable<TSource> SkipWhileAwaitWithCancellationCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<bool>> predicate)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, int, CancellationToken, ValueTask<bool>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				int index = -1;
				TSource element;
				do
				{
					if (!(await e.MoveNextAsync()))
					{
						yield break;
					}
					index = checked(index + 1);
					element = e.Current;
				}
				while (await func(element, index, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
				yield return element;
				while (await e.MoveNextAsync())
				{
					yield return e.Current;
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			throw null;
		}
	}

	public static ValueTask<int> SumAsync(this IAsyncEnumerable<int> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<int> Core(IAsyncEnumerable<int> source2, CancellationToken cancellationToken2)
		{
			int sum = 0;
			await foreach (int item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum = checked(sum + item);
			}
			return sum;
		}
	}

	public static ValueTask<int> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<int> Core(IAsyncEnumerable<TSource> source2, Func<TSource, int> func, CancellationToken cancellationToken2)
		{
			int sum = 0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				int num = func(item);
				sum = checked(sum + num);
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<int> SumAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<int> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<int>> func, CancellationToken cancellationToken2)
		{
			int sum = 0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum = checked(sum + await func(item).ConfigureAwait(continueOnCapturedContext: false));
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<int> SumAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<int> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<int>> func, CancellationToken cancellationToken2)
		{
			int sum = 0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum = checked(sum + await func(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false));
			}
			return sum;
		}
	}

	public static ValueTask<long> SumAsync(this IAsyncEnumerable<long> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<long> Core(IAsyncEnumerable<long> source2, CancellationToken cancellationToken2)
		{
			long sum = 0L;
			await foreach (long item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum = checked(sum + item);
			}
			return sum;
		}
	}

	public static ValueTask<long> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, long> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<long> Core(IAsyncEnumerable<TSource> source2, Func<TSource, long> func, CancellationToken cancellationToken2)
		{
			long sum = 0L;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				long num = func(item);
				sum = checked(sum + num);
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<long> SumAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<long> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<long>> func, CancellationToken cancellationToken2)
		{
			long sum = 0L;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum = checked(sum + await func(item).ConfigureAwait(continueOnCapturedContext: false));
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<long> SumAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<long> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<long>> func, CancellationToken cancellationToken2)
		{
			long sum = 0L;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum = checked(sum + await func(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false));
			}
			return sum;
		}
	}

	public static ValueTask<float> SumAsync(this IAsyncEnumerable<float> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<float> Core(IAsyncEnumerable<float> source2, CancellationToken cancellationToken2)
		{
			float sum = 0f;
			await foreach (float item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += item;
			}
			return sum;
		}
	}

	public static ValueTask<float> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, float> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float> Core(IAsyncEnumerable<TSource> source2, Func<TSource, float> func, CancellationToken cancellationToken2)
		{
			float sum = 0f;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				float num = func(item);
				sum += num;
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<float> SumAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<float>> func, CancellationToken cancellationToken2)
		{
			float sum = 0f;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += await func(item).ConfigureAwait(continueOnCapturedContext: false);
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<float> SumAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<float>> func, CancellationToken cancellationToken2)
		{
			float sum = 0f;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += await func(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			}
			return sum;
		}
	}

	public static ValueTask<double> SumAsync(this IAsyncEnumerable<double> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<double> source2, CancellationToken cancellationToken2)
		{
			double sum = 0.0;
			await foreach (double item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += item;
			}
			return sum;
		}
	}

	public static ValueTask<double> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, double> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<TSource> source2, Func<TSource, double> func, CancellationToken cancellationToken2)
		{
			double sum = 0.0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				double num = func(item);
				sum += num;
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<double> SumAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<double>> func, CancellationToken cancellationToken2)
		{
			double sum = 0.0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += await func(item).ConfigureAwait(continueOnCapturedContext: false);
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<double> SumAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<double>> func, CancellationToken cancellationToken2)
		{
			double sum = 0.0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += await func(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			}
			return sum;
		}
	}

	public static ValueTask<decimal> SumAsync(this IAsyncEnumerable<decimal> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<decimal> Core(IAsyncEnumerable<decimal> source2, CancellationToken cancellationToken2)
		{
			decimal sum = 0m;
			await foreach (decimal item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += item;
			}
			return sum;
		}
	}

	public static ValueTask<decimal> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, decimal> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal> Core(IAsyncEnumerable<TSource> source2, Func<TSource, decimal> func, CancellationToken cancellationToken2)
		{
			decimal sum = 0m;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				decimal num = func(item);
				sum += num;
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<decimal> SumAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<decimal>> func, CancellationToken cancellationToken2)
		{
			decimal sum = 0m;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += await func(item).ConfigureAwait(continueOnCapturedContext: false);
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<decimal> SumAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<decimal>> func, CancellationToken cancellationToken2)
		{
			decimal sum = 0m;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += await func(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			}
			return sum;
		}
	}

	public static ValueTask<int?> SumAsync(this IAsyncEnumerable<int?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<int?> Core(IAsyncEnumerable<int?> source2, CancellationToken cancellationToken2)
		{
			int sum = 0;
			await foreach (int? item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum = checked(sum + item.GetValueOrDefault());
			}
			return sum;
		}
	}

	public static ValueTask<int?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<int?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, int?> func, CancellationToken cancellationToken2)
		{
			int sum = 0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum = checked(sum + func(item).GetValueOrDefault());
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<int?> SumAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<int?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<int?>> func, CancellationToken cancellationToken2)
		{
			int sum = 0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum = checked(sum + (await func(item).ConfigureAwait(continueOnCapturedContext: false)).GetValueOrDefault());
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<int?> SumAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<int?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<int?>> func, CancellationToken cancellationToken2)
		{
			int sum = 0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum = checked(sum + (await func(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false)).GetValueOrDefault());
			}
			return sum;
		}
	}

	public static ValueTask<long?> SumAsync(this IAsyncEnumerable<long?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<long?> Core(IAsyncEnumerable<long?> source2, CancellationToken cancellationToken2)
		{
			long sum = 0L;
			await foreach (long? item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum = checked(sum + item.GetValueOrDefault());
			}
			return sum;
		}
	}

	public static ValueTask<long?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, long?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<long?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, long?> func, CancellationToken cancellationToken2)
		{
			long sum = 0L;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum = checked(sum + func(item).GetValueOrDefault());
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<long?> SumAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<long?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<long?>> func, CancellationToken cancellationToken2)
		{
			long sum = 0L;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum = checked(sum + (await func(item).ConfigureAwait(continueOnCapturedContext: false)).GetValueOrDefault());
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<long?> SumAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<long?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<long?>> func, CancellationToken cancellationToken2)
		{
			long sum = 0L;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum = checked(sum + (await func(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false)).GetValueOrDefault());
			}
			return sum;
		}
	}

	public static ValueTask<float?> SumAsync(this IAsyncEnumerable<float?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<float?> Core(IAsyncEnumerable<float?> source2, CancellationToken cancellationToken2)
		{
			float sum = 0f;
			await foreach (float? item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += item.GetValueOrDefault();
			}
			return sum;
		}
	}

	public static ValueTask<float?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, float?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, float?> func, CancellationToken cancellationToken2)
		{
			float sum = 0f;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += func(item).GetValueOrDefault();
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<float?> SumAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<float?>> func, CancellationToken cancellationToken2)
		{
			float sum = 0f;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += (await func(item).ConfigureAwait(continueOnCapturedContext: false)).GetValueOrDefault();
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<float?> SumAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<float?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<float?>> func, CancellationToken cancellationToken2)
		{
			float sum = 0f;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += (await func(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false)).GetValueOrDefault();
			}
			return sum;
		}
	}

	public static ValueTask<double?> SumAsync(this IAsyncEnumerable<double?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<double?> source2, CancellationToken cancellationToken2)
		{
			double sum = 0.0;
			await foreach (double? item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += item.GetValueOrDefault();
			}
			return sum;
		}
	}

	public static ValueTask<double?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, double?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, double?> func, CancellationToken cancellationToken2)
		{
			double sum = 0.0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += func(item).GetValueOrDefault();
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<double?> SumAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<double?>> func, CancellationToken cancellationToken2)
		{
			double sum = 0.0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += (await func(item).ConfigureAwait(continueOnCapturedContext: false)).GetValueOrDefault();
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<double?> SumAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<double?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<double?>> func, CancellationToken cancellationToken2)
		{
			double sum = 0.0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += (await func(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false)).GetValueOrDefault();
			}
			return sum;
		}
	}

	public static ValueTask<decimal?> SumAsync(this IAsyncEnumerable<decimal?> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<decimal?> Core(IAsyncEnumerable<decimal?> source2, CancellationToken cancellationToken2)
		{
			decimal sum = 0m;
			await foreach (decimal? item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += item.GetValueOrDefault();
			}
			return sum;
		}
	}

	public static ValueTask<decimal?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, decimal?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, decimal?> func, CancellationToken cancellationToken2)
		{
			decimal sum = 0m;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += func(item).GetValueOrDefault();
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<decimal?> SumAwaitAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<decimal?>> func, CancellationToken cancellationToken2)
		{
			decimal sum = 0m;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += (await func(item).ConfigureAwait(continueOnCapturedContext: false)).GetValueOrDefault();
			}
			return sum;
		}
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	private static ValueTask<decimal?> SumAwaitWithCancellationAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(source, selector, cancellationToken);
		static async ValueTask<decimal?> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<decimal?>> func, CancellationToken cancellationToken2)
		{
			decimal sum = 0m;
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				sum += (await func(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false)).GetValueOrDefault();
			}
			return sum;
		}
	}

	public static IAsyncEnumerable<TSource> Take<TSource>(this IAsyncEnumerable<TSource> source, int count)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (count <= 0)
		{
			return Empty<TSource>();
		}
		if (source is IAsyncPartition<TSource> asyncPartition)
		{
			return asyncPartition.Take(count);
		}
		if (source is IList<TSource> source2)
		{
			return new AsyncListPartition<TSource>(source2, 0, count - 1);
		}
		return new AsyncEnumerablePartition<TSource>(source, 0, count - 1);
	}

	public static IAsyncEnumerable<TSource> TakeLast<TSource>(this IAsyncEnumerable<TSource> source, int count)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (count <= 0)
		{
			return Empty<TSource>();
		}
		return Core(source, count);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> enumerable, int num, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			Queue<TSource> queue = default(Queue<TSource>);
			try
			{
				if (await e.MoveNextAsync())
				{
					queue = new Queue<TSource>();
					queue.Enqueue(e.Current);
					while (await e.MoveNextAsync())
					{
						if (queue.Count >= num)
						{
							do
							{
								queue.Dequeue();
								queue.Enqueue(e.Current);
							}
							while (await e.MoveNextAsync());
							break;
						}
						queue.Enqueue(e.Current);
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
			do
			{
				yield return queue.Dequeue();
			}
			while (queue.Count > 0);
		}
	}

	public static IAsyncEnumerable<TSource> TakeWhile<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> source2, Func<TSource, bool> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (!func(item))
				{
					break;
				}
				yield return item;
			}
		}
	}

	public static IAsyncEnumerable<TSource> TakeWhile<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, bool> predicate)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> source2, Func<TSource, int, bool> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			int index = -1;
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				index = checked(index + 1);
				if (!func(item, index))
				{
					break;
				}
				yield return item;
			}
		}
	}

	[Obsolete("Use TakeWhile. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the TakeWhileAwait functionality now exists as overloads of TakeWhile. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TSource> TakeWhileAwaitCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<bool>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			await foreach (TSource element in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (!(await func(element).ConfigureAwait(continueOnCapturedContext: false)))
				{
					break;
				}
				yield return element;
			}
		}
	}

	[Obsolete("Use TakeWhile. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the TakeWhileAwaitWithCancellation functionality now exists as overloads of TakeWhile.")]
	private static IAsyncEnumerable<TSource> TakeWhileAwaitWithCancellationCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<bool>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			await foreach (TSource element in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (!(await func(element, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
				{
					break;
				}
				yield return element;
			}
		}
	}

	[Obsolete("Use TakeWhile. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the TakeWhileAwait functionality now exists as overloads of TakeWhile. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TSource> TakeWhileAwaitCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, ValueTask<bool>> predicate)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> source2, Func<TSource, int, ValueTask<bool>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			int index = -1;
			await foreach (TSource element in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				index = checked(index + 1);
				if (!(await func(element, index).ConfigureAwait(continueOnCapturedContext: false)))
				{
					break;
				}
				yield return element;
			}
		}
	}

	[Obsolete("Use TakeWhile. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the TakeWhileAwaitWithCancellation functionality now exists as overloads of TakeWhile.")]
	private static IAsyncEnumerable<TSource> TakeWhileAwaitWithCancellationCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<bool>> predicate)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> source2, Func<TSource, int, CancellationToken, ValueTask<bool>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			int index = -1;
			await foreach (TSource element in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				index = checked(index + 1);
				if (!(await func(element, index, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
				{
					break;
				}
				yield return element;
			}
		}
	}

	public static ValueTask<TSource[]> ToArrayAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (source is IAsyncIListProvider<TSource> asyncIListProvider)
		{
			return asyncIListProvider.ToArrayAsync(cancellationToken);
		}
		return AsyncEnumerableHelpers.ToArray(source, cancellationToken);
	}

	public static IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(this IEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (!(source is IList<TSource> source2))
		{
			if (source is ICollection<TSource> source3)
			{
				return new AsyncICollectionEnumerableAdapter<TSource>(source3);
			}
			return new AsyncEnumerableAdapter<TSource>(source);
		}
		return new AsyncIListEnumerableAdapter<TSource>(source2);
	}

	public static IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(this IObservable<TSource> source)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return new ObservableAsyncEnumerable<TSource>(source);
	}

	public static IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(this Task<TSource> task)
	{
		if (task == null)
		{
			throw System.Error.ArgumentNull("task");
		}
		return new TaskToAsyncEnumerable<TSource>(task);
	}

	public static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return ToDictionaryAsync(source, keySelector, null, cancellationToken);
	}

	public static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw System.Error.ArgumentNull("keySelector");
		}
		return Core(source, keySelector, comparer, cancellationToken);
		static async ValueTask<Dictionary<TKey, TSource>> Core(IAsyncEnumerable<TSource> source2, Func<TSource, TKey> func, IEqualityComparer<TKey>? comparer2, CancellationToken cancellationToken2)
		{
			Dictionary<TKey, TSource> d = new Dictionary<TKey, TSource>(comparer2);
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				TKey key = func(item);
				d.Add(key, item);
			}
			return d;
		}
	}

	[Obsolete("Use ToDictionaryAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToDictionaryAwaitAsync functionality now exists as overloads of ToDictionaryAsync.")]
	private static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAwaitAsyncCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitAsyncCore(keySelector, null, cancellationToken);
	}

	[Obsolete("Use ToDictionaryAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToDictionaryAwaitAsync functionality now exists as overloads of ToDictionaryAsync.")]
	private static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAwaitAsyncCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw System.Error.ArgumentNull("keySelector");
		}
		return Core(source, keySelector, comparer, cancellationToken);
		static async ValueTask<Dictionary<TKey, TSource>> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<TKey>> func, IEqualityComparer<TKey>? comparer2, CancellationToken cancellationToken2)
		{
			Dictionary<TKey, TSource> d = new Dictionary<TKey, TSource>(comparer2);
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				d.Add(await func(item).ConfigureAwait(continueOnCapturedContext: false), item);
			}
			return d;
		}
	}

	[Obsolete("Use ToDictionaryAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToDictionaryAwaitWithCancellationAsync functionality now exists as overloads of ToDictionaryAsync.")]
	private static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAwaitWithCancellationAsyncCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitWithCancellationAsyncCore(keySelector, null, cancellationToken);
	}

	[Obsolete("Use ToDictionaryAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToDictionaryAwaitWithCancellationAsync functionality now exists as overloads of ToDictionaryAsync.")]
	private static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAwaitWithCancellationAsyncCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw System.Error.ArgumentNull("keySelector");
		}
		return Core(source, keySelector, comparer, cancellationToken);
		static async ValueTask<Dictionary<TKey, TSource>> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<TKey>> func, IEqualityComparer<TKey>? comparer2, CancellationToken cancellationToken2)
		{
			Dictionary<TKey, TSource> d = new Dictionary<TKey, TSource>(comparer2);
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				d.Add(await func(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false), item);
			}
			return d;
		}
	}

	public static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return ToDictionaryAsync(source, keySelector, elementSelector, null, cancellationToken);
	}

	public static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw System.Error.ArgumentNull("keySelector");
		}
		if (elementSelector == null)
		{
			throw System.Error.ArgumentNull("elementSelector");
		}
		return Core(source, keySelector, elementSelector, comparer, cancellationToken);
		static async ValueTask<Dictionary<TKey, TElement>> Core(IAsyncEnumerable<TSource> source2, Func<TSource, TKey> func, Func<TSource, TElement> func2, IEqualityComparer<TKey>? comparer2, CancellationToken cancellationToken2)
		{
			Dictionary<TKey, TElement> d = new Dictionary<TKey, TElement>(comparer2);
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				TKey key = func(item);
				TElement value = func2(item);
				d.Add(key, value);
			}
			return d;
		}
	}

	[Obsolete("Use ToDictionaryAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToDictionaryAwaitAsync functionality now exists as overloads of ToDictionaryAsync.")]
	private static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAwaitAsyncCore<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitAsyncCore(keySelector, elementSelector, null, cancellationToken);
	}

	[Obsolete("Use ToDictionaryAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToDictionaryAwaitAsync functionality now exists as overloads of ToDictionaryAsync.")]
	private static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAwaitAsyncCore<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw System.Error.ArgumentNull("keySelector");
		}
		if (elementSelector == null)
		{
			throw System.Error.ArgumentNull("elementSelector");
		}
		return Core(source, keySelector, elementSelector, comparer, cancellationToken);
		static async ValueTask<Dictionary<TKey, TElement>> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<TKey>> func, Func<TSource, ValueTask<TElement>> func2, IEqualityComparer<TKey>? comparer2, CancellationToken cancellationToken2)
		{
			Dictionary<TKey, TElement> d = new Dictionary<TKey, TElement>(comparer2);
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				d.Add(await func(item).ConfigureAwait(continueOnCapturedContext: false), await func2(item).ConfigureAwait(continueOnCapturedContext: false));
			}
			return d;
		}
	}

	[Obsolete("Use ToDictionaryAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToDictionaryAwaitWithCancellationAsync functionality now exists as overloads of ToDictionaryAsync.")]
	private static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAwaitWithCancellationAsyncCore<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitWithCancellationAsyncCore(keySelector, elementSelector, null, cancellationToken);
	}

	[Obsolete("Use ToDictionaryAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToDictionaryAwaitWithCancellationAsync functionality now exists as overloads of ToDictionaryAsync.")]
	private static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAwaitWithCancellationAsyncCore<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw System.Error.ArgumentNull("keySelector");
		}
		if (elementSelector == null)
		{
			throw System.Error.ArgumentNull("elementSelector");
		}
		return Core(source, keySelector, elementSelector, comparer, cancellationToken);
		static async ValueTask<Dictionary<TKey, TElement>> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<TKey>> func, Func<TSource, CancellationToken, ValueTask<TElement>> func2, IEqualityComparer<TKey>? comparer2, CancellationToken cancellationToken2)
		{
			Dictionary<TKey, TElement> d = new Dictionary<TKey, TElement>(comparer2);
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				d.Add(await func(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false), await func2(item, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false));
			}
			return d;
		}
	}

	[Obsolete("IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and it does not implement this method because 'sync over async' of this kind is an anti-pattern. Please use a different strategy.")]
	public static IEnumerable<TSource> ToEnumerable<TSource>(this IAsyncEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source);
		static IEnumerable<TSource> Core(IAsyncEnumerable<TSource> asyncEnumerable)
		{
			IAsyncEnumerator<TSource> e = asyncEnumerable.GetAsyncEnumerator();
			try
			{
				while (Wait(e.MoveNextAsync()))
				{
					yield return e.Current;
				}
			}
			finally
			{
				Wait(e.DisposeAsync());
			}
		}
	}

	private static void Wait(ValueTask task)
	{
		ValueTaskAwaiter awaiter = task.GetAwaiter();
		if (!awaiter.IsCompleted)
		{
			task.AsTask().GetAwaiter().GetResult();
		}
		else
		{
			awaiter.GetResult();
		}
	}

	private static T Wait<T>(ValueTask<T> task)
	{
		ValueTaskAwaiter<T> awaiter = task.GetAwaiter();
		if (!awaiter.IsCompleted)
		{
			return task.AsTask().GetAwaiter().GetResult();
		}
		return awaiter.GetResult();
	}

	public static ValueTask<HashSet<TSource>> ToHashSetAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ToHashSetAsync(source, null, cancellationToken);
	}

	public static ValueTask<HashSet<TSource>> ToHashSetAsync<TSource>(this IAsyncEnumerable<TSource> source, IEqualityComparer<TSource>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return Core(source, comparer, cancellationToken);
		static async ValueTask<HashSet<TSource>> Core(IAsyncEnumerable<TSource> source2, IEqualityComparer<TSource>? comparer2, CancellationToken cancellationToken2)
		{
			HashSet<TSource> set = new HashSet<TSource>(comparer2);
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				set.Add(item);
			}
			return set;
		}
	}

	public static ValueTask<List<TSource>> ToListAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (source is IAsyncIListProvider<TSource> asyncIListProvider)
		{
			return asyncIListProvider.ToListAsync(cancellationToken);
		}
		return Core(source, cancellationToken);
		static async ValueTask<List<TSource>> Core(IAsyncEnumerable<TSource> source2, CancellationToken cancellationToken2)
		{
			List<TSource> list = new List<TSource>();
			await foreach (TSource item in source2.WithCancellation(cancellationToken2).ConfigureAwait(continueOnCapturedContext: false))
			{
				list.Add(item);
			}
			return list;
		}
	}

	public static ValueTask<ILookup<TKey, TSource>> ToLookupAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ToLookupAsync(source, keySelector, null, cancellationToken);
	}

	public static ValueTask<ILookup<TKey, TSource>> ToLookupAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw System.Error.ArgumentNull("keySelector");
		}
		return Core(source, keySelector, comparer, cancellationToken);
		static async ValueTask<ILookup<TKey, TSource>> Core(IAsyncEnumerable<TSource> source2, Func<TSource, TKey> keySelector2, IEqualityComparer<TKey>? comparer2, CancellationToken cancellationToken2)
		{
			return await System.Linq.Internal.Lookup<TKey, TSource>.CreateAsync(source2, keySelector2, comparer2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	[Obsolete("Use ToLookupAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToLookupAwaitAsync functionality now exists as overloads of ToLookupAsync.")]
	private static ValueTask<ILookup<TKey, TSource>> ToLookupAwaitAsyncCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitAsyncCore(keySelector, null, cancellationToken);
	}

	[Obsolete("Use ToLookupAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToLookupAwaitAsync functionality now exists as overloads of ToLookupAsync.")]
	private static ValueTask<ILookup<TKey, TSource>> ToLookupAwaitAsyncCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw System.Error.ArgumentNull("keySelector");
		}
		return Core(source, keySelector, comparer, cancellationToken);
		static async ValueTask<ILookup<TKey, TSource>> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<TKey>> keySelector2, IEqualityComparer<TKey>? comparer2, CancellationToken cancellationToken2)
		{
			return await LookupWithTask<TKey, TSource>.CreateAsync(source2, keySelector2, comparer2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	[Obsolete("Use ToLookupAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToLookupAwaitWithCancellationAsync functionality now exists as overloads of ToLookupAsync.")]
	private static ValueTask<ILookup<TKey, TSource>> ToLookupAwaitWithCancellationAsyncCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitWithCancellationAsyncCore(keySelector, null, cancellationToken);
	}

	[Obsolete("Use ToLookupAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToLookupAwaitWithCancellationAsync functionality now exists as overloads of ToLookupAsync.")]
	private static ValueTask<ILookup<TKey, TSource>> ToLookupAwaitWithCancellationAsyncCore<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw System.Error.ArgumentNull("keySelector");
		}
		return Core(source, keySelector, comparer, cancellationToken);
		static async ValueTask<ILookup<TKey, TSource>> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector2, IEqualityComparer<TKey>? comparer2, CancellationToken cancellationToken2)
		{
			return await LookupWithTask<TKey, TSource>.CreateAsync(source2, keySelector2, comparer2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public static ValueTask<ILookup<TKey, TElement>> ToLookupAsync<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ToLookupAsync(source, keySelector, elementSelector, null, cancellationToken);
	}

	public static ValueTask<ILookup<TKey, TElement>> ToLookupAsync<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw System.Error.ArgumentNull("keySelector");
		}
		if (elementSelector == null)
		{
			throw System.Error.ArgumentNull("elementSelector");
		}
		return Core(source, keySelector, elementSelector, comparer, cancellationToken);
		static async ValueTask<ILookup<TKey, TElement>> Core(IAsyncEnumerable<TSource> source2, Func<TSource, TKey> keySelector2, Func<TSource, TElement> elementSelector2, IEqualityComparer<TKey>? comparer2, CancellationToken cancellationToken2)
		{
			return await System.Linq.Internal.Lookup<TKey, TElement>.CreateAsync(source2, keySelector2, elementSelector2, comparer2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	[Obsolete("Use ToLookupAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToLookupAwaitAsync functionality now exists as overloads of ToLookupAsync.")]
	private static ValueTask<ILookup<TKey, TElement>> ToLookupAwaitAsyncCore<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitAsyncCore(keySelector, elementSelector, null, cancellationToken);
	}

	[Obsolete("Use ToLookupAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToLookupAwaitAsync functionality now exists as overloads of ToLookupAsync.")]
	private static ValueTask<ILookup<TKey, TElement>> ToLookupAwaitAsyncCore<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw System.Error.ArgumentNull("keySelector");
		}
		if (elementSelector == null)
		{
			throw System.Error.ArgumentNull("elementSelector");
		}
		return Core(source, keySelector, elementSelector, comparer, cancellationToken);
		static async ValueTask<ILookup<TKey, TElement>> Core(IAsyncEnumerable<TSource> source2, Func<TSource, ValueTask<TKey>> keySelector2, Func<TSource, ValueTask<TElement>> elementSelector2, IEqualityComparer<TKey>? comparer2, CancellationToken cancellationToken2)
		{
			return await LookupWithTask<TKey, TElement>.CreateAsync(source2, keySelector2, elementSelector2, comparer2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	[Obsolete("Use ToLookupAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToLookupAwaitWithCancellationAsync functionality now exists as overloads of ToLookupAsync.")]
	private static ValueTask<ILookup<TKey, TElement>> ToLookupAwaitWithCancellationAsyncCore<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitWithCancellationAsyncCore(keySelector, elementSelector, null, cancellationToken);
	}

	[Obsolete("Use ToLookupAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToLookupAwaitWithCancellationAsync functionality now exists as overloads of ToLookupAsync.")]
	private static ValueTask<ILookup<TKey, TElement>> ToLookupAwaitWithCancellationAsyncCore<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw System.Error.ArgumentNull("keySelector");
		}
		if (elementSelector == null)
		{
			throw System.Error.ArgumentNull("elementSelector");
		}
		return Core(source, keySelector, elementSelector, comparer, cancellationToken);
		static async ValueTask<ILookup<TKey, TElement>> Core(IAsyncEnumerable<TSource> source2, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector2, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector2, IEqualityComparer<TKey>? comparer2, CancellationToken cancellationToken2)
		{
			return await LookupWithTask<TKey, TElement>.CreateAsync(source2, keySelector2, elementSelector2, comparer2, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public static IObservable<TSource> ToObservable<TSource>(this IAsyncEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		return new ToObservableObservable<TSource>(source);
	}

	public static IAsyncEnumerable<TSource> Union<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second)
	{
		return Union(first, second, null);
	}

	public static IAsyncEnumerable<TSource> Union<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second, IEqualityComparer<TSource>? comparer)
	{
		if (first == null)
		{
			throw System.Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw System.Error.ArgumentNull("second");
		}
		if (!(first is UnionAsyncIterator<TSource> unionAsyncIterator) || !AreEqualityComparersEqual(comparer, unionAsyncIterator._comparer))
		{
			return new UnionAsyncIterator2<TSource>(first, second, comparer);
		}
		return unionAsyncIterator.Union(second);
	}

	private static bool AreEqualityComparersEqual<TSource>(IEqualityComparer<TSource>? first, IEqualityComparer<TSource>? second)
	{
		if (first != second)
		{
			if (first != null && second != null)
			{
				return first.Equals(second);
			}
			return false;
		}
		return true;
	}

	public static IAsyncEnumerable<TSource> Where<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		if (source is System.Linq.AsyncIteratorBase<TSource> asyncIteratorBase)
		{
			return asyncIteratorBase.Where(predicate);
		}
		return new WhereEnumerableAsyncIterator<TSource>(source, predicate);
	}

	public static IAsyncEnumerable<TSource> Where<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, bool> predicate)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> source2, Func<TSource, int, bool> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			int index = -1;
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				index = checked(index + 1);
				if (func(item, index))
				{
					yield return item;
				}
			}
		}
	}

	[Obsolete("Use Where. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the WhereAwait functionality now exists as overloads of Where. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TSource> WhereAwaitCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		if (source is System.Linq.AsyncIteratorBase<TSource> asyncIteratorBase)
		{
			return asyncIteratorBase.Where(predicate);
		}
		return new WhereEnumerableAsyncIteratorWithTask<TSource>(source, predicate);
	}

	[Obsolete("Use Where. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the WhereAwaitWithCancellation functionality now exists as overloads of Where.")]
	private static IAsyncEnumerable<TSource> WhereAwaitWithCancellationCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		if (source is System.Linq.AsyncIteratorBase<TSource> asyncIteratorBase)
		{
			return asyncIteratorBase.Where(predicate);
		}
		return new WhereEnumerableAsyncIteratorWithTaskAndCancellation<TSource>(source, predicate);
	}

	[Obsolete("Use Where. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the WhereAwait functionality now exists as overloads of Where. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TSource> WhereAwaitCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, ValueTask<bool>> predicate)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> source2, Func<TSource, int, ValueTask<bool>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			int index = -1;
			await foreach (TSource element in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				index = checked(index + 1);
				if (await func(element, index).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return element;
				}
			}
		}
	}

	[Obsolete("Use Where. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the WhereAwaitWithCancellation functionality now exists as overloads of Where.")]
	private static IAsyncEnumerable<TSource> WhereAwaitWithCancellationCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<bool>> predicate)
	{
		if (source == null)
		{
			throw System.Error.ArgumentNull("source");
		}
		if (predicate == null)
		{
			throw System.Error.ArgumentNull("predicate");
		}
		return Core(source, predicate);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> source2, Func<TSource, int, CancellationToken, ValueTask<bool>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			int index = -1;
			await foreach (TSource element in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				index = checked(index + 1);
				if (await func(element, index, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return element;
				}
			}
		}
	}

	private static Func<TSource, bool> CombinePredicates<TSource>(Func<TSource, bool> predicate1, Func<TSource, bool> predicate2)
	{
		if (predicate1.Target is ICombinedPredicates<TSource> combinedPredicates)
		{
			return combinedPredicates.And(predicate2).Invoke;
		}
		return new CombinedPredicates2<TSource>(predicate1, predicate2).Invoke;
	}

	private static Func<TSource, ValueTask<bool>> CombinePredicates<TSource>(Func<TSource, ValueTask<bool>> predicate1, Func<TSource, ValueTask<bool>> predicate2)
	{
		if (predicate1.Target is ICombinedAsyncPredicates<TSource> combinedAsyncPredicates)
		{
			return combinedAsyncPredicates.And(predicate2).Invoke;
		}
		return new CombinedAsyncPredicates2<TSource>(predicate1, predicate2).Invoke;
	}

	private static Func<TSource, CancellationToken, ValueTask<bool>> CombinePredicates<TSource>(Func<TSource, CancellationToken, ValueTask<bool>> predicate1, Func<TSource, CancellationToken, ValueTask<bool>> predicate2)
	{
		if (predicate1.Target is ICombinedAsyncPredicatesWithCancellation<TSource> combinedAsyncPredicatesWithCancellation)
		{
			return combinedAsyncPredicatesWithCancellation.And(predicate2).Invoke;
		}
		return new CombinedAsyncPredicatesWithCancellation2<TSource>(predicate1, predicate2).Invoke;
	}

	public static IAsyncEnumerable<(TFirst First, TSecond Second)> Zip<TFirst, TSecond>(this IAsyncEnumerable<TFirst> first, IAsyncEnumerable<TSecond> second)
	{
		if (first == null)
		{
			throw System.Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw System.Error.ArgumentNull("second");
		}
		return Core(first, second);
		static async IAsyncEnumerable<(TFirst, TSecond)> Core(IAsyncEnumerable<TFirst> enumerable, IAsyncEnumerable<TSecond> enumerable2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TFirst>.Enumerator e1 = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				ConfiguredCancelableAsyncEnumerable<TSecond>.Enumerator e2 = enumerable2.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
				try
				{
					while (true)
					{
						bool flag = await e1.MoveNextAsync();
						if (flag)
						{
							flag = await e2.MoveNextAsync();
						}
						if (!flag)
						{
							break;
						}
						yield return (e1.Current, e2.Current);
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable2 = e2 as IAsyncDisposable;
					if (asyncDisposable2 != null)
					{
						await asyncDisposable2.DisposeAsync();
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e1 as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
		}
	}

	public static IAsyncEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IAsyncEnumerable<TFirst> first, IAsyncEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> selector)
	{
		if (first == null)
		{
			throw System.Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw System.Error.ArgumentNull("second");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(first, second, selector);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TFirst> enumerable, IAsyncEnumerable<TSecond> enumerable2, Func<TFirst, TSecond, TResult> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TFirst>.Enumerator e1 = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				ConfiguredCancelableAsyncEnumerable<TSecond>.Enumerator e2 = enumerable2.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
				try
				{
					while (true)
					{
						bool flag = await e1.MoveNextAsync();
						if (flag)
						{
							flag = await e2.MoveNextAsync();
						}
						if (!flag)
						{
							break;
						}
						yield return func(e1.Current, e2.Current);
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable2 = e2 as IAsyncDisposable;
					if (asyncDisposable2 != null)
					{
						await asyncDisposable2.DisposeAsync();
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e1 as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
		}
	}

	[Obsolete("Use Zip. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ZipAwait functionality now exists as an overload of Zip. You will need to modify your callback to take an additional CancellationToken argument.")]
	private static IAsyncEnumerable<TResult> ZipAwaitCore<TFirst, TSecond, TResult>(this IAsyncEnumerable<TFirst> first, IAsyncEnumerable<TSecond> second, Func<TFirst, TSecond, ValueTask<TResult>> selector)
	{
		if (first == null)
		{
			throw System.Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw System.Error.ArgumentNull("second");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(first, second, selector);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TFirst> enumerable, IAsyncEnumerable<TSecond> enumerable2, Func<TFirst, TSecond, ValueTask<TResult>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TFirst>.Enumerator e1 = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				ConfiguredCancelableAsyncEnumerable<TSecond>.Enumerator e2 = enumerable2.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
				try
				{
					while (true)
					{
						bool flag = await e1.MoveNextAsync();
						if (flag)
						{
							flag = await e2.MoveNextAsync();
						}
						if (!flag)
						{
							break;
						}
						yield return await func(e1.Current, e2.Current).ConfigureAwait(continueOnCapturedContext: false);
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable2 = e2 as IAsyncDisposable;
					if (asyncDisposable2 != null)
					{
						await asyncDisposable2.DisposeAsync();
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e1 as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
		}
	}

	[Obsolete("Use Zip. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ZipAwaitWithCancellation functionality now exists as an overload of Zip.")]
	private static IAsyncEnumerable<TResult> ZipAwaitWithCancellationCore<TFirst, TSecond, TResult>(this IAsyncEnumerable<TFirst> first, IAsyncEnumerable<TSecond> second, Func<TFirst, TSecond, CancellationToken, ValueTask<TResult>> selector)
	{
		if (first == null)
		{
			throw System.Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw System.Error.ArgumentNull("second");
		}
		if (selector == null)
		{
			throw System.Error.ArgumentNull("selector");
		}
		return Core(first, second, selector);
		static async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<TFirst> enumerable, IAsyncEnumerable<TSecond> enumerable2, Func<TFirst, TSecond, CancellationToken, ValueTask<TResult>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TFirst>.Enumerator e1 = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				ConfiguredCancelableAsyncEnumerable<TSecond>.Enumerator e2 = enumerable2.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
				try
				{
					while (true)
					{
						bool flag = await e1.MoveNextAsync();
						if (flag)
						{
							flag = await e2.MoveNextAsync();
						}
						if (!flag)
						{
							break;
						}
						yield return await func(e1.Current, e2.Current, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable2 = e2 as IAsyncDisposable;
					if (asyncDisposable2 != null)
					{
						await asyncDisposable2.DisposeAsync();
					}
				}
			}
			finally
			{
				IAsyncDisposable asyncDisposable = e1 as IAsyncDisposable;
				if (asyncDisposable != null)
				{
					await asyncDisposable.DisposeAsync();
				}
			}
		}
	}

	[Obsolete("Use AggregateAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AggregateAwaitAsync now exists as overloads of AggregateAsync.")]
	public static ValueTask<TSource> AggregateAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, TSource, ValueTask<TSource>> accumulator, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AggregateAwaitAsyncCore(accumulator, cancellationToken);
	}

	[Obsolete("Use AggregateAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AggregateAwaitWithCancellationAsync functionality now exists as overloads of AggregateAsync.")]
	public static ValueTask<TSource> AggregateAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, TSource, CancellationToken, ValueTask<TSource>> accumulator, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AggregateAwaitWithCancellationAsyncCore(accumulator, cancellationToken);
	}

	[Obsolete("Use AggregateAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AggregateAwaitAsync functionality now exists as overloads of AggregateAsync.")]
	public static ValueTask<TAccumulate> AggregateAwaitAsync<TSource, TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, ValueTask<TAccumulate>> accumulator, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AggregateAwaitAsyncCore(seed, accumulator, cancellationToken);
	}

	[Obsolete("Use AggregateAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AggregateAwaitWithCancellationAsync functionality now exists as overloads of AggregateAsync.")]
	public static ValueTask<TAccumulate> AggregateAwaitWithCancellationAsync<TSource, TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> accumulator, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AggregateAwaitWithCancellationAsyncCore(seed, accumulator, cancellationToken);
	}

	[Obsolete("Use AggregateAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AggregateAwaitAsync functionality now exists as overloads of AggregateAsync.")]
	public static ValueTask<TResult> AggregateAwaitAsync<TSource, TAccumulate, TResult>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, ValueTask<TAccumulate>> accumulator, Func<TAccumulate, ValueTask<TResult>> resultSelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AggregateAwaitAsyncCore(seed, accumulator, resultSelector, cancellationToken);
	}

	[Obsolete("Use AggregateAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AggregateAwaitWithCancellationAsync functionality now exists as overloads of AggregateAsync.")]
	public static ValueTask<TResult> AggregateAwaitWithCancellationAsync<TSource, TAccumulate, TResult>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> accumulator, Func<TAccumulate, CancellationToken, ValueTask<TResult>> resultSelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AggregateAwaitWithCancellationAsyncCore(seed, accumulator, resultSelector, cancellationToken);
	}

	[Obsolete("Use AllAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AllAwaitAsync functionality now exists as overloads of AllAsync.")]
	public static ValueTask<bool> AllAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AllAwaitAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use AllAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AllAwaitWithCancellationAsync functionality now exists as overloads of AllAsync.")]
	public static ValueTask<bool> AllAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AllAwaitWithCancellationAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use AnyAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AnyAwaitAsync functionality now exists as overloads of AnyAsync.")]
	public static ValueTask<bool> AnyAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AnyAwaitAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use AnyAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the AnyAwaitWithCancellationAsync functionality now exists as overloads of AnyAsync.")]
	public static ValueTask<bool> AnyAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AnyAwaitWithCancellationAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<double> AverageAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	public static ValueTask<double> AverageAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<double> AverageAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	public static ValueTask<double> AverageAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<float> AverageAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	public static ValueTask<float> AverageAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<double> AverageAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	public static ValueTask<double> AverageAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<decimal> AverageAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	public static ValueTask<decimal> AverageAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<double?> AverageAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	public static ValueTask<double?> AverageAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<double?> AverageAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	public static ValueTask<double?> AverageAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<float?> AverageAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	public static ValueTask<float?> AverageAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<double?> AverageAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	public static ValueTask<double?> AverageAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<decimal?> AverageAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use AverageAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its AverageAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take cancellable ValueTask-returning selectors are now overloads of AverageAsync, because this AverageAwaitWithCancellationAsyncCore method did not conform to current .NET naming guidelines.")]
	public static ValueTask<decimal?> AverageAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use CountAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the CountAwaitAsync functionality now exists as overloads of CountAsync.")]
	public static ValueTask<int> CountAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.CountAwaitAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use CountAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the CountAwaitWithCancellationAsync functionality now exists as overloads of CountAsync.")]
	public static ValueTask<int> CountAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.CountAwaitWithCancellationAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use FirstAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the FirstAwaitAsync functionality now exists as overloads of FirstAsync.")]
	public static ValueTask<TSource> FirstAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.FirstAwaitAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use FirstAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the FirstAwaitWithCancellationAsync functionality now exists as overloads of FirstAsync.")]
	public static ValueTask<TSource> FirstAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.FirstAwaitWithCancellationAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use FirstOrDefaultAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the FirstOrDefaultAwaitAsync functionality now exists as overloads of FirstOrDefaultAsync.")]
	public static ValueTask<TSource?> FirstOrDefaultAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.FirstOrDefaultAwaitAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use FirstOrDefaultAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the FirstOrDefaultAwaitWithCancellationAsync functionality now exists as overloads of FirstOrDefaultAsync.")]
	public static ValueTask<TSource?> FirstOrDefaultAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.FirstOrDefaultAwaitWithCancellationAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use the language support for async foreach instead.")]
	public static Task ForEachAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, Task> action, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ForEachAwaitAsyncCore(action, cancellationToken);
	}

	[Obsolete("Use the language support for async foreach instead.")]
	public static Task ForEachAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task> action, CancellationToken cancellationToken)
	{
		return source.ForEachAwaitWithCancellationAsyncCore(action, cancellationToken);
	}

	[Obsolete("Use the language support for async foreach instead.")]
	public static Task ForEachAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, Task> action, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ForEachAwaitAsyncCore(action, cancellationToken);
	}

	[Obsolete("Use the language support for async foreach instead.")]
	public static Task ForEachAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, Task> action, CancellationToken cancellationToken)
	{
		return source.ForEachAwaitWithCancellationAsyncCore(action, cancellationToken);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwait functionality now exists as overloads of GroupBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<IAsyncGrouping<TKey, TSource>> GroupByAwait<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector)
	{
		return source.GroupByAwaitCore(keySelector);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwait functionality now exists as overloads of GroupBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<IAsyncGrouping<TKey, TSource>> GroupByAwait<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer)
	{
		return source.GroupByAwaitCore(keySelector, comparer);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwaitWithCancellationAsync functionality now exists as overloads of GroupBy.")]
	public static IAsyncEnumerable<IAsyncGrouping<TKey, TSource>> GroupByAwaitWithCancellation<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		return source.GroupByAwaitWithCancellationCore(keySelector);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwaitWithCancellationAsync functionality now exists as overloads of GroupBy.")]
	public static IAsyncEnumerable<IAsyncGrouping<TKey, TSource>> GroupByAwaitWithCancellation<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer)
	{
		return source.GroupByAwaitWithCancellationCore(keySelector, comparer);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwait functionality now exists as overloads of GroupBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> GroupByAwait<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector)
	{
		return source.GroupByAwaitCore(keySelector, elementSelector);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwait functionality now exists as overloads of GroupBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> GroupByAwait<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer)
	{
		return source.GroupByAwaitCore(keySelector, elementSelector, comparer);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwaitWithCancellationAsync functionality now exists as overloads of GroupBy.")]
	public static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> GroupByAwaitWithCancellation<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector)
	{
		return source.GroupByAwaitWithCancellationCore(keySelector, elementSelector);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwaitWithCancellationAsync functionality now exists as overloads of GroupBy.")]
	public static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> GroupByAwaitWithCancellation<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer)
	{
		return source.GroupByAwaitWithCancellationCore(keySelector, elementSelector, comparer);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwait functionality now exists as overloads of GroupBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TResult> GroupByAwait<TSource, TKey, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TKey, IAsyncEnumerable<TSource>, ValueTask<TResult>> resultSelector)
	{
		return source.GroupByAwaitCore(keySelector, resultSelector);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwait functionality now exists as overloads of GroupBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TResult> GroupByAwait<TSource, TKey, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TKey, IAsyncEnumerable<TSource>, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return source.GroupByAwaitCore(keySelector, resultSelector, comparer);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwaitWithCancellationAsync functionality now exists as overloads of GroupBy.")]
	public static IAsyncEnumerable<TResult> GroupByAwaitWithCancellation<TSource, TKey, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TKey, IAsyncEnumerable<TSource>, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		return source.GroupByAwaitWithCancellationCore(keySelector, resultSelector);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwaitWithCancellationAsync functionality now exists as overloads of GroupBy.")]
	public static IAsyncEnumerable<TResult> GroupByAwaitWithCancellation<TSource, TKey, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TKey, IAsyncEnumerable<TSource>, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return source.GroupByAwaitWithCancellationCore(keySelector, resultSelector, comparer);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwait functionality now exists as overloads of GroupBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TResult> GroupByAwait<TSource, TKey, TElement, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, Func<TKey, IAsyncEnumerable<TElement>, ValueTask<TResult>> resultSelector)
	{
		return source.GroupByAwaitCore(keySelector, elementSelector, resultSelector);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwait functionality now exists as overloads of GroupBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TResult> GroupByAwait<TSource, TKey, TElement, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, Func<TKey, IAsyncEnumerable<TElement>, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return source.GroupByAwaitCore(keySelector, elementSelector, resultSelector, comparer);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwaitWithCancellationAsync functionality now exists as overloads of GroupBy.")]
	public static IAsyncEnumerable<TResult> GroupByAwaitWithCancellation<TSource, TKey, TElement, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, Func<TKey, IAsyncEnumerable<TElement>, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		return source.GroupByAwaitWithCancellationCore(keySelector, elementSelector, resultSelector);
	}

	[Obsolete("Use GroupBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupByAwaitWithCancellationAsync functionality now exists as overloads of GroupBy.")]
	public static IAsyncEnumerable<TResult> GroupByAwaitWithCancellation<TSource, TKey, TElement, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, Func<TKey, IAsyncEnumerable<TElement>, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return source.GroupByAwaitWithCancellationCore(keySelector, elementSelector, resultSelector, comparer);
	}

	[Obsolete("Use GroupJoin. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupJoinAwait functionality now exists as an overload of GroupJoin. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TResult> GroupJoinAwait<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, ValueTask<TKey>> outerKeySelector, Func<TInner, ValueTask<TKey>> innerKeySelector, Func<TOuter, IAsyncEnumerable<TInner>, ValueTask<TResult>> resultSelector)
	{
		return outer.GroupJoinAwaitCore(inner, outerKeySelector, innerKeySelector, resultSelector);
	}

	[Obsolete("Use GroupJoin. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupJoinAwait functionality now exists as an overload of GroupJoin. You will need to modify your callback to take an additional CancellationToken argument. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TResult> GroupJoinAwait<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, ValueTask<TKey>> outerKeySelector, Func<TInner, ValueTask<TKey>> innerKeySelector, Func<TOuter, IAsyncEnumerable<TInner>, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return outer.GroupJoinAwaitCore(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
	}

	[Obsolete("Use GroupJoin. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupJoinAwaitWithCancellation functionality now exists as an overload of GroupJoin.")]
	public static IAsyncEnumerable<TResult> GroupJoinAwaitWithCancellation<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, CancellationToken, ValueTask<TKey>> outerKeySelector, Func<TInner, CancellationToken, ValueTask<TKey>> innerKeySelector, Func<TOuter, IAsyncEnumerable<TInner>, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		return outer.GroupJoinAwaitWithCancellationCore(inner, outerKeySelector, innerKeySelector, resultSelector);
	}

	[Obsolete("Use GroupJoin. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the GroupJoinAwaitWithCancellation functionality now exists as an overload of GroupJoin.")]
	public static IAsyncEnumerable<TResult> GroupJoinAwaitWithCancellation<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, CancellationToken, ValueTask<TKey>> outerKeySelector, Func<TInner, CancellationToken, ValueTask<TKey>> innerKeySelector, Func<TOuter, IAsyncEnumerable<TInner>, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return outer.GroupJoinAwaitWithCancellationCore(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
	}

	[Obsolete("Use Join. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the JoinAwait functionality now exists as an overload of Join. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TResult> JoinAwait<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, ValueTask<TKey>> outerKeySelector, Func<TInner, ValueTask<TKey>> innerKeySelector, Func<TOuter, TInner, ValueTask<TResult>> resultSelector)
	{
		return outer.JoinAwaitCore(inner, outerKeySelector, innerKeySelector, resultSelector);
	}

	[Obsolete("Use Join. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the JoinAwait functionality now exists as an overload of Join. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TResult> JoinAwait<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, ValueTask<TKey>> outerKeySelector, Func<TInner, ValueTask<TKey>> innerKeySelector, Func<TOuter, TInner, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return outer.JoinAwaitCore(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
	}

	[Obsolete("Use Join. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the JoinAwaitWithCancellation functionality now exists as an overload of Join.")]
	public static IAsyncEnumerable<TResult> JoinAwaitWithCancellation<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, CancellationToken, ValueTask<TKey>> outerKeySelector, Func<TInner, CancellationToken, ValueTask<TKey>> innerKeySelector, Func<TOuter, TInner, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		return outer.JoinAwaitWithCancellationCore(inner, outerKeySelector, innerKeySelector, resultSelector);
	}

	[Obsolete("Use Join. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the JoinAwaitWithCancellation functionality now exists as an overload of Join.")]
	public static IAsyncEnumerable<TResult> JoinAwaitWithCancellation<TOuter, TInner, TKey, TResult>(this IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, CancellationToken, ValueTask<TKey>> outerKeySelector, Func<TInner, CancellationToken, ValueTask<TKey>> innerKeySelector, Func<TOuter, TInner, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return outer.JoinAwaitWithCancellationCore(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
	}

	[Obsolete("Use LastAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the LastAwaitAsync functionality now exists as overloads of LastAsync.")]
	public static ValueTask<TSource> LastAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.LastAwaitAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use LastAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the LastAwaitWithCancellationAsync functionality now exists as overloads of LastAsync.")]
	public static ValueTask<TSource> LastAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.LastAwaitWithCancellationAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use LastOrDefaultAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the LastOrDefaultAwaitAsync functionality now exists as overloads of LastOrDefaultAsync.")]
	public static ValueTask<TSource?> LastOrDefaultAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.LastOrDefaultAwaitAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use LastOrDefaultAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the LastOrDefaultAwaitWithCancellationAsync functionality now exists as overloads of LastOrDefaultAsync.")]
	public static ValueTask<TSource?> LastOrDefaultAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.LastOrDefaultAwaitWithCancellationAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use LongCountAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the LongCountAwaitAsync functionality now exists as overloads of LongCountAsync.")]
	public static ValueTask<long> LongCountAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.LongCountAwaitAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use LongCountAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the LongCountAwaitWithCancellationAsync functionality now exists as overloads of LongCountAsync.")]
	public static ValueTask<long> LongCountAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.LongCountAwaitWithCancellationAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<TResult> MaxAwaitAsync<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<TResult> MaxAwaitWithCancellationAsync<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<TResult> MinAwaitAsync<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<TResult> MinAwaitWithCancellationAsync<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<int> MaxAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<int> MaxAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<int?> MaxAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<int?> MaxAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<long> MaxAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<long> MaxAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<long?> MaxAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<long?> MaxAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<float> MaxAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<float> MaxAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<float?> MaxAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<float?> MaxAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<double> MaxAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<double> MaxAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<double?> MaxAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<double?> MaxAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<decimal> MaxAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<decimal> MaxAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<decimal?> MaxAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MaxByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MaxAwaitWithCancellationAsync now exists as an overload of MaxByAsync.")]
	public static ValueTask<decimal?> MaxAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<int> MinAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<int> MinAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<int?> MinAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<int?> MinAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<long> MinAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<long> MinAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<long?> MinAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<long?> MinAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<float> MinAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<float> MinAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<float?> MinAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<float?> MinAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<double> MinAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<double> MinAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<double?> MinAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<double?> MinAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<decimal> MinAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<decimal> MinAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<decimal?> MinAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use MinByAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality previously provided by MinAwaitWithCancellationAsync now exists as an overload of MinByAsync.")]
	public static ValueTask<decimal?> MinAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use OrderBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the OrderByAwait functionality now exists as an overload of OrderBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IOrderedAsyncEnumerable<TSource> OrderByAwait<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector)
	{
		return source.OrderByAwaitCore(keySelector);
	}

	[Obsolete("Use OrderBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the OrderByAwaitWithCancellation functionality now exists as an overload of OrderBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IOrderedAsyncEnumerable<TSource> OrderByAwaitWithCancellation<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		return source.OrderByAwaitWithCancellationCore(keySelector);
	}

	[Obsolete("Use OrderBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the OrderByAwait functionality now exists as an overload of OrderBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IOrderedAsyncEnumerable<TSource> OrderByAwait<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return source.OrderByAwaitCore(keySelector, comparer);
	}

	[Obsolete("Use OrderBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the OrderByAwaitWithCancellation functionality now exists as an overload of OrderBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IOrderedAsyncEnumerable<TSource> OrderByAwaitWithCancellation<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return source.OrderByAwaitWithCancellationCore(keySelector, comparer);
	}

	[Obsolete("Use OrderByDescending. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the OrderByDescendingAwait functionality now exists as an overload of OrderByDescending. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IOrderedAsyncEnumerable<TSource> OrderByDescendingAwait<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector)
	{
		return source.OrderByDescendingAwaitCore(keySelector);
	}

	[Obsolete("Use OrderByDescending. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the OrderByDescendingAwaitWithCancellation functionality now exists as an overload of OrderByDescending.")]
	public static IOrderedAsyncEnumerable<TSource> OrderByDescendingAwaitWithCancellation<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		return source.OrderByDescendingAwaitWithCancellationCore(keySelector);
	}

	[Obsolete("Use OrderByDescending. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the OrderByDescendingAwait functionality now exists as an overload of OrderByDescending. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IOrderedAsyncEnumerable<TSource> OrderByDescendingAwait<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return source.OrderByDescendingAwaitCore(keySelector, comparer);
	}

	[Obsolete("Use OrderByDescending. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the OrderByDescendingAwaitWithCancellation functionality now exists as an overload of OrderByDescending.")]
	public static IOrderedAsyncEnumerable<TSource> OrderByDescendingAwaitWithCancellation<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return source.OrderByDescendingAwaitWithCancellationCore(keySelector, comparer);
	}

	[Obsolete("Use ThenBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ThenByAwait functionality now exists as an overload of ThenBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IOrderedAsyncEnumerable<TSource> ThenByAwait<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector)
	{
		return source.ThenByAwaitCore(keySelector);
	}

	[Obsolete("Use ThenBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ThenByAwaitWithCancellation functionality now exists as an overload of ThenBy.")]
	public static IOrderedAsyncEnumerable<TSource> ThenByAwaitWithCancellation<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		return source.ThenByAwaitWithCancellationCore(keySelector);
	}

	[Obsolete("Use ThenBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ThenByAwait functionality now exists as an overload of ThenBy. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IOrderedAsyncEnumerable<TSource> ThenByAwait<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return source.ThenByAwaitCore(keySelector, comparer);
	}

	[Obsolete("Use ThenBy. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ThenByAwaitWithCancellation functionality now exists as an overload of ThenBy.")]
	public static IOrderedAsyncEnumerable<TSource> ThenByAwaitWithCancellation<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return source.ThenByAwaitWithCancellationCore(keySelector, comparer);
	}

	[Obsolete("Use ThenByDescending. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ThenByDescendingAwait functionality now exists as an overload of ThenByDescending. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IOrderedAsyncEnumerable<TSource> ThenByDescendingAwait<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector)
	{
		return source.ThenByDescendingAwaitCore(keySelector);
	}

	[Obsolete("Use ThenByDescending. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ThenByDescendingAwaitWithCancellation functionality now exists as an overload of ThenByDescending.")]
	public static IOrderedAsyncEnumerable<TSource> ThenByDescendingAwaitWithCancellation<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		return source.ThenByDescendingAwaitWithCancellationCore(keySelector);
	}

	[Obsolete("Use ThenByDescending. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ThenByDescendingAwait functionality now exists as an overload of ThenByDescending. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IOrderedAsyncEnumerable<TSource> ThenByDescendingAwait<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return source.ThenByDescendingAwaitCore(keySelector, comparer);
	}

	[Obsolete("Use ThenByDescending. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ThenByDescendingAwaitWithCancellation functionality now exists as an overload of ThenByDescending.")]
	public static IOrderedAsyncEnumerable<TSource> ThenByDescendingAwaitWithCancellation<TSource, TKey>(this IOrderedAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return source.ThenByDescendingAwaitWithCancellationCore(keySelector, comparer);
	}

	[Obsolete("Use Select. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectAwait functionality now exists as overloads of Select. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TResult> SelectAwait<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TResult>> selector)
	{
		return source.SelectAwaitCore(selector);
	}

	[Obsolete("Use Select. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectAwaitWithCancellation functionality now exists as overloads of Select.")]
	public static IAsyncEnumerable<TResult> SelectAwaitWithCancellation<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TResult>> selector)
	{
		return source.SelectAwaitWithCancellationCore(selector);
	}

	[Obsolete("Use Select. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectAwait functionality now exists as overloads of Select. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TResult> SelectAwait<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, ValueTask<TResult>> selector)
	{
		return source.SelectAwaitCore(selector);
	}

	[Obsolete("Use Select. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectAwaitWithCancellation functionality now exists as overloads of Select.")]
	public static IAsyncEnumerable<TResult> SelectAwaitWithCancellation<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<TResult>> selector)
	{
		return source.SelectAwaitWithCancellationCore(selector);
	}

	[Obsolete("Use SelectMany. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectManyAwait functionality now exists as overloads of SelectMany. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TResult> SelectManyAwait<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<IAsyncEnumerable<TResult>>> selector)
	{
		return source.SelectManyAwaitCore(selector);
	}

	[Obsolete("Use SelectMany. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectManyAwaitWithCancellation functionality now exists as overloads of SelectMany.")]
	public static IAsyncEnumerable<TResult> SelectManyAwaitWithCancellation<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<IAsyncEnumerable<TResult>>> selector)
	{
		return source.SelectManyAwaitWithCancellationCore(selector);
	}

	[Obsolete("Use SelectMany. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectManyAwait functionality now exists as overloads of SelectMany. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TResult> SelectManyAwait<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, ValueTask<IAsyncEnumerable<TResult>>> selector)
	{
		return source.SelectManyAwaitCore(selector);
	}

	[Obsolete("Use SelectMany. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectManyAwaitWithCancellation functionality now exists as overloads of SelectMany.")]
	public static IAsyncEnumerable<TResult> SelectManyAwaitWithCancellation<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<IAsyncEnumerable<TResult>>> selector)
	{
		return source.SelectManyAwaitWithCancellationCore(selector);
	}

	[Obsolete("Use SelectMany. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectManyAwait functionality now exists as overloads of SelectMany.")]
	public static IAsyncEnumerable<TResult> SelectManyAwait<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<IAsyncEnumerable<TCollection>>> collectionSelector, Func<TSource, TCollection, ValueTask<TResult>> resultSelector)
	{
		return source.SelectManyAwaitCore(collectionSelector, resultSelector);
	}

	[Obsolete("Use SelectMany. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectManyAwaitWithCancellation functionality now exists as overloads of SelectMany.")]
	public static IAsyncEnumerable<TResult> SelectManyAwaitWithCancellation<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<IAsyncEnumerable<TCollection>>> collectionSelector, Func<TSource, TCollection, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		return source.SelectManyAwaitWithCancellationCore(collectionSelector, resultSelector);
	}

	[Obsolete("Use SelectMany. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectManyAwait functionality now exists as overloads of SelectMany. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TResult> SelectManyAwait<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, ValueTask<IAsyncEnumerable<TCollection>>> collectionSelector, Func<TSource, TCollection, ValueTask<TResult>> resultSelector)
	{
		return source.SelectManyAwaitCore(collectionSelector, resultSelector);
	}

	[Obsolete("Use SelectMany. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SelectManyAwaitWithCancellation functionality now exists as overloads of SelectMany.")]
	public static IAsyncEnumerable<TResult> SelectManyAwaitWithCancellation<TSource, TCollection, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<IAsyncEnumerable<TCollection>>> collectionSelector, Func<TSource, TCollection, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		return source.SelectManyAwaitWithCancellationCore(collectionSelector, resultSelector);
	}

	[Obsolete("Use SingleAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SingleAwaitAsync functionality now exists as overloads of SingleAsync.")]
	public static ValueTask<TSource> SingleAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SingleAwaitAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use SingleAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SingleAwaitWithCancellationAsync functionality now exists as overloads of SingleAsync.")]
	public static ValueTask<TSource> SingleAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SingleAwaitWithCancellationAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use SingleOrDefaultAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SingleOrDefaultAwaitAsync functionality now exists as overloads of SingleOrDefaultAsync.")]
	public static ValueTask<TSource?> SingleOrDefaultAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SingleOrDefaultAwaitAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use SingleOrDefaultAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SingleOrDefaultAwaitWithCancellationAsync functionality now exists as overloads of SingleOrDefaultAsync.")]
	public static ValueTask<TSource?> SingleOrDefaultAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SingleOrDefaultAwaitWithCancellationAsyncCore(predicate, cancellationToken);
	}

	[Obsolete("Use SkipWhile. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SkipWhileAwait functionality now exists as overloads of SkipWhile. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TSource> SkipWhileAwait<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate)
	{
		return source.SkipWhileAwaitCore(predicate);
	}

	[Obsolete("Use SkipWhile. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SkipWhileAwaitWithCancellation functionality now exists as overloads of SkipWhile.")]
	public static IAsyncEnumerable<TSource> SkipWhileAwaitWithCancellation<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate)
	{
		return source.SkipWhileAwaitWithCancellationCore(predicate);
	}

	[Obsolete("Use SkipWhile. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SkipWhileAwait functionality now exists as overloads of SkipWhile. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TSource> SkipWhileAwait<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, ValueTask<bool>> predicate)
	{
		return source.SkipWhileAwaitCore(predicate);
	}

	[Obsolete("Use SkipWhile. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the SkipWhileAwaitWithCancellation functionality now exists as overloads of SkipWhile.")]
	public static IAsyncEnumerable<TSource> SkipWhileAwaitWithCancellation<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<bool>> predicate)
	{
		return source.SkipWhileAwaitWithCancellationCore(predicate);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<int> SumAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<int> SumAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<long> SumAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<long> SumAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<float> SumAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<float> SumAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<double> SumAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<double> SumAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<decimal> SumAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<decimal> SumAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<int?> SumAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<int?> SumAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<long?> SumAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<long?> SumAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<float?> SumAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<float?> SumAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<double?> SumAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<double?> SumAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<decimal?> SumAwaitAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use SumAsync in System.Interactive.Async. System.Linq.Async (a community-supported library) has been replaced by the (Microsoft supported) IAsyncEnumerable LINQ in System.Linq.AsyncEnumerable, and its SumAsync method does not include the overloads that take a selector. This functionality has moved to System.Interactive.Async, but the methods that take ValueTask-returning selectors are now overloads of SumAsync, because this SumAwaitWithCancellationAsync method did not conform to current .NET naming guidelines.")]
	public static ValueTask<decimal?> SumAwaitWithCancellationAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsyncCore(selector, cancellationToken);
	}

	[Obsolete("Use TakeWhile. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the TakeWhileAwait functionality now exists as overloads of TakeWhile. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TSource> TakeWhileAwait<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate)
	{
		return source.TakeWhileAwaitCore(predicate);
	}

	[Obsolete("Use TakeWhile. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the TakeWhileAwaitWithCancellation functionality now exists as overloads of TakeWhile.")]
	public static IAsyncEnumerable<TSource> TakeWhileAwaitWithCancellation<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate)
	{
		return source.TakeWhileAwaitWithCancellationCore(predicate);
	}

	[Obsolete("Use TakeWhile. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the TakeWhileAwait functionality now exists as overloads of TakeWhile. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TSource> TakeWhileAwait<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, ValueTask<bool>> predicate)
	{
		return source.TakeWhileAwaitCore(predicate);
	}

	[Obsolete("Use TakeWhile. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the TakeWhileAwaitWithCancellation functionality now exists as overloads of TakeWhile.")]
	public static IAsyncEnumerable<TSource> TakeWhileAwaitWithCancellation<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<bool>> predicate)
	{
		return source.TakeWhileAwaitWithCancellationCore(predicate);
	}

	[Obsolete("Use ToDictionaryAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToDictionaryAwaitAsync functionality now exists as overloads of ToDictionaryAsync.")]
	public static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAwaitAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitAsyncCore(keySelector, cancellationToken);
	}

	[Obsolete("Use ToDictionaryAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToDictionaryAwaitAsync functionality now exists as overloads of ToDictionaryAsync.")]
	public static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAwaitAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitAsyncCore(keySelector, comparer, cancellationToken);
	}

	[Obsolete("Use ToDictionaryAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToDictionaryAwaitWithCancellationAsync functionality now exists as overloads of ToDictionaryAsync.")]
	public static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAwaitWithCancellationAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitWithCancellationAsyncCore(keySelector, cancellationToken);
	}

	[Obsolete("Use ToDictionaryAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToDictionaryAwaitWithCancellationAsync functionality now exists as overloads of ToDictionaryAsync.")]
	public static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAwaitWithCancellationAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitWithCancellationAsyncCore(keySelector, comparer, cancellationToken);
	}

	[Obsolete("Use ToDictionaryAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToDictionaryAwaitAsync functionality now exists as overloads of ToDictionaryAsync.")]
	public static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAwaitAsync<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitAsyncCore(keySelector, elementSelector, cancellationToken);
	}

	[Obsolete("Use ToDictionaryAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToDictionaryAwaitAsync functionality now exists as overloads of ToDictionaryAsync.")]
	public static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAwaitAsync<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitAsyncCore(keySelector, elementSelector, comparer, cancellationToken);
	}

	[Obsolete("Use ToDictionaryAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToDictionaryAwaitWithCancellationAsync functionality now exists as overloads of ToDictionaryAsync.")]
	public static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAwaitWithCancellationAsync<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitWithCancellationAsyncCore(keySelector, elementSelector, cancellationToken);
	}

	[Obsolete("Use ToDictionaryAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToDictionaryAwaitWithCancellationAsync functionality now exists as overloads of ToDictionaryAsync.")]
	public static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAwaitWithCancellationAsync<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitWithCancellationAsyncCore(keySelector, elementSelector, comparer, cancellationToken);
	}

	[Obsolete("Use ToLookupAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToLookupAwaitAsync functionality now exists as overloads of ToLookupAsync.")]
	public static ValueTask<ILookup<TKey, TSource>> ToLookupAwaitAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitAsyncCore(keySelector, cancellationToken);
	}

	[Obsolete("Use ToLookupAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToLookupAwaitAsync functionality now exists as overloads of ToLookupAsync.")]
	public static ValueTask<ILookup<TKey, TSource>> ToLookupAwaitAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitAsyncCore(keySelector, comparer, cancellationToken);
	}

	[Obsolete("Use ToLookupAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToLookupAwaitWithCancellationAsync functionality now exists as overloads of ToLookupAsync.")]
	public static ValueTask<ILookup<TKey, TSource>> ToLookupAwaitWithCancellationAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitWithCancellationAsyncCore(keySelector, cancellationToken);
	}

	[Obsolete("Use ToLookupAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToLookupAwaitWithCancellationAsync functionality now exists as overloads of ToLookupAsync.")]
	public static ValueTask<ILookup<TKey, TSource>> ToLookupAwaitWithCancellationAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitWithCancellationAsyncCore(keySelector, comparer, cancellationToken);
	}

	[Obsolete("Use ToLookupAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToLookupAwaitAsync functionality now exists as overloads of ToLookupAsync.")]
	public static ValueTask<ILookup<TKey, TElement>> ToLookupAwaitAsync<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitAsyncCore(keySelector, elementSelector, cancellationToken);
	}

	[Obsolete("Use ToLookupAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToLookupAwaitAsync functionality now exists as overloads of ToLookupAsync.")]
	public static ValueTask<ILookup<TKey, TElement>> ToLookupAwaitAsync<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitAsyncCore(keySelector, elementSelector, comparer, cancellationToken);
	}

	[Obsolete("Use ToLookupAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToLookupAwaitWithCancellationAsync functionality now exists as overloads of ToLookupAsync.")]
	public static ValueTask<ILookup<TKey, TElement>> ToLookupAwaitWithCancellationAsync<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitWithCancellationAsyncCore(keySelector, elementSelector, cancellationToken);
	}

	[Obsolete("Use ToLookupAsync. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ToLookupAwaitWithCancellationAsync functionality now exists as overloads of ToLookupAsync.")]
	public static ValueTask<ILookup<TKey, TElement>> ToLookupAwaitWithCancellationAsync<TSource, TKey, TElement>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitWithCancellationAsyncCore(keySelector, elementSelector, comparer, cancellationToken);
	}

	[Obsolete("Use Where. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the WhereAwait functionality now exists as overloads of Where. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TSource> WhereAwait<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate)
	{
		return source.WhereAwaitCore(predicate);
	}

	[Obsolete("Use Where. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the WhereAwaitWithCancellation functionality now exists as overloads of Where.")]
	public static IAsyncEnumerable<TSource> WhereAwaitWithCancellation<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate)
	{
		return source.WhereAwaitWithCancellationCore(predicate);
	}

	[Obsolete("Use Where. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the WhereAwait functionality now exists as overloads of Where. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TSource> WhereAwait<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, ValueTask<bool>> predicate)
	{
		return source.WhereAwaitCore(predicate);
	}

	[Obsolete("Use Where. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the WhereAwaitWithCancellation functionality now exists as overloads of Where.")]
	public static IAsyncEnumerable<TSource> WhereAwaitWithCancellation<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<bool>> predicate)
	{
		return source.WhereAwaitWithCancellationCore(predicate);
	}

	[Obsolete("Use Zip. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ZipAwait functionality now exists as an overload of Zip. You will need to modify your callback to take an additional CancellationToken argument.")]
	public static IAsyncEnumerable<TResult> ZipAwait<TFirst, TSecond, TResult>(this IAsyncEnumerable<TFirst> first, IAsyncEnumerable<TSecond> second, Func<TFirst, TSecond, ValueTask<TResult>> selector)
	{
		return first.ZipAwaitCore(second, selector);
	}

	[Obsolete("Use Zip. IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the ZipAwaitWithCancellation functionality now exists as an overload of Zip.")]
	public static IAsyncEnumerable<TResult> ZipAwaitWithCancellation<TFirst, TSecond, TResult>(this IAsyncEnumerable<TFirst> first, IAsyncEnumerable<TSecond> second, Func<TFirst, TSecond, CancellationToken, ValueTask<TResult>> selector)
	{
		return first.ZipAwaitWithCancellationCore(second, selector);
	}
}
