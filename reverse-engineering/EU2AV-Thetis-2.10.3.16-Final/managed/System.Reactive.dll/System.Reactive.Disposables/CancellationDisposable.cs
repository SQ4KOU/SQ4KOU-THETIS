using System.Threading;

namespace System.Reactive.Disposables;

public sealed class CancellationDisposable : ICancelable, IDisposable
{
	private readonly CancellationTokenSource _cts;

	public CancellationToken Token => _cts.Token;

	public bool IsDisposed => _cts.IsCancellationRequested;

	public CancellationDisposable(CancellationTokenSource cts)
	{
		_cts = cts ?? throw new ArgumentNullException("cts");
	}

	public CancellationDisposable()
		: this(new CancellationTokenSource())
	{
	}

	public void Dispose()
	{
		_cts.Cancel();
	}
}
