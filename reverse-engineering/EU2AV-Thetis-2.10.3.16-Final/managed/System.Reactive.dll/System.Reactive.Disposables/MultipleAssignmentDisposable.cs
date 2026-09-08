namespace System.Reactive.Disposables;

public sealed class MultipleAssignmentDisposable : ICancelable, IDisposable
{
	private MultipleAssignmentDisposableValue _current;

	public bool IsDisposed => _current.IsDisposed;

	public IDisposable? Disposable
	{
		get
		{
			return _current.Disposable;
		}
		set
		{
			_current.Disposable = value;
		}
	}

	public void Dispose()
	{
		_current.Dispose();
	}
}
