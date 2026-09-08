using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Concurrency;

public class SynchronizationContextScheduler : LocalScheduler
{
	private readonly SynchronizationContext _context;

	private readonly bool _alwaysPost;

	public SynchronizationContextScheduler(SynchronizationContext context)
	{
		_context = context ?? throw new ArgumentNullException("context");
		_alwaysPost = true;
	}

	public SynchronizationContextScheduler(SynchronizationContext context, bool alwaysPost)
	{
		_context = context ?? throw new ArgumentNullException("context");
		_alwaysPost = alwaysPost;
	}

	public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (!_alwaysPost && _context == SynchronizationContext.Current)
		{
			return action(this, state);
		}
		SingleAssignmentDisposable d = new SingleAssignmentDisposable();
		_context.PostWithStartComplete(delegate
		{
			if (!d.IsDisposed)
			{
				d.Disposable = action(this, state);
			}
		});
		return d;
	}

	public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		TimeSpan dueTime2 = Scheduler.Normalize(dueTime);
		if (dueTime2.Ticks == 0L)
		{
			return Schedule(state, action);
		}
		return DefaultScheduler.Instance.Schedule(state, dueTime2, (IScheduler _, TState state2) => Schedule(state2, action));
	}
}
