using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class AmbManyEnumerable<T> : BasicProducer<T>
{
	private readonly IEnumerable<IObservable<T>> _sources;

	public AmbManyEnumerable(IEnumerable<IObservable<T>> sources)
	{
		_sources = sources;
	}

	protected override IDisposable Run(IObserver<T> observer)
	{
		IEnumerable<IObservable<T>> sources = _sources;
		IObservable<T>[] sources2;
		try
		{
			sources2 = sources.ToArray();
		}
		catch (Exception error)
		{
			observer.OnError(error);
			return Disposable.Empty;
		}
		return AmbCoordinator<T>.Create(observer, sources2);
	}
}
