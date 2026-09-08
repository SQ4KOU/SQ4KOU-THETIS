namespace System.Reactive.Subjects;

public interface IConnectableObservable<out T> : IObservable<T>
{
	IDisposable Connect();
}
