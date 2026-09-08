using System.Threading;

namespace System.Linq;

internal sealed class AnonymousDisposable : IDisposable
{
	private Action? _action;

	public AnonymousDisposable(Action action)
	{
		_action = action;
	}

	public void Dispose()
	{
		Interlocked.Exchange(ref _action, null)?.Invoke();
	}
}
