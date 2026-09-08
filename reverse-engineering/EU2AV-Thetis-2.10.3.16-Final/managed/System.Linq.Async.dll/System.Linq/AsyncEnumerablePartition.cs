using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

internal sealed class AsyncEnumerablePartition<TSource> : System.Linq.AsyncIterator<TSource>, IAsyncPartition<TSource>, IAsyncIListProvider<TSource>, IAsyncEnumerable<TSource>
{
	private readonly IAsyncEnumerable<TSource> _source;

	private readonly int _minIndexInclusive;

	private readonly int _maxIndexInclusive;

	private IAsyncEnumerator<TSource>? _enumerator;

	private bool _hasSkipped;

	private int _taken;

	private bool HasLimit => _maxIndexInclusive != -1;

	private int Limit => _maxIndexInclusive + 1 - _minIndexInclusive;

	internal AsyncEnumerablePartition(IAsyncEnumerable<TSource> source, int minIndexInclusive, int maxIndexInclusive)
	{
		_source = source;
		_minIndexInclusive = minIndexInclusive;
		_maxIndexInclusive = maxIndexInclusive;
	}

	public override System.Linq.AsyncIteratorBase<TSource> Clone()
	{
		return new AsyncEnumerablePartition<TSource>(_source, _minIndexInclusive, _maxIndexInclusive);
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

	public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
	{
		if (onlyIfCheap)
		{
			return new ValueTask<int>(-1);
		}
		return Core();
		async ValueTask<int> Core()
		{
			if (!HasLimit)
			{
				return Math.Max(await AsyncEnumerable.CountAsync(_source, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) - _minIndexInclusive, 0);
			}
			IAsyncEnumerator<TSource> en = _source.GetAsyncEnumerator(cancellationToken);
			int result;
			try
			{
				result = Math.Max((int)(await SkipAndCountAsync((uint)(_maxIndexInclusive + 1), en).ConfigureAwait(continueOnCapturedContext: false)) - _minIndexInclusive, 0);
			}
			finally
			{
				await en.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			return result;
		}
	}

	protected override async ValueTask<bool> MoveNextCore()
	{
		System.Linq.AsyncIteratorState state = _state;
		if (state != System.Linq.AsyncIteratorState.Allocated)
		{
			if (state != System.Linq.AsyncIteratorState.Iterating)
			{
				goto IL_01aa;
			}
		}
		else
		{
			_enumerator = _source.GetAsyncEnumerator(_cancellationToken);
			_hasSkipped = false;
			_taken = 0;
			_state = System.Linq.AsyncIteratorState.Iterating;
		}
		if (!_hasSkipped)
		{
			if (!(await SkipBeforeFirstAsync(_enumerator).ConfigureAwait(continueOnCapturedContext: false)))
			{
				goto IL_01aa;
			}
			_hasSkipped = true;
		}
		bool flag = !HasLimit || _taken < Limit;
		if (flag)
		{
			flag = await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		if (flag)
		{
			if (HasLimit)
			{
				_taken++;
			}
			_current = _enumerator.Current;
			return true;
		}
		goto IL_01aa;
		IL_01aa:
		await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		return false;
	}

	public IAsyncPartition<TSource> Skip(int count)
	{
		int num = _minIndexInclusive + count;
		if (!HasLimit)
		{
			if (num < 0)
			{
				return new AsyncEnumerablePartition<TSource>(this, count, -1);
			}
		}
		else if ((uint)num > (uint)_maxIndexInclusive)
		{
			return AsyncEnumerable.EmptyAsyncIterator<TSource>.Instance;
		}
		return new AsyncEnumerablePartition<TSource>(_source, num, _maxIndexInclusive);
	}

	public IAsyncPartition<TSource> Take(int count)
	{
		int num = _minIndexInclusive + count - 1;
		if (!HasLimit)
		{
			if (num < 0)
			{
				return new AsyncEnumerablePartition<TSource>(this, 0, count - 1);
			}
		}
		else if ((uint)num >= (uint)_maxIndexInclusive)
		{
			return this;
		}
		return new AsyncEnumerablePartition<TSource>(_source, _minIndexInclusive, num);
	}

	public async ValueTask<Maybe<TSource>> TryGetElementAtAsync(int index, CancellationToken cancellationToken)
	{
		if (index >= 0 && (!HasLimit || index < Limit))
		{
			IAsyncEnumerator<TSource> en = _source.GetAsyncEnumerator(cancellationToken);
			try
			{
				bool flag = await SkipBeforeAsync(_minIndexInclusive + index, en).ConfigureAwait(continueOnCapturedContext: false);
				if (flag)
				{
					flag = await en.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false);
				}
				if (flag)
				{
					return new Maybe<TSource>(en.Current);
				}
			}
			finally
			{
				await en.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		return default(Maybe<TSource>);
	}

	public async ValueTask<Maybe<TSource>> TryGetFirstAsync(CancellationToken cancellationToken)
	{
		IAsyncEnumerator<TSource> en = _source.GetAsyncEnumerator(cancellationToken);
		try
		{
			bool flag = await SkipBeforeFirstAsync(en).ConfigureAwait(continueOnCapturedContext: false);
			if (flag)
			{
				flag = await en.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			if (flag)
			{
				return new Maybe<TSource>(en.Current);
			}
		}
		finally
		{
			await en.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		return default(Maybe<TSource>);
	}

	public async ValueTask<Maybe<TSource>> TryGetLastAsync(CancellationToken cancellationToken)
	{
		IAsyncEnumerator<TSource> en = _source.GetAsyncEnumerator(cancellationToken);
		try
		{
			bool flag = await SkipBeforeFirstAsync(en).ConfigureAwait(continueOnCapturedContext: false);
			if (flag)
			{
				flag = await en.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			if (flag)
			{
				int remaining = Limit - 1;
				int comparand = ((!HasLimit) ? int.MinValue : 0);
				TSource result;
				do
				{
					remaining--;
					result = en.Current;
					flag = remaining >= comparand;
					if (flag)
					{
						flag = await en.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false);
					}
				}
				while (flag);
				return new Maybe<TSource>(result);
			}
		}
		finally
		{
			await en.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		return default(Maybe<TSource>);
	}

	public async ValueTask<TSource[]> ToArrayAsync(CancellationToken cancellationToken)
	{
		IAsyncEnumerator<TSource> en = _source.GetAsyncEnumerator(cancellationToken);
		try
		{
			bool flag = await SkipBeforeFirstAsync(en).ConfigureAwait(continueOnCapturedContext: false);
			if (flag)
			{
				flag = await en.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			if (flag)
			{
				int remaining = Limit - 1;
				int comparand = ((!HasLimit) ? int.MinValue : 0);
				List<TSource> builder = (HasLimit ? new List<TSource>(Limit) : new List<TSource>());
				do
				{
					remaining--;
					builder.Add(en.Current);
					flag = remaining >= comparand;
					if (flag)
					{
						flag = await en.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false);
					}
				}
				while (flag);
				return builder.ToArray();
			}
		}
		finally
		{
			await en.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		return Array.Empty<TSource>();
	}

	public async ValueTask<List<TSource>> ToListAsync(CancellationToken cancellationToken)
	{
		List<TSource> list = new List<TSource>();
		IAsyncEnumerator<TSource> en = _source.GetAsyncEnumerator(cancellationToken);
		try
		{
			bool flag = await SkipBeforeFirstAsync(en).ConfigureAwait(continueOnCapturedContext: false);
			if (flag)
			{
				flag = await en.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			if (flag)
			{
				int remaining = Limit - 1;
				int comparand = ((!HasLimit) ? int.MinValue : 0);
				do
				{
					remaining--;
					list.Add(en.Current);
					flag = remaining >= comparand;
					if (flag)
					{
						flag = await en.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false);
					}
				}
				while (flag);
			}
		}
		finally
		{
			await en.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		return list;
	}

	private ValueTask<bool> SkipBeforeFirstAsync(IAsyncEnumerator<TSource> en)
	{
		return SkipBeforeAsync(_minIndexInclusive, en);
	}

	private static async ValueTask<bool> SkipBeforeAsync(int index, IAsyncEnumerator<TSource> en)
	{
		return await SkipAndCountAsync(index, en).ConfigureAwait(continueOnCapturedContext: false) == index;
	}

	private static async ValueTask<int> SkipAndCountAsync(int index, IAsyncEnumerator<TSource> en)
	{
		return (int)(await SkipAndCountAsync((uint)index, en).ConfigureAwait(continueOnCapturedContext: false));
	}

	private static async ValueTask<uint> SkipAndCountAsync(uint index, IAsyncEnumerator<TSource> en)
	{
		for (uint i = 0u; i < index; i++)
		{
			if (!(await en.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false)))
			{
				return i;
			}
		}
		return index;
	}
}
