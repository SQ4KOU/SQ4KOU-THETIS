namespace System.Reactive;

internal interface IScheduledObserver<T> : IObserver<T>, IDisposable
{
	void EnsureActive();

	void EnsureActive(int count);
}
