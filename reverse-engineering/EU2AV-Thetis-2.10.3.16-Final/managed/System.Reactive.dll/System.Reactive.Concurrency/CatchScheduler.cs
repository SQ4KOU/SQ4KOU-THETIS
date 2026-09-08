using System.Reactive.Disposables;
using System.Runtime.CompilerServices;

namespace System.Reactive.Concurrency;

internal sealed class CatchScheduler<TException> : SchedulerWrapper where TException : Exception
{
	private class CatchSchedulerLongRunning : ISchedulerLongRunning
	{
		private readonly ISchedulerLongRunning _scheduler;

		private readonly Func<TException, bool> _handler;

		public CatchSchedulerLongRunning(ISchedulerLongRunning scheduler, Func<TException, bool> handler)
		{
			_scheduler = scheduler;
			_handler = handler;
		}

		public IDisposable ScheduleLongRunning<TState>(TState state, Action<TState, ICancelable> action)
		{
			return _scheduler.ScheduleLongRunning(state, delegate(TState arg, ICancelable cancel)
			{
				try
				{
					action(arg, cancel);
				}
				catch (TException arg2) when (_handler(arg2))
				{
				}
			});
		}
	}

	private sealed class CatchSchedulerPeriodic : ISchedulerPeriodic
	{
		private sealed class PeriodicallyScheduledWorkItem<TState> : IDisposable
		{
			private SingleAssignmentDisposableValue _cancel;

			private bool _failed;

			private readonly Func<TState, TState> _action;

			private readonly CatchSchedulerPeriodic _catchScheduler;

			public PeriodicallyScheduledWorkItem(CatchSchedulerPeriodic scheduler, TState state, TimeSpan period, Func<TState, TState> action)
			{
				_catchScheduler = scheduler;
				_action = action;
				_cancel.Disposable = scheduler._scheduler.SchedulePeriodic(state, period, (TState state2) => Tick(state2).state);
			}

			public void Dispose()
			{
				_cancel.Dispose();
			}

			private (PeriodicallyScheduledWorkItem<TState> @this, TState state) Tick(TState state)
			{
				if (_failed)
				{
					return default((PeriodicallyScheduledWorkItem<TState>, TState));
				}
				try
				{
					return (@this: this, state: _action(state));
				}
				catch (TException arg)
				{
					_failed = true;
					if (!_catchScheduler._handler(arg))
					{
						throw;
					}
					_cancel.Dispose();
					return default((PeriodicallyScheduledWorkItem<TState>, TState));
				}
			}
		}

		private readonly ISchedulerPeriodic _scheduler;

		private readonly Func<TException, bool> _handler;

		public CatchSchedulerPeriodic(ISchedulerPeriodic scheduler, Func<TException, bool> handler)
		{
			_scheduler = scheduler;
			_handler = handler;
		}

		public IDisposable SchedulePeriodic<TState>(TState state, TimeSpan period, Func<TState, TState> action)
		{
			return new PeriodicallyScheduledWorkItem<TState>(this, state, period, action);
		}
	}

	private readonly Func<TException, bool> _handler;

	public CatchScheduler(IScheduler scheduler, Func<TException, bool> handler)
		: base(scheduler)
	{
		_handler = handler;
	}

	protected override Func<IScheduler, TState, IDisposable> Wrap<TState>(Func<IScheduler, TState, IDisposable> action)
	{
		return delegate(IScheduler self, TState state)
		{
			try
			{
				return action(GetRecursiveWrapper(self), state);
			}
			catch (TException arg) when (_handler(arg))
			{
				return Disposable.Empty;
			}
		};
	}

	public CatchScheduler(IScheduler scheduler, Func<TException, bool> handler, ConditionalWeakTable<IScheduler, IScheduler> cache)
		: base(scheduler, cache)
	{
		_handler = handler;
	}

	protected override SchedulerWrapper Clone(IScheduler scheduler, ConditionalWeakTable<IScheduler, IScheduler> cache)
	{
		return new CatchScheduler<TException>(scheduler, _handler, cache);
	}

	protected override bool TryGetService(IServiceProvider provider, Type serviceType, out object? service)
	{
		service = provider.GetService(serviceType);
		if (service != null)
		{
			if (serviceType == typeof(ISchedulerLongRunning))
			{
				service = new CatchSchedulerLongRunning((ISchedulerLongRunning)service, _handler);
			}
			else if (serviceType == typeof(ISchedulerPeriodic))
			{
				service = new CatchSchedulerPeriodic((ISchedulerPeriodic)service, _handler);
			}
		}
		return true;
	}
}
