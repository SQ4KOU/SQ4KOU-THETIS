using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal static class Generate<TState, TResult>
{
	internal sealed class NoTime : Producer<TResult, NoTime._>
	{
		internal sealed class @_ : IdentitySink<TResult>
		{
			private readonly Func<TState, bool> _condition;

			private readonly Func<TState, TState> _iterate;

			private readonly Func<TState, TResult> _resultSelector;

			private TState _state;

			private bool _first;

			public _(NoTime parent, IObserver<TResult> observer)
				: base(observer)
			{
				_condition = parent._condition;
				_iterate = parent._iterate;
				_resultSelector = parent._resultSelector;
				_state = parent._initialState;
				_first = true;
			}

			public void Run(IScheduler scheduler)
			{
				ISchedulerLongRunning schedulerLongRunning = scheduler.AsLongRunning();
				if (schedulerLongRunning != null)
				{
					SetUpstream(schedulerLongRunning.ScheduleLongRunning(this, delegate(@_ @this, ICancelable c)
					{
						@this.Loop(c);
					}));
				}
				else
				{
					SetUpstream(scheduler.Schedule(this, delegate(@_ @this, Action<@_> a)
					{
						@this.LoopRec(a);
					}));
				}
			}

			private void Loop(ICancelable cancel)
			{
				while (!cancel.IsDisposed)
				{
					TResult value = default(TResult);
					bool flag;
					try
					{
						if (_first)
						{
							_first = false;
						}
						else
						{
							_state = _iterate(_state);
						}
						flag = _condition(_state);
						if (flag)
						{
							value = _resultSelector(_state);
						}
					}
					catch (Exception error)
					{
						ForwardOnError(error);
						return;
					}
					if (!flag)
					{
						break;
					}
					ForwardOnNext(value);
				}
				if (!cancel.IsDisposed)
				{
					ForwardOnCompleted();
				}
			}

			private void LoopRec(Action<@_> recurse)
			{
				TResult value = default(TResult);
				bool flag;
				try
				{
					if (_first)
					{
						_first = false;
					}
					else
					{
						_state = _iterate(_state);
					}
					flag = _condition(_state);
					if (flag)
					{
						value = _resultSelector(_state);
					}
				}
				catch (Exception error)
				{
					ForwardOnError(error);
					return;
				}
				if (flag)
				{
					ForwardOnNext(value);
					recurse(this);
				}
				else
				{
					ForwardOnCompleted();
				}
			}
		}

		private readonly TState _initialState;

		private readonly Func<TState, bool> _condition;

		private readonly Func<TState, TState> _iterate;

		private readonly Func<TState, TResult> _resultSelector;

		private readonly IScheduler _scheduler;

		public NoTime(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, IScheduler scheduler)
		{
			_initialState = initialState;
			_condition = condition;
			_iterate = iterate;
			_resultSelector = resultSelector;
			_scheduler = scheduler;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_scheduler);
		}
	}

	internal sealed class Absolute : Producer<TResult, Absolute._>
	{
		internal sealed class @_ : IdentitySink<TResult>
		{
			private readonly Func<TState, bool> _condition;

			private readonly Func<TState, TState> _iterate;

			private readonly Func<TState, TResult> _resultSelector;

			private readonly Func<TState, DateTimeOffset> _timeSelector;

			private bool _first;

			private bool _hasResult;

			private TResult? _result;

			private MultipleAssignmentDisposableValue _timerDisposable;

			public _(Absolute parent, IObserver<TResult> observer)
				: base(observer)
			{
				_condition = parent._condition;
				_iterate = parent._iterate;
				_resultSelector = parent._resultSelector;
				_timeSelector = parent._timeSelector;
				_first = true;
			}

			public void Run(IScheduler outerScheduler, TState initialState)
			{
				SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
				_timerDisposable.Disposable = singleAssignmentDisposable;
				singleAssignmentDisposable.Disposable = outerScheduler.Schedule((this, initialState), (IScheduler scheduler, (@_ @this, TState initialState) tuple) => tuple.@this.InvokeRec(scheduler, tuple.initialState));
			}

			protected override void Dispose(bool disposing)
			{
				_timerDisposable.Dispose();
				base.Dispose(disposing);
			}

			private IDisposable InvokeRec(IScheduler self, TState state)
			{
				if (_hasResult)
				{
					ForwardOnNext(_result);
				}
				DateTimeOffset dueTime = default(DateTimeOffset);
				try
				{
					if (_first)
					{
						_first = false;
					}
					else
					{
						state = _iterate(state);
					}
					_hasResult = _condition(state);
					if (_hasResult)
					{
						_result = _resultSelector(state);
						dueTime = _timeSelector(state);
					}
				}
				catch (Exception error)
				{
					ForwardOnError(error);
					return Disposable.Empty;
				}
				if (!_hasResult)
				{
					ForwardOnCompleted();
					return Disposable.Empty;
				}
				SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
				_timerDisposable.Disposable = singleAssignmentDisposable;
				singleAssignmentDisposable.Disposable = self.Schedule((this, state), dueTime, (IScheduler scheduler, (@_ @this, TState state) tuple) => tuple.@this.InvokeRec(scheduler, tuple.state));
				return Disposable.Empty;
			}
		}

		private readonly TState _initialState;

		private readonly Func<TState, bool> _condition;

		private readonly Func<TState, TState> _iterate;

		private readonly Func<TState, TResult> _resultSelector;

		private readonly Func<TState, DateTimeOffset> _timeSelector;

		private readonly IScheduler _scheduler;

		public Absolute(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, Func<TState, DateTimeOffset> timeSelector, IScheduler scheduler)
		{
			_initialState = initialState;
			_condition = condition;
			_iterate = iterate;
			_resultSelector = resultSelector;
			_timeSelector = timeSelector;
			_scheduler = scheduler;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_scheduler, _initialState);
		}
	}

	internal sealed class Relative : Producer<TResult, Relative._>
	{
		internal sealed class @_ : IdentitySink<TResult>
		{
			private readonly Func<TState, bool> _condition;

			private readonly Func<TState, TState> _iterate;

			private readonly Func<TState, TResult> _resultSelector;

			private readonly Func<TState, TimeSpan> _timeSelector;

			private bool _first;

			private bool _hasResult;

			private TResult? _result;

			private MultipleAssignmentDisposableValue _timerDisposable;

			public _(Relative parent, IObserver<TResult> observer)
				: base(observer)
			{
				_condition = parent._condition;
				_iterate = parent._iterate;
				_resultSelector = parent._resultSelector;
				_timeSelector = parent._timeSelector;
				_first = true;
			}

			public void Run(IScheduler outerScheduler, TState initialState)
			{
				SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
				_timerDisposable.Disposable = singleAssignmentDisposable;
				singleAssignmentDisposable.Disposable = outerScheduler.Schedule((this, initialState), (IScheduler scheduler, (@_ @this, TState initialState) tuple) => tuple.@this.InvokeRec(scheduler, tuple.initialState));
			}

			protected override void Dispose(bool disposing)
			{
				_timerDisposable.Dispose();
				base.Dispose(disposing);
			}

			private IDisposable InvokeRec(IScheduler self, TState state)
			{
				if (_hasResult)
				{
					ForwardOnNext(_result);
				}
				TimeSpan dueTime = default(TimeSpan);
				try
				{
					if (_first)
					{
						_first = false;
					}
					else
					{
						state = _iterate(state);
					}
					_hasResult = _condition(state);
					if (_hasResult)
					{
						_result = _resultSelector(state);
						dueTime = _timeSelector(state);
					}
				}
				catch (Exception error)
				{
					ForwardOnError(error);
					return Disposable.Empty;
				}
				if (!_hasResult)
				{
					ForwardOnCompleted();
					return Disposable.Empty;
				}
				SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
				_timerDisposable.Disposable = singleAssignmentDisposable;
				singleAssignmentDisposable.Disposable = self.Schedule((this, state), dueTime, (IScheduler scheduler, (@_ @this, TState state) tuple) => tuple.@this.InvokeRec(scheduler, tuple.state));
				return Disposable.Empty;
			}
		}

		private readonly TState _initialState;

		private readonly Func<TState, bool> _condition;

		private readonly Func<TState, TState> _iterate;

		private readonly Func<TState, TResult> _resultSelector;

		private readonly Func<TState, TimeSpan> _timeSelector;

		private readonly IScheduler _scheduler;

		public Relative(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, Func<TState, TimeSpan> timeSelector, IScheduler scheduler)
		{
			_initialState = initialState;
			_condition = condition;
			_iterate = iterate;
			_resultSelector = resultSelector;
			_timeSelector = timeSelector;
			_scheduler = scheduler;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_scheduler, _initialState);
		}
	}
}
