using System.Reactive.Concurrency;
using System.Threading;

namespace System.Reactive.Disposables;

public sealed class ContextDisposable : ICancelable, IDisposable
{
	private volatile IDisposable _disposable;

	public SynchronizationContext Context { get; }

	public bool IsDisposed => _disposable == BooleanDisposable.True;

	public ContextDisposable(SynchronizationContext context, IDisposable disposable)
	{
		Context = context ?? throw new ArgumentNullException("context");
		_disposable = disposable ?? throw new ArgumentNullException("disposable");
	}

	public void Dispose()
	{
		IDisposable disposable = Interlocked.Exchange(ref _disposable, BooleanDisposable.True);
		if (disposable != BooleanDisposable.True)
		{
			Context.PostWithStartComplete(delegate(IDisposable d)
			{
				d.Dispose();
			}, disposable);
		}
	}
}
