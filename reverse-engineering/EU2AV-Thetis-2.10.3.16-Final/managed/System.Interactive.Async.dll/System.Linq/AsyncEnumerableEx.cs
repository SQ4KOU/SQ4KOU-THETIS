using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

public static class AsyncEnumerableEx
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

	private sealed class DistinctAsyncIterator<TSource, TKey> : AsyncIterator<TSource>, IAsyncIListProvider<TSource>, IAsyncEnumerable<TSource>
	{
		private readonly IEqualityComparer<TKey>? _comparer;

		private readonly Func<TSource, TKey> _keySelector;

		private readonly IAsyncEnumerable<TSource> _source;

		private IAsyncEnumerator<TSource>? _enumerator;

		private System.Linq.Set<TKey>? _set;

		public DistinctAsyncIterator(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
		{
			_source = source;
			_keySelector = keySelector;
			_comparer = comparer;
		}

		public async ValueTask<TSource[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return (await FillSetAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToArray();
		}

		public async ValueTask<List<TSource>> ToListAsync(CancellationToken cancellationToken)
		{
			return await FillSetAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public async ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return -1;
			}
			int count = 0;
			System.Linq.Set<TKey> s = new System.Linq.Set<TKey>(_comparer);
			IAsyncEnumerator<TSource> enu = _source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await enu.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
				{
					TSource current = enu.Current;
					if (s.Add(_keySelector(current)))
					{
						count++;
					}
				}
			}
			finally
			{
				await enu.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			return count;
		}

		public override AsyncIteratorBase<TSource> Clone()
		{
			return new DistinctAsyncIterator<TSource, TKey>(_source, _keySelector, _comparer);
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
			AsyncIteratorState state = _state;
			TSource current;
			if (state != AsyncIteratorState.Allocated)
			{
				if (state == AsyncIteratorState.Iterating)
				{
					while (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
					{
						current = _enumerator.Current;
						if (_set.Add(_keySelector(current)))
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
			_set = new System.Linq.Set<TKey>(_comparer);
			_set.Add(_keySelector(current));
			_current = current;
			_state = AsyncIteratorState.Iterating;
			return true;
		}

		private async Task<List<TSource>> FillSetAsync(CancellationToken cancellationToken)
		{
			System.Linq.Set<TKey> s = new System.Linq.Set<TKey>(_comparer);
			List<TSource> r = new List<TSource>();
			IAsyncEnumerator<TSource> enu = _source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await enu.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
				{
					TSource current = enu.Current;
					if (s.Add(_keySelector(current)))
					{
						r.Add(current);
					}
				}
			}
			finally
			{
				await enu.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			return r;
		}
	}

	private sealed class DistinctAsyncIteratorWithTask<TSource, TKey> : AsyncIterator<TSource>, IAsyncIListProvider<TSource>, IAsyncEnumerable<TSource>
	{
		private readonly IEqualityComparer<TKey>? _comparer;

		private readonly Func<TSource, ValueTask<TKey>> _keySelector;

		private readonly IAsyncEnumerable<TSource> _source;

		private IAsyncEnumerator<TSource>? _enumerator;

		private System.Linq.Set<TKey>? _set;

		public DistinctAsyncIteratorWithTask(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer)
		{
			_source = source;
			_keySelector = keySelector;
			_comparer = comparer;
		}

		public async ValueTask<TSource[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return (await FillSetAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToArray();
		}

		public async ValueTask<List<TSource>> ToListAsync(CancellationToken cancellationToken)
		{
			return await FillSetAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public async ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return -1;
			}
			int count = 0;
			System.Linq.Set<TKey> s = new System.Linq.Set<TKey>(_comparer);
			IAsyncEnumerator<TSource> enu = _source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await enu.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
				{
					TSource current = enu.Current;
					System.Linq.Set<TKey> set = s;
					if (set.Add(await _keySelector(current).ConfigureAwait(continueOnCapturedContext: false)))
					{
						count++;
					}
				}
			}
			finally
			{
				await enu.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			return count;
		}

		public override AsyncIteratorBase<TSource> Clone()
		{
			return new DistinctAsyncIteratorWithTask<TSource, TKey>(_source, _keySelector, _comparer);
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
			AsyncIteratorState state = _state;
			TSource element;
			System.Linq.Set<TKey> set;
			if (state != AsyncIteratorState.Allocated)
			{
				if (state == AsyncIteratorState.Iterating)
				{
					while (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
					{
						element = _enumerator.Current;
						set = _set;
						if (set.Add(await _keySelector(element).ConfigureAwait(continueOnCapturedContext: false)))
						{
							_current = element;
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
			element = _enumerator.Current;
			_set = new System.Linq.Set<TKey>(_comparer);
			set = _set;
			set.Add(await _keySelector(element).ConfigureAwait(continueOnCapturedContext: false));
			_current = element;
			_state = AsyncIteratorState.Iterating;
			return true;
		}

		private async ValueTask<List<TSource>> FillSetAsync(CancellationToken cancellationToken)
		{
			System.Linq.Set<TKey> s = new System.Linq.Set<TKey>(_comparer);
			List<TSource> r = new List<TSource>();
			IAsyncEnumerator<TSource> enu = _source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await enu.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
				{
					TSource item = enu.Current;
					System.Linq.Set<TKey> set = s;
					if (set.Add(await _keySelector(item).ConfigureAwait(continueOnCapturedContext: false)))
					{
						r.Add(item);
					}
				}
			}
			finally
			{
				await enu.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			return r;
		}
	}

	private sealed class DistinctAsyncIteratorWithTaskAndCancellation<TSource, TKey> : AsyncIterator<TSource>, IAsyncIListProvider<TSource>, IAsyncEnumerable<TSource>
	{
		private readonly IEqualityComparer<TKey>? _comparer;

		private readonly Func<TSource, CancellationToken, ValueTask<TKey>> _keySelector;

		private readonly IAsyncEnumerable<TSource> _source;

		private IAsyncEnumerator<TSource>? _enumerator;

		private System.Linq.Set<TKey>? _set;

		public DistinctAsyncIteratorWithTaskAndCancellation(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer)
		{
			_source = source;
			_keySelector = keySelector;
			_comparer = comparer;
		}

		public async ValueTask<TSource[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return (await FillSetAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToArray();
		}

		public async ValueTask<List<TSource>> ToListAsync(CancellationToken cancellationToken)
		{
			return await FillSetAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		public async ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			if (onlyIfCheap)
			{
				return -1;
			}
			int count = 0;
			System.Linq.Set<TKey> s = new System.Linq.Set<TKey>(_comparer);
			IAsyncEnumerator<TSource> enu = _source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await enu.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
				{
					TSource current = enu.Current;
					System.Linq.Set<TKey> set = s;
					if (set.Add(await _keySelector(current, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
					{
						count++;
					}
				}
			}
			finally
			{
				await enu.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			return count;
		}

		public override AsyncIteratorBase<TSource> Clone()
		{
			return new DistinctAsyncIteratorWithTaskAndCancellation<TSource, TKey>(_source, _keySelector, _comparer);
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
			AsyncIteratorState state = _state;
			TSource element;
			System.Linq.Set<TKey> set;
			if (state != AsyncIteratorState.Allocated)
			{
				if (state == AsyncIteratorState.Iterating)
				{
					while (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
					{
						element = _enumerator.Current;
						set = _set;
						if (set.Add(await _keySelector(element, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
						{
							_current = element;
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
			element = _enumerator.Current;
			_set = new System.Linq.Set<TKey>(_comparer);
			set = _set;
			set.Add(await _keySelector(element, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
			_current = element;
			_state = AsyncIteratorState.Iterating;
			return true;
		}

		private async ValueTask<List<TSource>> FillSetAsync(CancellationToken cancellationToken)
		{
			System.Linq.Set<TKey> s = new System.Linq.Set<TKey>(_comparer);
			List<TSource> r = new List<TSource>();
			IAsyncEnumerator<TSource> enu = _source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await enu.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
				{
					TSource item = enu.Current;
					System.Linq.Set<TKey> set = s;
					if (set.Add(await _keySelector(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
					{
						r.Add(item);
					}
				}
			}
			finally
			{
				await enu.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			return r;
		}
	}

	private sealed class NeverAsyncEnumerable<TValue> : IAsyncEnumerable<TValue>
	{
		private sealed class NeverAsyncEnumerator(CancellationToken token) : IAsyncEnumerator<TValue>, IAsyncDisposable
		{
			private readonly CancellationToken _token = token;

			private CancellationTokenRegistration _registration;

			private bool _once;

			public TValue Current
			{
				get
				{
					throw new InvalidOperationException();
				}
			}

			public ValueTask DisposeAsync()
			{
				_registration.Dispose();
				return default(ValueTask);
			}

			public ValueTask<bool> MoveNextAsync()
			{
				if (_once)
				{
					return new ValueTask<bool>(result: false);
				}
				_once = true;
				TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
				_registration = _token.Register(delegate(object state)
				{
					((TaskCompletionSource<bool>)state).TrySetCanceled(_token);
				}, taskCompletionSource);
				return new ValueTask<bool>(taskCompletionSource.Task);
			}
		}

		internal static readonly NeverAsyncEnumerable<TValue> Instance = new NeverAsyncEnumerable<TValue>();

		public IAsyncEnumerator<TValue> GetAsyncEnumerator(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return new NeverAsyncEnumerator(cancellationToken);
		}
	}

	private sealed class OnErrorResumeNextAsyncIterator<TSource> : AsyncIterator<TSource>
	{
		private readonly IEnumerable<IAsyncEnumerable<TSource>> _sources;

		private IAsyncEnumerator<TSource>? _enumerator;

		private IEnumerator<IAsyncEnumerable<TSource>>? _sourcesEnumerator;

		public OnErrorResumeNextAsyncIterator(IEnumerable<IAsyncEnumerable<TSource>> sources)
		{
			_sources = sources;
		}

		public override AsyncIteratorBase<TSource> Clone()
		{
			return new OnErrorResumeNextAsyncIterator<TSource>(_sources);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_sourcesEnumerator != null)
			{
				_sourcesEnumerator.Dispose();
				_sourcesEnumerator = null;
			}
			if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			AsyncIteratorState state = _state;
			if (state != AsyncIteratorState.Allocated)
			{
				if (state != AsyncIteratorState.Iterating)
				{
					goto IL_0191;
				}
			}
			else
			{
				_sourcesEnumerator = _sources.GetEnumerator();
				_state = AsyncIteratorState.Iterating;
			}
			while (true)
			{
				if (_enumerator == null)
				{
					if (!_sourcesEnumerator.MoveNext())
					{
						break;
					}
					_enumerator = _sourcesEnumerator.Current.GetAsyncEnumerator(_cancellationToken);
				}
				try
				{
					if (await _enumerator.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
					{
						_current = _enumerator.Current;
						return true;
					}
				}
				catch
				{
				}
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
			}
			goto IL_0191;
			IL_0191:
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
		}
	}

	private sealed class ReturnEnumerable<TValue> : IAsyncEnumerable<TValue>, IAsyncIListProvider<TValue>
	{
		private sealed class ReturnEnumerator : IAsyncEnumerator<TValue>, IAsyncDisposable
		{
			private bool _once;

			public TValue Current { get; private set; }

			public ReturnEnumerator(TValue current)
			{
				Current = current;
			}

			public ValueTask DisposeAsync()
			{
				Current = default(TValue);
				return default(ValueTask);
			}

			public ValueTask<bool> MoveNextAsync()
			{
				if (_once)
				{
					return new ValueTask<bool>(result: false);
				}
				_once = true;
				return new ValueTask<bool>(result: true);
			}
		}

		private readonly TValue _value;

		public ReturnEnumerable(TValue value)
		{
			_value = value;
		}

		public IAsyncEnumerator<TValue> GetAsyncEnumerator(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return new ReturnEnumerator(_value);
		}

		public ValueTask<int> GetCountAsync(bool onlyIfCheap, CancellationToken cancellationToken)
		{
			return new ValueTask<int>(1);
		}

		public ValueTask<TValue[]> ToArrayAsync(CancellationToken cancellationToken)
		{
			return new ValueTask<TValue[]>(new TValue[1] { _value });
		}

		public ValueTask<List<TValue>> ToListAsync(CancellationToken cancellationToken)
		{
			return new ValueTask<List<TValue>>(new List<TValue>(1) { _value });
		}
	}

	private sealed class ThrowEnumerable<TValue> : IAsyncEnumerable<TValue>
	{
		private sealed class ThrowEnumerator : IAsyncEnumerator<TValue>, IAsyncDisposable
		{
			private ValueTask<bool> _moveNextThrows;

			public TValue Current => default(TValue);

			public ThrowEnumerator(ValueTask<bool> moveNextThrows)
			{
				_moveNextThrows = moveNextThrows;
			}

			public ValueTask DisposeAsync()
			{
				_moveNextThrows = new ValueTask<bool>(result: false);
				return default(ValueTask);
			}

			public ValueTask<bool> MoveNextAsync()
			{
				ValueTask<bool> moveNextThrows = _moveNextThrows;
				_moveNextThrows = new ValueTask<bool>(result: false);
				return moveNextThrows;
			}
		}

		private readonly ValueTask<bool> _moveNextThrows;

		public ThrowEnumerable(ValueTask<bool> moveNextThrows)
		{
			_moveNextThrows = moveNextThrows;
		}

		public IAsyncEnumerator<TValue> GetAsyncEnumerator(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return new ThrowEnumerator(_moveNextThrows);
		}
	}

	private sealed class TimeoutAsyncIterator<TSource> : AsyncIterator<TSource>
	{
		private readonly IAsyncEnumerable<TSource> _source;

		private readonly TimeSpan _timeout;

		private IAsyncEnumerator<TSource>? _enumerator;

		private Task? _loserTask;

		private CancellationTokenSource? _sourceCTS;

		public TimeoutAsyncIterator(IAsyncEnumerable<TSource> source, TimeSpan timeout)
		{
			_source = source;
			_timeout = timeout;
		}

		public override AsyncIteratorBase<TSource> Clone()
		{
			return new TimeoutAsyncIterator<TSource>(_source, _timeout);
		}

		public override async ValueTask DisposeAsync()
		{
			if (_loserTask != null)
			{
				await _loserTask.ConfigureAwait(continueOnCapturedContext: false);
				_loserTask = null;
				_enumerator = null;
			}
			else if (_enumerator != null)
			{
				await _enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_enumerator = null;
			}
			if (_sourceCTS != null)
			{
				_sourceCTS.Dispose();
				_sourceCTS = null;
			}
			await base.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			AsyncIteratorState state = _state;
			if (state != AsyncIteratorState.Allocated)
			{
				if (state != AsyncIteratorState.Iterating)
				{
					goto IL_0272;
				}
			}
			else
			{
				_sourceCTS = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
				_enumerator = _source.GetAsyncEnumerator(_sourceCTS.Token);
				_state = AsyncIteratorState.Iterating;
			}
			ValueTask<bool> moveNext = _enumerator.MoveNextAsync();
			if (!moveNext.IsCompleted)
			{
				using CancellationTokenSource delayCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
				Task delay = Task.Delay(_timeout, delayCts.Token);
				Task<bool> next = moveNext.AsTask();
				if (await Task.WhenAny(next, delay).ConfigureAwait(continueOnCapturedContext: false) == delay)
				{
					_loserTask = next.ContinueWith((Task<bool> _, object obj) => ((IAsyncDisposable)obj).DisposeAsync().AsTask(), _enumerator);
					_sourceCTS.Cancel();
					throw new TimeoutException();
				}
				delayCts.Cancel();
			}
			if (await moveNext.ConfigureAwait(continueOnCapturedContext: false))
			{
				_current = _enumerator.Current;
				return true;
			}
			goto IL_0272;
			IL_0272:
			await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
		}
	}

	private sealed class ObservableAsyncEnumerable<TSource> : AsyncIterator<TSource>, IObserver<TSource>
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

		public override AsyncIteratorBase<TSource> Clone()
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
			AsyncIteratorState state = _state;
			if (state != AsyncIteratorState.Allocated)
			{
				if (state != AsyncIteratorState.Iterating)
				{
					await DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
					return false;
				}
			}
			else
			{
				_subscription = _source.Subscribe(this);
				_ctr = _cancellationToken.Register(OnCanceled, null);
				_state = AsyncIteratorState.Iterating;
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
				if (taskCompletionSource == TaskExt.True)
				{
					break;
				}
				if (taskCompletionSource != null)
				{
					taskCompletionSource.TrySetResult(result: true);
					break;
				}
			}
			while (Interlocked.CompareExchange(ref _signal, TaskExt.True, null) != null);
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

	private sealed class TaskToAsyncEnumerable<T> : AsyncIterator<T>
	{
		private readonly Task<T> _task;

		public TaskToAsyncEnumerable(Task<T> task)
		{
			_task = task;
		}

		public override AsyncIteratorBase<T> Clone()
		{
			return new TaskToAsyncEnumerable<T>(_task);
		}

		protected override async ValueTask<bool> MoveNextCore()
		{
			if (_state == AsyncIteratorState.Allocated)
			{
				_state = AsyncIteratorState.Iterating;
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
			CancellationTokenDisposable ctd = new CancellationTokenDisposable();
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

	public static IAsyncEnumerable<T> Create<T>(Func<CancellationToken, IAsyncEnumerator<T>> getAsyncEnumerator)
	{
		if (getAsyncEnumerator == null)
		{
			throw Error.ArgumentNull("getAsyncEnumerator");
		}
		return new AnonymousAsyncEnumerable<T>(getAsyncEnumerator);
	}

	public static IAsyncEnumerable<TSource> Amb<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second)
	{
		if (first == null)
		{
			throw Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw Error.ArgumentNull("second");
		}
		return Core(first, second);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> asyncEnumerable, IAsyncEnumerable<TSource> asyncEnumerable2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			IAsyncEnumerator<TSource> firstEnumerator = null;
			IAsyncEnumerator<TSource> secondEnumerator = null;
			Task<bool> firstMoveNext = null;
			Task<bool> secondMoveNext = null;
			CancellationTokenSource firstCancelToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			CancellationTokenSource secondCancelToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			try
			{
				firstEnumerator = asyncEnumerable.GetAsyncEnumerator(firstCancelToken.Token);
				firstMoveNext = firstEnumerator.MoveNextAsync().AsTask();
				secondEnumerator = asyncEnumerable2.GetAsyncEnumerator(secondCancelToken.Token);
				secondMoveNext = secondEnumerator.MoveNextAsync().AsTask();
			}
			catch
			{
				secondCancelToken.Cancel();
				firstCancelToken.Cancel();
				await Task.WhenAll(AwaitMoveNextAsyncAndDispose(secondMoveNext, secondEnumerator), AwaitMoveNextAsyncAndDispose(firstMoveNext, firstEnumerator)).ConfigureAwait(continueOnCapturedContext: false);
				throw;
			}
			Task<bool> task = await Task.WhenAny<bool>(firstMoveNext, secondMoveNext).ConfigureAwait(continueOnCapturedContext: false);
			IAsyncEnumerator<TSource> winner;
			Task disposeLoser;
			if (task == firstMoveNext)
			{
				winner = firstEnumerator;
				secondCancelToken.Cancel();
				disposeLoser = AwaitMoveNextAsyncAndDispose(secondMoveNext, secondEnumerator);
			}
			else
			{
				winner = secondEnumerator;
				firstCancelToken.Cancel();
				disposeLoser = AwaitMoveNextAsyncAndDispose(firstMoveNext, firstEnumerator);
			}
			try
			{
				ConfiguredAsyncDisposable I_4 = winner.ConfigureAwait(continueOnCapturedContext: false);
				try
				{
					if (await task.ConfigureAwait(continueOnCapturedContext: false))
					{
						yield return winner.Current;
						while (await winner.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
						{
							yield return winner.Current;
						}
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable = I_4 as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
			}
			finally
			{
				await disposeLoser.ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	public static IAsyncEnumerable<TSource> Amb<TSource>(params IAsyncEnumerable<TSource>[] sources)
	{
		if (sources == null)
		{
			throw Error.ArgumentNull("sources");
		}
		return Core(sources);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource>[] array, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			int n = array.Length;
			IAsyncEnumerator<TSource>[] enumerators = new IAsyncEnumerator<TSource>[n];
			Task<bool>[] moveNexts = new Task<bool>[n];
			CancellationTokenSource[] individualTokenSources = new CancellationTokenSource[n];
			for (int i = 0; i < n; i++)
			{
				individualTokenSources[i] = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			}
			try
			{
				for (int j = 0; j < n; j++)
				{
					moveNexts[j] = (enumerators[j] = array[j].GetAsyncEnumerator(individualTokenSources[j].Token)).MoveNextAsync().AsTask();
				}
			}
			catch
			{
				Task[] array2 = new Task[n];
				for (int num = n - 1; num >= 0; num--)
				{
					individualTokenSources[num].Cancel();
					array2[num] = AwaitMoveNextAsyncAndDispose(moveNexts[num], enumerators[num]);
				}
				await Task.WhenAll(array2).ConfigureAwait(continueOnCapturedContext: false);
				throw;
			}
			Task<bool> task = await Task.WhenAny(moveNexts).ConfigureAwait(continueOnCapturedContext: false);
			int num2 = Array.IndexOf<Task<bool>>(moveNexts, task);
			IAsyncEnumerator<TSource> winner = enumerators[num2];
			List<Task> list = new List<Task>(n - 1);
			for (int num3 = n - 1; num3 >= 0; num3--)
			{
				if (num3 != num2)
				{
					individualTokenSources[num3].Cancel();
					Task item = AwaitMoveNextAsyncAndDispose(moveNexts[num3], enumerators[num3]);
					list.Add(item);
				}
			}
			Task cleanupLosers = Task.WhenAll(list);
			try
			{
				ConfiguredAsyncDisposable I_4 = winner.ConfigureAwait(continueOnCapturedContext: false);
				try
				{
					if (await task.ConfigureAwait(continueOnCapturedContext: false))
					{
						yield return winner.Current;
						while (await winner.MoveNextAsync().ConfigureAwait(continueOnCapturedContext: false))
						{
							yield return winner.Current;
						}
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable = I_4 as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
			}
			finally
			{
				await cleanupLosers.ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	public static IAsyncEnumerable<TSource> Amb<TSource>(this IEnumerable<IAsyncEnumerable<TSource>> sources)
	{
		if (sources == null)
		{
			throw Error.ArgumentNull("sources");
		}
		return Amb(sources.ToArray());
	}

	private static async Task AwaitMoveNextAsyncAndDispose<T>(Task<bool>? moveNextAsync, IAsyncEnumerator<T>? enumerator)
	{
		if (enumerator == null)
		{
			return;
		}
		ConfiguredAsyncDisposable I_0 = enumerator.ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			if (moveNextAsync != null)
			{
				try
				{
					await moveNextAsync.ConfigureAwait(continueOnCapturedContext: false);
				}
				catch (TaskCanceledException)
				{
				}
			}
		}
		finally
		{
			IAsyncDisposable asyncDisposable = I_0 as IAsyncDisposable;
			if (asyncDisposable != null)
			{
				await asyncDisposable.DisposeAsync();
			}
		}
	}

	public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(this IAsyncEnumerable<TSource> source)
	{
		return source;
	}

	public static ValueTask<double> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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
						throw Error.NoElements();
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

	private static ValueTask<double> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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
						throw Error.NoElements();
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

	private static ValueTask<double> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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
						throw Error.NoElements();
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

	public static ValueTask<double> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, long> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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
						throw Error.NoElements();
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

	private static ValueTask<double> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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
						throw Error.NoElements();
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

	private static ValueTask<double> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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
						throw Error.NoElements();
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

	public static ValueTask<float> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, float> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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
					throw Error.NoElements();
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

	private static ValueTask<float> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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
					throw Error.NoElements();
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

	private static ValueTask<float> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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
					throw Error.NoElements();
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

	public static ValueTask<double> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, double> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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
					throw Error.NoElements();
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

	private static ValueTask<double> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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
					throw Error.NoElements();
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

	private static ValueTask<double> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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
					throw Error.NoElements();
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

	public static ValueTask<decimal> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, decimal> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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
					throw Error.NoElements();
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

	private static ValueTask<decimal> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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
					throw Error.NoElements();
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

	private static ValueTask<decimal> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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
					throw Error.NoElements();
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

	public static ValueTask<double?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	private static ValueTask<double?> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	private static ValueTask<double?> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<double?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, long?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	private static ValueTask<double?> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	private static ValueTask<double?> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<float?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, float?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	private static ValueTask<float?> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	private static ValueTask<float?> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<double?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, double?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	private static ValueTask<double?> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	private static ValueTask<double?> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<decimal?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, decimal?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	private static ValueTask<decimal?> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	private static ValueTask<decimal?> AverageAsyncCore<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static IAsyncEnumerable<IList<TSource>> Buffer<TSource>(this IAsyncEnumerable<TSource> source, int count)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (count <= 0)
		{
			throw Error.ArgumentOutOfRange("count");
		}
		return Core(source, count);
		static async IAsyncEnumerable<IList<TSource>> Core(IAsyncEnumerable<TSource> source2, int num, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			List<TSource> buffer = new List<TSource>(num);
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				buffer.Add(item);
				if (buffer.Count == num)
				{
					yield return buffer;
					buffer = new List<TSource>(num);
				}
			}
			if (buffer.Count > 0)
			{
				yield return buffer;
			}
		}
	}

	public static IAsyncEnumerable<IList<TSource>> Buffer<TSource>(this IAsyncEnumerable<TSource> source, int count, int skip)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (count <= 0)
		{
			throw Error.ArgumentOutOfRange("count");
		}
		if (skip <= 0)
		{
			throw Error.ArgumentOutOfRange("skip");
		}
		return Core(source, count, skip);
		static async IAsyncEnumerable<IList<TSource>> Core(IAsyncEnumerable<TSource> source2, int num2, int num, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			Queue<IList<TSource>> buffers = new Queue<IList<TSource>>();
			int index = 0;
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				if (index++ % num == 0)
				{
					buffers.Enqueue(new List<TSource>(num2));
				}
				foreach (IList<TSource> item2 in buffers)
				{
					item2.Add(item);
				}
				if (buffers.Count > 0 && buffers.Peek().Count == num2)
				{
					yield return buffers.Dequeue();
				}
			}
			while (buffers.Count > 0)
			{
				yield return buffers.Dequeue();
			}
		}
	}

	public static IAsyncEnumerable<TSource> Catch<TSource, TException>(this IAsyncEnumerable<TSource> source, Func<TException, IAsyncEnumerable<TSource>> handler) where TException : Exception
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (handler == null)
		{
			throw Error.ArgumentNull("handler");
		}
		return Core(source, handler);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TException, IAsyncEnumerable<TSource>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			IAsyncEnumerable<TSource> err = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				while (true)
				{
					TSource current;
					try
					{
						if (await e.MoveNextAsync())
						{
							current = e.Current;
							goto IL_0127;
						}
					}
					catch (TException arg)
					{
						err = func(arg);
					}
					break;
					IL_0127:
					yield return current;
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
			if (err != null)
			{
				await foreach (TSource item in err.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Catch<TSource, TException>(this IAsyncEnumerable<TSource> source, Func<TException, ValueTask<IAsyncEnumerable<TSource>>> handler) where TException : Exception
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (handler == null)
		{
			throw Error.ArgumentNull("handler");
		}
		return Core(source, handler);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TException, ValueTask<IAsyncEnumerable<TSource>>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			IAsyncEnumerable<TSource> err = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				TSource c = default(TSource);
				object obj = default(object);
				while (true)
				{
					int num = 0;
					try
					{
						if (await e.MoveNextAsync())
						{
							c = e.Current;
							goto IL_0142;
						}
					}
					catch (TException ex)
					{
						obj = ex;
						num = 1;
						goto IL_0142;
					}
					break;
					IL_0142:
					if (num == 1)
					{
						TException arg = (TException)obj;
						err = await func(arg).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					obj = null;
					yield return c;
					c = default(TSource);
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
			if (err != null)
			{
				await foreach (TSource item in err.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Catch<TSource, TException>(this IAsyncEnumerable<TSource> source, Func<TException, CancellationToken, ValueTask<IAsyncEnumerable<TSource>>> handler) where TException : Exception
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (handler == null)
		{
			throw Error.ArgumentNull("handler");
		}
		return Core(source, handler);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TException, CancellationToken, ValueTask<IAsyncEnumerable<TSource>>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			IAsyncEnumerable<TSource> err = null;
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				TSource c = default(TSource);
				object obj = default(object);
				while (true)
				{
					int num = 0;
					try
					{
						if (await e.MoveNextAsync())
						{
							c = e.Current;
							goto IL_0142;
						}
					}
					catch (TException ex)
					{
						obj = ex;
						num = 1;
						goto IL_0142;
					}
					break;
					IL_0142:
					if (num == 1)
					{
						TException arg = (TException)obj;
						err = await func(arg, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						break;
					}
					obj = null;
					yield return c;
					c = default(TSource);
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
			if (err != null)
			{
				await foreach (TSource item in err.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Catch<TSource>(this IEnumerable<IAsyncEnumerable<TSource>> sources)
	{
		if (sources == null)
		{
			throw Error.ArgumentNull("sources");
		}
		return CatchCore(sources);
	}

	public static IAsyncEnumerable<TSource> Catch<TSource>(params IAsyncEnumerable<TSource>[] sources)
	{
		if (sources == null)
		{
			throw Error.ArgumentNull("sources");
		}
		return CatchCore(sources);
	}

	public static IAsyncEnumerable<TSource> Catch<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second)
	{
		if (first == null)
		{
			throw Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw Error.ArgumentNull("second");
		}
		return CatchCore(new IAsyncEnumerable<TSource>[2] { first, second });
	}

	private static async IAsyncEnumerable<TSource> CatchCore<TSource>(IEnumerable<IAsyncEnumerable<TSource>> sources, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
	{
		ExceptionDispatchInfo error = null;
		foreach (IAsyncEnumerable<TSource> source2 in sources)
		{
			{
				ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = source2.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
				try
				{
					error = null;
					while (true)
					{
						TSource current2;
						try
						{
							if (!(await e.MoveNextAsync()))
							{
								break;
							}
							current2 = e.Current;
							goto IL_0146;
						}
						catch (Exception source)
						{
							error = ExceptionDispatchInfo.Capture(source);
						}
						break;
						IL_0146:
						yield return current2;
					}
					if (error != null)
					{
						continue;
					}
					break;
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
		error?.Throw();
	}

	public static IAsyncEnumerable<TSource> Concat<TSource>(this IAsyncEnumerable<IAsyncEnumerable<TSource>> sources)
	{
		if (sources == null)
		{
			throw Error.ArgumentNull("sources");
		}
		return Core(sources);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<IAsyncEnumerable<TSource>> source, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			await foreach (IAsyncEnumerable<TSource> item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				await foreach (TSource item2 in item.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item2;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Concat<TSource>(this IEnumerable<IAsyncEnumerable<TSource>> sources)
	{
		if (sources == null)
		{
			throw Error.ArgumentNull("sources");
		}
		return Core(sources);
		static async IAsyncEnumerable<TSource> Core(IEnumerable<IAsyncEnumerable<TSource>> enumerable, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			foreach (IAsyncEnumerable<TSource> item in enumerable)
			{
				await foreach (TSource item2 in item.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item2;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Concat<TSource>(params IAsyncEnumerable<TSource>[] sources)
	{
		if (sources == null)
		{
			throw Error.ArgumentNull("sources");
		}
		return Core(sources);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource>[] array, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			foreach (IAsyncEnumerable<TSource> source in array)
			{
				await foreach (TSource item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Defer<TSource>(Func<IAsyncEnumerable<TSource>> factory)
	{
		if (factory == null)
		{
			throw Error.ArgumentNull("factory");
		}
		return Core(factory);
		static async IAsyncEnumerable<TSource> Core(Func<IAsyncEnumerable<TSource>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			await foreach (TSource item in func().WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				yield return item;
			}
		}
	}

	public static IAsyncEnumerable<TSource> Defer<TSource>(Func<Task<IAsyncEnumerable<TSource>>> factory)
	{
		if (factory == null)
		{
			throw Error.ArgumentNull("factory");
		}
		return Core(factory);
		static async IAsyncEnumerable<TSource> Core(Func<Task<IAsyncEnumerable<TSource>>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			await foreach (TSource item in (await func().ConfigureAwait(continueOnCapturedContext: false)).WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				yield return item;
			}
		}
	}

	public static IAsyncEnumerable<TSource> Defer<TSource>(Func<CancellationToken, Task<IAsyncEnumerable<TSource>>> factory)
	{
		if (factory == null)
		{
			throw Error.ArgumentNull("factory");
		}
		return Core(factory);
		static async IAsyncEnumerable<TSource> Core(Func<CancellationToken, Task<IAsyncEnumerable<TSource>>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			await foreach (TSource item in (await func(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				yield return item;
			}
		}
	}

	[Obsolete("Use DistinctBy.  IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality of selector-based overloads of Distinct now exists as DistinctBy.")]
	public static IAsyncEnumerable<TSource> Distinct<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return DistinctCore(source, keySelector, null);
	}

	[Obsolete("Use DistinctBy.  IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality of selector-based overloads of Distinct now exists as DistinctBy.")]
	public static IAsyncEnumerable<TSource> Distinct<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return DistinctCore(source, keySelector, comparer);
	}

	[Obsolete("Use DistinctBy.  IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality of selector-based overloads of Distinct now exists as DistinctBy.")]
	public static IAsyncEnumerable<TSource> Distinct<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return DistinctCore(source, keySelector, (IEqualityComparer<TKey>?)null);
	}

	[Obsolete("Use DistinctBy.  IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality of selector-based overloads of Distinct now exists as DistinctBy.")]
	public static IAsyncEnumerable<TSource> Distinct<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return DistinctCore(source, keySelector, null);
	}

	[Obsolete("Use DistinctBy.  IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality of selector-based overloads of Distinct now exists as DistinctBy.")]
	public static IAsyncEnumerable<TSource> Distinct<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return DistinctCore(source, keySelector, comparer);
	}

	[Obsolete("Use DistinctBy.  IAsyncEnumerable LINQ is now in System.Linq.AsyncEnumerable, and the functionality of selector-based overloads of Distinct now exists as DistinctBy.")]
	public static IAsyncEnumerable<TSource> Distinct<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return DistinctCore(source, keySelector, comparer);
	}

	private static IAsyncEnumerable<TSource> DistinctCore<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
	{
		return new DistinctAsyncIterator<TSource, TKey>(source, keySelector, comparer);
	}

	private static IAsyncEnumerable<TSource> DistinctCore<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer)
	{
		return new DistinctAsyncIteratorWithTask<TSource, TKey>(source, keySelector, comparer);
	}

	private static IAsyncEnumerable<TSource> DistinctCore<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer)
	{
		return new DistinctAsyncIteratorWithTaskAndCancellation<TSource, TKey>(source, keySelector, comparer);
	}

	public static IAsyncEnumerable<TSource> DistinctUntilChanged<TSource>(this IAsyncEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return DistinctUntilChangedCore(source, null);
	}

	public static IAsyncEnumerable<TSource> DistinctUntilChanged<TSource>(this IAsyncEnumerable<TSource> source, IEqualityComparer<TSource>? comparer)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return DistinctUntilChangedCore(source, comparer);
	}

	public static IAsyncEnumerable<TSource> DistinctUntilChanged<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return DistinctUntilChangedCore(source, keySelector, null);
	}

	public static IAsyncEnumerable<TSource> DistinctUntilChanged<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return DistinctUntilChangedCore(source, keySelector, comparer);
	}

	public static IAsyncEnumerable<TSource> DistinctUntilChanged<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return DistinctUntilChangedCore(source, keySelector, (IEqualityComparer<TKey>?)null);
	}

	public static IAsyncEnumerable<TSource> DistinctUntilChanged<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return DistinctUntilChangedCore(source, keySelector, null);
	}

	public static IAsyncEnumerable<TSource> DistinctUntilChanged<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return DistinctUntilChangedCore(source, keySelector, comparer);
	}

	public static IAsyncEnumerable<TSource> DistinctUntilChanged<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return DistinctUntilChangedCore(source, keySelector, comparer);
	}

	private static IAsyncEnumerable<TSource> DistinctUntilChangedCore<TSource>(IAsyncEnumerable<TSource> source, IEqualityComparer<TSource>? comparer)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<TSource>.Default;
		}
		return Core(source, comparer);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> enumerable, IEqualityComparer<TSource> equalityComparer, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					TSource latest = e.Current;
					yield return latest;
					while (await e.MoveNextAsync())
					{
						TSource current = e.Current;
						if (!equalityComparer.Equals(latest, current))
						{
							latest = current;
							yield return latest;
						}
					}
					yield break;
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

	private static IAsyncEnumerable<TSource> DistinctUntilChangedCore<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<TKey>.Default;
		}
		return Core(source, keySelector, comparer);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, TKey> func, IEqualityComparer<TKey>? equalityComparer, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					TSource current = e.Current;
					TKey latestKey = func(current);
					yield return current;
					while (await e.MoveNextAsync())
					{
						current = e.Current;
						TKey val = func(current);
						if (!equalityComparer.Equals(latestKey, val))
						{
							latestKey = val;
							yield return current;
						}
					}
					yield break;
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

	private static IAsyncEnumerable<TSource> DistinctUntilChangedCore<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<TKey>.Default;
		}
		return Core(source, keySelector, comparer);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, ValueTask<TKey>> func, IEqualityComparer<TKey>? equalityComparer, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					TSource item = e.Current;
					TKey latestKey = await func(item).ConfigureAwait(continueOnCapturedContext: false);
					yield return item;
					while (await e.MoveNextAsync())
					{
						item = e.Current;
						TKey val = await func(item).ConfigureAwait(continueOnCapturedContext: false);
						if (!equalityComparer.Equals(latestKey, val))
						{
							latestKey = val;
							yield return item;
						}
					}
					yield break;
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

	private static IAsyncEnumerable<TSource> DistinctUntilChangedCore<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<TKey>.Default;
		}
		return Core(source, keySelector, comparer);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask<TKey>> func, IEqualityComparer<TKey>? equalityComparer, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					TSource item = e.Current;
					TKey latestKey = await func(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					yield return item;
					while (await e.MoveNextAsync())
					{
						item = e.Current;
						TKey val = await func(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						if (!equalityComparer.Equals(latestKey, val))
						{
							latestKey = val;
							yield return item;
						}
					}
					yield break;
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

	public static IAsyncEnumerable<TSource> Do<TSource>(this IAsyncEnumerable<TSource> source, Action<TSource> onNext)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw Error.ArgumentNull("onNext");
		}
		return DoCore(source, onNext, null, null);
	}

	public static IAsyncEnumerable<TSource> Do<TSource>(this IAsyncEnumerable<TSource> source, Action<TSource> onNext, Action onCompleted)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw Error.ArgumentNull("onNext");
		}
		if (onCompleted == null)
		{
			throw Error.ArgumentNull("onCompleted");
		}
		return DoCore(source, onNext, null, onCompleted);
	}

	public static IAsyncEnumerable<TSource> Do<TSource>(this IAsyncEnumerable<TSource> source, Action<TSource> onNext, Action<Exception> onError)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw Error.ArgumentNull("onNext");
		}
		if (onError == null)
		{
			throw Error.ArgumentNull("onError");
		}
		return DoCore(source, onNext, onError, null);
	}

	public static IAsyncEnumerable<TSource> Do<TSource>(this IAsyncEnumerable<TSource> source, Action<TSource> onNext, Action<Exception> onError, Action onCompleted)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw Error.ArgumentNull("onNext");
		}
		if (onError == null)
		{
			throw Error.ArgumentNull("onError");
		}
		if (onCompleted == null)
		{
			throw Error.ArgumentNull("onCompleted");
		}
		return DoCore(source, onNext, onError, onCompleted);
	}

	public static IAsyncEnumerable<TSource> Do<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, Task> onNext)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw Error.ArgumentNull("onNext");
		}
		return DoCore(source, onNext, null, null);
	}

	public static IAsyncEnumerable<TSource> Do<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, Task> onNext, Func<Task> onCompleted)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw Error.ArgumentNull("onNext");
		}
		if (onCompleted == null)
		{
			throw Error.ArgumentNull("onCompleted");
		}
		return DoCore(source, onNext, null, onCompleted);
	}

	public static IAsyncEnumerable<TSource> Do<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, Task> onNext, Func<Exception, Task> onError)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw Error.ArgumentNull("onNext");
		}
		if (onError == null)
		{
			throw Error.ArgumentNull("onError");
		}
		return DoCore(source, onNext, onError, null);
	}

	public static IAsyncEnumerable<TSource> Do<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, Task> onNext, Func<Exception, Task> onError, Func<Task> onCompleted)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw Error.ArgumentNull("onNext");
		}
		if (onError == null)
		{
			throw Error.ArgumentNull("onError");
		}
		if (onCompleted == null)
		{
			throw Error.ArgumentNull("onCompleted");
		}
		return DoCore(source, onNext, onError, onCompleted);
	}

	public static IAsyncEnumerable<TSource> Do<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task> onNext)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw Error.ArgumentNull("onNext");
		}
		return DoCore(source, onNext, null, null);
	}

	public static IAsyncEnumerable<TSource> Do<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task> onNext, Func<CancellationToken, Task> onCompleted)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw Error.ArgumentNull("onNext");
		}
		if (onCompleted == null)
		{
			throw Error.ArgumentNull("onCompleted");
		}
		return DoCore(source, onNext, null, onCompleted);
	}

	public static IAsyncEnumerable<TSource> Do<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task> onNext, Func<Exception, CancellationToken, Task> onError)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw Error.ArgumentNull("onNext");
		}
		if (onError == null)
		{
			throw Error.ArgumentNull("onError");
		}
		return DoCore(source, onNext, onError, null);
	}

	public static IAsyncEnumerable<TSource> Do<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task> onNext, Func<Exception, CancellationToken, Task> onError, Func<CancellationToken, Task> onCompleted)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (onNext == null)
		{
			throw Error.ArgumentNull("onNext");
		}
		if (onError == null)
		{
			throw Error.ArgumentNull("onError");
		}
		if (onCompleted == null)
		{
			throw Error.ArgumentNull("onCompleted");
		}
		return DoCore(source, onNext, onError, onCompleted);
	}

	public static IAsyncEnumerable<TSource> Do<TSource>(this IAsyncEnumerable<TSource> source, IObserver<TSource> observer)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (observer == null)
		{
			throw Error.ArgumentNull("observer");
		}
		return DoCore(source, (Action<TSource>)observer.OnNext, (Action<Exception>?)observer.OnError, (Action?)observer.OnCompleted, default(CancellationToken));
	}

	private static async IAsyncEnumerable<TSource> DoCore<TSource>(IAsyncEnumerable<TSource> source, Action<TSource> onNext, Action<Exception>? onError, Action? onCompleted, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
	{
		ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = source.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
		try
		{
			while (true)
			{
				TSource current;
				try
				{
					if (!(await e.MoveNextAsync()))
					{
						break;
					}
					current = e.Current;
					onNext(current);
					goto IL_0133;
				}
				catch (OperationCanceledException)
				{
					throw;
				}
				catch (Exception obj) when (onError != null)
				{
					onError(obj);
					throw;
				}
				IL_0133:
				yield return current;
			}
			onCompleted?.Invoke();
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

	private static async IAsyncEnumerable<TSource> DoCore<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, Task> onNext, Func<Exception, Task>? onError, Func<Task>? onCompleted, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
	{
		ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = source.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
		try
		{
			while (true)
			{
				TSource item;
				try
				{
					if (!(await e.MoveNextAsync()))
					{
						break;
					}
					item = e.Current;
					await onNext(item).ConfigureAwait(continueOnCapturedContext: false);
					goto IL_02ad;
				}
				catch (OperationCanceledException)
				{
					throw;
				}
				catch (Exception arg) when (onError != null)
				{
					await onError(arg).ConfigureAwait(continueOnCapturedContext: false);
					throw;
				}
				IL_02ad:
				yield return item;
			}
			if (onCompleted != null)
			{
				await onCompleted().ConfigureAwait(continueOnCapturedContext: false);
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

	private static async IAsyncEnumerable<TSource> DoCore<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task> onNext, Func<Exception, CancellationToken, Task>? onError, Func<CancellationToken, Task>? onCompleted, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
	{
		ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = source.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
		try
		{
			while (true)
			{
				TSource item;
				try
				{
					if (!(await e.MoveNextAsync()))
					{
						break;
					}
					item = e.Current;
					await onNext(item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					goto IL_02b9;
				}
				catch (OperationCanceledException)
				{
					throw;
				}
				catch (Exception arg) when (onError != null)
				{
					await onError(arg, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					throw;
				}
				IL_02b9:
				yield return item;
			}
			if (onCompleted != null)
			{
				await onCompleted(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
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

	public static IAsyncEnumerable<TSource> Expand<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, IAsyncEnumerable<TSource>> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		return Core(source, selector);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> item, Func<TSource, IAsyncEnumerable<TSource>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			Queue<IAsyncEnumerable<TSource>> queue = new Queue<IAsyncEnumerable<TSource>>();
			queue.Enqueue(item);
			while (queue.Count > 0)
			{
				await foreach (TSource item in queue.Dequeue().WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					queue.Enqueue(func(item));
					yield return item;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Expand<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<IAsyncEnumerable<TSource>>> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		return Core(source, selector);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> item, Func<TSource, ValueTask<IAsyncEnumerable<TSource>>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			Queue<IAsyncEnumerable<TSource>> queue = new Queue<IAsyncEnumerable<TSource>>();
			queue.Enqueue(item);
			while (queue.Count > 0)
			{
				await foreach (TSource item2 in queue.Dequeue().WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					Queue<IAsyncEnumerable<TSource>> queue2 = queue;
					queue2.Enqueue(await func(item2).ConfigureAwait(continueOnCapturedContext: false));
					yield return item2;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Expand<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<IAsyncEnumerable<TSource>>> selector)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
		}
		return Core(source, selector);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> item, Func<TSource, CancellationToken, ValueTask<IAsyncEnumerable<TSource>>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			Queue<IAsyncEnumerable<TSource>> queue = new Queue<IAsyncEnumerable<TSource>>();
			queue.Enqueue(item);
			while (queue.Count > 0)
			{
				await foreach (TSource item2 in queue.Dequeue().WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					Queue<IAsyncEnumerable<TSource>> queue2 = queue;
					queue2.Enqueue(await func(item2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
					yield return item2;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Finally<TSource>(this IAsyncEnumerable<TSource> source, Action finallyAction)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (finallyAction == null)
		{
			throw Error.ArgumentNull("finallyAction");
		}
		return Core(source, finallyAction);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> source2, Action action, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item;
				}
			}
			finally
			{
				action();
			}
		}
	}

	public static IAsyncEnumerable<TSource> Finally<TSource>(this IAsyncEnumerable<TSource> source, Func<Task> finallyAction)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (finallyAction == null)
		{
			throw Error.ArgumentNull("finallyAction");
		}
		return Core(source, finallyAction);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> source2, Func<Task> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item;
				}
			}
			finally
			{
				await func().ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	public static IAsyncEnumerable<TResult> Generate<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector)
	{
		if (condition == null)
		{
			throw Error.ArgumentNull("condition");
		}
		if (iterate == null)
		{
			throw Error.ArgumentNull("iterate");
		}
		if (resultSelector == null)
		{
			throw Error.ArgumentNull("resultSelector");
		}
		return Core(initialState, condition, iterate, resultSelector);
		static async IAsyncEnumerable<TResult> Core(TState val, Func<TState, bool> func, Func<TState, TState> func3, Func<TState, TResult> func2)
		{
			TState state = val;
			while (func(state))
			{
				yield return func2(state);
				state = func3(state);
			}
		}
	}

	public static IAsyncEnumerable<TSource> IgnoreElements<TSource>(this IAsyncEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return Core(source);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> source2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				_ = item;
			}
			yield break;
		}
	}

	public static ValueTask<bool> IsEmptyAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return Core(source, cancellationToken);
		static async ValueTask<bool> Core(IAsyncEnumerable<TSource> enumerable, CancellationToken cancellationToken2)
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			bool result;
			try
			{
				result = !(await e.MoveNextAsync());
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

	public static ValueTask<TSource> MaxAsync<TSource>(this IAsyncEnumerable<TSource> source, IComparer<TSource>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return Core(source, comparer, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> enumerable, IComparer<TSource>? comparer2, CancellationToken cancellationToken2)
		{
			if (comparer2 == null)
			{
				comparer2 = Comparer<TSource>.Default;
			}
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TSource result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw Error.NoElements();
				}
				TSource max = e.Current;
				while (await e.MoveNextAsync())
				{
					TSource current = e.Current;
					if (comparer2.Compare(current, max) > 0)
					{
						max = current;
					}
				}
				result = max;
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

	public static ValueTask<IList<TSource>> MaxByAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MaxByCore(source, keySelector, null, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MaxByAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MaxByCore(source, keySelector, comparer, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MaxByAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MaxByCore(source, keySelector, (IComparer<TKey>?)null, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MaxByAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MaxByCore(source, keySelector, null, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MaxByAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MaxByCore(source, keySelector, comparer, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MaxByAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MaxByCore(source, keySelector, comparer, cancellationToken);
	}

	private static ValueTask<IList<TSource>> MaxByCore<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken)
	{
		if (comparer == null)
		{
			comparer = Comparer<TKey>.Default;
		}
		return ExtremaBy(source, keySelector, (TKey key, TKey minValue) => comparer.Compare(key, minValue), cancellationToken);
	}

	private static ValueTask<IList<TSource>> MaxByCore<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken)
	{
		if (comparer == null)
		{
			comparer = Comparer<TKey>.Default;
		}
		return ExtremaBy(source, keySelector, (TKey key, TKey minValue) => comparer.Compare(key, minValue), cancellationToken);
	}

	private static ValueTask<IList<TSource>> MaxByCore<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken)
	{
		if (comparer == null)
		{
			comparer = Comparer<TKey>.Default;
		}
		return ExtremaBy(source, keySelector, (TKey key, TKey minValue) => comparer.Compare(key, minValue), cancellationToken);
	}

	public static ValueTask<IList<TSource>> MaxByWithTiesAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MaxByWithTiesCore(source, keySelector, null, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MaxByWithTiesAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MaxByWithTiesCore(source, keySelector, comparer, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MaxByWithTiesAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MaxByWithTiesCore(source, keySelector, (IComparer<TKey>?)null, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MaxByWithTiesAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MaxByWithTiesCore(source, keySelector, null, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MaxByWithTiesAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MaxByWithTiesCore(source, keySelector, comparer, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MaxByWithTiesAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MaxByWithTiesCore(source, keySelector, comparer, cancellationToken);
	}

	private static ValueTask<IList<TSource>> MaxByWithTiesCore<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken)
	{
		if (comparer == null)
		{
			comparer = Comparer<TKey>.Default;
		}
		return ExtremaBy(source, keySelector, (TKey key, TKey minValue) => comparer.Compare(key, minValue), cancellationToken);
	}

	private static ValueTask<IList<TSource>> MaxByWithTiesCore<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken)
	{
		if (comparer == null)
		{
			comparer = Comparer<TKey>.Default;
		}
		return ExtremaBy(source, keySelector, (TKey key, TKey minValue) => comparer.Compare(key, minValue), cancellationToken);
	}

	private static ValueTask<IList<TSource>> MaxByWithTiesCore<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken)
	{
		if (comparer == null)
		{
			comparer = Comparer<TKey>.Default;
		}
		return ExtremaBy(source, keySelector, (TKey key, TKey minValue) => comparer.Compare(key, minValue), cancellationToken);
	}

	public static IAsyncEnumerable<TSource> Merge<TSource>(params IAsyncEnumerable<TSource>[] sources)
	{
		if (sources == null)
		{
			throw Error.ArgumentNull("sources");
		}
		return Core(sources);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource>[] array, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			int count = array.Length;
			IAsyncEnumerator<TSource>?[] enumerators = new IAsyncEnumerator<TSource>[count];
			Task<bool>[] moveNextTasks = new Task<bool>[count];
			try
			{
				for (int i = 0; i < count; i++)
				{
					moveNextTasks[i] = (enumerators[i] = array[i].GetAsyncEnumerator(cancellationToken)).MoveNextAsync().AsTask();
				}
				int active = count;
				while (active > 0)
				{
					Task<bool> task = await Task.WhenAny(moveNextTasks).ConfigureAwait(continueOnCapturedContext: false);
					int index = Array.IndexOf<Task<bool>>(moveNextTasks, task);
					IAsyncEnumerator<TSource> enumerator = enumerators[index];
					if (!(await task.ConfigureAwait(continueOnCapturedContext: false)))
					{
						moveNextTasks[index] = TaskExt.Never;
						enumerators[index] = null;
						await enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
						active--;
					}
					else
					{
						TSource current = enumerator.Current;
						moveNextTasks[index] = enumerator.MoveNextAsync().AsTask();
						yield return current;
					}
				}
			}
			finally
			{
				List<Exception> errors = null;
				for (int active = count - 1; active >= 0; active--)
				{
					Task<bool> task2 = moveNextTasks[active];
					IAsyncEnumerator<TSource> enumerator = enumerators[active];
					try
					{
						try
						{
							if (task2 != null && task2 != TaskExt.Never)
							{
								await task2.ConfigureAwait(continueOnCapturedContext: false);
							}
						}
						finally
						{
							if (enumerator != null)
							{
								await enumerator.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
							}
						}
					}
					catch (Exception item)
					{
						List<Exception> list = errors;
						if (list == null)
						{
							List<Exception> list2;
							errors = (list2 = new List<Exception>());
							list = list2;
						}
						list.Add(item);
					}
				}
				if (errors != null)
				{
					throw new AggregateException(errors);
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Merge<TSource>(this IEnumerable<IAsyncEnumerable<TSource>> sources)
	{
		if (sources == null)
		{
			throw Error.ArgumentNull("sources");
		}
		return sources.ToAsyncEnumerable().SelectMany((IAsyncEnumerable<TSource> source) => source);
	}

	public static IAsyncEnumerable<TSource> Merge<TSource>(this IAsyncEnumerable<IAsyncEnumerable<TSource>> sources)
	{
		if (sources == null)
		{
			throw Error.ArgumentNull("sources");
		}
		return sources.SelectMany((IAsyncEnumerable<TSource> source) => source);
	}

	public static ValueTask<TSource> MinAsync<TSource>(this IAsyncEnumerable<TSource> source, IComparer<TSource>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return Core(source, comparer, cancellationToken);
		static async ValueTask<TSource> Core(IAsyncEnumerable<TSource> enumerable, IComparer<TSource>? comparer2, CancellationToken cancellationToken2)
		{
			if (comparer2 == null)
			{
				comparer2 = Comparer<TSource>.Default;
			}
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken2, continueOnCapturedContext: false);
			TSource result;
			try
			{
				if (!(await e.MoveNextAsync()))
				{
					throw Error.NoElements();
				}
				TSource min = e.Current;
				while (await e.MoveNextAsync())
				{
					TSource current = e.Current;
					if (comparer2.Compare(current, min) < 0)
					{
						min = current;
					}
				}
				result = min;
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

	public static ValueTask<IList<TSource>> MinByAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MinByCore(source, keySelector, null, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MinByAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MinByCore(source, keySelector, comparer, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MinByAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MinByCore(source, keySelector, (IComparer<TKey>?)null, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MinByAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MinByCore(source, keySelector, null, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MinByAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MinByCore(source, keySelector, comparer, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MinByAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey> comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MinByCore(source, keySelector, comparer, cancellationToken);
	}

	private static ValueTask<IList<TSource>> MinByCore<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken)
	{
		if (comparer == null)
		{
			comparer = Comparer<TKey>.Default;
		}
		return ExtremaBy(source, keySelector, (TKey key, TKey minValue) => -comparer.Compare(key, minValue), cancellationToken);
	}

	private static ValueTask<IList<TSource>> MinByCore<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken)
	{
		if (comparer == null)
		{
			comparer = Comparer<TKey>.Default;
		}
		return ExtremaBy(source, keySelector, (TKey key, TKey minValue) => -comparer.Compare(key, minValue), cancellationToken);
	}

	private static ValueTask<IList<TSource>> MinByCore<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken)
	{
		if (comparer == null)
		{
			comparer = Comparer<TKey>.Default;
		}
		return ExtremaBy(source, keySelector, (TKey key, TKey minValue) => -comparer.Compare(key, minValue), cancellationToken);
	}

	public static ValueTask<IList<TSource>> MinByWithTiesAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MinByWithTiesCore(source, keySelector, null, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MinByWithTiesAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MinByWithTiesCore(source, keySelector, comparer, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MinByWithTiesAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MinByWithTiesCore(source, keySelector, (IComparer<TKey>?)null, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MinByWithTiesAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MinByWithTiesCore(source, keySelector, null, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MinByWithTiesAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MinByWithTiesCore(source, keySelector, comparer, cancellationToken);
	}

	public static ValueTask<IList<TSource>> MinByWithTiesAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey> comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (keySelector == null)
		{
			throw Error.ArgumentNull("keySelector");
		}
		return MinByWithTiesCore(source, keySelector, comparer, cancellationToken);
	}

	private static ValueTask<IList<TSource>> MinByWithTiesCore<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken)
	{
		if (comparer == null)
		{
			comparer = Comparer<TKey>.Default;
		}
		return ExtremaBy(source, keySelector, (TKey key, TKey minValue) => -comparer.Compare(key, minValue), cancellationToken);
	}

	private static ValueTask<IList<TSource>> MinByWithTiesCore<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken)
	{
		if (comparer == null)
		{
			comparer = Comparer<TKey>.Default;
		}
		return ExtremaBy(source, keySelector, (TKey key, TKey minValue) => -comparer.Compare(key, minValue), cancellationToken);
	}

	private static ValueTask<IList<TSource>> MinByWithTiesCore<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey>? comparer, CancellationToken cancellationToken)
	{
		if (comparer == null)
		{
			comparer = Comparer<TKey>.Default;
		}
		return ExtremaBy(source, keySelector, (TKey key, TKey minValue) => -comparer.Compare(key, minValue), cancellationToken);
	}

	private static async ValueTask<IList<TSource>> ExtremaBy<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TKey, int> compare, CancellationToken cancellationToken)
	{
		List<TSource> result = new List<TSource>();
		ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = source.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
		try
		{
			if (!(await e.MoveNextAsync()))
			{
				throw Error.NoElements();
			}
			TSource current = e.Current;
			TKey resKey = keySelector(current);
			result.Add(current);
			while (await e.MoveNextAsync())
			{
				TSource current2 = e.Current;
				TKey val = keySelector(current2);
				int num = compare(val, resKey);
				if (num == 0)
				{
					result.Add(current2);
				}
				else if (num > 0)
				{
					result = new List<TSource>(1) { current2 };
					resKey = val;
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
		return result;
	}

	private static async ValueTask<IList<TSource>> ExtremaBy<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TKey, TKey, int> compare, CancellationToken cancellationToken)
	{
		List<TSource> result = new List<TSource>();
		ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = source.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
		try
		{
			if (!(await e.MoveNextAsync()))
			{
				throw Error.NoElements();
			}
			TSource current = e.Current;
			TKey resKey = await keySelector(current).ConfigureAwait(continueOnCapturedContext: false);
			result.Add(current);
			while (await e.MoveNextAsync())
			{
				TSource cur = e.Current;
				TKey val = await keySelector(cur).ConfigureAwait(continueOnCapturedContext: false);
				int num = compare(val, resKey);
				if (num == 0)
				{
					result.Add(cur);
				}
				else if (num > 0)
				{
					result = new List<TSource>(1) { cur };
					resKey = val;
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
		return result;
	}

	private static async ValueTask<IList<TSource>> ExtremaBy<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TKey, TKey, int> compare, CancellationToken cancellationToken)
	{
		List<TSource> result = new List<TSource>();
		ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = source.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
		try
		{
			if (!(await e.MoveNextAsync()))
			{
				throw Error.NoElements();
			}
			TSource current = e.Current;
			TKey resKey = await keySelector(current, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			result.Add(current);
			while (await e.MoveNextAsync())
			{
				TSource cur = e.Current;
				TKey val = await keySelector(cur, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				int num = compare(val, resKey);
				if (num == 0)
				{
					result.Add(cur);
				}
				else if (num > 0)
				{
					result = new List<TSource>(1) { cur };
					resKey = val;
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
		return result;
	}

	public static IAsyncEnumerable<TValue> Never<TValue>()
	{
		return NeverAsyncEnumerable<TValue>.Instance;
	}

	public static IAsyncEnumerable<TSource> OnErrorResumeNext<TSource>(this IAsyncEnumerable<TSource> first, IAsyncEnumerable<TSource> second)
	{
		if (first == null)
		{
			throw Error.ArgumentNull("first");
		}
		if (second == null)
		{
			throw Error.ArgumentNull("second");
		}
		return OnErrorResumeNextCore(new IAsyncEnumerable<TSource>[2] { first, second });
	}

	public static IAsyncEnumerable<TSource> OnErrorResumeNext<TSource>(params IAsyncEnumerable<TSource>[] sources)
	{
		if (sources == null)
		{
			throw Error.ArgumentNull("sources");
		}
		return OnErrorResumeNextCore(sources);
	}

	public static IAsyncEnumerable<TSource> OnErrorResumeNext<TSource>(this IEnumerable<IAsyncEnumerable<TSource>> sources)
	{
		if (sources == null)
		{
			throw Error.ArgumentNull("sources");
		}
		return OnErrorResumeNextCore(sources);
	}

	private static IAsyncEnumerable<TSource> OnErrorResumeNextCore<TSource>(IEnumerable<IAsyncEnumerable<TSource>> sources)
	{
		return new OnErrorResumeNextAsyncIterator<TSource>(sources);
	}

	public static IAsyncEnumerable<TResult> Repeat<TResult>(TResult element)
	{
		return Core(element);
		static async IAsyncEnumerable<TResult> Core(TResult val, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			while (true)
			{
				cancellationToken.ThrowIfCancellationRequested();
				yield return val;
			}
		}
	}

	public static IAsyncEnumerable<TSource> Repeat<TSource>(this IAsyncEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return Core(source);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> source2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			while (true)
			{
				await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Repeat<TSource>(this IAsyncEnumerable<TSource> source, int count)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (count < 0)
		{
			throw Error.ArgumentOutOfRange("count");
		}
		return Core(source, count);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> source2, int num, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			for (int i = 0; i < num; i++)
			{
				await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					yield return item;
				}
			}
		}
	}

	public static IAsyncEnumerable<TSource> Retry<TSource>(this IAsyncEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return ((IEnumerable<IAsyncEnumerable<TSource>>)new IAsyncEnumerable<TSource>[1] { source }).Repeat().Catch();
	}

	public static IAsyncEnumerable<TSource> Retry<TSource>(this IAsyncEnumerable<TSource> source, int retryCount)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (retryCount < 0)
		{
			throw Error.ArgumentOutOfRange("retryCount");
		}
		return new IAsyncEnumerable<TSource>[1] { source }.Repeat(retryCount).Catch();
	}

	private static IEnumerable<TSource> Repeat<TSource>(this IEnumerable<TSource> source)
	{
		while (true)
		{
			foreach (TSource item in source)
			{
				yield return item;
			}
		}
	}

	private static IEnumerable<TSource> Repeat<TSource>(this IEnumerable<TSource> source, int count)
	{
		for (int i = 0; i < count; i++)
		{
			foreach (TSource item in source)
			{
				yield return item;
			}
		}
	}

	public static IAsyncEnumerable<TValue> Return<TValue>(TValue value)
	{
		return new ReturnEnumerable<TValue>(value);
	}

	public static IAsyncEnumerable<TSource> Scan<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, TSource, TSource> accumulator)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw Error.ArgumentNull("accumulator");
		}
		return Core(source, accumulator);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, TSource, TSource> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					TSource res = e.Current;
					while (await e.MoveNextAsync())
					{
						res = func(res, e.Current);
						yield return res;
					}
					yield break;
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

	public static IAsyncEnumerable<TAccumulate> Scan<TSource, TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw Error.ArgumentNull("accumulator");
		}
		return Core(source, seed, accumulator);
		static async IAsyncEnumerable<TAccumulate> Core(IAsyncEnumerable<TSource> source2, TAccumulate val, Func<TAccumulate, TSource, TAccumulate> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			TAccumulate res = val;
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				res = func(res, item);
				yield return res;
			}
		}
	}

	public static IAsyncEnumerable<TSource> Scan<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, TSource, ValueTask<TSource>> accumulator)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw Error.ArgumentNull("accumulator");
		}
		return Core(source, accumulator);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, TSource, ValueTask<TSource>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					TSource res = e.Current;
					while (await e.MoveNextAsync())
					{
						res = await func(res, e.Current).ConfigureAwait(continueOnCapturedContext: false);
						yield return res;
					}
					yield break;
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

	public static IAsyncEnumerable<TSource> Scan<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, TSource, CancellationToken, ValueTask<TSource>> accumulator)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw Error.ArgumentNull("accumulator");
		}
		return Core(source, accumulator);
		static async IAsyncEnumerable<TSource> Core(IAsyncEnumerable<TSource> enumerable, Func<TSource, TSource, CancellationToken, ValueTask<TSource>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			ConfiguredCancelableAsyncEnumerable<TSource>.Enumerator e = enumerable.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
			try
			{
				if (await e.MoveNextAsync())
				{
					TSource res = e.Current;
					while (await e.MoveNextAsync())
					{
						res = await func(res, e.Current, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						yield return res;
					}
					yield break;
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

	public static IAsyncEnumerable<TAccumulate> Scan<TSource, TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, ValueTask<TAccumulate>> accumulator)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw Error.ArgumentNull("accumulator");
		}
		return Core(source, seed, accumulator);
		static async IAsyncEnumerable<TAccumulate> Core(IAsyncEnumerable<TSource> source2, TAccumulate val, Func<TAccumulate, TSource, ValueTask<TAccumulate>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			TAccumulate res = val;
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				res = await func(res, item).ConfigureAwait(continueOnCapturedContext: false);
				yield return res;
			}
		}
	}

	public static IAsyncEnumerable<TAccumulate> Scan<TSource, TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> accumulator)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (accumulator == null)
		{
			throw Error.ArgumentNull("accumulator");
		}
		return Core(source, seed, accumulator);
		static async IAsyncEnumerable<TAccumulate> Core(IAsyncEnumerable<TSource> source2, TAccumulate val, Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> func, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			TAccumulate res = val;
			await foreach (TSource item in source2.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				res = await func(res, item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				yield return res;
			}
		}
	}

	public static IAsyncEnumerable<TOther> SelectMany<TSource, TOther>(this IAsyncEnumerable<TSource> source, IAsyncEnumerable<TOther> other)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (other == null)
		{
			throw Error.ArgumentNull("other");
		}
		return source.SelectMany((TSource _) => other);
	}

	public static IAsyncEnumerable<TSource> StartWith<TSource>(this IAsyncEnumerable<TSource> source, params TSource[] values)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (values == null)
		{
			throw Error.ArgumentNull("values");
		}
		return values.ToAsyncEnumerable().Concat(source);
	}

	public static ValueTask<int> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<int> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<int> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<long> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, long> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<long> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<long> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<float> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, float> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<float> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<float> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<double> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, double> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<double> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<double> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<decimal> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, decimal> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<decimal> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<decimal> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<int?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, int?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<int?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<int?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<long?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, long?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<long?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<long?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<float?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, float?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<float?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<float?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<double?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, double?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<double?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<double?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<decimal?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, decimal?> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<decimal?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static ValueTask<decimal?> SumAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (selector == null)
		{
			throw Error.ArgumentNull("selector");
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

	public static IAsyncEnumerable<TValue> Throw<TValue>(Exception exception)
	{
		if (exception == null)
		{
			throw Error.ArgumentNull("exception");
		}
		return new ThrowEnumerable<TValue>(new ValueTask<bool>(Task.FromException<bool>(exception)));
	}

	public static IAsyncEnumerable<TSource> Timeout<TSource>(this IAsyncEnumerable<TSource> source, TimeSpan timeout)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw Error.ArgumentOutOfRange("timeout");
		}
		return new TimeoutAsyncIterator<TSource>(source, timeout);
	}

	public static IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(this IObservable<TSource> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return new ObservableAsyncEnumerable<TSource>(source);
	}

	public static IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(this Task<TSource> task)
	{
		if (task == null)
		{
			throw Error.ArgumentNull("task");
		}
		return new TaskToAsyncEnumerable<TSource>(task);
	}

	public static IObservable<TSource> ToObservable<TSource>(this IAsyncEnumerable<TSource> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		return new ToObservableObservable<TSource>(source);
	}

	public static IAsyncEnumerable<TSource> Using<TSource, TResource>(Func<TResource> resourceFactory, Func<TResource, IAsyncEnumerable<TSource>> enumerableFactory) where TResource : IDisposable
	{
		if (resourceFactory == null)
		{
			throw Error.ArgumentNull("resourceFactory");
		}
		if (enumerableFactory == null)
		{
			throw Error.ArgumentNull("enumerableFactory");
		}
		return Core(resourceFactory, enumerableFactory);
		static async IAsyncEnumerable<TSource> Core(Func<TResource> func, Func<TResource, IAsyncEnumerable<TSource>> func2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			using TResource resource = func();
			await foreach (TSource item in func2(resource).WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				yield return item;
			}
		}
	}

	public static IAsyncEnumerable<TSource> Using<TSource, TResource>(Func<Task<TResource>> resourceFactory, Func<TResource, ValueTask<IAsyncEnumerable<TSource>>> enumerableFactory) where TResource : IDisposable
	{
		if (resourceFactory == null)
		{
			throw Error.ArgumentNull("resourceFactory");
		}
		if (enumerableFactory == null)
		{
			throw Error.ArgumentNull("enumerableFactory");
		}
		return Core(resourceFactory, enumerableFactory);
		static async IAsyncEnumerable<TSource> Core(Func<Task<TResource>> func, Func<TResource, ValueTask<IAsyncEnumerable<TSource>>> func2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			using TResource resource = await func().ConfigureAwait(continueOnCapturedContext: false);
			await foreach (TSource item in (await func2(resource).ConfigureAwait(continueOnCapturedContext: false)).WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				yield return item;
			}
		}
	}

	public static IAsyncEnumerable<TSource> Using<TSource, TResource>(Func<CancellationToken, Task<TResource>> resourceFactory, Func<TResource, CancellationToken, ValueTask<IAsyncEnumerable<TSource>>> enumerableFactory) where TResource : IDisposable
	{
		if (resourceFactory == null)
		{
			throw Error.ArgumentNull("resourceFactory");
		}
		if (enumerableFactory == null)
		{
			throw Error.ArgumentNull("enumerableFactory");
		}
		return Core(resourceFactory, enumerableFactory);
		static async IAsyncEnumerable<TSource> Core(Func<CancellationToken, Task<TResource>> func, Func<TResource, CancellationToken, ValueTask<IAsyncEnumerable<TSource>>> func2, [EnumeratorCancellation] CancellationToken cancellationToken = default(CancellationToken))
		{
			using TResource resource = await func(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			await foreach (TSource item in (await func2(resource, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				yield return item;
			}
		}
	}

	public static ValueTask<double> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<double> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<double> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<double> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<float> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<float> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<double> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<double> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<decimal> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<decimal> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<double?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<double?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<double?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<double?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<float?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<float?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<double?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<double?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<decimal?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}

	public static ValueTask<decimal?> AverageAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAsyncCore(selector, cancellationToken);
	}
}
