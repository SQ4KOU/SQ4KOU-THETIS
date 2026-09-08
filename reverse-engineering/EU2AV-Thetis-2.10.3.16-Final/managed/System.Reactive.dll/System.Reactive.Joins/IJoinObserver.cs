namespace System.Reactive.Joins;

internal interface IJoinObserver : IDisposable
{
	void Subscribe(object gate);

	void Dequeue();
}
