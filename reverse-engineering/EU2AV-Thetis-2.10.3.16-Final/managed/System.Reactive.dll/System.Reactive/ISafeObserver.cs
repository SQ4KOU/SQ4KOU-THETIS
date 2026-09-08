namespace System.Reactive;

internal interface ISafeObserver<in T> : IObserver<T>, IDisposable
{
	void SetResource(IDisposable resource);
}
