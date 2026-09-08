using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Joins;
using System.Reactive.Linq.ObservableImpl;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace System.Reactive.Linq;

internal class QueryLanguage : IQueryLanguage
{
	private sealed class StartAsyncObservable<TSource> : ObservableBase<TSource>
	{
		private readonly CancellationDisposable _cancellable;

		private readonly IObservable<TSource> _result;

		public StartAsyncObservable(CancellationDisposable cancellable, IObservable<TSource> result)
		{
			_cancellable = cancellable;
			_result = result;
		}

		protected override IDisposable SubscribeCore(IObserver<TSource> observer)
		{
			IDisposable disposable = _result.Subscribe(observer);
			return StableCompositeDisposable.Create(_cancellable, disposable);
		}
	}

	private sealed class CreateWithDisposableObservable<TSource> : ObservableBase<TSource>
	{
		private readonly Func<IObserver<TSource>, IDisposable> _subscribe;

		public CreateWithDisposableObservable(Func<IObserver<TSource>, IDisposable> subscribe)
		{
			_subscribe = subscribe;
		}

		protected override IDisposable SubscribeCore(IObserver<TSource> observer)
		{
			return _subscribe(observer) ?? Disposable.Empty;
		}
	}

	private sealed class CreateWithActionDisposable<TSource> : ObservableBase<TSource>
	{
		private readonly Func<IObserver<TSource>, Action> _subscribe;

		public CreateWithActionDisposable(Func<IObserver<TSource>, Action> subscribe)
		{
			_subscribe = subscribe;
		}

		protected override IDisposable SubscribeCore(IObserver<TSource> observer)
		{
			Action action = _subscribe(observer);
			if (action == null)
			{
				return Disposable.Empty;
			}
			return Disposable.Create(action);
		}
	}

	private sealed class CreateWithTaskTokenObservable<TResult> : ObservableBase<TResult>
	{
		private sealed class Subscription : IDisposable
		{
			private sealed class TaskCompletionObserver : IObserver<Unit>
			{
				private readonly IObserver<TResult> _observer;

				public TaskCompletionObserver(IObserver<TResult> observer)
				{
					_observer = observer;
				}

				public void OnCompleted()
				{
					_observer.OnCompleted();
				}

				public void OnError(Exception error)
				{
					_observer.OnError(error);
				}

				public void OnNext(Unit value)
				{
				}
			}

			private readonly IDisposable _subscription;

			private readonly CancellationTokenSource _cts = new CancellationTokenSource();

			public Subscription(Func<IObserver<TResult>, CancellationToken, Task> subscribeAsync, IObserver<TResult> observer)
			{
				_subscription = subscribeAsync(observer, _cts.Token).Subscribe(new TaskCompletionObserver(observer));
			}

			public void Dispose()
			{
				_cts.Cancel();
				_subscription.Dispose();
			}
		}

		private readonly Func<IObserver<TResult>, CancellationToken, Task> _subscribeAsync;

		public CreateWithTaskTokenObservable(Func<IObserver<TResult>, CancellationToken, Task> subscribeAsync)
		{
			_subscribeAsync = subscribeAsync;
		}

		protected override IDisposable SubscribeCore(IObserver<TResult> observer)
		{
			return new Subscription(_subscribeAsync, observer);
		}
	}

	private sealed class CreateWithTaskDisposable<TResult> : ObservableBase<TResult>
	{
		private sealed class Subscription : IDisposable
		{
			private sealed class TaskDisposeCompletionObserver : IObserver<IDisposable>, IDisposable
			{
				private readonly IObserver<TResult> _observer;

				private SingleAssignmentDisposableValue _disposable;

				public TaskDisposeCompletionObserver(IObserver<TResult> observer)
				{
					_observer = observer;
				}

				public void Dispose()
				{
					_disposable.Dispose();
				}

				public void OnCompleted()
				{
				}

				public void OnError(Exception error)
				{
					_observer.OnError(error);
				}

				public void OnNext(IDisposable value)
				{
					_disposable.Disposable = value;
				}
			}

			private readonly TaskDisposeCompletionObserver _observer;

			private readonly CancellationTokenSource _cts = new CancellationTokenSource();

			public Subscription(Func<IObserver<TResult>, CancellationToken, Task<IDisposable>> subscribeAsync, IObserver<TResult> observer)
			{
				subscribeAsync(observer, _cts.Token).Subscribe(_observer = new TaskDisposeCompletionObserver(observer));
			}

			public void Dispose()
			{
				_cts.Cancel();
				_observer.Dispose();
			}
		}

		private readonly Func<IObserver<TResult>, CancellationToken, Task<IDisposable>> _subscribeAsync;

		public CreateWithTaskDisposable(Func<IObserver<TResult>, CancellationToken, Task<IDisposable>> subscribeAsync)
		{
			_subscribeAsync = subscribeAsync;
		}

		protected override IDisposable SubscribeCore(IObserver<TResult> observer)
		{
			return new Subscription(_subscribeAsync, observer);
		}
	}

	private sealed class CreateWithTaskActionObservable<TResult> : ObservableBase<TResult>
	{
		private sealed class Subscription : IDisposable
		{
			private sealed class TaskDisposeCompletionObserver : IObserver<Action>, IDisposable
			{
				private readonly IObserver<TResult> _observer;

				private Action? _disposable;

				public TaskDisposeCompletionObserver(IObserver<TResult> observer)
				{
					_observer = observer;
				}

				public void Dispose()
				{
					Interlocked.Exchange(ref _disposable, Stubs.Nop)?.Invoke();
				}

				public void OnCompleted()
				{
				}

				public void OnError(Exception error)
				{
					_observer.OnError(error);
				}

				public void OnNext(Action value)
				{
					if (Interlocked.CompareExchange(ref _disposable, value, null) != null)
					{
						value?.Invoke();
					}
				}
			}

			private readonly TaskDisposeCompletionObserver _observer;

			private readonly CancellationTokenSource _cts = new CancellationTokenSource();

			public Subscription(Func<IObserver<TResult>, CancellationToken, Task<Action>> subscribeAsync, IObserver<TResult> observer)
			{
				subscribeAsync(observer, _cts.Token).Subscribe(_observer = new TaskDisposeCompletionObserver(observer));
			}

			public void Dispose()
			{
				_cts.Cancel();
				_observer.Dispose();
			}
		}

		private readonly Func<IObserver<TResult>, CancellationToken, Task<Action>> _subscribeAsync;

		public CreateWithTaskActionObservable(Func<IObserver<TResult>, CancellationToken, Task<Action>> subscribeAsync)
		{
			_subscribeAsync = subscribeAsync;
		}

		protected override IDisposable SubscribeCore(IObserver<TResult> observer)
		{
			return new Subscription(_subscribeAsync, observer);
		}
	}

	private sealed class WhenObservable<TResult> : ObservableBase<TResult>
	{
		private sealed class OutObserver : IObserver<TResult>
		{
			private readonly IObserver<TResult> _observer;

			private readonly Dictionary<object, IJoinObserver> _externalSubscriptions;

			public OutObserver(IObserver<TResult> observer, Dictionary<object, IJoinObserver> externalSubscriptions)
			{
				_observer = observer;
				_externalSubscriptions = externalSubscriptions;
			}

			public void OnCompleted()
			{
				_observer.OnCompleted();
			}

			public void OnError(Exception exception)
			{
				foreach (IJoinObserver value in _externalSubscriptions.Values)
				{
					value.Dispose();
				}
				_observer.OnError(exception);
			}

			public void OnNext(TResult value)
			{
				_observer.OnNext(value);
			}
		}

		private readonly IEnumerable<Plan<TResult>> _plans;

		public WhenObservable(IEnumerable<Plan<TResult>> plans)
		{
			_plans = plans;
		}

		protected override IDisposable SubscribeCore(IObserver<TResult> observer)
		{
			Dictionary<object, IJoinObserver> dictionary = new Dictionary<object, IJoinObserver>();
			object gate = new object();
			List<ActivePlan> activePlans = new List<ActivePlan>();
			OutObserver outObserver = new OutObserver(observer, dictionary);
			try
			{
				foreach (Plan<TResult> plan in _plans)
				{
					activePlans.Add(plan.Activate(dictionary, outObserver, onDeactivate));
				}
			}
			catch (Exception error)
			{
				observer.OnError(error);
				return Disposable.Empty;
			}
			CompositeDisposable compositeDisposable = new CompositeDisposable(dictionary.Values.Count);
			foreach (IJoinObserver value in dictionary.Values)
			{
				value.Subscribe(gate);
				compositeDisposable.Add(value);
			}
			return compositeDisposable;
			void onDeactivate(ActivePlan activePlan)
			{
				activePlans.Remove(activePlan);
				if (activePlans.Count == 0)
				{
					outObserver.OnCompleted();
				}
			}
		}
	}

	public virtual IObservable<TAccumulate> Aggregate<TSource, TAccumulate>(IObservable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator)
	{
		return new Aggregate<TSource, TAccumulate>(source, seed, accumulator);
	}

	public virtual IObservable<TResult> Aggregate<TSource, TAccumulate, TResult>(IObservable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator, Func<TAccumulate, TResult> resultSelector)
	{
		return new Aggregate<TSource, TAccumulate, TResult>(source, seed, accumulator, resultSelector);
	}

	public virtual IObservable<TSource> Aggregate<TSource>(IObservable<TSource> source, Func<TSource, TSource, TSource> accumulator)
	{
		return new Aggregate<TSource>(source, accumulator);
	}

	public virtual IObservable<double> Average<TSource>(IObservable<TSource> source, Func<TSource, double> selector)
	{
		return Average(Select(source, selector));
	}

	public virtual IObservable<float> Average<TSource>(IObservable<TSource> source, Func<TSource, float> selector)
	{
		return Average(Select(source, selector));
	}

	public virtual IObservable<decimal> Average<TSource>(IObservable<TSource> source, Func<TSource, decimal> selector)
	{
		return Average(Select(source, selector));
	}

	public virtual IObservable<double> Average<TSource>(IObservable<TSource> source, Func<TSource, int> selector)
	{
		return Average(Select(source, selector));
	}

	public virtual IObservable<double> Average<TSource>(IObservable<TSource> source, Func<TSource, long> selector)
	{
		return Average(Select(source, selector));
	}

	public virtual IObservable<double?> Average<TSource>(IObservable<TSource> source, Func<TSource, double?> selector)
	{
		return Average(Select(source, selector));
	}

	public virtual IObservable<float?> Average<TSource>(IObservable<TSource> source, Func<TSource, float?> selector)
	{
		return Average(Select(source, selector));
	}

	public virtual IObservable<decimal?> Average<TSource>(IObservable<TSource> source, Func<TSource, decimal?> selector)
	{
		return Average(Select(source, selector));
	}

	public virtual IObservable<double?> Average<TSource>(IObservable<TSource> source, Func<TSource, int?> selector)
	{
		return Average(Select(source, selector));
	}

	public virtual IObservable<double?> Average<TSource>(IObservable<TSource> source, Func<TSource, long?> selector)
	{
		return Average(Select(source, selector));
	}

	public virtual IObservable<bool> All<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		return new All<TSource>(source, predicate);
	}

	public virtual IObservable<bool> Any<TSource>(IObservable<TSource> source)
	{
		return new Any<TSource>.Count(source);
	}

	public virtual IObservable<bool> Any<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		return new Any<TSource>.Predicate(source, predicate);
	}

	public virtual IObservable<double> Average(IObservable<double> source)
	{
		return new AverageDouble(source);
	}

	public virtual IObservable<float> Average(IObservable<float> source)
	{
		return new AverageSingle(source);
	}

	public virtual IObservable<decimal> Average(IObservable<decimal> source)
	{
		return new AverageDecimal(source);
	}

	public virtual IObservable<double> Average(IObservable<int> source)
	{
		return new AverageInt32(source);
	}

	public virtual IObservable<double> Average(IObservable<long> source)
	{
		return new AverageInt64(source);
	}

	public virtual IObservable<double?> Average(IObservable<double?> source)
	{
		return new AverageDoubleNullable(source);
	}

	public virtual IObservable<float?> Average(IObservable<float?> source)
	{
		return new AverageSingleNullable(source);
	}

	public virtual IObservable<decimal?> Average(IObservable<decimal?> source)
	{
		return new AverageDecimalNullable(source);
	}

	public virtual IObservable<double?> Average(IObservable<int?> source)
	{
		return new AverageInt32Nullable(source);
	}

	public virtual IObservable<double?> Average(IObservable<long?> source)
	{
		return new AverageInt64Nullable(source);
	}

	public virtual IObservable<bool> Contains<TSource>(IObservable<TSource> source, TSource value)
	{
		return new Contains<TSource>(source, value, EqualityComparer<TSource>.Default);
	}

	public virtual IObservable<bool> Contains<TSource>(IObservable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
	{
		return new Contains<TSource>(source, value, comparer);
	}

	public virtual IObservable<int> Count<TSource>(IObservable<TSource> source)
	{
		return new Count<TSource>.All(source);
	}

	public virtual IObservable<int> Count<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		return new Count<TSource>.Predicate(source, predicate);
	}

	public virtual IObservable<TSource> ElementAt<TSource>(IObservable<TSource> source, int index)
	{
		return new ElementAt<TSource>(source, index);
	}

	public virtual IObservable<TSource?> ElementAtOrDefault<TSource>(IObservable<TSource> source, int index)
	{
		return new ElementAtOrDefault<TSource>(source, index);
	}

	public virtual IObservable<TSource> FirstAsync<TSource>(IObservable<TSource> source)
	{
		return new FirstAsync<TSource>.Sequence(source);
	}

	public virtual IObservable<TSource> FirstAsync<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		return new FirstAsync<TSource>.Predicate(source, predicate);
	}

	public virtual IObservable<TSource?> FirstOrDefaultAsync<TSource>(IObservable<TSource> source)
	{
		return new FirstOrDefaultAsync<TSource>.Sequence(source);
	}

	public virtual IObservable<TSource?> FirstOrDefaultAsync<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		return new FirstOrDefaultAsync<TSource>.Predicate(source, predicate);
	}

	public virtual IObservable<bool> IsEmpty<TSource>(IObservable<TSource> source)
	{
		return new IsEmpty<TSource>(source);
	}

	public virtual IObservable<TSource> LastAsync<TSource>(IObservable<TSource> source)
	{
		return new LastAsync<TSource>.Sequence(source);
	}

	public virtual IObservable<TSource> LastAsync<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		return new LastAsync<TSource>.Predicate(source, predicate);
	}

	public virtual IObservable<TSource?> LastOrDefaultAsync<TSource>(IObservable<TSource> source)
	{
		return new LastOrDefaultAsync<TSource>.Sequence(source);
	}

	public virtual IObservable<TSource?> LastOrDefaultAsync<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		return new LastOrDefaultAsync<TSource>.Predicate(source, predicate);
	}

	public virtual IObservable<long> LongCount<TSource>(IObservable<TSource> source)
	{
		return new LongCount<TSource>.All(source);
	}

	public virtual IObservable<long> LongCount<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		return new LongCount<TSource>.Predicate(source, predicate);
	}

	public virtual IObservable<TSource> Max<TSource>(IObservable<TSource> source)
	{
		return new Max<TSource>(source, Comparer<TSource>.Default);
	}

	public virtual IObservable<TSource> Max<TSource>(IObservable<TSource> source, IComparer<TSource> comparer)
	{
		return new Max<TSource>(source, comparer);
	}

	public virtual IObservable<double> Max(IObservable<double> source)
	{
		return new MaxDouble(source);
	}

	public virtual IObservable<float> Max(IObservable<float> source)
	{
		return new MaxSingle(source);
	}

	public virtual IObservable<decimal> Max(IObservable<decimal> source)
	{
		return new MaxDecimal(source);
	}

	public virtual IObservable<int> Max(IObservable<int> source)
	{
		return new MaxInt32(source);
	}

	public virtual IObservable<long> Max(IObservable<long> source)
	{
		return new MaxInt64(source);
	}

	public virtual IObservable<double?> Max(IObservable<double?> source)
	{
		return new MaxDoubleNullable(source);
	}

	public virtual IObservable<float?> Max(IObservable<float?> source)
	{
		return new MaxSingleNullable(source);
	}

	public virtual IObservable<decimal?> Max(IObservable<decimal?> source)
	{
		return new MaxDecimalNullable(source);
	}

	public virtual IObservable<int?> Max(IObservable<int?> source)
	{
		return new MaxInt32Nullable(source);
	}

	public virtual IObservable<long?> Max(IObservable<long?> source)
	{
		return new MaxInt64Nullable(source);
	}

	public virtual IObservable<TResult> Max<TSource, TResult>(IObservable<TSource> source, Func<TSource, TResult> selector)
	{
		return Max(Select(source, selector));
	}

	public virtual IObservable<TResult> Max<TSource, TResult>(IObservable<TSource> source, Func<TSource, TResult> selector, IComparer<TResult> comparer)
	{
		return Max(Select(source, selector), comparer);
	}

	public virtual IObservable<double> Max<TSource>(IObservable<TSource> source, Func<TSource, double> selector)
	{
		return Max(Select(source, selector));
	}

	public virtual IObservable<float> Max<TSource>(IObservable<TSource> source, Func<TSource, float> selector)
	{
		return Max(Select(source, selector));
	}

	public virtual IObservable<decimal> Max<TSource>(IObservable<TSource> source, Func<TSource, decimal> selector)
	{
		return Max(Select(source, selector));
	}

	public virtual IObservable<int> Max<TSource>(IObservable<TSource> source, Func<TSource, int> selector)
	{
		return Max(Select(source, selector));
	}

	public virtual IObservable<long> Max<TSource>(IObservable<TSource> source, Func<TSource, long> selector)
	{
		return Max(Select(source, selector));
	}

	public virtual IObservable<double?> Max<TSource>(IObservable<TSource> source, Func<TSource, double?> selector)
	{
		return Max(Select(source, selector));
	}

	public virtual IObservable<float?> Max<TSource>(IObservable<TSource> source, Func<TSource, float?> selector)
	{
		return Max(Select(source, selector));
	}

	public virtual IObservable<decimal?> Max<TSource>(IObservable<TSource> source, Func<TSource, decimal?> selector)
	{
		return Max(Select(source, selector));
	}

	public virtual IObservable<int?> Max<TSource>(IObservable<TSource> source, Func<TSource, int?> selector)
	{
		return Max(Select(source, selector));
	}

	public virtual IObservable<long?> Max<TSource>(IObservable<TSource> source, Func<TSource, long?> selector)
	{
		return Max(Select(source, selector));
	}

	public virtual IObservable<IList<TSource>> MaxBy<TSource, TKey>(IObservable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return new MaxBy<TSource, TKey>(source, keySelector, Comparer<TKey>.Default);
	}

	public virtual IObservable<IList<TSource>> MaxBy<TSource, TKey>(IObservable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
	{
		return new MaxBy<TSource, TKey>(source, keySelector, comparer);
	}

	public virtual IObservable<TSource> Min<TSource>(IObservable<TSource> source)
	{
		return new Min<TSource>(source, Comparer<TSource>.Default);
	}

	public virtual IObservable<TSource> Min<TSource>(IObservable<TSource> source, IComparer<TSource> comparer)
	{
		return new Min<TSource>(source, comparer);
	}

	public virtual IObservable<double> Min(IObservable<double> source)
	{
		return new MinDouble(source);
	}

	public virtual IObservable<float> Min(IObservable<float> source)
	{
		return new MinSingle(source);
	}

	public virtual IObservable<decimal> Min(IObservable<decimal> source)
	{
		return new MinDecimal(source);
	}

	public virtual IObservable<int> Min(IObservable<int> source)
	{
		return new MinInt32(source);
	}

	public virtual IObservable<long> Min(IObservable<long> source)
	{
		return new MinInt64(source);
	}

	public virtual IObservable<double?> Min(IObservable<double?> source)
	{
		return new MinDoubleNullable(source);
	}

	public virtual IObservable<float?> Min(IObservable<float?> source)
	{
		return new MinSingleNullable(source);
	}

	public virtual IObservable<decimal?> Min(IObservable<decimal?> source)
	{
		return new MinDecimalNullable(source);
	}

	public virtual IObservable<int?> Min(IObservable<int?> source)
	{
		return new MinInt32Nullable(source);
	}

	public virtual IObservable<long?> Min(IObservable<long?> source)
	{
		return new MinInt64Nullable(source);
	}

	public virtual IObservable<TResult> Min<TSource, TResult>(IObservable<TSource> source, Func<TSource, TResult> selector)
	{
		return Min(Select(source, selector));
	}

	public virtual IObservable<TResult> Min<TSource, TResult>(IObservable<TSource> source, Func<TSource, TResult> selector, IComparer<TResult> comparer)
	{
		return Min(Select(source, selector), comparer);
	}

	public virtual IObservable<double> Min<TSource>(IObservable<TSource> source, Func<TSource, double> selector)
	{
		return Min(Select(source, selector));
	}

	public virtual IObservable<float> Min<TSource>(IObservable<TSource> source, Func<TSource, float> selector)
	{
		return Min(Select(source, selector));
	}

	public virtual IObservable<decimal> Min<TSource>(IObservable<TSource> source, Func<TSource, decimal> selector)
	{
		return Min(Select(source, selector));
	}

	public virtual IObservable<int> Min<TSource>(IObservable<TSource> source, Func<TSource, int> selector)
	{
		return Min(Select(source, selector));
	}

	public virtual IObservable<long> Min<TSource>(IObservable<TSource> source, Func<TSource, long> selector)
	{
		return Min(Select(source, selector));
	}

	public virtual IObservable<double?> Min<TSource>(IObservable<TSource> source, Func<TSource, double?> selector)
	{
		return Min(Select(source, selector));
	}

	public virtual IObservable<float?> Min<TSource>(IObservable<TSource> source, Func<TSource, float?> selector)
	{
		return Min(Select(source, selector));
	}

	public virtual IObservable<decimal?> Min<TSource>(IObservable<TSource> source, Func<TSource, decimal?> selector)
	{
		return Min(Select(source, selector));
	}

	public virtual IObservable<int?> Min<TSource>(IObservable<TSource> source, Func<TSource, int?> selector)
	{
		return Min(Select(source, selector));
	}

	public virtual IObservable<long?> Min<TSource>(IObservable<TSource> source, Func<TSource, long?> selector)
	{
		return Min(Select(source, selector));
	}

	public virtual IObservable<IList<TSource>> MinBy<TSource, TKey>(IObservable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return new MinBy<TSource, TKey>(source, keySelector, Comparer<TKey>.Default);
	}

	public virtual IObservable<IList<TSource>> MinBy<TSource, TKey>(IObservable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
	{
		return new MinBy<TSource, TKey>(source, keySelector, comparer);
	}

	public virtual IObservable<bool> SequenceEqual<TSource>(IObservable<TSource> first, IObservable<TSource> second)
	{
		return new SequenceEqual<TSource>.Observable(first, second, EqualityComparer<TSource>.Default);
	}

	public virtual IObservable<bool> SequenceEqual<TSource>(IObservable<TSource> first, IObservable<TSource> second, IEqualityComparer<TSource> comparer)
	{
		return new SequenceEqual<TSource>.Observable(first, second, comparer);
	}

	public virtual IObservable<bool> SequenceEqual<TSource>(IObservable<TSource> first, IEnumerable<TSource> second)
	{
		return new SequenceEqual<TSource>.Enumerable(first, second, EqualityComparer<TSource>.Default);
	}

	public virtual IObservable<bool> SequenceEqual<TSource>(IObservable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
	{
		return new SequenceEqual<TSource>.Enumerable(first, second, comparer);
	}

	public virtual IObservable<TSource> SingleAsync<TSource>(IObservable<TSource> source)
	{
		return new SingleAsync<TSource>.Sequence(source);
	}

	public virtual IObservable<TSource> SingleAsync<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		return new SingleAsync<TSource>.Predicate(source, predicate);
	}

	public virtual IObservable<TSource?> SingleOrDefaultAsync<TSource>(IObservable<TSource> source)
	{
		return new SingleOrDefaultAsync<TSource>.Sequence(source);
	}

	public virtual IObservable<TSource?> SingleOrDefaultAsync<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		return new SingleOrDefaultAsync<TSource>.Predicate(source, predicate);
	}

	public virtual IObservable<double> Sum(IObservable<double> source)
	{
		return new SumDouble(source);
	}

	public virtual IObservable<float> Sum(IObservable<float> source)
	{
		return new SumSingle(source);
	}

	public virtual IObservable<decimal> Sum(IObservable<decimal> source)
	{
		return new SumDecimal(source);
	}

	public virtual IObservable<int> Sum(IObservable<int> source)
	{
		return new SumInt32(source);
	}

	public virtual IObservable<long> Sum(IObservable<long> source)
	{
		return new SumInt64(source);
	}

	public virtual IObservable<double?> Sum(IObservable<double?> source)
	{
		return new SumDoubleNullable(source);
	}

	public virtual IObservable<float?> Sum(IObservable<float?> source)
	{
		return new SumSingleNullable(source);
	}

	public virtual IObservable<decimal?> Sum(IObservable<decimal?> source)
	{
		return new SumDecimalNullable(source);
	}

	public virtual IObservable<int?> Sum(IObservable<int?> source)
	{
		return new SumInt32Nullable(source);
	}

	public virtual IObservable<long?> Sum(IObservable<long?> source)
	{
		return new SumInt64Nullable(source);
	}

	public virtual IObservable<double> Sum<TSource>(IObservable<TSource> source, Func<TSource, double> selector)
	{
		return Sum(Select(source, selector));
	}

	public virtual IObservable<float> Sum<TSource>(IObservable<TSource> source, Func<TSource, float> selector)
	{
		return Sum(Select(source, selector));
	}

	public virtual IObservable<decimal> Sum<TSource>(IObservable<TSource> source, Func<TSource, decimal> selector)
	{
		return Sum(Select(source, selector));
	}

	public virtual IObservable<int> Sum<TSource>(IObservable<TSource> source, Func<TSource, int> selector)
	{
		return Sum(Select(source, selector));
	}

	public virtual IObservable<long> Sum<TSource>(IObservable<TSource> source, Func<TSource, long> selector)
	{
		return Sum(Select(source, selector));
	}

	public virtual IObservable<double?> Sum<TSource>(IObservable<TSource> source, Func<TSource, double?> selector)
	{
		return Sum(Select(source, selector));
	}

	public virtual IObservable<float?> Sum<TSource>(IObservable<TSource> source, Func<TSource, float?> selector)
	{
		return Sum(Select(source, selector));
	}

	public virtual IObservable<decimal?> Sum<TSource>(IObservable<TSource> source, Func<TSource, decimal?> selector)
	{
		return Sum(Select(source, selector));
	}

	public virtual IObservable<int?> Sum<TSource>(IObservable<TSource> source, Func<TSource, int?> selector)
	{
		return Sum(Select(source, selector));
	}

	public virtual IObservable<long?> Sum<TSource>(IObservable<TSource> source, Func<TSource, long?> selector)
	{
		return Sum(Select(source, selector));
	}

	public virtual IObservable<TSource[]> ToArray<TSource>(IObservable<TSource> source)
	{
		return new ToArray<TSource>(source);
	}

	public virtual IObservable<IDictionary<TKey, TElement>> ToDictionary<TSource, TKey, TElement>(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) where TKey : notnull
	{
		return new ToDictionary<TSource, TKey, TElement>(source, keySelector, elementSelector, comparer);
	}

	public virtual IObservable<IDictionary<TKey, TElement>> ToDictionary<TSource, TKey, TElement>(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) where TKey : notnull
	{
		return new ToDictionary<TSource, TKey, TElement>(source, keySelector, elementSelector, EqualityComparer<TKey>.Default);
	}

	public virtual IObservable<IDictionary<TKey, TSource>> ToDictionary<TSource, TKey>(IObservable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) where TKey : notnull
	{
		return new ToDictionary<TSource, TKey, TSource>(source, keySelector, (TSource x) => x, comparer);
	}

	public virtual IObservable<IDictionary<TKey, TSource>> ToDictionary<TSource, TKey>(IObservable<TSource> source, Func<TSource, TKey> keySelector) where TKey : notnull
	{
		return new ToDictionary<TSource, TKey, TSource>(source, keySelector, (TSource x) => x, EqualityComparer<TKey>.Default);
	}

	public virtual IObservable<IList<TSource>> ToList<TSource>(IObservable<TSource> source)
	{
		return new ToList<TSource>(source);
	}

	public virtual IObservable<ILookup<TKey, TElement>> ToLookup<TSource, TKey, TElement>(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
	{
		return new ToLookup<TSource, TKey, TElement>(source, keySelector, elementSelector, comparer);
	}

	public virtual IObservable<ILookup<TKey, TSource>> ToLookup<TSource, TKey>(IObservable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		return new ToLookup<TSource, TKey, TSource>(source, keySelector, (TSource x) => x, comparer);
	}

	public virtual IObservable<ILookup<TKey, TElement>> ToLookup<TSource, TKey, TElement>(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
	{
		return new ToLookup<TSource, TKey, TElement>(source, keySelector, elementSelector, EqualityComparer<TKey>.Default);
	}

	public virtual IObservable<ILookup<TKey, TSource>> ToLookup<TSource, TKey>(IObservable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return new ToLookup<TSource, TKey, TSource>(source, keySelector, (TSource x) => x, EqualityComparer<TKey>.Default);
	}

	public virtual Func<IObservable<TResult>> FromAsyncPattern<TResult>(Func<AsyncCallback, object?, IAsyncResult> begin, Func<IAsyncResult, TResult> end)
	{
		return delegate
		{
			AsyncSubject<TResult> subject = new AsyncSubject<TResult>();
			try
			{
				begin(delegate(IAsyncResult iar)
				{
					TResult value;
					try
					{
						value = end(iar);
					}
					catch (Exception error)
					{
						subject.OnError(error);
						return;
					}
					subject.OnNext(value);
					subject.OnCompleted();
				}, null);
			}
			catch (Exception exception)
			{
				return Observable.Throw<TResult>(exception, SchedulerDefaults.AsyncConversions);
			}
			return subject.AsObservable();
		};
	}

	public virtual Func<T1, IObservable<TResult>> FromAsyncPattern<T1, TResult>(Func<T1, AsyncCallback, object?, IAsyncResult> begin, Func<IAsyncResult, TResult> end)
	{
		return delegate(T1 x)
		{
			AsyncSubject<TResult> subject = new AsyncSubject<TResult>();
			try
			{
				begin(x, delegate(IAsyncResult iar)
				{
					TResult value;
					try
					{
						value = end(iar);
					}
					catch (Exception error)
					{
						subject.OnError(error);
						return;
					}
					subject.OnNext(value);
					subject.OnCompleted();
				}, null);
			}
			catch (Exception exception)
			{
				return Observable.Throw<TResult>(exception, SchedulerDefaults.AsyncConversions);
			}
			return subject.AsObservable();
		};
	}

	public virtual Func<T1, T2, IObservable<TResult>> FromAsyncPattern<T1, T2, TResult>(Func<T1, T2, AsyncCallback, object?, IAsyncResult> begin, Func<IAsyncResult, TResult> end)
	{
		return delegate(T1 x, T2 y)
		{
			AsyncSubject<TResult> subject = new AsyncSubject<TResult>();
			try
			{
				begin(x, y, delegate(IAsyncResult iar)
				{
					TResult value;
					try
					{
						value = end(iar);
					}
					catch (Exception error)
					{
						subject.OnError(error);
						return;
					}
					subject.OnNext(value);
					subject.OnCompleted();
				}, null);
			}
			catch (Exception exception)
			{
				return Observable.Throw<TResult>(exception, SchedulerDefaults.AsyncConversions);
			}
			return subject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, IObservable<TResult>> FromAsyncPattern<T1, T2, T3, TResult>(Func<T1, T2, T3, AsyncCallback, object?, IAsyncResult> begin, Func<IAsyncResult, TResult> end)
	{
		return delegate(T1 x, T2 y, T3 z)
		{
			AsyncSubject<TResult> subject = new AsyncSubject<TResult>();
			try
			{
				begin(x, y, z, delegate(IAsyncResult iar)
				{
					TResult value;
					try
					{
						value = end(iar);
					}
					catch (Exception error)
					{
						subject.OnError(error);
						return;
					}
					subject.OnNext(value);
					subject.OnCompleted();
				}, null);
			}
			catch (Exception exception)
			{
				return Observable.Throw<TResult>(exception, SchedulerDefaults.AsyncConversions);
			}
			return subject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, IObservable<TResult>> FromAsyncPattern<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, AsyncCallback, object?, IAsyncResult> begin, Func<IAsyncResult, TResult> end)
	{
		return delegate(T1 x, T2 y, T3 z, T4 a)
		{
			AsyncSubject<TResult> subject = new AsyncSubject<TResult>();
			try
			{
				begin(x, y, z, a, delegate(IAsyncResult iar)
				{
					TResult value;
					try
					{
						value = end(iar);
					}
					catch (Exception error)
					{
						subject.OnError(error);
						return;
					}
					subject.OnNext(value);
					subject.OnCompleted();
				}, null);
			}
			catch (Exception exception)
			{
				return Observable.Throw<TResult>(exception, SchedulerDefaults.AsyncConversions);
			}
			return subject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, IObservable<TResult>> FromAsyncPattern<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, AsyncCallback, object?, IAsyncResult> begin, Func<IAsyncResult, TResult> end)
	{
		return delegate(T1 x, T2 y, T3 z, T4 a, T5 b)
		{
			AsyncSubject<TResult> subject = new AsyncSubject<TResult>();
			try
			{
				begin(x, y, z, a, b, delegate(IAsyncResult iar)
				{
					TResult value;
					try
					{
						value = end(iar);
					}
					catch (Exception error)
					{
						subject.OnError(error);
						return;
					}
					subject.OnNext(value);
					subject.OnCompleted();
				}, null);
			}
			catch (Exception exception)
			{
				return Observable.Throw<TResult>(exception, SchedulerDefaults.AsyncConversions);
			}
			return subject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, IObservable<TResult>> FromAsyncPattern<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, AsyncCallback, object?, IAsyncResult> begin, Func<IAsyncResult, TResult> end)
	{
		return delegate(T1 x, T2 y, T3 z, T4 a, T5 b, T6 c)
		{
			AsyncSubject<TResult> subject = new AsyncSubject<TResult>();
			try
			{
				begin(x, y, z, a, b, c, delegate(IAsyncResult iar)
				{
					TResult value;
					try
					{
						value = end(iar);
					}
					catch (Exception error)
					{
						subject.OnError(error);
						return;
					}
					subject.OnNext(value);
					subject.OnCompleted();
				}, null);
			}
			catch (Exception exception)
			{
				return Observable.Throw<TResult>(exception, SchedulerDefaults.AsyncConversions);
			}
			return subject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, IObservable<TResult>> FromAsyncPattern<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, AsyncCallback, object?, IAsyncResult> begin, Func<IAsyncResult, TResult> end)
	{
		return delegate(T1 x, T2 y, T3 z, T4 a, T5 b, T6 c, T7 d)
		{
			AsyncSubject<TResult> subject = new AsyncSubject<TResult>();
			try
			{
				begin(x, y, z, a, b, c, d, delegate(IAsyncResult iar)
				{
					TResult value;
					try
					{
						value = end(iar);
					}
					catch (Exception error)
					{
						subject.OnError(error);
						return;
					}
					subject.OnNext(value);
					subject.OnCompleted();
				}, null);
			}
			catch (Exception exception)
			{
				return Observable.Throw<TResult>(exception, SchedulerDefaults.AsyncConversions);
			}
			return subject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, IObservable<TResult>> FromAsyncPattern<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, AsyncCallback, object?, IAsyncResult> begin, Func<IAsyncResult, TResult> end)
	{
		return delegate(T1 x, T2 y, T3 z, T4 a, T5 b, T6 c, T7 d, T8 e)
		{
			AsyncSubject<TResult> subject = new AsyncSubject<TResult>();
			try
			{
				begin(x, y, z, a, b, c, d, e, delegate(IAsyncResult iar)
				{
					TResult value;
					try
					{
						value = end(iar);
					}
					catch (Exception error)
					{
						subject.OnError(error);
						return;
					}
					subject.OnNext(value);
					subject.OnCompleted();
				}, null);
			}
			catch (Exception exception)
			{
				return Observable.Throw<TResult>(exception, SchedulerDefaults.AsyncConversions);
			}
			return subject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, IObservable<TResult>> FromAsyncPattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, AsyncCallback, object?, IAsyncResult> begin, Func<IAsyncResult, TResult> end)
	{
		return delegate(T1 x, T2 y, T3 z, T4 a, T5 b, T6 c, T7 d, T8 e, T9 f)
		{
			AsyncSubject<TResult> subject = new AsyncSubject<TResult>();
			try
			{
				begin(x, y, z, a, b, c, d, e, f, delegate(IAsyncResult iar)
				{
					TResult value;
					try
					{
						value = end(iar);
					}
					catch (Exception error)
					{
						subject.OnError(error);
						return;
					}
					subject.OnNext(value);
					subject.OnCompleted();
				}, null);
			}
			catch (Exception exception)
			{
				return Observable.Throw<TResult>(exception, SchedulerDefaults.AsyncConversions);
			}
			return subject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, IObservable<TResult>> FromAsyncPattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, AsyncCallback, object?, IAsyncResult> begin, Func<IAsyncResult, TResult> end)
	{
		return delegate(T1 x, T2 y, T3 z, T4 a, T5 b, T6 c, T7 d, T8 e, T9 f, T10 g)
		{
			AsyncSubject<TResult> subject = new AsyncSubject<TResult>();
			try
			{
				begin(x, y, z, a, b, c, d, e, f, g, delegate(IAsyncResult iar)
				{
					TResult value;
					try
					{
						value = end(iar);
					}
					catch (Exception error)
					{
						subject.OnError(error);
						return;
					}
					subject.OnNext(value);
					subject.OnCompleted();
				}, null);
			}
			catch (Exception exception)
			{
				return Observable.Throw<TResult>(exception, SchedulerDefaults.AsyncConversions);
			}
			return subject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, IObservable<TResult>> FromAsyncPattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, AsyncCallback, object?, IAsyncResult> begin, Func<IAsyncResult, TResult> end)
	{
		return delegate(T1 x, T2 y, T3 z, T4 a, T5 b, T6 c, T7 d, T8 e, T9 f, T10 g, T11 h)
		{
			AsyncSubject<TResult> subject = new AsyncSubject<TResult>();
			try
			{
				begin(x, y, z, a, b, c, d, e, f, g, h, delegate(IAsyncResult iar)
				{
					TResult value;
					try
					{
						value = end(iar);
					}
					catch (Exception error)
					{
						subject.OnError(error);
						return;
					}
					subject.OnNext(value);
					subject.OnCompleted();
				}, null);
			}
			catch (Exception exception)
			{
				return Observable.Throw<TResult>(exception, SchedulerDefaults.AsyncConversions);
			}
			return subject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, IObservable<TResult>> FromAsyncPattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, AsyncCallback, object?, IAsyncResult> begin, Func<IAsyncResult, TResult> end)
	{
		return delegate(T1 x, T2 y, T3 z, T4 a, T5 b, T6 c, T7 d, T8 e, T9 f, T10 g, T11 h, T12 i)
		{
			AsyncSubject<TResult> subject = new AsyncSubject<TResult>();
			try
			{
				begin(x, y, z, a, b, c, d, e, f, g, h, i, delegate(IAsyncResult iar)
				{
					TResult value;
					try
					{
						value = end(iar);
					}
					catch (Exception error)
					{
						subject.OnError(error);
						return;
					}
					subject.OnNext(value);
					subject.OnCompleted();
				}, null);
			}
			catch (Exception exception)
			{
				return Observable.Throw<TResult>(exception, SchedulerDefaults.AsyncConversions);
			}
			return subject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, IObservable<TResult>> FromAsyncPattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, AsyncCallback, object?, IAsyncResult> begin, Func<IAsyncResult, TResult> end)
	{
		return delegate(T1 x, T2 y, T3 z, T4 a, T5 b, T6 c, T7 d, T8 e, T9 f, T10 g, T11 h, T12 i, T13 j)
		{
			AsyncSubject<TResult> subject = new AsyncSubject<TResult>();
			try
			{
				begin(x, y, z, a, b, c, d, e, f, g, h, i, j, delegate(IAsyncResult iar)
				{
					TResult value;
					try
					{
						value = end(iar);
					}
					catch (Exception error)
					{
						subject.OnError(error);
						return;
					}
					subject.OnNext(value);
					subject.OnCompleted();
				}, null);
			}
			catch (Exception exception)
			{
				return Observable.Throw<TResult>(exception, SchedulerDefaults.AsyncConversions);
			}
			return subject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, IObservable<TResult>> FromAsyncPattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, AsyncCallback, object?, IAsyncResult> begin, Func<IAsyncResult, TResult> end)
	{
		return delegate(T1 x, T2 y, T3 z, T4 a, T5 b, T6 c, T7 d, T8 e, T9 f, T10 g, T11 h, T12 i, T13 j, T14 k)
		{
			AsyncSubject<TResult> subject = new AsyncSubject<TResult>();
			try
			{
				begin(x, y, z, a, b, c, d, e, f, g, h, i, j, k, delegate(IAsyncResult iar)
				{
					TResult value;
					try
					{
						value = end(iar);
					}
					catch (Exception error)
					{
						subject.OnError(error);
						return;
					}
					subject.OnNext(value);
					subject.OnCompleted();
				}, null);
			}
			catch (Exception exception)
			{
				return Observable.Throw<TResult>(exception, SchedulerDefaults.AsyncConversions);
			}
			return subject.AsObservable();
		};
	}

	public virtual Func<IObservable<Unit>> FromAsyncPattern(Func<AsyncCallback, object?, IAsyncResult> begin, Action<IAsyncResult> end)
	{
		return FromAsyncPattern(begin, delegate(IAsyncResult iar)
		{
			end(iar);
			return Unit.Default;
		});
	}

	public virtual Func<T1, IObservable<Unit>> FromAsyncPattern<T1>(Func<T1, AsyncCallback, object?, IAsyncResult> begin, Action<IAsyncResult> end)
	{
		return FromAsyncPattern(begin, delegate(IAsyncResult iar)
		{
			end(iar);
			return Unit.Default;
		});
	}

	public virtual Func<T1, T2, IObservable<Unit>> FromAsyncPattern<T1, T2>(Func<T1, T2, AsyncCallback, object?, IAsyncResult> begin, Action<IAsyncResult> end)
	{
		return FromAsyncPattern(begin, delegate(IAsyncResult iar)
		{
			end(iar);
			return Unit.Default;
		});
	}

	public virtual Func<T1, T2, T3, IObservable<Unit>> FromAsyncPattern<T1, T2, T3>(Func<T1, T2, T3, AsyncCallback, object?, IAsyncResult> begin, Action<IAsyncResult> end)
	{
		return FromAsyncPattern(begin, delegate(IAsyncResult iar)
		{
			end(iar);
			return Unit.Default;
		});
	}

	public virtual Func<T1, T2, T3, T4, IObservable<Unit>> FromAsyncPattern<T1, T2, T3, T4>(Func<T1, T2, T3, T4, AsyncCallback, object?, IAsyncResult> begin, Action<IAsyncResult> end)
	{
		return FromAsyncPattern(begin, delegate(IAsyncResult iar)
		{
			end(iar);
			return Unit.Default;
		});
	}

	public virtual Func<T1, T2, T3, T4, T5, IObservable<Unit>> FromAsyncPattern<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, AsyncCallback, object?, IAsyncResult> begin, Action<IAsyncResult> end)
	{
		return FromAsyncPattern(begin, delegate(IAsyncResult iar)
		{
			end(iar);
			return Unit.Default;
		});
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, IObservable<Unit>> FromAsyncPattern<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, AsyncCallback, object?, IAsyncResult> begin, Action<IAsyncResult> end)
	{
		return FromAsyncPattern(begin, delegate(IAsyncResult iar)
		{
			end(iar);
			return Unit.Default;
		});
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, IObservable<Unit>> FromAsyncPattern<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, AsyncCallback, object?, IAsyncResult> begin, Action<IAsyncResult> end)
	{
		return FromAsyncPattern(begin, delegate(IAsyncResult iar)
		{
			end(iar);
			return Unit.Default;
		});
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, IObservable<Unit>> FromAsyncPattern<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, AsyncCallback, object?, IAsyncResult> begin, Action<IAsyncResult> end)
	{
		return FromAsyncPattern(begin, delegate(IAsyncResult iar)
		{
			end(iar);
			return Unit.Default;
		});
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, IObservable<Unit>> FromAsyncPattern<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, AsyncCallback, object?, IAsyncResult> begin, Action<IAsyncResult> end)
	{
		return FromAsyncPattern(begin, delegate(IAsyncResult iar)
		{
			end(iar);
			return Unit.Default;
		});
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, IObservable<Unit>> FromAsyncPattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, AsyncCallback, object?, IAsyncResult> begin, Action<IAsyncResult> end)
	{
		return FromAsyncPattern(begin, delegate(IAsyncResult iar)
		{
			end(iar);
			return Unit.Default;
		});
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, IObservable<Unit>> FromAsyncPattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, AsyncCallback, object?, IAsyncResult> begin, Action<IAsyncResult> end)
	{
		return FromAsyncPattern(begin, delegate(IAsyncResult iar)
		{
			end(iar);
			return Unit.Default;
		});
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, IObservable<Unit>> FromAsyncPattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, AsyncCallback, object?, IAsyncResult> begin, Action<IAsyncResult> end)
	{
		return FromAsyncPattern(begin, delegate(IAsyncResult iar)
		{
			end(iar);
			return Unit.Default;
		});
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, IObservable<Unit>> FromAsyncPattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, AsyncCallback, object?, IAsyncResult> begin, Action<IAsyncResult> end)
	{
		return FromAsyncPattern(begin, delegate(IAsyncResult iar)
		{
			end(iar);
			return Unit.Default;
		});
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, IObservable<Unit>> FromAsyncPattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, AsyncCallback, object?, IAsyncResult> begin, Action<IAsyncResult> end)
	{
		return FromAsyncPattern(begin, delegate(IAsyncResult iar)
		{
			end(iar);
			return Unit.Default;
		});
	}

	public virtual IObservable<TSource> Start<TSource>(Func<TSource> function)
	{
		return ToAsync(function)();
	}

	public virtual IObservable<TSource> Start<TSource>(Func<TSource> function, IScheduler scheduler)
	{
		return ToAsync(function, scheduler)();
	}

	public virtual IObservable<TSource> StartAsync<TSource>(Func<Task<TSource>> functionAsync)
	{
		return StartAsyncImpl(functionAsync, new TaskObservationOptions.Value(null, ignoreExceptionsAfterUnsubscribe: false));
	}

	public virtual IObservable<TSource> StartAsync<TSource>(Func<Task<TSource>> functionAsync, in TaskObservationOptions.Value options)
	{
		return StartAsyncImpl(functionAsync, in options);
	}

	private IObservable<TSource> StartAsyncImpl<TSource>(Func<Task<TSource>> functionAsync, in TaskObservationOptions.Value options)
	{
		Task<TSource> task;
		try
		{
			task = functionAsync();
		}
		catch (Exception exception)
		{
			return Throw<TSource>(exception);
		}
		return task.ToObservable(options);
	}

	public virtual IObservable<TSource> StartAsync<TSource>(Func<CancellationToken, Task<TSource>> functionAsync)
	{
		return StartAsyncImpl(functionAsync, new TaskObservationOptions.Value(null, ignoreExceptionsAfterUnsubscribe: false));
	}

	public virtual IObservable<TSource> StartAsync<TSource>(Func<CancellationToken, Task<TSource>> functionAsync, in TaskObservationOptions.Value options)
	{
		return StartAsyncImpl(functionAsync, in options);
	}

	private IObservable<TSource> StartAsyncImpl<TSource>(Func<CancellationToken, Task<TSource>> functionAsync, in TaskObservationOptions.Value options)
	{
		CancellationDisposable cancellationDisposable = new CancellationDisposable();
		Task<TSource> task;
		try
		{
			task = functionAsync(cancellationDisposable.Token);
		}
		catch (Exception exception)
		{
			return Throw<TSource>(exception);
		}
		IObservable<TSource> result = task.ToObservable(options);
		return new StartAsyncObservable<TSource>(cancellationDisposable, result);
	}

	public virtual IObservable<Unit> Start(Action action)
	{
		return ToAsync(action, SchedulerDefaults.AsyncConversions)();
	}

	public virtual IObservable<Unit> Start(Action action, IScheduler scheduler)
	{
		return ToAsync(action, scheduler)();
	}

	public virtual IObservable<Unit> StartAsync(Func<Task> actionAsync)
	{
		return StartAsyncImpl(actionAsync, new TaskObservationOptions.Value(null, ignoreExceptionsAfterUnsubscribe: false));
	}

	public virtual IObservable<Unit> StartAsync(Func<Task> actionAsync, in TaskObservationOptions.Value options)
	{
		return StartAsyncImpl(actionAsync, in options);
	}

	private IObservable<Unit> StartAsyncImpl(Func<Task> actionAsync, in TaskObservationOptions.Value options)
	{
		Task task;
		try
		{
			task = actionAsync();
		}
		catch (Exception exception)
		{
			return Throw<Unit>(exception);
		}
		return task.ToObservable(options);
	}

	public virtual IObservable<Unit> StartAsync(Func<CancellationToken, Task> actionAsync)
	{
		return StartAsyncImpl(actionAsync, new TaskObservationOptions.Value(null, ignoreExceptionsAfterUnsubscribe: false));
	}

	public virtual IObservable<Unit> StartAsync(Func<CancellationToken, Task> actionAsync, in TaskObservationOptions.Value options)
	{
		return StartAsyncImpl(actionAsync, in options);
	}

	private IObservable<Unit> StartAsyncImpl(Func<CancellationToken, Task> actionAsync, in TaskObservationOptions.Value options)
	{
		CancellationDisposable cancellationDisposable = new CancellationDisposable();
		Task task;
		try
		{
			task = actionAsync(cancellationDisposable.Token);
		}
		catch (Exception exception)
		{
			return Throw<Unit>(exception);
		}
		IObservable<Unit> result = task.ToObservable(options);
		return new StartAsyncObservable<Unit>(cancellationDisposable, result);
	}

	public virtual IObservable<TResult> FromAsync<TResult>(Func<Task<TResult>> functionAsync)
	{
		return Defer(() => StartAsync(functionAsync));
	}

	public virtual IObservable<TResult> FromAsync<TResult>(Func<CancellationToken, Task<TResult>> functionAsync)
	{
		return Defer(() => StartAsync(functionAsync));
	}

	public virtual IObservable<TResult> FromAsync<TResult>(Func<Task<TResult>> functionAsync, TaskObservationOptions.Value options)
	{
		return Defer(() => StartAsync(functionAsync, in options));
	}

	public virtual IObservable<TResult> FromAsync<TResult>(Func<CancellationToken, Task<TResult>> functionAsync, TaskObservationOptions.Value options)
	{
		return Defer(() => StartAsync(functionAsync, in options));
	}

	public virtual IObservable<Unit> FromAsync(Func<Task> actionAsync)
	{
		return Defer(() => StartAsync(actionAsync));
	}

	public virtual IObservable<Unit> FromAsync(Func<CancellationToken, Task> actionAsync)
	{
		return Defer(() => StartAsync(actionAsync));
	}

	public virtual IObservable<Unit> FromAsync(Func<Task> actionAsync, TaskObservationOptions.Value options)
	{
		return Defer(() => StartAsync(actionAsync, in options));
	}

	public virtual IObservable<Unit> FromAsync(Func<CancellationToken, Task> actionAsync, TaskObservationOptions.Value options)
	{
		return Defer(() => StartAsync(actionAsync, in options));
	}

	public virtual Func<IObservable<TResult>> ToAsync<TResult>(Func<TResult> function)
	{
		return ToAsync(function, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<IObservable<TResult>> ToAsync<TResult>(Func<TResult> function, IScheduler scheduler)
	{
		return delegate
		{
			AsyncSubject<TResult> asyncSubject = new AsyncSubject<TResult>();
			scheduler.ScheduleAction((function, asyncSubject), delegate((Func<TResult> function, AsyncSubject<TResult> subject) state)
			{
				TResult value;
				try
				{
					value = state.function();
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(value);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T, IObservable<TResult>> ToAsync<T, TResult>(Func<T, TResult> function)
	{
		return ToAsync(function, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T, IObservable<TResult>> ToAsync<T, TResult>(Func<T, TResult> function, IScheduler scheduler)
	{
		return delegate(T first)
		{
			AsyncSubject<TResult> asyncSubject = new AsyncSubject<TResult>();
			scheduler.ScheduleAction((function, asyncSubject, first), delegate((Func<T, TResult> function, AsyncSubject<TResult> subject, T first) state)
			{
				TResult value;
				try
				{
					value = state.function(state.first);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(value);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, IObservable<TResult>> ToAsync<T1, T2, TResult>(Func<T1, T2, TResult> function)
	{
		return ToAsync(function, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, IObservable<TResult>> ToAsync<T1, T2, TResult>(Func<T1, T2, TResult> function, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second)
		{
			AsyncSubject<TResult> asyncSubject = new AsyncSubject<TResult>();
			scheduler.ScheduleAction((asyncSubject, function, first, second), delegate((AsyncSubject<TResult> subject, Func<T1, T2, TResult> function, T1 first, T2 second) state)
			{
				TResult value;
				try
				{
					value = state.function(state.first, state.second);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(value);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, IObservable<TResult>> ToAsync<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> function)
	{
		return ToAsync(function, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, IObservable<TResult>> ToAsync<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> function, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third)
		{
			AsyncSubject<TResult> asyncSubject = new AsyncSubject<TResult>();
			scheduler.ScheduleAction((asyncSubject, function, first, second, third), delegate((AsyncSubject<TResult> subject, Func<T1, T2, T3, TResult> function, T1 first, T2 second, T3 third) state)
			{
				TResult value;
				try
				{
					value = state.function(state.first, state.second, state.third);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(value);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, IObservable<TResult>> ToAsync<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> function)
	{
		return ToAsync(function, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, IObservable<TResult>> ToAsync<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> function, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth)
		{
			AsyncSubject<TResult> asyncSubject = new AsyncSubject<TResult>();
			scheduler.ScheduleAction((asyncSubject, function, first, second, third, fourth), delegate((AsyncSubject<TResult> subject, Func<T1, T2, T3, T4, TResult> function, T1 first, T2 second, T3 third, T4 fourth) state)
			{
				TResult value;
				try
				{
					value = state.function(state.first, state.second, state.third, state.fourth);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(value);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> function)
	{
		return ToAsync(function, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> function, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth)
		{
			AsyncSubject<TResult> asyncSubject = new AsyncSubject<TResult>();
			scheduler.ScheduleAction((asyncSubject, function, first, second, third, fourth, fifth), delegate((AsyncSubject<TResult> subject, Func<T1, T2, T3, T4, T5, TResult> function, T1 first, T2 second, T3 third, T4 fourth, T5 fifth) state)
			{
				TResult value;
				try
				{
					value = state.function(state.first, state.second, state.third, state.fourth, state.fifth);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(value);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> function)
	{
		return ToAsync(function, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> function, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth)
		{
			AsyncSubject<TResult> asyncSubject = new AsyncSubject<TResult>();
			scheduler.ScheduleAction((asyncSubject, function, first, second, third, fourth, fifth, sixth), delegate((AsyncSubject<TResult> subject, Func<T1, T2, T3, T4, T5, T6, TResult> function, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth) state)
			{
				TResult value;
				try
				{
					value = state.function(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(value);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> function)
	{
		return ToAsync(function, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> function, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh)
		{
			AsyncSubject<TResult> asyncSubject = new AsyncSubject<TResult>();
			scheduler.ScheduleAction((asyncSubject, function, first, second, third, fourth, fifth, sixth, seventh), delegate((AsyncSubject<TResult> subject, Func<T1, T2, T3, T4, T5, T6, T7, TResult> function, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh) state)
			{
				TResult value;
				try
				{
					value = state.function(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(value);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> function)
	{
		return ToAsync(function, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> function, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eight)
		{
			AsyncSubject<TResult> asyncSubject = new AsyncSubject<TResult>();
			scheduler.ScheduleAction((asyncSubject, function, first, second, third, fourth, fifth, sixth, seventh, eight), delegate((AsyncSubject<TResult> subject, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> function, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eight) state)
			{
				TResult value;
				try
				{
					value = state.function(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2, state.Rest.Item3);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(value);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> function)
	{
		return ToAsync(function, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> function, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eight, T9 ninth)
		{
			AsyncSubject<TResult> asyncSubject = new AsyncSubject<TResult>();
			scheduler.ScheduleAction((asyncSubject, function, first, second, third, fourth, fifth, sixth, seventh, eight, ninth), delegate((AsyncSubject<TResult> subject, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> function, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eight, T9 ninth) state)
			{
				TResult value;
				try
				{
					value = state.function(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2, state.Rest.Item3, state.Rest.Item4);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(value);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> function)
	{
		return ToAsync(function, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> function, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eight, T9 ninth, T10 tenth)
		{
			AsyncSubject<TResult> asyncSubject = new AsyncSubject<TResult>();
			scheduler.ScheduleAction((asyncSubject, function, first, second, third, fourth, fifth, sixth, seventh, eight, ninth, tenth), delegate((AsyncSubject<TResult> subject, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> function, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eight, T9 ninth, T10 tenth) state)
			{
				TResult value;
				try
				{
					value = state.function(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2, state.Rest.Item3, state.Rest.Item4, state.Rest.Item5);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(value);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> function)
	{
		return ToAsync(function, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> function, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eight, T9 ninth, T10 tenth, T11 eleventh)
		{
			AsyncSubject<TResult> asyncSubject = new AsyncSubject<TResult>();
			scheduler.ScheduleAction((asyncSubject, function, first, second, third, fourth, fifth, sixth, seventh, eight, ninth, tenth, eleventh), delegate((AsyncSubject<TResult> subject, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> function, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eight, T9 ninth, T10 tenth, T11 eleventh) state)
			{
				TResult value;
				try
				{
					value = state.function(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2, state.Rest.Item3, state.Rest.Item4, state.Rest.Item5, state.Rest.Item6);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(value);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> function)
	{
		return ToAsync(function, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> function, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eight, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth)
		{
			AsyncSubject<TResult> asyncSubject = new AsyncSubject<TResult>();
			scheduler.ScheduleAction((asyncSubject, function, first, second, third, fourth, fifth, sixth, seventh, eight, ninth, tenth, eleventh, twelfth), delegate((AsyncSubject<TResult> subject, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> function, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eight, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth) state)
			{
				TResult value;
				try
				{
					value = state.function(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2, state.Rest.Item3, state.Rest.Item4, state.Rest.Item5, state.Rest.Item6, state.Rest.Item7);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(value);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> function)
	{
		return ToAsync(function, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> function, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eight, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth)
		{
			AsyncSubject<TResult> asyncSubject = new AsyncSubject<TResult>();
			scheduler.ScheduleAction((asyncSubject, function, first, second, third, fourth, fifth, sixth, seventh, eight, ninth, tenth, eleventh, twelfth, thirteenth), delegate((AsyncSubject<TResult> subject, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> function, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eight, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth) state)
			{
				TResult value;
				try
				{
					value = state.function(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2, state.Rest.Item3, state.Rest.Item4, state.Rest.Item5, state.Rest.Item6, state.Rest.Item7, state.Rest.Rest.Item1);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(value);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> function)
	{
		return ToAsync(function, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> function, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eight, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth, T14 fourteenth)
		{
			AsyncSubject<TResult> asyncSubject = new AsyncSubject<TResult>();
			scheduler.ScheduleAction((asyncSubject, function, first, second, third, fourth, fifth, sixth, seventh, eight, ninth, tenth, eleventh, twelfth, thirteenth, fourteenth), delegate((AsyncSubject<TResult> subject, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> function, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eight, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth, T14 fourteenth) state)
			{
				TResult value;
				try
				{
					value = state.function(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2, state.Rest.Item3, state.Rest.Item4, state.Rest.Item5, state.Rest.Item6, state.Rest.Item7, state.Rest.Rest.Item1, state.Rest.Rest.Item2);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(value);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> function)
	{
		return ToAsync(function, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> function, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eight, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth, T14 fourteenth, T15 fifteenth)
		{
			AsyncSubject<TResult> asyncSubject = new AsyncSubject<TResult>();
			scheduler.ScheduleAction((asyncSubject, function, first, second, third, fourth, fifth, sixth, seventh, eight, ninth, tenth, eleventh, twelfth, thirteenth, fourteenth, fifteenth), delegate((AsyncSubject<TResult> subject, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> function, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eight, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth, T14 fourteenth, T15 fifteenth) state)
			{
				TResult value;
				try
				{
					value = state.function(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2, state.Rest.Item3, state.Rest.Item4, state.Rest.Item5, state.Rest.Item6, state.Rest.Item7, state.Rest.Rest.Item1, state.Rest.Rest.Item2, state.Rest.Rest.Item3);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(value);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> function)
	{
		return ToAsync(function, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, IObservable<TResult>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> function, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eight, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth, T14 fourteenth, T15 fifteenth, T16 sixteenth)
		{
			AsyncSubject<TResult> asyncSubject = new AsyncSubject<TResult>();
			scheduler.ScheduleAction((asyncSubject, function, first, second, third, fourth, fifth, sixth, seventh, eight, ninth, tenth, eleventh, twelfth, thirteenth, fourteenth, fifteenth, sixteenth), delegate((AsyncSubject<TResult> subject, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> function, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eight, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth, T14 fourteenth, T15 fifteenth, T16 sixteenth) state)
			{
				TResult value;
				try
				{
					value = state.function(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2, state.Rest.Item3, state.Rest.Item4, state.Rest.Item5, state.Rest.Item6, state.Rest.Item7, state.Rest.Rest.Item1, state.Rest.Rest.Item2, state.Rest.Rest.Item3, state.Rest.Rest.Item4);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(value);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<IObservable<Unit>> ToAsync(Action action)
	{
		return ToAsync(action, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<IObservable<Unit>> ToAsync(Action action, IScheduler scheduler)
	{
		return delegate
		{
			AsyncSubject<Unit> asyncSubject = new AsyncSubject<Unit>();
			scheduler.ScheduleAction((asyncSubject, action), delegate((AsyncSubject<Unit> subject, Action action) state)
			{
				try
				{
					state.action();
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(Unit.Default);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<TSource, IObservable<Unit>> ToAsync<TSource>(Action<TSource> action)
	{
		return ToAsync(action, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<TSource, IObservable<Unit>> ToAsync<TSource>(Action<TSource> action, IScheduler scheduler)
	{
		return delegate(TSource first)
		{
			AsyncSubject<Unit> asyncSubject = new AsyncSubject<Unit>();
			scheduler.ScheduleAction((asyncSubject, action, first), delegate((AsyncSubject<Unit> subject, Action<TSource> action, TSource first) state)
			{
				try
				{
					state.action(state.first);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(Unit.Default);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, IObservable<Unit>> ToAsync<T1, T2>(Action<T1, T2> action)
	{
		return ToAsync(action, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, IObservable<Unit>> ToAsync<T1, T2>(Action<T1, T2> action, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second)
		{
			AsyncSubject<Unit> asyncSubject = new AsyncSubject<Unit>();
			scheduler.ScheduleAction((asyncSubject, action, first, second), delegate((AsyncSubject<Unit> subject, Action<T1, T2> action, T1 first, T2 second) state)
			{
				try
				{
					state.action(state.first, state.second);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(Unit.Default);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, IObservable<Unit>> ToAsync<T1, T2, T3>(Action<T1, T2, T3> action)
	{
		return ToAsync(action, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, IObservable<Unit>> ToAsync<T1, T2, T3>(Action<T1, T2, T3> action, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third)
		{
			AsyncSubject<Unit> asyncSubject = new AsyncSubject<Unit>();
			scheduler.ScheduleAction((asyncSubject, action, first, second, third), delegate((AsyncSubject<Unit> subject, Action<T1, T2, T3> action, T1 first, T2 second, T3 third) state)
			{
				try
				{
					state.action(state.first, state.second, state.third);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(Unit.Default);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, IObservable<Unit>> ToAsync<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
	{
		return ToAsync(action, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, IObservable<Unit>> ToAsync<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth)
		{
			AsyncSubject<Unit> asyncSubject = new AsyncSubject<Unit>();
			scheduler.ScheduleAction((asyncSubject, action, first, second, third, fourth), delegate((AsyncSubject<Unit> subject, Action<T1, T2, T3, T4> action, T1 first, T2 second, T3 third, T4 fourth) state)
			{
				try
				{
					state.action(state.first, state.second, state.third, state.fourth);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(Unit.Default);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action)
	{
		return ToAsync(action, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth)
		{
			AsyncSubject<Unit> asyncSubject = new AsyncSubject<Unit>();
			scheduler.ScheduleAction((asyncSubject, action, first, second, third, fourth, fifth), delegate((AsyncSubject<Unit> subject, Action<T1, T2, T3, T4, T5> action, T1 first, T2 second, T3 third, T4 fourth, T5 fifth) state)
			{
				try
				{
					state.action(state.first, state.second, state.third, state.fourth, state.fifth);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(Unit.Default);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action)
	{
		return ToAsync(action, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth)
		{
			AsyncSubject<Unit> asyncSubject = new AsyncSubject<Unit>();
			scheduler.ScheduleAction((asyncSubject, action, first, second, third, fourth, fifth, sixth), delegate((AsyncSubject<Unit> subject, Action<T1, T2, T3, T4, T5, T6> action, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth) state)
			{
				try
				{
					state.action(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(Unit.Default);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action)
	{
		return ToAsync(action, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh)
		{
			AsyncSubject<Unit> asyncSubject = new AsyncSubject<Unit>();
			scheduler.ScheduleAction((asyncSubject, action, first, second, third, fourth, fifth, sixth, seventh), delegate((AsyncSubject<Unit> subject, Action<T1, T2, T3, T4, T5, T6, T7> action, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh) state)
			{
				try
				{
					state.action(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(Unit.Default);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action)
	{
		return ToAsync(action, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth)
		{
			AsyncSubject<Unit> asyncSubject = new AsyncSubject<Unit>();
			scheduler.ScheduleAction((asyncSubject, action, first, second, third, fourth, fifth, sixth, seventh, eighth), delegate((AsyncSubject<Unit> subject, Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth) state)
			{
				try
				{
					state.action(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2, state.Rest.Item3);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(Unit.Default);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action)
	{
		return ToAsync(action, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth)
		{
			AsyncSubject<Unit> asyncSubject = new AsyncSubject<Unit>();
			scheduler.ScheduleAction((asyncSubject, action, first, second, third, fourth, fifth, sixth, seventh, eighth, ninth), delegate((AsyncSubject<Unit> subject, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth) state)
			{
				try
				{
					state.action(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2, state.Rest.Item3, state.Rest.Item4);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(Unit.Default);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action)
	{
		return ToAsync(action, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth)
		{
			AsyncSubject<Unit> asyncSubject = new AsyncSubject<Unit>();
			scheduler.ScheduleAction((asyncSubject, action, first, second, third, fourth, fifth, sixth, seventh, eighth, ninth, tenth), delegate((AsyncSubject<Unit> subject, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth) state)
			{
				try
				{
					state.action(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2, state.Rest.Item3, state.Rest.Item4, state.Rest.Item5);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(Unit.Default);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action)
	{
		return ToAsync(action, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth, T11 eleventh)
		{
			AsyncSubject<Unit> asyncSubject = new AsyncSubject<Unit>();
			scheduler.ScheduleAction((asyncSubject, action, first, second, third, fourth, fifth, sixth, seventh, eighth, ninth, tenth, eleventh), delegate((AsyncSubject<Unit> subject, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth, T11 eleventh) state)
			{
				try
				{
					state.action(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2, state.Rest.Item3, state.Rest.Item4, state.Rest.Item5, state.Rest.Item6);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(Unit.Default);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action)
	{
		return ToAsync(action, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth)
		{
			AsyncSubject<Unit> asyncSubject = new AsyncSubject<Unit>();
			scheduler.ScheduleAction((asyncSubject, action, first, second, third, fourth, fifth, sixth, seventh, eighth, ninth, tenth, eleventh, twelfth), delegate((AsyncSubject<Unit> subject, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth) state)
			{
				try
				{
					state.action(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2, state.Rest.Item3, state.Rest.Item4, state.Rest.Item5, state.Rest.Item6, state.Rest.Item7);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(Unit.Default);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action)
	{
		return ToAsync(action, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth)
		{
			AsyncSubject<Unit> asyncSubject = new AsyncSubject<Unit>();
			scheduler.ScheduleAction((asyncSubject, action, first, second, third, fourth, fifth, sixth, seventh, eighth, ninth, tenth, eleventh, twelfth, thirteenth), delegate((AsyncSubject<Unit> subject, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth) state)
			{
				try
				{
					state.action(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2, state.Rest.Item3, state.Rest.Item4, state.Rest.Item5, state.Rest.Item6, state.Rest.Item7, state.Rest.Rest.Item1);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(Unit.Default);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action)
	{
		return ToAsync(action, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth, T14 fourteenth)
		{
			AsyncSubject<Unit> asyncSubject = new AsyncSubject<Unit>();
			scheduler.ScheduleAction((asyncSubject, action, first, second, third, fourth, fifth, sixth, seventh, eighth, ninth, tenth, eleventh, twelfth, thirteenth, fourteenth), delegate((AsyncSubject<Unit> subject, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth, T14 fourteenth) state)
			{
				try
				{
					state.action(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2, state.Rest.Item3, state.Rest.Item4, state.Rest.Item5, state.Rest.Item6, state.Rest.Item7, state.Rest.Rest.Item1, state.Rest.Rest.Item2);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(Unit.Default);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action)
	{
		return ToAsync(action, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth, T14 fourteenth, T15 fifteenth)
		{
			AsyncSubject<Unit> asyncSubject = new AsyncSubject<Unit>();
			scheduler.ScheduleAction((asyncSubject, action, first, second, third, fourth, fifth, sixth, seventh, eighth, ninth, tenth, eleventh, twelfth, thirteenth, fourteenth, fifteenth), delegate((AsyncSubject<Unit> subject, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth, T14 fourteenth, T15 fifteenth) state)
			{
				try
				{
					state.action(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2, state.Rest.Item3, state.Rest.Item4, state.Rest.Item5, state.Rest.Item6, state.Rest.Item7, state.Rest.Rest.Item1, state.Rest.Rest.Item2, state.Rest.Rest.Item3);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(Unit.Default);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action)
	{
		return ToAsync(action, SchedulerDefaults.AsyncConversions);
	}

	public virtual Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, IObservable<Unit>> ToAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action, IScheduler scheduler)
	{
		return delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth, T14 fourteenth, T15 fifteenth, T16 sixteenth)
		{
			AsyncSubject<Unit> asyncSubject = new AsyncSubject<Unit>();
			scheduler.ScheduleAction((asyncSubject, action, first, second, third, fourth, fifth, sixth, seventh, eighth, ninth, tenth, eleventh, twelfth, thirteenth, fourteenth, fifteenth, sixteenth), delegate((AsyncSubject<Unit> subject, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action, T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth, T14 fourteenth, T15 fifteenth, T16 sixteenth) state)
			{
				try
				{
					state.action(state.first, state.second, state.third, state.fourth, state.fifth, state.Rest.Item1, state.Rest.Item2, state.Rest.Item3, state.Rest.Item4, state.Rest.Item5, state.Rest.Item6, state.Rest.Item7, state.Rest.Rest.Item1, state.Rest.Rest.Item2, state.Rest.Rest.Item3, state.Rest.Rest.Item4);
				}
				catch (Exception error)
				{
					state.subject.OnError(error);
					return;
				}
				state.subject.OnNext(Unit.Default);
				state.subject.OnCompleted();
			});
			return asyncSubject.AsObservable();
		};
	}

	public virtual AsyncSubject<TSource> GetAwaiter<TSource>(IObservable<TSource> source)
	{
		return RunAsync(source, CancellationToken.None);
	}

	public virtual AsyncSubject<TSource> GetAwaiter<TSource>(IConnectableObservable<TSource> source)
	{
		return RunAsync(source, CancellationToken.None);
	}

	public virtual AsyncSubject<TSource> RunAsync<TSource>(IObservable<TSource> source, CancellationToken cancellationToken)
	{
		AsyncSubject<TSource> asyncSubject = new AsyncSubject<TSource>();
		if (cancellationToken.IsCancellationRequested)
		{
			return Cancel(asyncSubject, cancellationToken);
		}
		IDisposable subscription = source.SubscribeSafe(asyncSubject);
		if (cancellationToken.CanBeCanceled)
		{
			RegisterCancelation(asyncSubject, subscription, cancellationToken);
		}
		return asyncSubject;
	}

	public virtual AsyncSubject<TSource> RunAsync<TSource>(IConnectableObservable<TSource> source, CancellationToken cancellationToken)
	{
		AsyncSubject<TSource> asyncSubject = new AsyncSubject<TSource>();
		if (cancellationToken.IsCancellationRequested)
		{
			return Cancel(asyncSubject, cancellationToken);
		}
		IDisposable disposable = source.SubscribeSafe(asyncSubject);
		IDisposable disposable2 = source.Connect();
		if (cancellationToken.CanBeCanceled)
		{
			RegisterCancelation(asyncSubject, StableCompositeDisposable.Create(disposable, disposable2), cancellationToken);
		}
		return asyncSubject;
	}

	private static AsyncSubject<T> Cancel<T>(AsyncSubject<T> subject, CancellationToken cancellationToken)
	{
		subject.OnError(new OperationCanceledException(cancellationToken));
		return subject;
	}

	private static void RegisterCancelation<T>(AsyncSubject<T> subject, IDisposable subscription, CancellationToken token)
	{
		CancellationTokenRegistration ctr = token.Register(delegate
		{
			subscription.Dispose();
			Cancel(subject, token);
		});
		subject.Subscribe(Stubs<T>.Ignore, delegate
		{
			ctr.Dispose();
		}, ((CancellationTokenRegistration)ctr).Dispose);
	}

	public virtual IConnectableObservable<TResult> Multicast<TSource, TResult>(IObservable<TSource> source, ISubject<TSource, TResult> subject)
	{
		return new ConnectableObservable<TSource, TResult>(source, subject);
	}

	public virtual IObservable<TResult> Multicast<TSource, TIntermediate, TResult>(IObservable<TSource> source, Func<ISubject<TSource, TIntermediate>> subjectSelector, Func<IObservable<TIntermediate>, IObservable<TResult>> selector)
	{
		return new Multicast<TSource, TIntermediate, TResult>(source, subjectSelector, selector);
	}

	public virtual IConnectableObservable<TSource> Publish<TSource>(IObservable<TSource> source)
	{
		return source.Multicast(new Subject<TSource>());
	}

	public virtual IObservable<TResult> Publish<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector)
	{
		return source.Multicast(() => new Subject<TSource>(), selector);
	}

	public virtual IConnectableObservable<TSource> Publish<TSource>(IObservable<TSource> source, TSource initialValue)
	{
		return source.Multicast(new BehaviorSubject<TSource>(initialValue));
	}

	public virtual IObservable<TResult> Publish<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector, TSource initialValue)
	{
		return source.Multicast(() => new BehaviorSubject<TSource>(initialValue), selector);
	}

	public virtual IConnectableObservable<TSource> PublishLast<TSource>(IObservable<TSource> source)
	{
		return source.Multicast(new AsyncSubject<TSource>());
	}

	public virtual IObservable<TResult> PublishLast<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector)
	{
		return source.Multicast(() => new AsyncSubject<TSource>(), selector);
	}

	public virtual IObservable<TSource> RefCount<TSource>(IConnectableObservable<TSource> source)
	{
		return RefCount(source, 1);
	}

	public virtual IObservable<TSource> RefCount<TSource>(IConnectableObservable<TSource> source, TimeSpan disconnectTime)
	{
		return RefCount(source, disconnectTime, Scheduler.Default);
	}

	public virtual IObservable<TSource> RefCount<TSource>(IConnectableObservable<TSource> source, TimeSpan disconnectTime, IScheduler scheduler)
	{
		return RefCount(source, 1, disconnectTime, scheduler);
	}

	public virtual IObservable<TSource> RefCount<TSource>(IConnectableObservable<TSource> source, int minObservers)
	{
		return new RefCount<TSource>.Eager(source, minObservers);
	}

	public virtual IObservable<TSource> RefCount<TSource>(IConnectableObservable<TSource> source, int minObservers, TimeSpan disconnectTime)
	{
		return RefCount(source, minObservers, disconnectTime, Scheduler.Default);
	}

	public virtual IObservable<TSource> RefCount<TSource>(IConnectableObservable<TSource> source, int minObservers, TimeSpan disconnectTime, IScheduler scheduler)
	{
		return new RefCount<TSource>.Lazy(source, disconnectTime, scheduler, minObservers);
	}

	public virtual IObservable<TSource> AutoConnect<TSource>(IConnectableObservable<TSource> source, int minObservers = 1, Action<IDisposable>? onConnect = null)
	{
		if (minObservers <= 0)
		{
			IDisposable obj = source.Connect();
			onConnect?.Invoke(obj);
			return source;
		}
		return new AutoConnect<TSource>(source, minObservers, onConnect);
	}

	public virtual IConnectableObservable<TSource> Replay<TSource>(IObservable<TSource> source)
	{
		return source.Multicast(new ReplaySubject<TSource>());
	}

	public virtual IConnectableObservable<TSource> Replay<TSource>(IObservable<TSource> source, IScheduler scheduler)
	{
		return source.Multicast(new ReplaySubject<TSource>(scheduler));
	}

	public virtual IObservable<TResult> Replay<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector)
	{
		return source.Multicast(() => new ReplaySubject<TSource>(), selector);
	}

	public virtual IObservable<TResult> Replay<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector, IScheduler scheduler)
	{
		return source.Multicast(() => new ReplaySubject<TSource>(scheduler), selector);
	}

	public virtual IConnectableObservable<TSource> Replay<TSource>(IObservable<TSource> source, TimeSpan window)
	{
		return source.Multicast(new ReplaySubject<TSource>(window));
	}

	public virtual IObservable<TResult> Replay<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector, TimeSpan window)
	{
		return source.Multicast(() => new ReplaySubject<TSource>(window), selector);
	}

	public virtual IConnectableObservable<TSource> Replay<TSource>(IObservable<TSource> source, TimeSpan window, IScheduler scheduler)
	{
		return source.Multicast(new ReplaySubject<TSource>(window, scheduler));
	}

	public virtual IObservable<TResult> Replay<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector, TimeSpan window, IScheduler scheduler)
	{
		return source.Multicast(() => new ReplaySubject<TSource>(window, scheduler), selector);
	}

	public virtual IConnectableObservable<TSource> Replay<TSource>(IObservable<TSource> source, int bufferSize, IScheduler scheduler)
	{
		return source.Multicast(new ReplaySubject<TSource>(bufferSize, scheduler));
	}

	public virtual IObservable<TResult> Replay<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector, int bufferSize, IScheduler scheduler)
	{
		return source.Multicast(() => new ReplaySubject<TSource>(bufferSize, scheduler), selector);
	}

	public virtual IConnectableObservable<TSource> Replay<TSource>(IObservable<TSource> source, int bufferSize)
	{
		return source.Multicast(new ReplaySubject<TSource>(bufferSize));
	}

	public virtual IObservable<TResult> Replay<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector, int bufferSize)
	{
		return source.Multicast(() => new ReplaySubject<TSource>(bufferSize), selector);
	}

	public virtual IConnectableObservable<TSource> Replay<TSource>(IObservable<TSource> source, int bufferSize, TimeSpan window)
	{
		return source.Multicast(new ReplaySubject<TSource>(bufferSize, window));
	}

	public virtual IObservable<TResult> Replay<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector, int bufferSize, TimeSpan window)
	{
		return source.Multicast(() => new ReplaySubject<TSource>(bufferSize, window), selector);
	}

	public virtual IConnectableObservable<TSource> Replay<TSource>(IObservable<TSource> source, int bufferSize, TimeSpan window, IScheduler scheduler)
	{
		return source.Multicast(new ReplaySubject<TSource>(bufferSize, window, scheduler));
	}

	public virtual IObservable<TResult> Replay<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector, int bufferSize, TimeSpan window, IScheduler scheduler)
	{
		return source.Multicast(() => new ReplaySubject<TSource>(bufferSize, window, scheduler), selector);
	}

	public virtual IEnumerable<IList<TSource>> Chunkify<TSource>(IObservable<TSource> source)
	{
		return source.Collect(() => new List<TSource>(), delegate(IList<TSource> lst, TSource x)
		{
			lst.Add(x);
			return lst;
		}, (IList<TSource> _) => new List<TSource>());
	}

	public virtual IEnumerable<TResult> Collect<TSource, TResult>(IObservable<TSource> source, Func<TResult> newCollector, Func<TResult, TSource, TResult> merge)
	{
		return Collect_(source, newCollector, merge, (TResult _) => newCollector());
	}

	public virtual IEnumerable<TResult> Collect<TSource, TResult>(IObservable<TSource> source, Func<TResult> getInitialCollector, Func<TResult, TSource, TResult> merge, Func<TResult, TResult> getNewCollector)
	{
		return Collect_(source, getInitialCollector, merge, getNewCollector);
	}

	private static IEnumerable<TResult> Collect_<TSource, TResult>(IObservable<TSource> source, Func<TResult> getInitialCollector, Func<TResult, TSource, TResult> merge, Func<TResult, TResult> getNewCollector)
	{
		return new Collect<TSource, TResult>(source, getInitialCollector, merge, getNewCollector);
	}

	public virtual TSource First<TSource>(IObservable<TSource> source)
	{
		return FirstOrDefaultInternal(source, throwOnEmpty: true);
	}

	public virtual TSource First<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		return First(Where(source, predicate));
	}

	[return: MaybeNull]
	public virtual TSource FirstOrDefault<TSource>(IObservable<TSource> source)
	{
		return FirstOrDefaultInternal(source, throwOnEmpty: false);
	}

	[return: MaybeNull]
	public virtual TSource FirstOrDefault<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		return FirstOrDefault(Where(source, predicate));
	}

	[return: MaybeNull]
	private static TSource FirstOrDefaultInternal<TSource>(IObservable<TSource> source, bool throwOnEmpty)
	{
		using FirstBlocking<TSource> firstBlocking = new FirstBlocking<TSource>();
		using (source.Subscribe(firstBlocking))
		{
			firstBlocking.Wait();
		}
		firstBlocking._error?.Throw();
		if (throwOnEmpty && !firstBlocking._hasValue)
		{
			throw new InvalidOperationException(Strings_Linq.NO_ELEMENTS);
		}
		return firstBlocking._value;
	}

	public virtual void ForEach<TSource>(IObservable<TSource> source, Action<TSource> onNext)
	{
		using ForEach<TSource>.Observer observer = new ForEach<TSource>.Observer(onNext);
		using (source.SubscribeSafe(observer))
		{
			observer.Wait();
		}
		observer.Error?.Throw();
	}

	public virtual void ForEach<TSource>(IObservable<TSource> source, Action<TSource, int> onNext)
	{
		using ForEach<TSource>.ObserverIndexed observerIndexed = new ForEach<TSource>.ObserverIndexed(onNext);
		using (source.SubscribeSafe(observerIndexed))
		{
			observerIndexed.Wait();
		}
		observerIndexed.Error?.Throw();
	}

	public virtual IEnumerator<TSource> GetEnumerator<TSource>(IObservable<TSource> source)
	{
		return new GetEnumerator<TSource>().Run(source);
	}

	public virtual TSource Last<TSource>(IObservable<TSource> source)
	{
		return LastOrDefaultInternal(source, throwOnEmpty: true);
	}

	public virtual TSource Last<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		return Last(Where(source, predicate));
	}

	[return: MaybeNull]
	public virtual TSource LastOrDefault<TSource>(IObservable<TSource> source)
	{
		return LastOrDefaultInternal(source, throwOnEmpty: false);
	}

	[return: MaybeNull]
	public virtual TSource LastOrDefault<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		return LastOrDefault(Where(source, predicate));
	}

	[return: MaybeNull]
	private static TSource LastOrDefaultInternal<TSource>(IObservable<TSource> source, bool throwOnEmpty)
	{
		using LastBlocking<TSource> lastBlocking = new LastBlocking<TSource>();
		using (source.Subscribe(lastBlocking))
		{
			lastBlocking.Wait();
		}
		lastBlocking._error?.Throw();
		if (throwOnEmpty && !lastBlocking._hasValue)
		{
			throw new InvalidOperationException(Strings_Linq.NO_ELEMENTS);
		}
		return lastBlocking._value;
	}

	public virtual IEnumerable<TSource> Latest<TSource>(IObservable<TSource> source)
	{
		return new Latest<TSource>(source);
	}

	public virtual IEnumerable<TSource> MostRecent<TSource>(IObservable<TSource> source, TSource initialValue)
	{
		return new MostRecent<TSource>(source, initialValue);
	}

	public virtual IEnumerable<TSource> Next<TSource>(IObservable<TSource> source)
	{
		return new Next<TSource>(source);
	}

	public virtual TSource Single<TSource>(IObservable<TSource> source)
	{
		return SingleOrDefaultInternal(source, throwOnEmpty: true);
	}

	public virtual TSource Single<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		return Single(Where(source, predicate));
	}

	[return: MaybeNull]
	public virtual TSource SingleOrDefault<TSource>(IObservable<TSource> source)
	{
		return SingleOrDefaultInternal(source, throwOnEmpty: false);
	}

	[return: MaybeNull]
	public virtual TSource SingleOrDefault<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		return SingleOrDefault(Where(source, predicate));
	}

	[return: MaybeNull]
	private static TSource SingleOrDefaultInternal<TSource>(IObservable<TSource> source, bool throwOnEmpty)
	{
		using SingleBlocking<TSource> singleBlocking = new SingleBlocking<TSource>();
		using (source.Subscribe(singleBlocking))
		{
			singleBlocking.Wait();
		}
		singleBlocking._error?.Throw();
		if (singleBlocking._hasMoreThanOneElement)
		{
			throw new InvalidOperationException(Strings_Linq.MORE_THAN_ONE_ELEMENT);
		}
		if (throwOnEmpty && !singleBlocking._hasValue)
		{
			throw new InvalidOperationException(Strings_Linq.NO_ELEMENTS);
		}
		return singleBlocking._value;
	}

	public virtual TSource Wait<TSource>(IObservable<TSource> source)
	{
		return LastOrDefaultInternal(source, throwOnEmpty: true);
	}

	public virtual IObservable<TSource> ObserveOn<TSource>(IObservable<TSource> source, IScheduler scheduler)
	{
		return Synchronization.ObserveOn(source, scheduler);
	}

	public virtual IObservable<TSource> ObserveOn<TSource>(IObservable<TSource> source, SynchronizationContext context)
	{
		return Synchronization.ObserveOn(source, context);
	}

	public virtual IObservable<TSource> SubscribeOn<TSource>(IObservable<TSource> source, IScheduler scheduler)
	{
		return Synchronization.SubscribeOn(source, scheduler);
	}

	public virtual IObservable<TSource> SubscribeOn<TSource>(IObservable<TSource> source, SynchronizationContext context)
	{
		return Synchronization.SubscribeOn(source, context);
	}

	public virtual IObservable<TSource> Synchronize<TSource>(IObservable<TSource> source)
	{
		return Synchronization.Synchronize(source);
	}

	public virtual IObservable<TSource> Synchronize<TSource>(IObservable<TSource> source, object gate)
	{
		return Synchronization.Synchronize(source, gate);
	}

	public virtual IDisposable Subscribe<TSource>(IEnumerable<TSource> source, IObserver<TSource> observer)
	{
		return Subscribe_(source, observer, SchedulerDefaults.Iteration);
	}

	public virtual IDisposable Subscribe<TSource>(IEnumerable<TSource> source, IObserver<TSource> observer, IScheduler scheduler)
	{
		return Subscribe_(source, observer, scheduler);
	}

	private static IDisposable Subscribe_<TSource>(IEnumerable<TSource> source, IObserver<TSource> observer, IScheduler scheduler)
	{
		ISchedulerLongRunning schedulerLongRunning = scheduler.AsLongRunning();
		if (schedulerLongRunning != null)
		{
			return new ToObservableLongRunning<TSource>(source, schedulerLongRunning).Subscribe(observer);
		}
		return new ToObservableRecursive<TSource>(source, scheduler).Subscribe(observer);
	}

	public virtual IEnumerable<TSource> ToEnumerable<TSource>(IObservable<TSource> source)
	{
		return new AnonymousEnumerable<TSource>(() => source.GetEnumerator());
	}

	public virtual IEventSource<Unit> ToEvent(IObservable<Unit> source)
	{
		return new EventSource<Unit>(source, delegate(Action<Unit> h, Unit _)
		{
			h(Unit.Default);
		});
	}

	public virtual IEventSource<TSource> ToEvent<TSource>(IObservable<TSource> source)
	{
		return new EventSource<TSource>(source, delegate(Action<TSource> h, TSource value)
		{
			h(value);
		});
	}

	public virtual IEventPatternSource<TEventArgs> ToEventPattern<TEventArgs>(IObservable<EventPattern<TEventArgs>> source)
	{
		return new EventPatternSource<TEventArgs>(source, delegate(Action<object?, TEventArgs> h, EventPattern<object, TEventArgs> evt)
		{
			h(evt.Sender, evt.EventArgs);
		});
	}

	public virtual IObservable<TSource> ToObservable<TSource>(IEnumerable<TSource> source)
	{
		return ToObservable_(source, SchedulerDefaults.Iteration);
	}

	public virtual IObservable<TSource> ToObservable<TSource>(IEnumerable<TSource> source, IScheduler scheduler)
	{
		return ToObservable_(source, scheduler);
	}

	private static IObservable<TSource> ToObservable_<TSource>(IEnumerable<TSource> source, IScheduler scheduler)
	{
		ISchedulerLongRunning schedulerLongRunning = scheduler.AsLongRunning();
		if (schedulerLongRunning != null)
		{
			return new ToObservableLongRunning<TSource>(source, schedulerLongRunning);
		}
		return new ToObservableRecursive<TSource>(source, scheduler);
	}

	public virtual IObservable<TSource> Create<TSource>(Func<IObserver<TSource>, IDisposable> subscribe)
	{
		return new CreateWithDisposableObservable<TSource>(subscribe);
	}

	public virtual IObservable<TSource> Create<TSource>(Func<IObserver<TSource>, Action> subscribe)
	{
		return new CreateWithActionDisposable<TSource>(subscribe);
	}

	public virtual IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, CancellationToken, Task> subscribeAsync)
	{
		return new CreateWithTaskTokenObservable<TResult>(subscribeAsync);
	}

	public virtual IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, Task> subscribeAsync)
	{
		return Create((IObserver<TResult> observer, CancellationToken token) => subscribeAsync(observer));
	}

	public virtual IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, CancellationToken, Task<IDisposable>> subscribeAsync)
	{
		return new CreateWithTaskDisposable<TResult>(subscribeAsync);
	}

	public virtual IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, Task<IDisposable>> subscribeAsync)
	{
		return Create((IObserver<TResult> observer, CancellationToken token) => subscribeAsync(observer));
	}

	public virtual IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, CancellationToken, Task<Action>> subscribeAsync)
	{
		return new CreateWithTaskActionObservable<TResult>(subscribeAsync);
	}

	public virtual IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, Task<Action>> subscribeAsync)
	{
		return Create((IObserver<TResult> observer, CancellationToken token) => subscribeAsync(observer));
	}

	public virtual IObservable<TValue> Defer<TValue>(Func<IObservable<TValue>> observableFactory)
	{
		return new Defer<TValue>(observableFactory);
	}

	public virtual IObservable<TValue> Defer<TValue>(Func<Task<IObservable<TValue>>> observableFactoryAsync, bool ignoreExceptionsAfterUnsubscribe)
	{
		return Defer(() => StartAsync(observableFactoryAsync, new TaskObservationOptions.Value(null, ignoreExceptionsAfterUnsubscribe)).Merge());
	}

	public virtual IObservable<TValue> Defer<TValue>(Func<CancellationToken, Task<IObservable<TValue>>> observableFactoryAsync, bool ignoreExceptionsAfterUnsubscribe)
	{
		return Defer(() => StartAsync(observableFactoryAsync, new TaskObservationOptions.Value(null, ignoreExceptionsAfterUnsubscribe)).Merge());
	}

	public virtual IObservable<TResult> Empty<TResult>()
	{
		return EmptyDirect<TResult>.Instance;
	}

	public virtual IObservable<TResult> Empty<TResult>(IScheduler scheduler)
	{
		return new Empty<TResult>(scheduler);
	}

	public virtual IObservable<TResult> Generate<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector)
	{
		return new Generate<TState, TResult>.NoTime(initialState, condition, iterate, resultSelector, SchedulerDefaults.Iteration);
	}

	public virtual IObservable<TResult> Generate<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, IScheduler scheduler)
	{
		return new Generate<TState, TResult>.NoTime(initialState, condition, iterate, resultSelector, scheduler);
	}

	public virtual IObservable<TResult> Never<TResult>()
	{
		return System.Reactive.Linq.ObservableImpl.Never<TResult>.Default;
	}

	public virtual IObservable<int> Range(int start, int count)
	{
		return Range_(start, count, SchedulerDefaults.Iteration);
	}

	public virtual IObservable<int> Range(int start, int count, IScheduler scheduler)
	{
		return Range_(start, count, scheduler);
	}

	private static IObservable<int> Range_(int start, int count, IScheduler scheduler)
	{
		ISchedulerLongRunning schedulerLongRunning = scheduler.AsLongRunning();
		if (schedulerLongRunning != null)
		{
			return new RangeLongRunning(start, count, schedulerLongRunning);
		}
		return new RangeRecursive(start, count, scheduler);
	}

	public virtual IObservable<TResult> Repeat<TResult>(TResult value)
	{
		return Repeat_(value, SchedulerDefaults.Iteration);
	}

	public virtual IObservable<TResult> Repeat<TResult>(TResult value, IScheduler scheduler)
	{
		return Repeat_(value, scheduler);
	}

	private static IObservable<TResult> Repeat_<TResult>(TResult value, IScheduler scheduler)
	{
		ISchedulerLongRunning schedulerLongRunning = scheduler.AsLongRunning();
		if (schedulerLongRunning != null)
		{
			return new Repeat<TResult>.ForeverLongRunning(value, schedulerLongRunning);
		}
		return new Repeat<TResult>.ForeverRecursive(value, scheduler);
	}

	public virtual IObservable<TResult> Repeat<TResult>(TResult value, int repeatCount)
	{
		return Repeat_(value, repeatCount, SchedulerDefaults.Iteration);
	}

	public virtual IObservable<TResult> Repeat<TResult>(TResult value, int repeatCount, IScheduler scheduler)
	{
		return Repeat_(value, repeatCount, scheduler);
	}

	private static IObservable<TResult> Repeat_<TResult>(TResult value, int repeatCount, IScheduler scheduler)
	{
		ISchedulerLongRunning schedulerLongRunning = scheduler.AsLongRunning();
		if (schedulerLongRunning != null)
		{
			return new Repeat<TResult>.CountLongRunning(value, repeatCount, schedulerLongRunning);
		}
		return new Repeat<TResult>.CountRecursive(value, repeatCount, scheduler);
	}

	public virtual IObservable<TResult> Return<TResult>(TResult value)
	{
		return Return(value, SchedulerDefaults.ConstantTimeOperations);
	}

	public virtual IObservable<TResult> Return<TResult>(TResult value, IScheduler scheduler)
	{
		if (scheduler == ImmediateScheduler.Instance)
		{
			return new ReturnImmediate<TResult>(value);
		}
		return new Return<TResult>(value, scheduler);
	}

	public virtual IObservable<TResult> Throw<TResult>(Exception exception)
	{
		return Throw<TResult>(exception, SchedulerDefaults.ConstantTimeOperations);
	}

	public virtual IObservable<TResult> Throw<TResult>(Exception exception, IScheduler scheduler)
	{
		if (scheduler == ImmediateScheduler.Instance)
		{
			return new ThrowImmediate<TResult>(exception);
		}
		return new Throw<TResult>(exception, scheduler);
	}

	public virtual IObservable<TSource> Using<TSource, TResource>(Func<TResource> resourceFactory, Func<TResource, IObservable<TSource>> observableFactory) where TResource : IDisposable
	{
		return new Using<TSource, TResource>(resourceFactory, observableFactory);
	}

	public virtual IObservable<TSource> Using<TSource, TResource>(Func<CancellationToken, Task<TResource>> resourceFactoryAsync, Func<TResource, CancellationToken, Task<IObservable<TSource>>> observableFactoryAsync) where TResource : IDisposable
	{
		return Observable.FromAsync(resourceFactoryAsync).SelectMany((TResource resource) => Observable.Using(() => resource, (TResource resource_) => Observable.FromAsync((CancellationToken ct) => observableFactoryAsync(resource_, ct)).Merge()));
	}

	public virtual IObservable<EventPattern<object>> FromEventPattern(Action<EventHandler> addHandler, Action<EventHandler> removeHandler)
	{
		return FromEventPattern_(addHandler, removeHandler, GetSchedulerForCurrentContext());
	}

	public virtual IObservable<EventPattern<object>> FromEventPattern(Action<EventHandler> addHandler, Action<EventHandler> removeHandler, IScheduler scheduler)
	{
		return FromEventPattern_(addHandler, removeHandler, scheduler);
	}

	private static IObservable<EventPattern<object>> FromEventPattern_(Action<EventHandler> addHandler, Action<EventHandler> removeHandler, IScheduler scheduler)
	{
		return new FromEventPattern.Impl<EventHandler, object>((EventHandler<object> e) => e.Invoke, addHandler, removeHandler, scheduler);
	}

	public virtual IObservable<EventPattern<TEventArgs>> FromEventPattern<TDelegate, TEventArgs>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
	{
		return FromEventPattern_<TDelegate, TEventArgs>(addHandler, removeHandler, GetSchedulerForCurrentContext());
	}

	public virtual IObservable<EventPattern<TEventArgs>> FromEventPattern<TDelegate, TEventArgs>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
	{
		return FromEventPattern_<TDelegate, TEventArgs>(addHandler, removeHandler, scheduler);
	}

	private static IObservable<EventPattern<TEventArgs>> FromEventPattern_<TDelegate, TEventArgs>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
	{
		return new FromEventPattern.Impl<TDelegate, TEventArgs>(addHandler, removeHandler, scheduler);
	}

	public virtual IObservable<EventPattern<TEventArgs>> FromEventPattern<TDelegate, TEventArgs>(Func<EventHandler<TEventArgs>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
	{
		return FromEventPattern_(conversion, addHandler, removeHandler, GetSchedulerForCurrentContext());
	}

	public virtual IObservable<EventPattern<TEventArgs>> FromEventPattern<TDelegate, TEventArgs>(Func<EventHandler<TEventArgs>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
	{
		return FromEventPattern_(conversion, addHandler, removeHandler, scheduler);
	}

	private static IObservable<EventPattern<TEventArgs>> FromEventPattern_<TDelegate, TEventArgs>(Func<EventHandler<TEventArgs>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
	{
		return new FromEventPattern.Impl<TDelegate, TEventArgs>(conversion, addHandler, removeHandler, scheduler);
	}

	public virtual IObservable<EventPattern<TSender, TEventArgs>> FromEventPattern<TDelegate, TSender, TEventArgs>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
	{
		return FromEventPattern_<TDelegate, TSender, TEventArgs>(addHandler, removeHandler, GetSchedulerForCurrentContext());
	}

	public virtual IObservable<EventPattern<TSender, TEventArgs>> FromEventPattern<TDelegate, TSender, TEventArgs>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
	{
		return FromEventPattern_<TDelegate, TSender, TEventArgs>(addHandler, removeHandler, scheduler);
	}

	private static IObservable<EventPattern<TSender, TEventArgs>> FromEventPattern_<TDelegate, TSender, TEventArgs>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
	{
		return new FromEventPattern.Impl<TDelegate, TSender, TEventArgs>(addHandler, removeHandler, scheduler);
	}

	public virtual IObservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(Action<EventHandler<TEventArgs>> addHandler, Action<EventHandler<TEventArgs>> removeHandler)
	{
		return FromEventPattern_(addHandler, removeHandler, GetSchedulerForCurrentContext());
	}

	public virtual IObservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(Action<EventHandler<TEventArgs>> addHandler, Action<EventHandler<TEventArgs>> removeHandler, IScheduler scheduler)
	{
		return FromEventPattern_(addHandler, removeHandler, scheduler);
	}

	private static IObservable<EventPattern<TEventArgs>> FromEventPattern_<TEventArgs>(Action<EventHandler<TEventArgs>> addHandler, Action<EventHandler<TEventArgs>> removeHandler, IScheduler scheduler)
	{
		return new FromEventPattern.Impl<EventHandler<TEventArgs>, TEventArgs>((EventHandler<TEventArgs> handler) => handler, addHandler, removeHandler, scheduler);
	}

	public virtual IObservable<EventPattern<object>> FromEventPattern(object target, string eventName)
	{
		return FromEventPattern_(target, eventName, GetSchedulerForCurrentContext());
	}

	public virtual IObservable<EventPattern<object>> FromEventPattern(object target, string eventName, IScheduler scheduler)
	{
		return FromEventPattern_(target, eventName, scheduler);
	}

	private static IObservable<EventPattern<object>> FromEventPattern_(object target, string eventName, IScheduler scheduler)
	{
		return FromEventPattern_(target.GetType(), target, eventName, (object sender, object args) => new EventPattern<object>(sender, args), scheduler);
	}

	public virtual IObservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(object target, string eventName)
	{
		return FromEventPattern_<TEventArgs>(target, eventName, GetSchedulerForCurrentContext());
	}

	public virtual IObservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(object target, string eventName, IScheduler scheduler)
	{
		return FromEventPattern_<TEventArgs>(target, eventName, scheduler);
	}

	private static IObservable<EventPattern<TEventArgs>> FromEventPattern_<TEventArgs>(object target, string eventName, IScheduler scheduler)
	{
		return FromEventPattern_(target.GetType(), target, eventName, (object sender, TEventArgs args) => new EventPattern<TEventArgs>(sender, args), scheduler);
	}

	public virtual IObservable<EventPattern<TSender, TEventArgs>> FromEventPattern<TSender, TEventArgs>(object target, string eventName)
	{
		return FromEventPattern_<TSender, TEventArgs>(target, eventName, GetSchedulerForCurrentContext());
	}

	public virtual IObservable<EventPattern<TSender, TEventArgs>> FromEventPattern<TSender, TEventArgs>(object target, string eventName, IScheduler scheduler)
	{
		return FromEventPattern_<TSender, TEventArgs>(target, eventName, scheduler);
	}

	private static IObservable<EventPattern<TSender, TEventArgs>> FromEventPattern_<TSender, TEventArgs>(object target, string eventName, IScheduler scheduler)
	{
		return FromEventPattern_(target.GetType(), target, eventName, (TSender sender, TEventArgs args) => new EventPattern<TSender, TEventArgs>(sender, args), scheduler);
	}

	public virtual IObservable<EventPattern<object>> FromEventPattern(Type type, string eventName)
	{
		return FromEventPattern_(type, eventName, GetSchedulerForCurrentContext());
	}

	public virtual IObservable<EventPattern<object>> FromEventPattern(Type type, string eventName, IScheduler scheduler)
	{
		return FromEventPattern_(type, eventName, scheduler);
	}

	private static IObservable<EventPattern<object>> FromEventPattern_(Type type, string eventName, IScheduler scheduler)
	{
		return FromEventPattern_(type, null, eventName, (object sender, object args) => new EventPattern<object>(sender, args), scheduler);
	}

	public virtual IObservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(Type type, string eventName)
	{
		return FromEventPattern_<TEventArgs>(type, eventName, GetSchedulerForCurrentContext());
	}

	public virtual IObservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(Type type, string eventName, IScheduler scheduler)
	{
		return FromEventPattern_<TEventArgs>(type, eventName, scheduler);
	}

	private static IObservable<EventPattern<TEventArgs>> FromEventPattern_<TEventArgs>(Type type, string eventName, IScheduler scheduler)
	{
		return FromEventPattern_(type, null, eventName, (object sender, TEventArgs args) => new EventPattern<TEventArgs>(sender, args), scheduler);
	}

	public virtual IObservable<EventPattern<TSender, TEventArgs>> FromEventPattern<TSender, TEventArgs>(Type type, string eventName)
	{
		return FromEventPattern_<TSender, TEventArgs>(type, eventName, GetSchedulerForCurrentContext());
	}

	public virtual IObservable<EventPattern<TSender, TEventArgs>> FromEventPattern<TSender, TEventArgs>(Type type, string eventName, IScheduler scheduler)
	{
		return FromEventPattern_<TSender, TEventArgs>(type, eventName, scheduler);
	}

	private static IObservable<EventPattern<TSender, TEventArgs>> FromEventPattern_<TSender, TEventArgs>(Type type, string eventName, IScheduler scheduler)
	{
		return FromEventPattern_(type, null, eventName, (TSender sender, TEventArgs args) => new EventPattern<TSender, TEventArgs>(sender, args), scheduler);
	}

	private static IObservable<TResult> FromEventPattern_<TSender, TEventArgs, TResult>(Type targetType, object? target, string eventName, Func<TSender, TEventArgs, TResult> getResult, IScheduler scheduler)
	{
		ReflectionUtils.GetEventMethods<TSender, TEventArgs>(targetType, target, eventName, out MethodInfo addMethod, out MethodInfo removeMethod, out Type delegateType, out bool isWinRT);
		return new FromEventPattern.Handler<TSender, TEventArgs, TResult>(target, delegateType, addMethod, removeMethod, getResult, isWinRT, scheduler);
	}

	public virtual IObservable<TEventArgs> FromEvent<TDelegate, TEventArgs>(Func<Action<TEventArgs>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
	{
		return FromEvent_(conversion, addHandler, removeHandler, GetSchedulerForCurrentContext());
	}

	public virtual IObservable<TEventArgs> FromEvent<TDelegate, TEventArgs>(Func<Action<TEventArgs>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
	{
		return FromEvent_(conversion, addHandler, removeHandler, scheduler);
	}

	private static IObservable<TEventArgs> FromEvent_<TDelegate, TEventArgs>(Func<Action<TEventArgs>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
	{
		return new FromEvent<TDelegate, TEventArgs>(conversion, addHandler, removeHandler, scheduler);
	}

	public virtual IObservable<TEventArgs> FromEvent<TDelegate, TEventArgs>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
	{
		return FromEvent_<TDelegate, TEventArgs>(addHandler, removeHandler, GetSchedulerForCurrentContext());
	}

	public virtual IObservable<TEventArgs> FromEvent<TDelegate, TEventArgs>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
	{
		return FromEvent_<TDelegate, TEventArgs>(addHandler, removeHandler, scheduler);
	}

	private static IObservable<TEventArgs> FromEvent_<TDelegate, TEventArgs>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
	{
		return new FromEvent<TDelegate, TEventArgs>(addHandler, removeHandler, scheduler);
	}

	public virtual IObservable<TEventArgs> FromEvent<TEventArgs>(Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler)
	{
		return FromEvent_(addHandler, removeHandler, GetSchedulerForCurrentContext());
	}

	public virtual IObservable<TEventArgs> FromEvent<TEventArgs>(Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler, IScheduler scheduler)
	{
		return FromEvent_(addHandler, removeHandler, scheduler);
	}

	private static IObservable<TEventArgs> FromEvent_<TEventArgs>(Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler, IScheduler scheduler)
	{
		return new FromEvent<Action<TEventArgs>, TEventArgs>((Action<TEventArgs> h) => h, addHandler, removeHandler, scheduler);
	}

	public virtual IObservable<Unit> FromEvent(Action<Action> addHandler, Action<Action> removeHandler)
	{
		return FromEvent_(addHandler, removeHandler, GetSchedulerForCurrentContext());
	}

	public virtual IObservable<Unit> FromEvent(Action<Action> addHandler, Action<Action> removeHandler, IScheduler scheduler)
	{
		return FromEvent_(addHandler, removeHandler, scheduler);
	}

	private static IObservable<Unit> FromEvent_(Action<Action> addHandler, Action<Action> removeHandler, IScheduler scheduler)
	{
		return new FromEvent<Action, Unit>((Action<Unit> h) => delegate
		{
			h(default(Unit));
		}, addHandler, removeHandler, scheduler);
	}

	private static IScheduler GetSchedulerForCurrentContext()
	{
		SynchronizationContext current = SynchronizationContext.Current;
		if (current != null)
		{
			return new SynchronizationContextScheduler(current, alwaysPost: false);
		}
		return SchedulerDefaults.ConstantTimeOperations;
	}

	public virtual Task ForEachAsync<TSource>(IObservable<TSource> source, Action<TSource> onNext)
	{
		return ForEachAsync_(source, onNext, CancellationToken.None);
	}

	public virtual Task ForEachAsync<TSource>(IObservable<TSource> source, Action<TSource> onNext, CancellationToken cancellationToken)
	{
		return ForEachAsync_(source, onNext, cancellationToken);
	}

	public virtual Task ForEachAsync<TSource>(IObservable<TSource> source, Action<TSource, int> onNext)
	{
		int i = 0;
		return ForEachAsync_(source, delegate(TSource x)
		{
			onNext(x, checked(i++));
		}, CancellationToken.None);
	}

	public virtual Task ForEachAsync<TSource>(IObservable<TSource> source, Action<TSource, int> onNext, CancellationToken cancellationToken)
	{
		int i = 0;
		return ForEachAsync_(source, delegate(TSource x)
		{
			onNext(x, checked(i++));
		}, cancellationToken);
	}

	private static Task ForEachAsync_<TSource>(IObservable<TSource> source, Action<TSource> onNext, CancellationToken cancellationToken)
	{
		TaskCompletionSource<object?> tcs = new TaskCompletionSource<object>();
		SingleAssignmentDisposable subscription = new SingleAssignmentDisposable();
		CancellationTokenRegistration ctr = default(CancellationTokenRegistration);
		if (cancellationToken.CanBeCanceled)
		{
			ctr = cancellationToken.Register(delegate
			{
				tcs.TrySetCanceled(cancellationToken);
				subscription.Dispose();
			});
		}
		if (!cancellationToken.IsCancellationRequested)
		{
			Action<Action> dispose = delegate(Action action)
			{
				try
				{
					ctr.Dispose();
					subscription.Dispose();
				}
				catch (Exception exception2)
				{
					tcs.TrySetException(exception2);
					return;
				}
				action();
			};
			AnonymousObserver<TSource> observer = new AnonymousObserver<TSource>(delegate(TSource x)
			{
				if (!subscription.IsDisposed)
				{
					try
					{
						onNext(x);
					}
					catch (Exception ex)
					{
						Exception ex2 = ex;
						Exception exception2 = ex2;
						dispose(delegate
						{
							tcs.TrySetException(exception2);
						});
					}
				}
			}, delegate(Exception exception2)
			{
				dispose(delegate
				{
					tcs.TrySetException(exception2);
				});
			}, delegate
			{
				dispose(delegate
				{
					tcs.TrySetResult(null);
				});
			});
			try
			{
				subscription.Disposable = source.Subscribe(observer);
			}
			catch (Exception exception)
			{
				tcs.TrySetException(exception);
			}
		}
		return tcs.Task;
	}

	public virtual IObservable<TResult> Case<TValue, TResult>(Func<TValue> selector, IDictionary<TValue, IObservable<TResult>> sources) where TValue : notnull
	{
		return Case(selector, sources, Empty<TResult>());
	}

	public virtual IObservable<TResult> Case<TValue, TResult>(Func<TValue> selector, IDictionary<TValue, IObservable<TResult>> sources, IScheduler scheduler) where TValue : notnull
	{
		return Case(selector, sources, Empty<TResult>(scheduler));
	}

	public virtual IObservable<TResult> Case<TValue, TResult>(Func<TValue> selector, IDictionary<TValue, IObservable<TResult>> sources, IObservable<TResult> defaultSource) where TValue : notnull
	{
		return new Case<TValue, TResult>(selector, sources, defaultSource);
	}

	public virtual IObservable<TSource> DoWhile<TSource>(IObservable<TSource> source, Func<bool> condition)
	{
		return new DoWhile<TSource>(source, condition);
	}

	public virtual IObservable<TResult> For<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, IObservable<TResult>> resultSelector)
	{
		return new For<TSource, TResult>(source, resultSelector);
	}

	public virtual IObservable<TResult> If<TResult>(Func<bool> condition, IObservable<TResult> thenSource)
	{
		return If(condition, thenSource, Empty<TResult>());
	}

	public virtual IObservable<TResult> If<TResult>(Func<bool> condition, IObservable<TResult> thenSource, IScheduler scheduler)
	{
		return If(condition, thenSource, Empty<TResult>(scheduler));
	}

	public virtual IObservable<TResult> If<TResult>(Func<bool> condition, IObservable<TResult> thenSource, IObservable<TResult> elseSource)
	{
		return new If<TResult>(condition, thenSource, elseSource);
	}

	public virtual IObservable<TSource> While<TSource>(Func<bool> condition, IObservable<TSource> source)
	{
		return new While<TSource>(condition, source);
	}

	public virtual Pattern<TLeft, TRight> And<TLeft, TRight>(IObservable<TLeft> left, IObservable<TRight> right)
	{
		return new Pattern<TLeft, TRight>(left, right);
	}

	public virtual Plan<TResult> Then<TSource, TResult>(IObservable<TSource> source, Func<TSource, TResult> selector)
	{
		return new Pattern<TSource>(source).Then(selector);
	}

	public virtual IObservable<TResult> When<TResult>(params Plan<TResult>[] plans)
	{
		return When((IEnumerable<Plan<TResult>>)plans);
	}

	public virtual IObservable<TResult> When<TResult>(IEnumerable<Plan<TResult>> plans)
	{
		return new WhenObservable<TResult>(plans);
	}

	public virtual IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, Func<TSource1, TSource2, TSource3, TResult> resultSelector)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TResult>(source1, source2, source3, resultSelector);
	}

	public virtual IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, Func<TSource1, TSource2, TSource3, TSource4, TResult> resultSelector)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TSource4, TResult>(source1, source2, source3, source4, resultSelector);
	}

	public virtual IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TResult> resultSelector)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(source1, source2, source3, source4, source5, resultSelector);
	}

	public virtual IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult> resultSelector)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(source1, source2, source3, source4, source5, source6, resultSelector);
	}

	public virtual IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult> resultSelector)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(source1, source2, source3, source4, source5, source6, source7, resultSelector);
	}

	public virtual IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult> resultSelector)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, resultSelector);
	}

	public virtual IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult> resultSelector)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, resultSelector);
	}

	public virtual IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult> resultSelector)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, resultSelector);
	}

	public virtual IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult> resultSelector)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, resultSelector);
	}

	public virtual IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult> resultSelector)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, resultSelector);
	}

	public virtual IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult> resultSelector)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, resultSelector);
	}

	public virtual IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, IObservable<TSource14> source14, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TResult> resultSelector)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, source14, resultSelector);
	}

	public virtual IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, IObservable<TSource14> source14, IObservable<TSource15> source15, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TResult> resultSelector)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, source14, source15, resultSelector);
	}

	public virtual IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, IObservable<TSource14> source14, IObservable<TSource15> source15, IObservable<TSource16> source16, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16, TResult> resultSelector)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, source14, source15, source16, resultSelector);
	}

	public virtual IObservable<TSource> Amb<TSource>(IObservable<TSource> first, IObservable<TSource> second)
	{
		return new Amb<TSource>(first, second);
	}

	public virtual IObservable<TSource> Amb<TSource>(params IObservable<TSource>[] sources)
	{
		return new AmbManyArray<TSource>(sources);
	}

	public virtual IObservable<TSource> Amb<TSource>(IEnumerable<IObservable<TSource>> sources)
	{
		return new AmbManyEnumerable<TSource>(sources);
	}

	public virtual IObservable<IList<TSource>> Buffer<TSource, TBufferClosing>(IObservable<TSource> source, Func<IObservable<TBufferClosing>> bufferClosingSelector)
	{
		return new Buffer<TSource, TBufferClosing>.Selector(source, bufferClosingSelector);
	}

	public virtual IObservable<IList<TSource>> Buffer<TSource, TBufferOpening, TBufferClosing>(IObservable<TSource> source, IObservable<TBufferOpening> bufferOpenings, Func<TBufferOpening, IObservable<TBufferClosing>> bufferClosingSelector)
	{
		return source.Window(bufferOpenings, bufferClosingSelector).SelectMany(ToList);
	}

	public virtual IObservable<IList<TSource>> Buffer<TSource, TBufferBoundary>(IObservable<TSource> source, IObservable<TBufferBoundary> bufferBoundaries)
	{
		return new Buffer<TSource, TBufferBoundary>.Boundaries(source, bufferBoundaries);
	}

	public virtual IObservable<TSource> Catch<TSource, TException>(IObservable<TSource> source, Func<TException, IObservable<TSource>> handler) where TException : Exception
	{
		return new Catch<TSource, TException>(source, handler);
	}

	public virtual IObservable<TSource> Catch<TSource>(IObservable<TSource> first, IObservable<TSource> second)
	{
		return Catch_(new IObservable<TSource>[2] { first, second });
	}

	public virtual IObservable<TSource> Catch<TSource>(params IObservable<TSource>[] sources)
	{
		return Catch_(sources);
	}

	public virtual IObservable<TSource> Catch<TSource>(IEnumerable<IObservable<TSource>> sources)
	{
		return Catch_(sources);
	}

	private static IObservable<TSource> Catch_<TSource>(IEnumerable<IObservable<TSource>> sources)
	{
		return new Catch<TSource>(sources);
	}

	public virtual IObservable<TResult> CombineLatest<TFirst, TSecond, TResult>(IObservable<TFirst> first, IObservable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
	{
		return new CombineLatest<TFirst, TSecond, TResult>(first, second, resultSelector);
	}

	public virtual IObservable<TResult> CombineLatest<TSource, TResult>(IEnumerable<IObservable<TSource>> sources, Func<IList<TSource>, TResult> resultSelector)
	{
		return CombineLatest_(sources, resultSelector);
	}

	public virtual IObservable<IList<TSource>> CombineLatest<TSource>(IEnumerable<IObservable<TSource>> sources)
	{
		return CombineLatest_(sources, (IList<TSource> res) => res.ToList());
	}

	public virtual IObservable<IList<TSource>> CombineLatest<TSource>(params IObservable<TSource>[] sources)
	{
		return CombineLatest_(sources, (IList<TSource> res) => res.ToList());
	}

	private static IObservable<TResult> CombineLatest_<TSource, TResult>(IEnumerable<IObservable<TSource>> sources, Func<IList<TSource>, TResult> resultSelector)
	{
		return new CombineLatest<TSource, TResult>(sources, resultSelector);
	}

	public virtual IObservable<TSource> Concat<TSource>(IObservable<TSource> first, IObservable<TSource> second)
	{
		return Concat_(new IObservable<TSource>[2] { first, second });
	}

	public virtual IObservable<TSource> Concat<TSource>(params IObservable<TSource>[] sources)
	{
		return Concat_(sources);
	}

	public virtual IObservable<TSource> Concat<TSource>(IEnumerable<IObservable<TSource>> sources)
	{
		return Concat_(sources);
	}

	private static IObservable<TSource> Concat_<TSource>(IEnumerable<IObservable<TSource>> sources)
	{
		return new Concat<TSource>(sources);
	}

	public virtual IObservable<TSource> Concat<TSource>(IObservable<IObservable<TSource>> sources)
	{
		return Concat_(sources);
	}

	public virtual IObservable<TSource> Concat<TSource>(IObservable<Task<TSource>> sources)
	{
		return Concat_(Select(sources, TaskObservableExtensions.ToObservable));
	}

	private static IObservable<TSource> Concat_<TSource>(IObservable<IObservable<TSource>> sources)
	{
		return new ConcatMany<TSource>(sources);
	}

	public virtual IObservable<TSource> Merge<TSource>(IObservable<IObservable<TSource>> sources)
	{
		return Merge_(sources);
	}

	public virtual IObservable<TSource> Merge<TSource>(IObservable<Task<TSource>> sources)
	{
		return new Merge<TSource>.Tasks(sources);
	}

	public virtual IObservable<TSource> Merge<TSource>(IObservable<IObservable<TSource>> sources, int maxConcurrent)
	{
		return Merge_(sources, maxConcurrent);
	}

	public virtual IObservable<TSource> Merge<TSource>(IEnumerable<IObservable<TSource>> sources, int maxConcurrent)
	{
		return Merge_(sources.ToObservable(SchedulerDefaults.ConstantTimeOperations), maxConcurrent);
	}

	public virtual IObservable<TSource> Merge<TSource>(IEnumerable<IObservable<TSource>> sources, int maxConcurrent, IScheduler scheduler)
	{
		return Merge_(sources.ToObservable(scheduler), maxConcurrent);
	}

	public virtual IObservable<TSource> Merge<TSource>(IObservable<TSource> first, IObservable<TSource> second)
	{
		return Merge_(new IObservable<TSource>[2] { first, second }.ToObservable(SchedulerDefaults.ConstantTimeOperations));
	}

	public virtual IObservable<TSource> Merge<TSource>(IObservable<TSource> first, IObservable<TSource> second, IScheduler scheduler)
	{
		return Merge_(new IObservable<TSource>[2] { first, second }.ToObservable(scheduler));
	}

	public virtual IObservable<TSource> Merge<TSource>(params IObservable<TSource>[] sources)
	{
		return Merge_(sources.ToObservable(SchedulerDefaults.ConstantTimeOperations));
	}

	public virtual IObservable<TSource> Merge<TSource>(IScheduler scheduler, params IObservable<TSource>[] sources)
	{
		return Merge_(sources.ToObservable(scheduler));
	}

	public virtual IObservable<TSource> Merge<TSource>(IEnumerable<IObservable<TSource>> sources)
	{
		return Merge_(sources.ToObservable(SchedulerDefaults.ConstantTimeOperations));
	}

	public virtual IObservable<TSource> Merge<TSource>(IEnumerable<IObservable<TSource>> sources, IScheduler scheduler)
	{
		return Merge_(sources.ToObservable(scheduler));
	}

	private static IObservable<TSource> Merge_<TSource>(IObservable<IObservable<TSource>> sources)
	{
		return new Merge<TSource>.Observables(sources);
	}

	private static IObservable<TSource> Merge_<TSource>(IObservable<IObservable<TSource>> sources, int maxConcurrent)
	{
		return new Merge<TSource>.ObservablesMaxConcurrency(sources, maxConcurrent);
	}

	public virtual IObservable<TSource> OnErrorResumeNext<TSource>(IObservable<TSource> first, IObservable<TSource> second)
	{
		return OnErrorResumeNext_(new IObservable<TSource>[2] { first, second });
	}

	public virtual IObservable<TSource> OnErrorResumeNext<TSource>(params IObservable<TSource>[] sources)
	{
		return OnErrorResumeNext_(sources);
	}

	public virtual IObservable<TSource> OnErrorResumeNext<TSource>(IEnumerable<IObservable<TSource>> sources)
	{
		return OnErrorResumeNext_(sources);
	}

	private static IObservable<TSource> OnErrorResumeNext_<TSource>(IEnumerable<IObservable<TSource>> sources)
	{
		return new OnErrorResumeNext<TSource>(sources);
	}

	public virtual IObservable<TSource> SkipUntil<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
	{
		return new SkipUntil<TSource, TOther>(source, other);
	}

	public virtual IObservable<TSource> Switch<TSource>(IObservable<IObservable<TSource>> sources)
	{
		return Switch_(sources);
	}

	public virtual IObservable<TSource> Switch<TSource>(IObservable<Task<TSource>> sources)
	{
		return Switch_(Select(sources, TaskObservableExtensions.ToObservable));
	}

	private static IObservable<TSource> Switch_<TSource>(IObservable<IObservable<TSource>> sources)
	{
		return new Switch<TSource>(sources);
	}

	public virtual IObservable<TSource> TakeUntil<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
	{
		return new TakeUntil<TSource, TOther>(source, other);
	}

	public virtual IObservable<TSource> TakeUntil<TSource>(IObservable<TSource> source, Func<TSource, bool> stopPredicate)
	{
		return new TakeUntilPredicate<TSource>(source, stopPredicate);
	}

	public virtual IObservable<TSource> TakeUntil<TSource>(IObservable<TSource> source, CancellationToken cancellationToken)
	{
		return new TakeUntilCancellationToken<TSource>(source, cancellationToken);
	}

	public virtual IObservable<IObservable<TSource>> Window<TSource, TWindowClosing>(IObservable<TSource> source, Func<IObservable<TWindowClosing>> windowClosingSelector)
	{
		return new Window<TSource, TWindowClosing>.Selector(source, windowClosingSelector);
	}

	public virtual IObservable<IObservable<TSource>> Window<TSource, TWindowOpening, TWindowClosing>(IObservable<TSource> source, IObservable<TWindowOpening> windowOpenings, Func<TWindowOpening, IObservable<TWindowClosing>> windowClosingSelector)
	{
		return windowOpenings.GroupJoin(source, windowClosingSelector, (TSource _) => Observable.Empty<Unit>(), (TWindowOpening _, IObservable<TSource> window) => window);
	}

	public virtual IObservable<IObservable<TSource>> Window<TSource, TWindowBoundary>(IObservable<TSource> source, IObservable<TWindowBoundary> windowBoundaries)
	{
		return new Window<TSource, TWindowBoundary>.Boundaries(source, windowBoundaries);
	}

	public virtual IObservable<TResult> WithLatestFrom<TFirst, TSecond, TResult>(IObservable<TFirst> first, IObservable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
	{
		return new WithLatestFrom<TFirst, TSecond, TResult>(first, second, resultSelector);
	}

	public virtual IObservable<TResult> Zip<TFirst, TSecond, TResult>(IObservable<TFirst> first, IObservable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
	{
		return new Zip<TFirst, TSecond, TResult>.Observable(first, second, resultSelector);
	}

	public virtual IObservable<TResult> Zip<TSource, TResult>(IEnumerable<IObservable<TSource>> sources, Func<IList<TSource>, TResult> resultSelector)
	{
		return Zip_(sources).Select<IList<TSource>, TResult>(resultSelector);
	}

	public virtual IObservable<IList<TSource>> Zip<TSource>(IEnumerable<IObservable<TSource>> sources)
	{
		return Zip_(sources);
	}

	public virtual IObservable<IList<TSource>> Zip<TSource>(params IObservable<TSource>[] sources)
	{
		return Zip_(sources);
	}

	private static IObservable<IList<TSource>> Zip_<TSource>(IEnumerable<IObservable<TSource>> sources)
	{
		return new Zip<TSource>(sources);
	}

	public virtual IObservable<TResult> Zip<TFirst, TSecond, TResult>(IObservable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
	{
		return new Zip<TFirst, TSecond, TResult>.Enumerable(first, second, resultSelector);
	}

	public virtual IObservable<TResult> Zip<TSource1, TSource2, TSource3, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, Func<TSource1, TSource2, TSource3, TResult> resultSelector)
	{
		return new Zip<TSource1, TSource2, TSource3, TResult>(source1, source2, source3, resultSelector);
	}

	public virtual IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, Func<TSource1, TSource2, TSource3, TSource4, TResult> resultSelector)
	{
		return new Zip<TSource1, TSource2, TSource3, TSource4, TResult>(source1, source2, source3, source4, resultSelector);
	}

	public virtual IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TResult> resultSelector)
	{
		return new Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(source1, source2, source3, source4, source5, resultSelector);
	}

	public virtual IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult> resultSelector)
	{
		return new Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(source1, source2, source3, source4, source5, source6, resultSelector);
	}

	public virtual IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult> resultSelector)
	{
		return new Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(source1, source2, source3, source4, source5, source6, source7, resultSelector);
	}

	public virtual IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult> resultSelector)
	{
		return new Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, resultSelector);
	}

	public virtual IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult> resultSelector)
	{
		return new Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, resultSelector);
	}

	public virtual IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult> resultSelector)
	{
		return new Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, resultSelector);
	}

	public virtual IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult> resultSelector)
	{
		return new Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, resultSelector);
	}

	public virtual IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult> resultSelector)
	{
		return new Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, resultSelector);
	}

	public virtual IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult> resultSelector)
	{
		return new Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, resultSelector);
	}

	public virtual IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, IObservable<TSource14> source14, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TResult> resultSelector)
	{
		return new Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, source14, resultSelector);
	}

	public virtual IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, IObservable<TSource14> source14, IObservable<TSource15> source15, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TResult> resultSelector)
	{
		return new Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, source14, source15, resultSelector);
	}

	public virtual IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16, TResult>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, IObservable<TSource14> source14, IObservable<TSource15> source15, IObservable<TSource16> source16, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16, TResult> resultSelector)
	{
		return new Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16, TResult>(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, source14, source15, source16, resultSelector);
	}

	public virtual IObservable<TSource> Append<TSource>(IObservable<TSource> source, TSource value)
	{
		return Append_(source, value, SchedulerDefaults.ConstantTimeOperations);
	}

	public virtual IObservable<TSource> Append<TSource>(IObservable<TSource> source, TSource value, IScheduler scheduler)
	{
		return Append_(source, value, scheduler);
	}

	private static IObservable<TSource> Append_<TSource>(IObservable<TSource> source, TSource value, IScheduler scheduler)
	{
		if (source is AppendPrepend<TSource>.IAppendPrepend appendPrepend && appendPrepend.Scheduler == scheduler)
		{
			return appendPrepend.Append(value);
		}
		if (scheduler == ImmediateScheduler.Instance)
		{
			return new AppendPrepend<TSource>.SingleImmediate(source, value, append: true);
		}
		return new AppendPrepend<TSource>.SingleValue(source, value, scheduler, append: true);
	}

	public virtual IObservable<TSource> AsObservable<TSource>(IObservable<TSource> source)
	{
		if (source is AsObservable<TSource> result)
		{
			return result;
		}
		return new AsObservable<TSource>(source);
	}

	public virtual IObservable<IList<TSource>> Buffer<TSource>(IObservable<TSource> source, int count)
	{
		return new Buffer<TSource>.CountExact(source, count);
	}

	public virtual IObservable<IList<TSource>> Buffer<TSource>(IObservable<TSource> source, int count, int skip)
	{
		if (count > skip)
		{
			return new Buffer<TSource>.CountOverlap(source, count, skip);
		}
		if (count < skip)
		{
			return new Buffer<TSource>.CountSkip(source, count, skip);
		}
		return new Buffer<TSource>.CountExact(source, count);
	}

	public virtual IObservable<TSource> ResetExceptionDispatchState<TSource>(IObservable<TSource> source)
	{
		return new ResetExceptionDispatchState<TSource>(source);
	}

	public virtual IObservable<TSource> Dematerialize<TSource>(IObservable<Notification<TSource>> source)
	{
		if (source is Materialize<TSource> materialize)
		{
			return materialize.Dematerialize();
		}
		return new Dematerialize<TSource>(source);
	}

	public virtual IObservable<TSource> DistinctUntilChanged<TSource>(IObservable<TSource> source)
	{
		return DistinctUntilChanged_(source, (TSource x) => x, EqualityComparer<TSource>.Default);
	}

	public virtual IObservable<TSource> DistinctUntilChanged<TSource>(IObservable<TSource> source, IEqualityComparer<TSource> comparer)
	{
		return DistinctUntilChanged_(source, (TSource x) => x, comparer);
	}

	public virtual IObservable<TSource> DistinctUntilChanged<TSource, TKey>(IObservable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return DistinctUntilChanged_(source, keySelector, EqualityComparer<TKey>.Default);
	}

	public virtual IObservable<TSource> DistinctUntilChanged<TSource, TKey>(IObservable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		return DistinctUntilChanged_(source, keySelector, comparer);
	}

	private static IObservable<TSource> DistinctUntilChanged_<TSource, TKey>(IObservable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		return new DistinctUntilChanged<TSource, TKey>(source, keySelector, comparer);
	}

	public virtual IObservable<TSource> Do<TSource>(IObservable<TSource> source, Action<TSource> onNext)
	{
		return new Do<TSource>.OnNext(source, onNext);
	}

	public virtual IObservable<TSource> Do<TSource>(IObservable<TSource> source, Action<TSource> onNext, Action onCompleted)
	{
		return Do_(source, onNext, Stubs<Exception>.Ignore, onCompleted);
	}

	public virtual IObservable<TSource> Do<TSource>(IObservable<TSource> source, Action<TSource> onNext, Action<Exception> onError)
	{
		return Do_(source, onNext, onError, Stubs.Nop);
	}

	public virtual IObservable<TSource> Do<TSource>(IObservable<TSource> source, Action<TSource> onNext, Action<Exception> onError, Action onCompleted)
	{
		return Do_(source, onNext, onError, onCompleted);
	}

	public virtual IObservable<TSource> Do<TSource>(IObservable<TSource> source, IObserver<TSource> observer)
	{
		return new Do<TSource>.Observer(source, observer);
	}

	private static IObservable<TSource> Do_<TSource>(IObservable<TSource> source, Action<TSource> onNext, Action<Exception> onError, Action onCompleted)
	{
		return new Do<TSource>.Actions(source, onNext, onError, onCompleted);
	}

	public virtual IObservable<TSource> Finally<TSource>(IObservable<TSource> source, Action finallyAction)
	{
		return new Finally<TSource>(source, finallyAction);
	}

	public virtual IObservable<TSource> IgnoreElements<TSource>(IObservable<TSource> source)
	{
		if (source is IgnoreElements<TSource> result)
		{
			return result;
		}
		return new IgnoreElements<TSource>(source);
	}

	public virtual IObservable<Notification<TSource>> Materialize<TSource>(IObservable<TSource> source)
	{
		return new Materialize<TSource>(source);
	}

	public virtual IObservable<TSource> Prepend<TSource>(IObservable<TSource> source, TSource value)
	{
		return Prepend_(source, value, SchedulerDefaults.ConstantTimeOperations);
	}

	public virtual IObservable<TSource> Prepend<TSource>(IObservable<TSource> source, TSource value, IScheduler scheduler)
	{
		return Prepend_(source, value, scheduler);
	}

	private static IObservable<TSource> Prepend_<TSource>(IObservable<TSource> source, TSource value, IScheduler scheduler)
	{
		if (source is AppendPrepend<TSource>.IAppendPrepend appendPrepend && appendPrepend.Scheduler == scheduler)
		{
			return appendPrepend.Prepend(value);
		}
		if (scheduler == ImmediateScheduler.Instance)
		{
			return new AppendPrepend<TSource>.SingleImmediate(source, value, append: false);
		}
		return new AppendPrepend<TSource>.SingleValue(source, value, scheduler, append: false);
	}

	public virtual IObservable<TSource> Repeat<TSource>(IObservable<TSource> source)
	{
		return RepeatInfinite(source).Concat();
	}

	private static IEnumerable<T> RepeatInfinite<T>(T value)
	{
		while (true)
		{
			yield return value;
		}
	}

	public virtual IObservable<TSource> Repeat<TSource>(IObservable<TSource> source, int repeatCount)
	{
		return Enumerable.Repeat(source, repeatCount).Concat();
	}

	public virtual IObservable<TSource> RepeatWhen<TSource, TSignal>(IObservable<TSource> source, Func<IObservable<object>, IObservable<TSignal>> handler)
	{
		return new RepeatWhen<TSource, TSignal>(source, handler);
	}

	public virtual IObservable<TSource> Retry<TSource>(IObservable<TSource> source)
	{
		return RepeatInfinite(source).Catch();
	}

	public virtual IObservable<TSource> Retry<TSource>(IObservable<TSource> source, int retryCount)
	{
		return Enumerable.Repeat(source, retryCount).Catch();
	}

	public virtual IObservable<TSource> RetryWhen<TSource, TSignal>(IObservable<TSource> source, Func<IObservable<Exception>, IObservable<TSignal>> handler)
	{
		return new RetryWhen<TSource, TSignal>(source, handler);
	}

	public virtual IObservable<TAccumulate> Scan<TSource, TAccumulate>(IObservable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator)
	{
		return new Scan<TSource, TAccumulate>(source, seed, accumulator);
	}

	public virtual IObservable<TSource> Scan<TSource>(IObservable<TSource> source, Func<TSource, TSource, TSource> accumulator)
	{
		return new Scan<TSource>(source, accumulator);
	}

	public virtual IObservable<TSource> SkipLast<TSource>(IObservable<TSource> source, int count)
	{
		return new SkipLast<TSource>.Count(source, count);
	}

	public virtual IObservable<TSource> StartWith<TSource>(IObservable<TSource> source, params TSource[] values)
	{
		return StartWith_(source, SchedulerDefaults.ConstantTimeOperations, values);
	}

	public virtual IObservable<TSource> StartWith<TSource>(IObservable<TSource> source, IScheduler scheduler, params TSource[] values)
	{
		return StartWith_(source, scheduler, values);
	}

	public virtual IObservable<TSource> StartWith<TSource>(IObservable<TSource> source, IEnumerable<TSource> values)
	{
		return StartWith(source, SchedulerDefaults.ConstantTimeOperations, values);
	}

	public virtual IObservable<TSource> StartWith<TSource>(IObservable<TSource> source, IScheduler scheduler, IEnumerable<TSource> values)
	{
		TSource[] array = values as TSource[];
		if (array == null)
		{
			array = new List<TSource>(values).ToArray();
		}
		return StartWith_(source, scheduler, array);
	}

	private static IObservable<TSource> StartWith_<TSource>(IObservable<TSource> source, IScheduler scheduler, params TSource[] values)
	{
		return values.ToObservable(scheduler).Concat(source);
	}

	public virtual IObservable<TSource> TakeLast<TSource>(IObservable<TSource> source, int count)
	{
		return TakeLast_(source, count, SchedulerDefaults.Iteration);
	}

	public virtual IObservable<TSource> TakeLast<TSource>(IObservable<TSource> source, int count, IScheduler scheduler)
	{
		return TakeLast_(source, count, scheduler);
	}

	private static IObservable<TSource> TakeLast_<TSource>(IObservable<TSource> source, int count, IScheduler scheduler)
	{
		return new TakeLast<TSource>.Count(source, count, scheduler);
	}

	public virtual IObservable<IList<TSource>> TakeLastBuffer<TSource>(IObservable<TSource> source, int count)
	{
		return new TakeLastBuffer<TSource>.Count(source, count);
	}

	public virtual IObservable<IObservable<TSource>> Window<TSource>(IObservable<TSource> source, int count, int skip)
	{
		return Window_(source, count, skip);
	}

	public virtual IObservable<IObservable<TSource>> Window<TSource>(IObservable<TSource> source, int count)
	{
		return Window_(source, count, count);
	}

	private static IObservable<IObservable<TSource>> Window_<TSource>(IObservable<TSource> source, int count, int skip)
	{
		return new Window<TSource>.Count(source, count, skip);
	}

	public virtual IObservable<TResult> Cast<TResult>(IObservable<object> source)
	{
		return new Cast<object, TResult>(source);
	}

	public virtual IObservable<TSource?> DefaultIfEmpty<TSource>(IObservable<TSource> source)
	{
		return new DefaultIfEmpty<TSource>(source, default(TSource));
	}

	public virtual IObservable<TSource> DefaultIfEmpty<TSource>(IObservable<TSource> source, TSource defaultValue)
	{
		return new DefaultIfEmpty<TSource>(source, defaultValue);
	}

	public virtual IObservable<TSource> Distinct<TSource>(IObservable<TSource> source)
	{
		return new Distinct<TSource, TSource>(source, (TSource x) => x, EqualityComparer<TSource>.Default);
	}

	public virtual IObservable<TSource> Distinct<TSource>(IObservable<TSource> source, IEqualityComparer<TSource> comparer)
	{
		return new Distinct<TSource, TSource>(source, (TSource x) => x, comparer);
	}

	public virtual IObservable<TSource> Distinct<TSource, TKey>(IObservable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return new Distinct<TSource, TKey>(source, keySelector, EqualityComparer<TKey>.Default);
	}

	public virtual IObservable<TSource> Distinct<TSource, TKey>(IObservable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		return new Distinct<TSource, TKey>(source, keySelector, comparer);
	}

	public virtual IObservable<IGroupedObservable<TKey, TElement>> GroupBy<TSource, TKey, TElement>(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
	{
		return GroupBy_(source, keySelector, elementSelector, null, EqualityComparer<TKey>.Default);
	}

	public virtual IObservable<IGroupedObservable<TKey, TSource>> GroupBy<TSource, TKey>(IObservable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		return GroupBy_(source, keySelector, (TSource x) => x, null, comparer);
	}

	public virtual IObservable<IGroupedObservable<TKey, TSource>> GroupBy<TSource, TKey>(IObservable<TSource> source, Func<TSource, TKey> keySelector)
	{
		return GroupBy_(source, keySelector, (TSource x) => x, null, EqualityComparer<TKey>.Default);
	}

	public virtual IObservable<IGroupedObservable<TKey, TElement>> GroupBy<TSource, TKey, TElement>(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
	{
		return GroupBy_(source, keySelector, elementSelector, null, comparer);
	}

	public virtual IObservable<IGroupedObservable<TKey, TElement>> GroupBy<TSource, TKey, TElement>(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, int capacity)
	{
		return GroupBy_(source, keySelector, elementSelector, capacity, EqualityComparer<TKey>.Default);
	}

	public virtual IObservable<IGroupedObservable<TKey, TSource>> GroupBy<TSource, TKey>(IObservable<TSource> source, Func<TSource, TKey> keySelector, int capacity, IEqualityComparer<TKey> comparer)
	{
		return GroupBy_(source, keySelector, (TSource x) => x, capacity, comparer);
	}

	public virtual IObservable<IGroupedObservable<TKey, TSource>> GroupBy<TSource, TKey>(IObservable<TSource> source, Func<TSource, TKey> keySelector, int capacity)
	{
		return GroupBy_(source, keySelector, (TSource x) => x, capacity, EqualityComparer<TKey>.Default);
	}

	public virtual IObservable<IGroupedObservable<TKey, TElement>> GroupBy<TSource, TKey, TElement>(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, int capacity, IEqualityComparer<TKey> comparer)
	{
		return GroupBy_(source, keySelector, elementSelector, capacity, comparer);
	}

	private static IObservable<IGroupedObservable<TKey, TElement>> GroupBy_<TSource, TKey, TElement>(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, int? capacity, IEqualityComparer<TKey> comparer)
	{
		return new GroupBy<TSource, TKey, TElement>(source, keySelector, elementSelector, capacity, comparer);
	}

	public virtual IObservable<IGroupedObservable<TKey, TElement>> GroupByUntil<TSource, TKey, TElement, TDuration>(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<IGroupedObservable<TKey, TElement>, IObservable<TDuration>> durationSelector, IEqualityComparer<TKey> comparer)
	{
		return GroupByUntil_(source, keySelector, elementSelector, durationSelector, null, comparer);
	}

	public virtual IObservable<IGroupedObservable<TKey, TElement>> GroupByUntil<TSource, TKey, TElement, TDuration>(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<IGroupedObservable<TKey, TElement>, IObservable<TDuration>> durationSelector)
	{
		return GroupByUntil_(source, keySelector, elementSelector, durationSelector, null, EqualityComparer<TKey>.Default);
	}

	public virtual IObservable<IGroupedObservable<TKey, TSource>> GroupByUntil<TSource, TKey, TDuration>(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<IGroupedObservable<TKey, TSource>, IObservable<TDuration>> durationSelector, IEqualityComparer<TKey> comparer)
	{
		return GroupByUntil_(source, keySelector, (TSource x) => x, durationSelector, null, comparer);
	}

	public virtual IObservable<IGroupedObservable<TKey, TSource>> GroupByUntil<TSource, TKey, TDuration>(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<IGroupedObservable<TKey, TSource>, IObservable<TDuration>> durationSelector)
	{
		return GroupByUntil_(source, keySelector, (TSource x) => x, durationSelector, null, EqualityComparer<TKey>.Default);
	}

	public virtual IObservable<IGroupedObservable<TKey, TElement>> GroupByUntil<TSource, TKey, TElement, TDuration>(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<IGroupedObservable<TKey, TElement>, IObservable<TDuration>> durationSelector, int capacity, IEqualityComparer<TKey> comparer)
	{
		return GroupByUntil_(source, keySelector, elementSelector, durationSelector, capacity, comparer);
	}

	public virtual IObservable<IGroupedObservable<TKey, TElement>> GroupByUntil<TSource, TKey, TElement, TDuration>(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<IGroupedObservable<TKey, TElement>, IObservable<TDuration>> durationSelector, int capacity)
	{
		return GroupByUntil_(source, keySelector, elementSelector, durationSelector, capacity, EqualityComparer<TKey>.Default);
	}

	public virtual IObservable<IGroupedObservable<TKey, TSource>> GroupByUntil<TSource, TKey, TDuration>(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<IGroupedObservable<TKey, TSource>, IObservable<TDuration>> durationSelector, int capacity, IEqualityComparer<TKey> comparer)
	{
		return GroupByUntil_(source, keySelector, (TSource x) => x, durationSelector, capacity, comparer);
	}

	public virtual IObservable<IGroupedObservable<TKey, TSource>> GroupByUntil<TSource, TKey, TDuration>(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<IGroupedObservable<TKey, TSource>, IObservable<TDuration>> durationSelector, int capacity)
	{
		return GroupByUntil_(source, keySelector, (TSource x) => x, durationSelector, capacity, EqualityComparer<TKey>.Default);
	}

	private static IObservable<IGroupedObservable<TKey, TElement>> GroupByUntil_<TSource, TKey, TElement, TDuration>(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<IGroupedObservable<TKey, TElement>, IObservable<TDuration>> durationSelector, int? capacity, IEqualityComparer<TKey> comparer)
	{
		return new GroupByUntil<TSource, TKey, TElement, TDuration>(source, keySelector, elementSelector, durationSelector, capacity, comparer);
	}

	public virtual IObservable<TResult> GroupJoin<TLeft, TRight, TLeftDuration, TRightDuration, TResult>(IObservable<TLeft> left, IObservable<TRight> right, Func<TLeft, IObservable<TLeftDuration>> leftDurationSelector, Func<TRight, IObservable<TRightDuration>> rightDurationSelector, Func<TLeft, IObservable<TRight>, TResult> resultSelector)
	{
		return GroupJoin_(left, right, leftDurationSelector, rightDurationSelector, resultSelector);
	}

	private static IObservable<TResult> GroupJoin_<TLeft, TRight, TLeftDuration, TRightDuration, TResult>(IObservable<TLeft> left, IObservable<TRight> right, Func<TLeft, IObservable<TLeftDuration>> leftDurationSelector, Func<TRight, IObservable<TRightDuration>> rightDurationSelector, Func<TLeft, IObservable<TRight>, TResult> resultSelector)
	{
		return new GroupJoin<TLeft, TRight, TLeftDuration, TRightDuration, TResult>(left, right, leftDurationSelector, rightDurationSelector, resultSelector);
	}

	public virtual IObservable<TResult> Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>(IObservable<TLeft> left, IObservable<TRight> right, Func<TLeft, IObservable<TLeftDuration>> leftDurationSelector, Func<TRight, IObservable<TRightDuration>> rightDurationSelector, Func<TLeft, TRight, TResult> resultSelector)
	{
		return Join_(left, right, leftDurationSelector, rightDurationSelector, resultSelector);
	}

	private static IObservable<TResult> Join_<TLeft, TRight, TLeftDuration, TRightDuration, TResult>(IObservable<TLeft> left, IObservable<TRight> right, Func<TLeft, IObservable<TLeftDuration>> leftDurationSelector, Func<TRight, IObservable<TRightDuration>> rightDurationSelector, Func<TLeft, TRight, TResult> resultSelector)
	{
		return new Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>(left, right, leftDurationSelector, rightDurationSelector, resultSelector);
	}

	public virtual IObservable<TResult> OfType<TResult>(IObservable<object> source)
	{
		return new OfType<object, TResult>(source);
	}

	public virtual IObservable<TResult> Select<TSource, TResult>(IObservable<TSource> source, Func<TSource, TResult> selector)
	{
		return new Select<TSource, TResult>.Selector(source, selector);
	}

	public virtual IObservable<TResult> Select<TSource, TResult>(IObservable<TSource> source, Func<TSource, int, TResult> selector)
	{
		return new Select<TSource, TResult>.SelectorIndexed(source, selector);
	}

	public virtual IObservable<TOther> SelectMany<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
	{
		return SelectMany_(source, (TSource _) => other);
	}

	public virtual IObservable<TResult> SelectMany<TSource, TResult>(IObservable<TSource> source, Func<TSource, IObservable<TResult>> selector)
	{
		return SelectMany_(source, selector);
	}

	public virtual IObservable<TResult> SelectMany<TSource, TResult>(IObservable<TSource> source, Func<TSource, int, IObservable<TResult>> selector)
	{
		return SelectMany_(source, selector);
	}

	public virtual IObservable<TResult> SelectMany<TSource, TResult>(IObservable<TSource> source, Func<TSource, Task<TResult>> selector)
	{
		return new SelectMany<TSource, TResult>.TaskSelector(source, (TSource x, CancellationToken token) => selector(x));
	}

	public virtual IObservable<TResult> SelectMany<TSource, TResult>(IObservable<TSource> source, Func<TSource, int, Task<TResult>> selector)
	{
		return new SelectMany<TSource, TResult>.TaskSelectorIndexed(source, (TSource x, int i, CancellationToken token) => selector(x, i));
	}

	public virtual IObservable<TResult> SelectMany<TSource, TResult>(IObservable<TSource> source, Func<TSource, CancellationToken, Task<TResult>> selector)
	{
		return new SelectMany<TSource, TResult>.TaskSelector(source, selector);
	}

	public virtual IObservable<TResult> SelectMany<TSource, TResult>(IObservable<TSource> source, Func<TSource, int, CancellationToken, Task<TResult>> selector)
	{
		return new SelectMany<TSource, TResult>.TaskSelectorIndexed(source, selector);
	}

	public virtual IObservable<TResult> SelectMany<TSource, TCollection, TResult>(IObservable<TSource> source, Func<TSource, IObservable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
	{
		return SelectMany_(source, collectionSelector, resultSelector);
	}

	public virtual IObservable<TResult> SelectMany<TSource, TCollection, TResult>(IObservable<TSource> source, Func<TSource, int, IObservable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector)
	{
		return SelectMany_(source, collectionSelector, resultSelector);
	}

	public virtual IObservable<TResult> SelectMany<TSource, TTaskResult, TResult>(IObservable<TSource> source, Func<TSource, Task<TTaskResult>> taskSelector, Func<TSource, TTaskResult, TResult> resultSelector)
	{
		return new SelectMany<TSource, TTaskResult, TResult>.TaskSelector(source, (TSource x, CancellationToken token) => taskSelector(x), resultSelector);
	}

	public virtual IObservable<TResult> SelectMany<TSource, TTaskResult, TResult>(IObservable<TSource> source, Func<TSource, int, Task<TTaskResult>> taskSelector, Func<TSource, int, TTaskResult, TResult> resultSelector)
	{
		return new SelectMany<TSource, TTaskResult, TResult>.TaskSelectorIndexed(source, (TSource x, int i, CancellationToken token) => taskSelector(x, i), resultSelector);
	}

	public virtual IObservable<TResult> SelectMany<TSource, TTaskResult, TResult>(IObservable<TSource> source, Func<TSource, CancellationToken, Task<TTaskResult>> taskSelector, Func<TSource, TTaskResult, TResult> resultSelector)
	{
		return new SelectMany<TSource, TTaskResult, TResult>.TaskSelector(source, taskSelector, resultSelector);
	}

	public virtual IObservable<TResult> SelectMany<TSource, TTaskResult, TResult>(IObservable<TSource> source, Func<TSource, int, CancellationToken, Task<TTaskResult>> taskSelector, Func<TSource, int, TTaskResult, TResult> resultSelector)
	{
		return new SelectMany<TSource, TTaskResult, TResult>.TaskSelectorIndexed(source, taskSelector, resultSelector);
	}

	private static IObservable<TResult> SelectMany_<TSource, TResult>(IObservable<TSource> source, Func<TSource, IObservable<TResult>> selector)
	{
		return new SelectMany<TSource, TResult>.ObservableSelector(source, selector);
	}

	private static IObservable<TResult> SelectMany_<TSource, TResult>(IObservable<TSource> source, Func<TSource, int, IObservable<TResult>> selector)
	{
		return new SelectMany<TSource, TResult>.ObservableSelectorIndexed(source, selector);
	}

	private static IObservable<TResult> SelectMany_<TSource, TCollection, TResult>(IObservable<TSource> source, Func<TSource, IObservable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
	{
		return new SelectMany<TSource, TCollection, TResult>.ObservableSelector(source, collectionSelector, resultSelector);
	}

	private static IObservable<TResult> SelectMany_<TSource, TCollection, TResult>(IObservable<TSource> source, Func<TSource, int, IObservable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector)
	{
		return new SelectMany<TSource, TCollection, TResult>.ObservableSelectorIndexed(source, collectionSelector, resultSelector);
	}

	public virtual IObservable<TResult> SelectMany<TSource, TResult>(IObservable<TSource> source, Func<TSource, IObservable<TResult>> onNext, Func<Exception, IObservable<TResult>> onError, Func<IObservable<TResult>> onCompleted)
	{
		return new SelectMany<TSource, TResult>.ObservableSelectors(source, onNext, onError, onCompleted);
	}

	public virtual IObservable<TResult> SelectMany<TSource, TResult>(IObservable<TSource> source, Func<TSource, int, IObservable<TResult>> onNext, Func<Exception, IObservable<TResult>> onError, Func<IObservable<TResult>> onCompleted)
	{
		return new SelectMany<TSource, TResult>.ObservableSelectorsIndexed(source, onNext, onError, onCompleted);
	}

	public virtual IObservable<TResult> SelectMany<TSource, TResult>(IObservable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
	{
		return new SelectMany<TSource, TResult>.EnumerableSelector(source, selector);
	}

	public virtual IObservable<TResult> SelectMany<TSource, TResult>(IObservable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
	{
		return new SelectMany<TSource, TResult>.EnumerableSelectorIndexed(source, selector);
	}

	public virtual IObservable<TResult> SelectMany<TSource, TCollection, TResult>(IObservable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
	{
		return SelectMany_(source, collectionSelector, resultSelector);
	}

	public virtual IObservable<TResult> SelectMany<TSource, TCollection, TResult>(IObservable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector)
	{
		return SelectMany_(source, collectionSelector, resultSelector);
	}

	private static IObservable<TResult> SelectMany_<TSource, TCollection, TResult>(IObservable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
	{
		return new SelectMany<TSource, TCollection, TResult>.EnumerableSelector(source, collectionSelector, resultSelector);
	}

	private static IObservable<TResult> SelectMany_<TSource, TCollection, TResult>(IObservable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector)
	{
		return new SelectMany<TSource, TCollection, TResult>.EnumerableSelectorIndexed(source, collectionSelector, resultSelector);
	}

	public virtual IObservable<TSource> Skip<TSource>(IObservable<TSource> source, int count)
	{
		if (source is Skip<TSource>.Count count2)
		{
			return count2.Combine(count);
		}
		return new Skip<TSource>.Count(source, count);
	}

	public virtual IObservable<TSource> SkipWhile<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		return new SkipWhile<TSource>.Predicate(source, predicate);
	}

	public virtual IObservable<TSource> SkipWhile<TSource>(IObservable<TSource> source, Func<TSource, int, bool> predicate)
	{
		return new SkipWhile<TSource>.PredicateIndexed(source, predicate);
	}

	public virtual IObservable<TSource> Take<TSource>(IObservable<TSource> source, int count)
	{
		if (count == 0)
		{
			return Empty<TSource>();
		}
		return Take_(source, count);
	}

	public virtual IObservable<TSource> Take<TSource>(IObservable<TSource> source, int count, IScheduler scheduler)
	{
		if (count == 0)
		{
			return Empty<TSource>(scheduler);
		}
		return Take_(source, count);
	}

	private static IObservable<TSource> Take_<TSource>(IObservable<TSource> source, int count)
	{
		if (source is Take<TSource>.Count count2)
		{
			return count2.Combine(count);
		}
		return new Take<TSource>.Count(source, count);
	}

	public virtual IObservable<TSource> TakeWhile<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		return new TakeWhile<TSource>.Predicate(source, predicate);
	}

	public virtual IObservable<TSource> TakeWhile<TSource>(IObservable<TSource> source, Func<TSource, int, bool> predicate)
	{
		return new TakeWhile<TSource>.PredicateIndexed(source, predicate);
	}

	public virtual IObservable<TSource> Where<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source is Where<TSource>.Predicate predicate2)
		{
			return predicate2.Combine(predicate);
		}
		return new Where<TSource>.Predicate(source, predicate);
	}

	public virtual IObservable<TSource> Where<TSource>(IObservable<TSource> source, Func<TSource, int, bool> predicate)
	{
		return new Where<TSource>.PredicateIndexed(source, predicate);
	}

	public virtual IObservable<IList<TSource>> Buffer<TSource>(IObservable<TSource> source, TimeSpan timeSpan)
	{
		return Buffer_(source, timeSpan, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<IList<TSource>> Buffer<TSource>(IObservable<TSource> source, TimeSpan timeSpan, IScheduler scheduler)
	{
		return Buffer_(source, timeSpan, scheduler);
	}

	private static IObservable<IList<TSource>> Buffer_<TSource>(IObservable<TSource> source, TimeSpan timeSpan, IScheduler scheduler)
	{
		return new Buffer<TSource>.TimeHopping(source, timeSpan, scheduler);
	}

	public virtual IObservable<IList<TSource>> Buffer<TSource>(IObservable<TSource> source, TimeSpan timeSpan, TimeSpan timeShift)
	{
		return Buffer_(source, timeSpan, timeShift, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<IList<TSource>> Buffer<TSource>(IObservable<TSource> source, TimeSpan timeSpan, TimeSpan timeShift, IScheduler scheduler)
	{
		return Buffer_(source, timeSpan, timeShift, scheduler);
	}

	private static IObservable<IList<TSource>> Buffer_<TSource>(IObservable<TSource> source, TimeSpan timeSpan, TimeSpan timeShift, IScheduler scheduler)
	{
		return new Buffer<TSource>.TimeSliding(source, timeSpan, timeShift, scheduler);
	}

	public virtual IObservable<IList<TSource>> Buffer<TSource>(IObservable<TSource> source, TimeSpan timeSpan, int count)
	{
		return Buffer_(source, timeSpan, count, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<IList<TSource>> Buffer<TSource>(IObservable<TSource> source, TimeSpan timeSpan, int count, IScheduler scheduler)
	{
		return Buffer_(source, timeSpan, count, scheduler);
	}

	private static IObservable<IList<TSource>> Buffer_<TSource>(IObservable<TSource> source, TimeSpan timeSpan, int count, IScheduler scheduler)
	{
		return new Buffer<TSource>.Ferry(source, timeSpan, count, scheduler);
	}

	public virtual IObservable<TSource> Delay<TSource>(IObservable<TSource> source, TimeSpan dueTime)
	{
		return Delay_(source, dueTime, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<TSource> Delay<TSource>(IObservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
	{
		return Delay_(source, dueTime, scheduler);
	}

	private static IObservable<TSource> Delay_<TSource>(IObservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
	{
		return new Delay<TSource>.Relative(source, dueTime, scheduler);
	}

	public virtual IObservable<TSource> Delay<TSource>(IObservable<TSource> source, DateTimeOffset dueTime)
	{
		return Delay_(source, dueTime, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<TSource> Delay<TSource>(IObservable<TSource> source, DateTimeOffset dueTime, IScheduler scheduler)
	{
		return Delay_(source, dueTime, scheduler);
	}

	private static IObservable<TSource> Delay_<TSource>(IObservable<TSource> source, DateTimeOffset dueTime, IScheduler scheduler)
	{
		return new Delay<TSource>.Absolute(source, dueTime, scheduler);
	}

	public virtual IObservable<TSource> Delay<TSource, TDelay>(IObservable<TSource> source, Func<TSource, IObservable<TDelay>> delayDurationSelector)
	{
		return new Delay<TSource, TDelay>.Selector(source, delayDurationSelector);
	}

	public virtual IObservable<TSource> Delay<TSource, TDelay>(IObservable<TSource> source, IObservable<TDelay> subscriptionDelay, Func<TSource, IObservable<TDelay>> delayDurationSelector)
	{
		return new Delay<TSource, TDelay>.SelectorWithSubscriptionDelay(source, subscriptionDelay, delayDurationSelector);
	}

	public virtual IObservable<TSource> DelaySubscription<TSource>(IObservable<TSource> source, TimeSpan dueTime)
	{
		return DelaySubscription_(source, dueTime, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<TSource> DelaySubscription<TSource>(IObservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
	{
		return DelaySubscription_(source, dueTime, scheduler);
	}

	private static IObservable<TSource> DelaySubscription_<TSource>(IObservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
	{
		return new DelaySubscription<TSource>.Relative(source, dueTime, scheduler);
	}

	public virtual IObservable<TSource> DelaySubscription<TSource>(IObservable<TSource> source, DateTimeOffset dueTime)
	{
		return DelaySubscription_(source, dueTime, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<TSource> DelaySubscription<TSource>(IObservable<TSource> source, DateTimeOffset dueTime, IScheduler scheduler)
	{
		return DelaySubscription_(source, dueTime, scheduler);
	}

	private static IObservable<TSource> DelaySubscription_<TSource>(IObservable<TSource> source, DateTimeOffset dueTime, IScheduler scheduler)
	{
		return new DelaySubscription<TSource>.Absolute(source, dueTime, scheduler);
	}

	public virtual IObservable<TResult> Generate<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, Func<TState, TimeSpan> timeSelector)
	{
		return Generate_(initialState, condition, iterate, resultSelector, timeSelector, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<TResult> Generate<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, Func<TState, TimeSpan> timeSelector, IScheduler scheduler)
	{
		return Generate_(initialState, condition, iterate, resultSelector, timeSelector, scheduler);
	}

	private static IObservable<TResult> Generate_<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, Func<TState, TimeSpan> timeSelector, IScheduler scheduler)
	{
		return new Generate<TState, TResult>.Relative(initialState, condition, iterate, resultSelector, timeSelector, scheduler);
	}

	public virtual IObservable<TResult> Generate<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, Func<TState, DateTimeOffset> timeSelector)
	{
		return Generate_(initialState, condition, iterate, resultSelector, timeSelector, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<TResult> Generate<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, Func<TState, DateTimeOffset> timeSelector, IScheduler scheduler)
	{
		return Generate_(initialState, condition, iterate, resultSelector, timeSelector, scheduler);
	}

	private static IObservable<TResult> Generate_<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, Func<TState, DateTimeOffset> timeSelector, IScheduler scheduler)
	{
		return new Generate<TState, TResult>.Absolute(initialState, condition, iterate, resultSelector, timeSelector, scheduler);
	}

	public virtual IObservable<long> Interval(TimeSpan period)
	{
		return Timer_(period, period, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<long> Interval(TimeSpan period, IScheduler scheduler)
	{
		return Timer_(period, period, scheduler);
	}

	public virtual IObservable<TSource> Sample<TSource>(IObservable<TSource> source, TimeSpan interval)
	{
		return Sample_(source, interval, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<TSource> Sample<TSource>(IObservable<TSource> source, TimeSpan interval, IScheduler scheduler)
	{
		return Sample_(source, interval, scheduler);
	}

	private static IObservable<TSource> Sample_<TSource>(IObservable<TSource> source, TimeSpan interval, IScheduler scheduler)
	{
		return new Sample<TSource>(source, interval, scheduler);
	}

	public virtual IObservable<TSource> Sample<TSource, TSample>(IObservable<TSource> source, IObservable<TSample> sampler)
	{
		return Sample_(source, sampler);
	}

	private static IObservable<TSource> Sample_<TSource, TSample>(IObservable<TSource> source, IObservable<TSample> sampler)
	{
		return new Sample<TSource, TSample>(source, sampler);
	}

	public virtual IObservable<TSource> Skip<TSource>(IObservable<TSource> source, TimeSpan duration)
	{
		return Skip_(source, duration, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<TSource> Skip<TSource>(IObservable<TSource> source, TimeSpan duration, IScheduler scheduler)
	{
		return Skip_(source, duration, scheduler);
	}

	private static IObservable<TSource> Skip_<TSource>(IObservable<TSource> source, TimeSpan duration, IScheduler scheduler)
	{
		if (source is Skip<TSource>.Time time && time._scheduler == scheduler)
		{
			return time.Combine(duration);
		}
		return new Skip<TSource>.Time(source, duration, scheduler);
	}

	public virtual IObservable<TSource> SkipLast<TSource>(IObservable<TSource> source, TimeSpan duration)
	{
		return SkipLast_(source, duration, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<TSource> SkipLast<TSource>(IObservable<TSource> source, TimeSpan duration, IScheduler scheduler)
	{
		return SkipLast_(source, duration, scheduler);
	}

	private static IObservable<TSource> SkipLast_<TSource>(IObservable<TSource> source, TimeSpan duration, IScheduler scheduler)
	{
		return new SkipLast<TSource>.Time(source, duration, scheduler);
	}

	public virtual IObservable<TSource> SkipUntil<TSource>(IObservable<TSource> source, DateTimeOffset startTime)
	{
		return SkipUntil_(source, startTime, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<TSource> SkipUntil<TSource>(IObservable<TSource> source, DateTimeOffset startTime, IScheduler scheduler)
	{
		return SkipUntil_(source, startTime, scheduler);
	}

	private static IObservable<TSource> SkipUntil_<TSource>(IObservable<TSource> source, DateTimeOffset startTime, IScheduler scheduler)
	{
		if (source is SkipUntil<TSource> skipUntil && skipUntil._scheduler == scheduler)
		{
			return skipUntil.Combine(startTime);
		}
		return new SkipUntil<TSource>(source, startTime, scheduler);
	}

	public virtual IObservable<TSource> Take<TSource>(IObservable<TSource> source, TimeSpan duration)
	{
		return Take_(source, duration, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<TSource> Take<TSource>(IObservable<TSource> source, TimeSpan duration, IScheduler scheduler)
	{
		return Take_(source, duration, scheduler);
	}

	private static IObservable<TSource> Take_<TSource>(IObservable<TSource> source, TimeSpan duration, IScheduler scheduler)
	{
		if (source is Take<TSource>.Time time && time._scheduler == scheduler)
		{
			return time.Combine(duration);
		}
		return new Take<TSource>.Time(source, duration, scheduler);
	}

	public virtual IObservable<TSource> TakeLast<TSource>(IObservable<TSource> source, TimeSpan duration)
	{
		return TakeLast_(source, duration, SchedulerDefaults.TimeBasedOperations, SchedulerDefaults.Iteration);
	}

	public virtual IObservable<TSource> TakeLast<TSource>(IObservable<TSource> source, TimeSpan duration, IScheduler scheduler)
	{
		return TakeLast_(source, duration, scheduler, SchedulerDefaults.Iteration);
	}

	public virtual IObservable<TSource> TakeLast<TSource>(IObservable<TSource> source, TimeSpan duration, IScheduler timerScheduler, IScheduler loopScheduler)
	{
		return TakeLast_(source, duration, timerScheduler, loopScheduler);
	}

	private static IObservable<TSource> TakeLast_<TSource>(IObservable<TSource> source, TimeSpan duration, IScheduler timerScheduler, IScheduler loopScheduler)
	{
		return new TakeLast<TSource>.Time(source, duration, timerScheduler, loopScheduler);
	}

	public virtual IObservable<IList<TSource>> TakeLastBuffer<TSource>(IObservable<TSource> source, TimeSpan duration)
	{
		return TakeLastBuffer_(source, duration, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<IList<TSource>> TakeLastBuffer<TSource>(IObservable<TSource> source, TimeSpan duration, IScheduler scheduler)
	{
		return TakeLastBuffer_(source, duration, scheduler);
	}

	private static IObservable<IList<TSource>> TakeLastBuffer_<TSource>(IObservable<TSource> source, TimeSpan duration, IScheduler scheduler)
	{
		return new TakeLastBuffer<TSource>.Time(source, duration, scheduler);
	}

	public virtual IObservable<TSource> TakeUntil<TSource>(IObservable<TSource> source, DateTimeOffset endTime)
	{
		return TakeUntil_(source, endTime, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<TSource> TakeUntil<TSource>(IObservable<TSource> source, DateTimeOffset endTime, IScheduler scheduler)
	{
		return TakeUntil_(source, endTime, scheduler);
	}

	private static IObservable<TSource> TakeUntil_<TSource>(IObservable<TSource> source, DateTimeOffset endTime, IScheduler scheduler)
	{
		if (source is TakeUntil<TSource> takeUntil && takeUntil._scheduler == scheduler)
		{
			return takeUntil.Combine(endTime);
		}
		return new TakeUntil<TSource>(source, endTime, scheduler);
	}

	public virtual IObservable<TSource> Throttle<TSource>(IObservable<TSource> source, TimeSpan dueTime)
	{
		return Throttle_(source, dueTime, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<TSource> Throttle<TSource>(IObservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
	{
		return Throttle_(source, dueTime, scheduler);
	}

	private static IObservable<TSource> Throttle_<TSource>(IObservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
	{
		return new Throttle<TSource>(source, dueTime, scheduler);
	}

	public virtual IObservable<TSource> Throttle<TSource, TThrottle>(IObservable<TSource> source, Func<TSource, IObservable<TThrottle>> throttleDurationSelector)
	{
		return new Throttle<TSource, TThrottle>(source, throttleDurationSelector);
	}

	public virtual IObservable<TimeInterval<TSource>> TimeInterval<TSource>(IObservable<TSource> source)
	{
		return TimeInterval_(source, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<TimeInterval<TSource>> TimeInterval<TSource>(IObservable<TSource> source, IScheduler scheduler)
	{
		return TimeInterval_(source, scheduler);
	}

	private static IObservable<TimeInterval<TSource>> TimeInterval_<TSource>(IObservable<TSource> source, IScheduler scheduler)
	{
		return new System.Reactive.Linq.ObservableImpl.TimeInterval<TSource>(source, scheduler);
	}

	public virtual IObservable<TSource> Timeout<TSource>(IObservable<TSource> source, TimeSpan dueTime)
	{
		return Timeout_(source, dueTime, Observable.Throw<TSource>(new TimeoutException()), SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<TSource> Timeout<TSource>(IObservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
	{
		return Timeout_(source, dueTime, Observable.Throw<TSource>(new TimeoutException()), scheduler);
	}

	public virtual IObservable<TSource> Timeout<TSource>(IObservable<TSource> source, TimeSpan dueTime, IObservable<TSource> other)
	{
		return Timeout_(source, dueTime, other, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<TSource> Timeout<TSource>(IObservable<TSource> source, TimeSpan dueTime, IObservable<TSource> other, IScheduler scheduler)
	{
		return Timeout_(source, dueTime, other, scheduler);
	}

	private static IObservable<TSource> Timeout_<TSource>(IObservable<TSource> source, TimeSpan dueTime, IObservable<TSource> other, IScheduler scheduler)
	{
		return new Timeout<TSource>.Relative(source, dueTime, other, scheduler);
	}

	public virtual IObservable<TSource> Timeout<TSource>(IObservable<TSource> source, DateTimeOffset dueTime)
	{
		return Timeout_(source, dueTime, Observable.Throw<TSource>(new TimeoutException()), SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<TSource> Timeout<TSource>(IObservable<TSource> source, DateTimeOffset dueTime, IScheduler scheduler)
	{
		return Timeout_(source, dueTime, Observable.Throw<TSource>(new TimeoutException()), scheduler);
	}

	public virtual IObservable<TSource> Timeout<TSource>(IObservable<TSource> source, DateTimeOffset dueTime, IObservable<TSource> other)
	{
		return Timeout_(source, dueTime, other, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<TSource> Timeout<TSource>(IObservable<TSource> source, DateTimeOffset dueTime, IObservable<TSource> other, IScheduler scheduler)
	{
		return Timeout_(source, dueTime, other, scheduler);
	}

	private static IObservable<TSource> Timeout_<TSource>(IObservable<TSource> source, DateTimeOffset dueTime, IObservable<TSource> other, IScheduler scheduler)
	{
		return new Timeout<TSource>.Absolute(source, dueTime, other, scheduler);
	}

	public virtual IObservable<TSource> Timeout<TSource, TTimeout>(IObservable<TSource> source, Func<TSource, IObservable<TTimeout>> timeoutDurationSelector)
	{
		return Timeout_(source, Observable.Never<TTimeout>(), timeoutDurationSelector, Observable.Throw<TSource>(new TimeoutException()));
	}

	public virtual IObservable<TSource> Timeout<TSource, TTimeout>(IObservable<TSource> source, Func<TSource, IObservable<TTimeout>> timeoutDurationSelector, IObservable<TSource> other)
	{
		return Timeout_(source, Observable.Never<TTimeout>(), timeoutDurationSelector, other);
	}

	public virtual IObservable<TSource> Timeout<TSource, TTimeout>(IObservable<TSource> source, IObservable<TTimeout> firstTimeout, Func<TSource, IObservable<TTimeout>> timeoutDurationSelector)
	{
		return Timeout_(source, firstTimeout, timeoutDurationSelector, Observable.Throw<TSource>(new TimeoutException()));
	}

	public virtual IObservable<TSource> Timeout<TSource, TTimeout>(IObservable<TSource> source, IObservable<TTimeout> firstTimeout, Func<TSource, IObservable<TTimeout>> timeoutDurationSelector, IObservable<TSource> other)
	{
		return Timeout_(source, firstTimeout, timeoutDurationSelector, other);
	}

	private static IObservable<TSource> Timeout_<TSource, TTimeout>(IObservable<TSource> source, IObservable<TTimeout> firstTimeout, Func<TSource, IObservable<TTimeout>> timeoutDurationSelector, IObservable<TSource> other)
	{
		return new Timeout<TSource, TTimeout>(source, firstTimeout, timeoutDurationSelector, other);
	}

	public virtual IObservable<long> Timer(TimeSpan dueTime)
	{
		return Timer_(dueTime, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<long> Timer(DateTimeOffset dueTime)
	{
		return Timer_(dueTime, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<long> Timer(TimeSpan dueTime, TimeSpan period)
	{
		return Timer_(dueTime, period, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<long> Timer(DateTimeOffset dueTime, TimeSpan period)
	{
		return Timer_(dueTime, period, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<long> Timer(TimeSpan dueTime, IScheduler scheduler)
	{
		return Timer_(dueTime, scheduler);
	}

	public virtual IObservable<long> Timer(DateTimeOffset dueTime, IScheduler scheduler)
	{
		return Timer_(dueTime, scheduler);
	}

	public virtual IObservable<long> Timer(TimeSpan dueTime, TimeSpan period, IScheduler scheduler)
	{
		return Timer_(dueTime, period, scheduler);
	}

	public virtual IObservable<long> Timer(DateTimeOffset dueTime, TimeSpan period, IScheduler scheduler)
	{
		return Timer_(dueTime, period, scheduler);
	}

	private static IObservable<long> Timer_(TimeSpan dueTime, IScheduler scheduler)
	{
		return new System.Reactive.Linq.ObservableImpl.Timer.Single.Relative(dueTime, scheduler);
	}

	private static IObservable<long> Timer_(TimeSpan dueTime, TimeSpan period, IScheduler scheduler)
	{
		return new System.Reactive.Linq.ObservableImpl.Timer.Periodic.Relative(dueTime, period, scheduler);
	}

	private static IObservable<long> Timer_(DateTimeOffset dueTime, IScheduler scheduler)
	{
		return new System.Reactive.Linq.ObservableImpl.Timer.Single.Absolute(dueTime, scheduler);
	}

	private static IObservable<long> Timer_(DateTimeOffset dueTime, TimeSpan period, IScheduler scheduler)
	{
		return new System.Reactive.Linq.ObservableImpl.Timer.Periodic.Absolute(dueTime, period, scheduler);
	}

	public virtual IObservable<Timestamped<TSource>> Timestamp<TSource>(IObservable<TSource> source)
	{
		return Timestamp_(source, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<Timestamped<TSource>> Timestamp<TSource>(IObservable<TSource> source, IScheduler scheduler)
	{
		return Timestamp_(source, scheduler);
	}

	private static IObservable<Timestamped<TSource>> Timestamp_<TSource>(IObservable<TSource> source, IScheduler scheduler)
	{
		return new Timestamp<TSource>(source, scheduler);
	}

	public virtual IObservable<IObservable<TSource>> Window<TSource>(IObservable<TSource> source, TimeSpan timeSpan)
	{
		return Window_(source, timeSpan, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<IObservable<TSource>> Window<TSource>(IObservable<TSource> source, TimeSpan timeSpan, IScheduler scheduler)
	{
		return Window_(source, timeSpan, scheduler);
	}

	private static IObservable<IObservable<TSource>> Window_<TSource>(IObservable<TSource> source, TimeSpan timeSpan, IScheduler scheduler)
	{
		return new Window<TSource>.TimeHopping(source, timeSpan, scheduler);
	}

	public virtual IObservable<IObservable<TSource>> Window<TSource>(IObservable<TSource> source, TimeSpan timeSpan, TimeSpan timeShift)
	{
		return Window_(source, timeSpan, timeShift, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<IObservable<TSource>> Window<TSource>(IObservable<TSource> source, TimeSpan timeSpan, TimeSpan timeShift, IScheduler scheduler)
	{
		return Window_(source, timeSpan, timeShift, scheduler);
	}

	private static IObservable<IObservable<TSource>> Window_<TSource>(IObservable<TSource> source, TimeSpan timeSpan, TimeSpan timeShift, IScheduler scheduler)
	{
		return new Window<TSource>.TimeSliding(source, timeSpan, timeShift, scheduler);
	}

	public virtual IObservable<IObservable<TSource>> Window<TSource>(IObservable<TSource> source, TimeSpan timeSpan, int count)
	{
		return Window_(source, timeSpan, count, SchedulerDefaults.TimeBasedOperations);
	}

	public virtual IObservable<IObservable<TSource>> Window<TSource>(IObservable<TSource> source, TimeSpan timeSpan, int count, IScheduler scheduler)
	{
		return Window_(source, timeSpan, count, scheduler);
	}

	private static IObservable<IObservable<TSource>> Window_<TSource>(IObservable<TSource> source, TimeSpan timeSpan, int count, IScheduler scheduler)
	{
		return new Window<TSource>.Ferry(source, timeSpan, count, scheduler);
	}
}
