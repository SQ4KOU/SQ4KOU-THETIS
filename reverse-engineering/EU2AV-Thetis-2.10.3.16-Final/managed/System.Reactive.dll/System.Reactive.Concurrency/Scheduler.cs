using System.Globalization;
using System.Reactive.Disposables;
using System.Reactive.PlatformServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Reactive.Concurrency;

public static class Scheduler
{
	private sealed class AsyncInvocation<TState> : IDisposable
	{
		private readonly CancellationTokenSource _cts = new CancellationTokenSource();

		private SingleAssignmentDisposableValue _run;

		public IDisposable Run(IScheduler self, TState s, Func<IScheduler, TState, CancellationToken, Task<IDisposable>> action)
		{
			if (_cts.IsCancellationRequested)
			{
				return Disposable.Empty;
			}
			action(new CancelableScheduler(self, _cts.Token), s, _cts.Token).ContinueWith(delegate(Task<IDisposable> t, object thisObject)
			{
				AsyncInvocation<TState> obj = (AsyncInvocation<TState>)thisObject;
				t.Exception?.Handle((Exception e) => e is OperationCanceledException);
				obj._run.Disposable = t.Result;
			}, this, TaskContinuationOptions.NotOnCanceled | TaskContinuationOptions.ExecuteSynchronously);
			return this;
		}

		public void Dispose()
		{
			_cts.Cancel();
			_run.Dispose();
		}
	}

	private sealed class CancelableScheduler : IScheduler
	{
		private readonly IScheduler _scheduler;

		private readonly CancellationToken _cancellationToken;

		public CancellationToken Token => _cancellationToken;

		public DateTimeOffset Now => _scheduler.Now;

		public CancelableScheduler(IScheduler scheduler, CancellationToken cancellationToken)
		{
			_scheduler = scheduler;
			_cancellationToken = cancellationToken;
		}

		public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
		{
			return _scheduler.Schedule(state, action);
		}

		public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
		{
			return _scheduler.Schedule(state, dueTime, action);
		}

		public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
		{
			return _scheduler.Schedule(state, dueTime, action);
		}
	}

	private abstract class InvokeRecBaseState : IDisposable
	{
		protected readonly IScheduler Scheduler;

		protected readonly CompositeDisposable Group = new CompositeDisposable();

		protected InvokeRecBaseState(IScheduler scheduler)
		{
			Scheduler = scheduler;
		}

		public void Dispose()
		{
			Group.Dispose();
		}
	}

	private sealed class InvokeRec1State<TState> : InvokeRecBaseState
	{
		private readonly Action<TState, Action<TState>> _action;

		private readonly Action<TState> _recurseCallback;

		public InvokeRec1State(IScheduler scheduler, Action<TState, Action<TState>> action)
			: base(scheduler)
		{
			_action = action;
			_recurseCallback = delegate(TState state)
			{
				InvokeNext(state);
			};
		}

		private void InvokeNext(TState state)
		{
			SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
			Group.Add(singleAssignmentDisposable);
			singleAssignmentDisposable.Disposable = Scheduler.ScheduleAction((state, singleAssignmentDisposable, this), delegate((TState state, SingleAssignmentDisposable sad, InvokeRec1State<TState> @this) nextState)
			{
				nextState.@this.Group.Remove(nextState.sad);
				nextState.@this.InvokeFirst(nextState.state);
			});
		}

		internal void InvokeFirst(TState state)
		{
			_action(state, _recurseCallback);
		}
	}

	private sealed class InvokeRec2State<TState> : InvokeRecBaseState
	{
		private readonly Action<TState, Action<TState, TimeSpan>> _action;

		private readonly Action<TState, TimeSpan> _recurseCallback;

		public InvokeRec2State(IScheduler scheduler, Action<TState, Action<TState, TimeSpan>> action)
			: base(scheduler)
		{
			_action = action;
			_recurseCallback = delegate(TState state, TimeSpan time)
			{
				InvokeNext(state, time);
			};
		}

		private void InvokeNext(TState state, TimeSpan time)
		{
			SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
			Group.Add(singleAssignmentDisposable);
			singleAssignmentDisposable.Disposable = Scheduler.ScheduleAction((state, singleAssignmentDisposable, this), time, delegate((TState state, SingleAssignmentDisposable sad, InvokeRec2State<TState> @this) nextState)
			{
				nextState.@this.Group.Remove(nextState.sad);
				nextState.@this.InvokeFirst(nextState.state);
			});
		}

		internal void InvokeFirst(TState state)
		{
			_action(state, _recurseCallback);
		}
	}

	private sealed class InvokeRec3State<TState> : InvokeRecBaseState
	{
		private readonly Action<TState, Action<TState, DateTimeOffset>> _action;

		private readonly Action<TState, DateTimeOffset> _recurseCallback;

		public InvokeRec3State(IScheduler scheduler, Action<TState, Action<TState, DateTimeOffset>> action)
			: base(scheduler)
		{
			_action = action;
			_recurseCallback = delegate(TState state, DateTimeOffset dtOffset)
			{
				InvokeNext(state, dtOffset);
			};
		}

		private void InvokeNext(TState state, DateTimeOffset dtOffset)
		{
			SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
			Group.Add(singleAssignmentDisposable);
			singleAssignmentDisposable.Disposable = Scheduler.ScheduleAction((state, singleAssignmentDisposable, this), dtOffset, delegate((TState state, SingleAssignmentDisposable sad, InvokeRec3State<TState> @this) nextState)
			{
				nextState.@this.Group.Remove(nextState.sad);
				nextState.@this.InvokeFirst(nextState.state);
			});
		}

		internal void InvokeFirst(TState state)
		{
			_action(state, _recurseCallback);
		}
	}

	private sealed class SchedulePeriodicStopwatch<TState> : IDisposable
	{
		private readonly IScheduler _scheduler;

		private readonly TimeSpan _period;

		private readonly Func<TState, TState> _action;

		private readonly IStopwatchProvider _stopwatchProvider;

		private TState _state;

		private readonly object _gate = new object();

		private readonly AutoResetEvent _resumeEvent = new AutoResetEvent(initialState: false);

		private volatile int _runState;

		private IStopwatch? _stopwatch;

		private TimeSpan _nextDue;

		private TimeSpan _suspendedAt;

		private TimeSpan _inactiveTime;

		private const int Stopped = 0;

		private const int Running = 1;

		private const int Suspended = 2;

		private const int Disposed = 3;

		private SingleAssignmentDisposableValue _task;

		public SchedulePeriodicStopwatch(IScheduler scheduler, TState state, TimeSpan period, Func<TState, TState> action, IStopwatchProvider stopwatchProvider)
		{
			_scheduler = scheduler;
			_period = period;
			_action = action;
			_stopwatchProvider = stopwatchProvider;
			_state = state;
			_runState = 0;
		}

		public IDisposable Start()
		{
			RegisterHostLifecycleEventHandlers();
			_stopwatch = _stopwatchProvider.StartStopwatch();
			_nextDue = _period;
			_runState = 1;
			_task.Disposable = _scheduler.Schedule(this, _nextDue, delegate(SchedulePeriodicStopwatch<TState> @this, Action<SchedulePeriodicStopwatch<TState>, TimeSpan> a)
			{
				@this.Tick(a);
			});
			return this;
		}

		void IDisposable.Dispose()
		{
			_task.Dispose();
			Cancel();
		}

		private void Tick(Action<SchedulePeriodicStopwatch<TState>, TimeSpan> recurse)
		{
			_nextDue += _period;
			_state = _action(_state);
			TimeSpan arg = default(TimeSpan);
			while (true)
			{
				lock (_gate)
				{
					if (_runState == 1)
					{
						arg = Normalize(_nextDue - (_stopwatch.Elapsed - _inactiveTime));
						break;
					}
					if (_runState == 3)
					{
						return;
					}
				}
				_resumeEvent.WaitOne();
			}
			recurse(this, arg);
		}

		private void Cancel()
		{
			UnregisterHostLifecycleEventHandlers();
			lock (_gate)
			{
				_runState = 3;
				if (!Environment.HasShutdownStarted)
				{
					_resumeEvent.Set();
				}
			}
		}

		private void Suspending(object? sender, HostSuspendingEventArgs args)
		{
			lock (_gate)
			{
				if (_runState == 1)
				{
					_suspendedAt = _stopwatch.Elapsed;
					_runState = 2;
					if (!Environment.HasShutdownStarted)
					{
						_resumeEvent.Reset();
					}
				}
			}
		}

		private void Resuming(object? sender, HostResumingEventArgs args)
		{
			lock (_gate)
			{
				if (_runState == 2)
				{
					_inactiveTime += _stopwatch.Elapsed - _suspendedAt;
					_runState = 1;
					if (!Environment.HasShutdownStarted)
					{
						_resumeEvent.Set();
					}
				}
			}
		}

		private void RegisterHostLifecycleEventHandlers()
		{
			HostLifecycleService.Suspending += Suspending;
			HostLifecycleService.Resuming += Resuming;
			HostLifecycleService.AddRef();
		}

		private void UnregisterHostLifecycleEventHandlers()
		{
			HostLifecycleService.Suspending -= Suspending;
			HostLifecycleService.Resuming -= Resuming;
			HostLifecycleService.Release();
		}
	}

	private sealed class SchedulePeriodicRecursive<TState>
	{
		private readonly IScheduler _scheduler;

		private readonly TimeSpan _period;

		private readonly Func<TState, TState> _action;

		private TState _state;

		private int _pendingTickCount;

		private IDisposable? _cancel;

		private const int TICK = 0;

		private const int DispatchStart = 1;

		private const int DispatchEnd = 2;

		public SchedulePeriodicRecursive(IScheduler scheduler, TState state, TimeSpan period, Func<TState, TState> action)
		{
			_scheduler = scheduler;
			_period = period;
			_action = action;
			_state = state;
		}

		public IDisposable Start()
		{
			_pendingTickCount = 0;
			SingleAssignmentDisposable singleAssignmentDisposable = (SingleAssignmentDisposable)(_cancel = new SingleAssignmentDisposable());
			singleAssignmentDisposable.Disposable = _scheduler.Schedule(0, _period, Tick);
			return singleAssignmentDisposable;
		}

		private void Tick(int command, Action<int, TimeSpan> recurse)
		{
			switch (command)
			{
			case 0:
				recurse(0, _period);
				if (Interlocked.Increment(ref _pendingTickCount) != 1)
				{
					break;
				}
				goto case 1;
			case 1:
				try
				{
					_state = _action(_state);
				}
				catch (Exception exception)
				{
					_cancel.Dispose();
					exception.Throw();
				}
				recurse(2, TimeSpan.Zero);
				break;
			case 2:
				if (Interlocked.Decrement(ref _pendingTickCount) > 0)
				{
					recurse(1, TimeSpan.Zero);
				}
				break;
			}
		}
	}

	private sealed class EmulatedStopwatch : IStopwatch
	{
		private readonly IScheduler _scheduler;

		private readonly DateTimeOffset _start;

		public TimeSpan Elapsed => Normalize(_scheduler.Now - _start);

		public EmulatedStopwatch(IScheduler scheduler)
		{
			_scheduler = scheduler;
			_start = _scheduler.Now;
		}
	}

	private static readonly Lazy<IScheduler> _threadPool = new Lazy<IScheduler>(() => Initialize("ThreadPool"));

	private static readonly Lazy<IScheduler> _newThread = new Lazy<IScheduler>(() => Initialize("NewThread"));

	private static readonly Lazy<IScheduler> _taskPool = new Lazy<IScheduler>(() => Initialize("TaskPool"));

	internal static Type[] Optimizations = new Type[3]
	{
		typeof(ISchedulerLongRunning),
		typeof(IStopwatchProvider),
		typeof(ISchedulerPeriodic)
	};

	public static DateTimeOffset Now => SystemClock.UtcNow;

	public static ImmediateScheduler Immediate => ImmediateScheduler.Instance;

	public static CurrentThreadScheduler CurrentThread => CurrentThreadScheduler.Instance;

	public static DefaultScheduler Default => DefaultScheduler.Instance;

	[Obsolete("This property is no longer supported due to refactoring of the API surface and elimination of platform-specific dependencies. Consider using Scheduler.Default to obtain the platform's most appropriate pool-based scheduler. In order to access a specific pool-based scheduler, please add a reference to the System.Reactive.PlatformServices assembly for your target platform and use the appropriate scheduler in the System.Reactive.Concurrency namespace.")]
	public static IScheduler ThreadPool => _threadPool.Value;

	[Obsolete("This property is no longer supported due to refactoring of the API surface and elimination of platform-specific dependencies. Please use NewThreadScheduler.Default to obtain an instance of this scheduler type.")]
	public static IScheduler NewThread => _newThread.Value;

	[Obsolete("This property is no longer supported due to refactoring of the API surface and elimination of platform-specific dependencies. Please use TaskPoolScheduler.Default to obtain an instance of this scheduler type.")]
	public static IScheduler TaskPool => _taskPool.Value;

	public static SchedulerOperation Yield(this IScheduler scheduler)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return new SchedulerOperation((Action a) => scheduler.Schedule(a), scheduler.GetCancellationToken());
	}

	public static SchedulerOperation Yield(this IScheduler scheduler, CancellationToken cancellationToken)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return new SchedulerOperation((Action a) => scheduler.Schedule(a), cancellationToken);
	}

	public static SchedulerOperation Sleep(this IScheduler scheduler, TimeSpan dueTime)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return new SchedulerOperation((Action a) => scheduler.Schedule(dueTime, a), scheduler.GetCancellationToken());
	}

	public static SchedulerOperation Sleep(this IScheduler scheduler, TimeSpan dueTime, CancellationToken cancellationToken)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return new SchedulerOperation((Action a) => scheduler.Schedule(dueTime, a), cancellationToken);
	}

	public static SchedulerOperation Sleep(this IScheduler scheduler, DateTimeOffset dueTime)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return new SchedulerOperation((Action a) => scheduler.Schedule(dueTime, a), scheduler.GetCancellationToken());
	}

	public static SchedulerOperation Sleep(this IScheduler scheduler, DateTimeOffset dueTime, CancellationToken cancellationToken)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return new SchedulerOperation((Action a) => scheduler.Schedule(dueTime, a), cancellationToken);
	}

	public static IDisposable ScheduleAsync<TState>(this IScheduler scheduler, TState state, Func<IScheduler, TState, CancellationToken, Task> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return ScheduleAsync_(scheduler, state, action);
	}

	public static IDisposable ScheduleAsync<TState>(this IScheduler scheduler, TState state, Func<IScheduler, TState, CancellationToken, Task<IDisposable>> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return ScheduleAsync_(scheduler, state, action);
	}

	public static IDisposable ScheduleAsync(this IScheduler scheduler, Func<IScheduler, CancellationToken, Task> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return ScheduleAsync_(scheduler, action, (IScheduler self, Func<IScheduler, CancellationToken, Task> closureAction, CancellationToken ct) => closureAction(self, ct));
	}

	public static IDisposable ScheduleAsync(this IScheduler scheduler, Func<IScheduler, CancellationToken, Task<IDisposable>> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return ScheduleAsync_(scheduler, action, (IScheduler self, Func<IScheduler, CancellationToken, Task<IDisposable>> closureAction, CancellationToken ct) => closureAction(self, ct));
	}

	public static IDisposable ScheduleAsync<TState>(this IScheduler scheduler, TState state, TimeSpan dueTime, Func<IScheduler, TState, CancellationToken, Task> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return ScheduleAsync_(scheduler, state, dueTime, action);
	}

	public static IDisposable ScheduleAsync<TState>(this IScheduler scheduler, TState state, TimeSpan dueTime, Func<IScheduler, TState, CancellationToken, Task<IDisposable>> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return ScheduleAsync_(scheduler, state, dueTime, action);
	}

	public static IDisposable ScheduleAsync(this IScheduler scheduler, TimeSpan dueTime, Func<IScheduler, CancellationToken, Task> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return ScheduleAsync_(scheduler, action, dueTime, (IScheduler self, Func<IScheduler, CancellationToken, Task> closureAction, CancellationToken ct) => closureAction(self, ct));
	}

	public static IDisposable ScheduleAsync(this IScheduler scheduler, TimeSpan dueTime, Func<IScheduler, CancellationToken, Task<IDisposable>> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return ScheduleAsync_(scheduler, action, dueTime, (IScheduler self, Func<IScheduler, CancellationToken, Task<IDisposable>> closureAction, CancellationToken ct) => closureAction(self, ct));
	}

	public static IDisposable ScheduleAsync<TState>(this IScheduler scheduler, TState state, DateTimeOffset dueTime, Func<IScheduler, TState, CancellationToken, Task> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return ScheduleAsync_(scheduler, state, dueTime, action);
	}

	public static IDisposable ScheduleAsync<TState>(this IScheduler scheduler, TState state, DateTimeOffset dueTime, Func<IScheduler, TState, CancellationToken, Task<IDisposable>> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return ScheduleAsync_(scheduler, state, dueTime, action);
	}

	public static IDisposable ScheduleAsync(this IScheduler scheduler, DateTimeOffset dueTime, Func<IScheduler, CancellationToken, Task> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return ScheduleAsync_(scheduler, action, dueTime, (IScheduler self, Func<IScheduler, CancellationToken, Task> closureAction, CancellationToken ct) => closureAction(self, ct));
	}

	public static IDisposable ScheduleAsync(this IScheduler scheduler, DateTimeOffset dueTime, Func<IScheduler, CancellationToken, Task<IDisposable>> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return ScheduleAsync_(scheduler, action, dueTime, (IScheduler self, Func<IScheduler, CancellationToken, Task<IDisposable>> closureAction, CancellationToken ct) => closureAction(self, ct));
	}

	private static IDisposable ScheduleAsync_<TState>(IScheduler scheduler, TState state, Func<IScheduler, TState, CancellationToken, Task> action)
	{
		return scheduler.Schedule((state, action), (IScheduler self, (TState state, Func<IScheduler, TState, CancellationToken, Task> action) t) => InvokeAsync(self, t.state, t.action));
	}

	private static IDisposable ScheduleAsync_<TState>(IScheduler scheduler, TState state, Func<IScheduler, TState, CancellationToken, Task<IDisposable>> action)
	{
		return scheduler.Schedule((state, action), (IScheduler self, (TState state, Func<IScheduler, TState, CancellationToken, Task<IDisposable>> action) t) => InvokeAsync(self, t.state, t.action));
	}

	private static IDisposable ScheduleAsync_<TState>(IScheduler scheduler, TState state, TimeSpan dueTime, Func<IScheduler, TState, CancellationToken, Task> action)
	{
		return scheduler.Schedule((state, action), dueTime, (IScheduler self, (TState state, Func<IScheduler, TState, CancellationToken, Task> action) t) => InvokeAsync(self, t.state, t.action));
	}

	private static IDisposable ScheduleAsync_<TState>(IScheduler scheduler, TState state, TimeSpan dueTime, Func<IScheduler, TState, CancellationToken, Task<IDisposable>> action)
	{
		return scheduler.Schedule((state, action), dueTime, (IScheduler self, (TState state, Func<IScheduler, TState, CancellationToken, Task<IDisposable>> action) t) => InvokeAsync(self, t.state, t.action));
	}

	private static IDisposable ScheduleAsync_<TState>(IScheduler scheduler, TState state, DateTimeOffset dueTime, Func<IScheduler, TState, CancellationToken, Task> action)
	{
		return scheduler.Schedule((state, action), dueTime, (IScheduler self, (TState state, Func<IScheduler, TState, CancellationToken, Task> action) t) => InvokeAsync(self, t.state, t.action));
	}

	private static IDisposable ScheduleAsync_<TState>(IScheduler scheduler, TState state, DateTimeOffset dueTime, Func<IScheduler, TState, CancellationToken, Task<IDisposable>> action)
	{
		return scheduler.Schedule((state, action), dueTime, (IScheduler self, (TState state, Func<IScheduler, TState, CancellationToken, Task<IDisposable>> action) t) => InvokeAsync(self, t.state, t.action));
	}

	private static IDisposable InvokeAsync<TState>(IScheduler self, TState s, Func<IScheduler, TState, CancellationToken, Task<IDisposable>> action)
	{
		return new AsyncInvocation<TState>().Run(self, s, action);
	}

	private static IDisposable InvokeAsync<TState>(IScheduler self, TState s, Func<IScheduler, TState, CancellationToken, Task> action)
	{
		return InvokeAsync(self, (action, s), (IScheduler self_, (Func<IScheduler, TState, CancellationToken, Task> action, TState state) t, CancellationToken ct) => t.action(self_, t.state, ct).ContinueWith((Task _) => Disposable.Empty));
	}

	private static CancellationToken GetCancellationToken(this IScheduler scheduler)
	{
		if (!(scheduler is CancelableScheduler cancelableScheduler))
		{
			return CancellationToken.None;
		}
		return cancelableScheduler.Token;
	}

	public static TimeSpan Normalize(TimeSpan timeSpan)
	{
		if (timeSpan.Ticks >= 0)
		{
			return timeSpan;
		}
		return TimeSpan.Zero;
	}

	private static IScheduler Initialize(string name)
	{
		return PlatformEnlightenmentProvider.Current.GetService<IScheduler>(new object[1] { name }) ?? throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Strings_Core.CANT_OBTAIN_SCHEDULER, name));
	}

	public static IDisposable Schedule(this IScheduler scheduler, Action<Action> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return scheduler.Schedule(action, delegate(Action<Action> a, Action<Action<Action>> self)
		{
			a(delegate
			{
				self(a);
			});
		});
	}

	public static IDisposable Schedule<TState>(this IScheduler scheduler, TState state, Action<TState, Action<TState>> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return scheduler.Schedule((state, action), (IScheduler s, (TState state, Action<TState, Action<TState>> action) p) => InvokeRec1(s, p));
	}

	private static IDisposable InvokeRec1<TState>(IScheduler scheduler, (TState state, Action<TState, Action<TState>> action) tuple)
	{
		InvokeRec1State<TState> invokeRec1State = new InvokeRec1State<TState>(scheduler, tuple.action);
		invokeRec1State.InvokeFirst(tuple.state);
		return invokeRec1State;
	}

	public static IDisposable Schedule(this IScheduler scheduler, TimeSpan dueTime, Action<Action<TimeSpan>> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return scheduler.Schedule(action, dueTime, delegate(Action<Action<TimeSpan>> a, Action<Action<Action<TimeSpan>>, TimeSpan> self)
		{
			a(delegate(TimeSpan ts)
			{
				self(a, ts);
			});
		});
	}

	public static IDisposable Schedule<TState>(this IScheduler scheduler, TState state, TimeSpan dueTime, Action<TState, Action<TState, TimeSpan>> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return scheduler.Schedule((state, action), dueTime, (IScheduler s, (TState state, Action<TState, Action<TState, TimeSpan>> action) p) => InvokeRec2(s, p));
	}

	private static IDisposable InvokeRec2<TState>(IScheduler scheduler, (TState state, Action<TState, Action<TState, TimeSpan>> action) tuple)
	{
		InvokeRec2State<TState> invokeRec2State = new InvokeRec2State<TState>(scheduler, tuple.action);
		invokeRec2State.InvokeFirst(tuple.state);
		return invokeRec2State;
	}

	public static IDisposable Schedule(this IScheduler scheduler, DateTimeOffset dueTime, Action<Action<DateTimeOffset>> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return scheduler.Schedule(action, dueTime, delegate(Action<Action<DateTimeOffset>> a, Action<Action<Action<DateTimeOffset>>, DateTimeOffset> self)
		{
			a(delegate(DateTimeOffset dt)
			{
				self(a, dt);
			});
		});
	}

	public static IDisposable Schedule<TState>(this IScheduler scheduler, TState state, DateTimeOffset dueTime, Action<TState, Action<TState, DateTimeOffset>> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return scheduler.Schedule((state, action), dueTime, (IScheduler s, (TState state, Action<TState, Action<TState, DateTimeOffset>> action) p) => InvokeRec3(s, p));
	}

	private static IDisposable InvokeRec3<TState>(IScheduler scheduler, (TState state, Action<TState, Action<TState, DateTimeOffset>> action) tuple)
	{
		InvokeRec3State<TState> invokeRec3State = new InvokeRec3State<TState>(scheduler, tuple.action);
		invokeRec3State.InvokeFirst(tuple.state);
		return invokeRec3State;
	}

	public static ISchedulerLongRunning? AsLongRunning(this IScheduler scheduler)
	{
		return As<ISchedulerLongRunning>(scheduler);
	}

	public static IStopwatchProvider? AsStopwatchProvider(this IScheduler scheduler)
	{
		return As<IStopwatchProvider>(scheduler);
	}

	public static ISchedulerPeriodic? AsPeriodic(this IScheduler scheduler)
	{
		return As<ISchedulerPeriodic>(scheduler);
	}

	private static T? As<T>(IScheduler scheduler) where T : class
	{
		if (scheduler is IServiceProvider serviceProvider)
		{
			return (T)serviceProvider.GetService(typeof(T));
		}
		return null;
	}

	public static IDisposable SchedulePeriodic<TState>(this IScheduler scheduler, TState state, TimeSpan period, Func<TState, TState> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (period < TimeSpan.Zero)
		{
			throw new ArgumentOutOfRangeException("period");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return SchedulePeriodic_(scheduler, state, period, action);
	}

	public static IDisposable SchedulePeriodic<TState>(this IScheduler scheduler, TState state, TimeSpan period, Action<TState> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (period < TimeSpan.Zero)
		{
			throw new ArgumentOutOfRangeException("period");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return SchedulePeriodic_(scheduler, (state, action), period, delegate((TState state, Action<TState> action) t)
		{
			t.action(t.state);
			return t;
		});
	}

	public static IDisposable SchedulePeriodic(this IScheduler scheduler, TimeSpan period, Action action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (period < TimeSpan.Zero)
		{
			throw new ArgumentOutOfRangeException("period");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return SchedulePeriodic_(scheduler, action, period, delegate(Action a)
		{
			a();
			return a;
		});
	}

	public static IStopwatch StartStopwatch(this IScheduler scheduler)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		IStopwatchProvider stopwatchProvider = scheduler.AsStopwatchProvider();
		if (stopwatchProvider != null)
		{
			return stopwatchProvider.StartStopwatch();
		}
		return new EmulatedStopwatch(scheduler);
	}

	private static IDisposable SchedulePeriodic_<TState>(IScheduler scheduler, TState state, TimeSpan period, Func<TState, TState> action)
	{
		ISchedulerPeriodic schedulerPeriodic = scheduler.AsPeriodic();
		if (schedulerPeriodic != null)
		{
			return schedulerPeriodic.SchedulePeriodic(state, period, action);
		}
		IStopwatchProvider stopwatchProvider = scheduler.AsStopwatchProvider();
		if (stopwatchProvider != null)
		{
			return new SchedulePeriodicStopwatch<TState>(scheduler, state, period, action, stopwatchProvider).Start();
		}
		return new SchedulePeriodicRecursive<TState>(scheduler, state, period, action).Start();
	}

	public static IDisposable Schedule(this IScheduler scheduler, Action action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return scheduler.Schedule(action, (IScheduler _, Action a) => Invoke(a));
	}

	internal static IDisposable ScheduleAction<TState>(this IScheduler scheduler, TState state, Action<TState> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return scheduler.Schedule((action, state), delegate(IScheduler _, (Action<TState> action, TState state) tuple)
		{
			tuple.action(tuple.state);
			return Disposable.Empty;
		});
	}

	internal static IDisposable ScheduleAction<TState>(this IScheduler scheduler, TState state, Func<TState, IDisposable> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return scheduler.Schedule((action, state), (IScheduler _, (Func<TState, IDisposable> action, TState state) tuple) => tuple.action(tuple.state));
	}

	public static IDisposable Schedule(this IScheduler scheduler, TimeSpan dueTime, Action action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return scheduler.Schedule(action, dueTime, (IScheduler _, Action a) => Invoke(a));
	}

	internal static IDisposable ScheduleAction<TState>(this IScheduler scheduler, TState state, TimeSpan dueTime, Action<TState> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return scheduler.Schedule((state, action), dueTime, (IScheduler _, (TState state, Action<TState> action) tuple) => Invoke(tuple));
	}

	internal static IDisposable ScheduleAction<TState>(this IScheduler scheduler, TState state, TimeSpan dueTime, Func<TState, IDisposable> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return scheduler.Schedule((state, action), dueTime, (IScheduler _, (TState state, Func<TState, IDisposable> action) tuple) => Invoke(tuple));
	}

	public static IDisposable Schedule(this IScheduler scheduler, DateTimeOffset dueTime, Action action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return scheduler.Schedule(action, dueTime, (IScheduler _, Action a) => Invoke(a));
	}

	internal static IDisposable ScheduleAction<TState>(this IScheduler scheduler, TState state, DateTimeOffset dueTime, Action<TState> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return scheduler.Schedule((state, action), dueTime, (IScheduler _, (TState state, Action<TState> action) tuple) => Invoke(tuple));
	}

	internal static IDisposable ScheduleAction<TState>(this IScheduler scheduler, TState state, DateTimeOffset dueTime, Func<TState, IDisposable> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return scheduler.Schedule((state, action), dueTime, (IScheduler _, (TState state, Func<TState, IDisposable> action) tuple) => Invoke(tuple));
	}

	public static IDisposable ScheduleLongRunning(this ISchedulerLongRunning scheduler, Action<ICancelable> action)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return scheduler.ScheduleLongRunning(action, delegate(Action<ICancelable> a, ICancelable c)
		{
			a(c);
		});
	}

	private static IDisposable Invoke(Action action)
	{
		action();
		return Disposable.Empty;
	}

	private static IDisposable Invoke<TState>((TState state, Action<TState> action) tuple)
	{
		tuple.action(tuple.state);
		return Disposable.Empty;
	}

	private static IDisposable Invoke<TState>((TState state, Func<TState, IDisposable> action) tuple)
	{
		return tuple.action(tuple.state);
	}

	public static IScheduler DisableOptimizations(this IScheduler scheduler)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return new DisableOptimizationsScheduler(scheduler);
	}

	public static IScheduler DisableOptimizations(this IScheduler scheduler, params Type[] optimizationInterfaces)
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (optimizationInterfaces == null)
		{
			throw new ArgumentNullException("optimizationInterfaces");
		}
		return new DisableOptimizationsScheduler(scheduler, optimizationInterfaces);
	}

	public static IScheduler Catch<TException>(this IScheduler scheduler, Func<TException, bool> handler) where TException : Exception
	{
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		return new CatchScheduler<TException>(scheduler, handler);
	}
}
