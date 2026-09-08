namespace System.Reactive.Concurrency;

public sealed class TaskObservationOptions
{
	internal readonly struct Value
	{
		public IScheduler? Scheduler { get; }

		public bool IgnoreExceptionsAfterUnsubscribe { get; }

		internal Value(IScheduler? scheduler, bool ignoreExceptionsAfterUnsubscribe)
		{
			Scheduler = scheduler;
			IgnoreExceptionsAfterUnsubscribe = ignoreExceptionsAfterUnsubscribe;
		}
	}

	public IScheduler? Scheduler { get; }

	public bool IgnoreExceptionsAfterUnsubscribe { get; }

	public TaskObservationOptions(IScheduler? scheduler, bool ignoreExceptionsAfterUnsubscribe)
	{
		Scheduler = scheduler;
		IgnoreExceptionsAfterUnsubscribe = ignoreExceptionsAfterUnsubscribe;
	}

	internal Value ToValue()
	{
		return new Value(Scheduler, IgnoreExceptionsAfterUnsubscribe);
	}
}
