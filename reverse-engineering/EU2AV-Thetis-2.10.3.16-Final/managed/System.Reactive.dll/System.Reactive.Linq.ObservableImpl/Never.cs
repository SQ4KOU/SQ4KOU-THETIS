using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Never<TResult> : IObservable<TResult>
{
	internal static readonly IObservable<TResult> Default = new Never<TResult>();

	private Never()
	{
	}

	public IDisposable Subscribe(IObserver<TResult> observer)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		return Disposable.Empty;
	}
}
