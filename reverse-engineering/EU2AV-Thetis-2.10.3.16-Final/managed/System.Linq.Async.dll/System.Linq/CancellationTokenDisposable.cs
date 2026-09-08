using System.Threading;

namespace System.Linq;

internal sealed class CancellationTokenDisposable : IDisposable
{
	private readonly CancellationTokenSource _cts = new CancellationTokenSource();

	public CancellationToken Token => _cts.Token;

	public void Dispose()
	{
		if (!_cts.IsCancellationRequested)
		{
			_cts.Cancel();
		}
	}
}
