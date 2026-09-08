using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class WithLatestFrom<TFirst, TSecond, TResult> : Producer<TResult, WithLatestFrom<TFirst, TSecond, TResult>._>
{
	internal sealed class @_ : IdentitySink<TResult>
	{
		private sealed class FirstObserver : IObserver<TFirst>
		{
			private readonly @_ _parent;

			public FirstObserver(@_ parent)
			{
				_parent = parent;
			}

			public void OnCompleted()
			{
				lock (_parent._gate)
				{
					_parent.ForwardOnCompleted();
				}
			}

			public void OnError(Exception error)
			{
				lock (_parent._gate)
				{
					_parent.ForwardOnError(error);
				}
			}

			public void OnNext(TFirst value)
			{
				if (!_parent._hasLatest)
				{
					return;
				}
				TSecond latest;
				lock (_parent._latestGate)
				{
					latest = _parent._latest;
				}
				TResult value2;
				try
				{
					value2 = _parent._resultSelector(value, latest);
				}
				catch (Exception error)
				{
					lock (_parent._gate)
					{
						_parent.ForwardOnError(error);
						return;
					}
				}
				lock (_parent._gate)
				{
					_parent.ForwardOnNext(value2);
				}
			}
		}

		private sealed class SecondObserver : IObserver<TSecond>
		{
			private readonly @_ _parent;

			public SecondObserver(@_ parent)
			{
				_parent = parent;
			}

			public void OnCompleted()
			{
				_parent._secondDisposable.Dispose();
			}

			public void OnError(Exception error)
			{
				lock (_parent._gate)
				{
					_parent.ForwardOnError(error);
				}
			}

			public void OnNext(TSecond value)
			{
				lock (_parent._latestGate)
				{
					_parent._latest = value;
				}
				if (!_parent._hasLatest)
				{
					_parent._hasLatest = true;
				}
			}
		}

		private readonly object _gate = new object();

		private readonly object _latestGate = new object();

		private readonly Func<TFirst, TSecond, TResult> _resultSelector;

		private volatile bool _hasLatest;

		private TSecond? _latest;

		private SingleAssignmentDisposableValue _secondDisposable;

		public _(Func<TFirst, TSecond, TResult> resultSelector, IObserver<TResult> observer)
			: base(observer)
		{
			_resultSelector = resultSelector;
		}

		public void Run(IObservable<TFirst> first, IObservable<TSecond> second)
		{
			FirstObserver observer = new FirstObserver(this);
			SecondObserver observer2 = new SecondObserver(this);
			_secondDisposable.Disposable = second.SubscribeSafe(observer2);
			SetUpstream(first.SubscribeSafe(observer));
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_secondDisposable.Dispose();
			}
			base.Dispose(disposing);
		}
	}

	private readonly IObservable<TFirst> _first;

	private readonly IObservable<TSecond> _second;

	private readonly Func<TFirst, TSecond, TResult> _resultSelector;

	public WithLatestFrom(IObservable<TFirst> first, IObservable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
	{
		_first = first;
		_second = second;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(_resultSelector, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_first, _second);
	}
}
