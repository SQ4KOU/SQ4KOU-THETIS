using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Reactive.Concurrency;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class SchedulerOperationAwaiter : INotifyCompletion
{
	private readonly Func<Action, IDisposable> _schedule;

	private readonly CancellationToken _cancellationToken;

	private readonly bool _postBackToOriginalContext;

	private readonly CancellationTokenRegistration _ctr;

	private volatile Action? _continuation;

	private volatile IDisposable? _work;

	public bool IsCompleted => _cancellationToken.IsCancellationRequested;

	internal SchedulerOperationAwaiter(Func<Action, IDisposable> schedule, bool postBackToOriginalContext, CancellationToken cancellationToken)
	{
		_schedule = schedule;
		_cancellationToken = cancellationToken;
		_postBackToOriginalContext = postBackToOriginalContext;
		if (cancellationToken.CanBeCanceled)
		{
			_ctr = _cancellationToken.Register(delegate(object @this)
			{
				((SchedulerOperationAwaiter)@this).Cancel();
			}, this);
		}
	}

	public void GetResult()
	{
		_cancellationToken.ThrowIfCancellationRequested();
	}

	public void OnCompleted(Action continuation)
	{
		if (continuation == null)
		{
			throw new ArgumentNullException("continuation");
		}
		if (_continuation != null)
		{
			throw new InvalidOperationException(Strings_Core.SCHEDULER_OPERATION_ALREADY_AWAITED);
		}
		if (_postBackToOriginalContext)
		{
			SynchronizationContext ctx = SynchronizationContext.Current;
			if (ctx != null)
			{
				Action original = continuation;
				continuation = delegate
				{
					ctx.Post(delegate(object a)
					{
						((Action)a)();
					}, original);
				};
			}
		}
		int ran = 0;
		_continuation = delegate
		{
			if (Interlocked.Exchange(ref ran, 1) == 0)
			{
				_ctr.Dispose();
				continuation();
			}
		};
		_work = _schedule(_continuation);
	}

	private void Cancel()
	{
		_work?.Dispose();
		_continuation?.Invoke();
	}
}
