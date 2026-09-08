namespace System.Reactive.Linq.ObservableImpl;

internal sealed class AmbManyArray<T> : BasicProducer<T>
{
	private readonly IObservable<T>[] _sources;

	public AmbManyArray(IObservable<T>[] sources)
	{
		_sources = sources;
	}

	protected override IDisposable Run(IObserver<T> observer)
	{
		return AmbCoordinator<T>.Create(observer, _sources);
	}
}
