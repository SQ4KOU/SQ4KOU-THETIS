namespace System.Reactive;

internal interface IProducer<out TSource> : IObservable<TSource>
{
	IDisposable SubscribeRaw(IObserver<TSource> observer, bool enableSafeguard);
}
