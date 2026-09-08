using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace System.Reactive.Linq;

internal sealed class GroupedObservable<TKey, TElement> : ObservableBase<TElement>, IGroupedObservable<TKey, TElement>, IObservable<TElement>
{
	private readonly IObservable<TElement> _subject;

	private readonly RefCountDisposable? _refCount;

	public TKey Key { get; }

	public GroupedObservable(TKey key, ISubject<TElement> subject, RefCountDisposable refCount)
	{
		Key = key;
		_subject = subject;
		_refCount = refCount;
	}

	public GroupedObservable(TKey key, ISubject<TElement> subject)
	{
		Key = key;
		_subject = subject;
	}

	protected override IDisposable SubscribeCore(IObserver<TElement> observer)
	{
		if (_refCount != null)
		{
			IDisposable disposable = _refCount.GetDisposable();
			IDisposable disposable2 = _subject.Subscribe(observer);
			return StableCompositeDisposable.Create(disposable, disposable2);
		}
		return _subject.Subscribe(observer);
	}
}
