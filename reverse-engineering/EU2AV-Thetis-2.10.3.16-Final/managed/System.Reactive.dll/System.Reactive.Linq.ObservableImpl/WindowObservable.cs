using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class WindowObservable<TSource> : AddRef<TSource>
{
	public WindowObservable(IObservable<TSource> source, RefCountDisposable refCount)
		: base(source, refCount)
	{
	}
}
