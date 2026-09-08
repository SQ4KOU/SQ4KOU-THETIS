namespace System.Reactive.Subjects;

public abstract class SubjectBase<T> : ISubject<T>, ISubject<T, T>, IObserver<T>, IObservable<T>, IDisposable
{
	public abstract bool HasObservers { get; }

	public abstract bool IsDisposed { get; }

	public abstract void Dispose();

	public abstract void OnCompleted();

	public abstract void OnError(Exception error);

	public abstract void OnNext(T value);

	public abstract IDisposable Subscribe(IObserver<T> observer);
}
