using System.Threading;

namespace System.Reactive.Concurrency;

public sealed class SchedulerOperation
{
	private readonly Func<Action, IDisposable> _schedule;

	private readonly CancellationToken _cancellationToken;

	private readonly bool _postBackToOriginalContext;

	internal SchedulerOperation(Func<Action, IDisposable> schedule, CancellationToken cancellationToken)
		: this(schedule, postBackToOriginalContext: false, cancellationToken)
	{
	}

	internal SchedulerOperation(Func<Action, IDisposable> schedule, bool postBackToOriginalContext, CancellationToken cancellationToken)
	{
		_schedule = schedule;
		_cancellationToken = cancellationToken;
		_postBackToOriginalContext = postBackToOriginalContext;
	}

	public SchedulerOperation ConfigureAwait(bool continueOnCapturedContext)
	{
		return new SchedulerOperation(_schedule, continueOnCapturedContext, _cancellationToken);
	}

	public SchedulerOperationAwaiter GetAwaiter()
	{
		return new SchedulerOperationAwaiter(_schedule, _postBackToOriginalContext, _cancellationToken);
	}
}
