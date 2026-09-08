namespace System.Reactive.Disposables;

public sealed class BooleanDisposable : ICancelable, IDisposable
{
	internal static readonly BooleanDisposable True = new BooleanDisposable(isDisposed: true);

	private volatile bool _isDisposed;

	public bool IsDisposed => _isDisposed;

	public BooleanDisposable()
	{
	}

	private BooleanDisposable(bool isDisposed)
	{
		_isDisposed = isDisposed;
	}

	public void Dispose()
	{
		_isDisposed = true;
	}
}
