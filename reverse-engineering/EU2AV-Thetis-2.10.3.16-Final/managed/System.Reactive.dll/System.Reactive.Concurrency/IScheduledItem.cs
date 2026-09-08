namespace System.Reactive.Concurrency;

public interface IScheduledItem<TAbsolute>
{
	TAbsolute DueTime { get; }

	void Invoke();
}
