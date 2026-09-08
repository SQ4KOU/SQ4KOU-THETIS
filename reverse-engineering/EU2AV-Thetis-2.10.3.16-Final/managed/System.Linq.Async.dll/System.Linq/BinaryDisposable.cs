using System.Threading;

namespace System.Linq;

internal sealed class BinaryDisposable : IDisposable
{
	private IDisposable? _d1;

	private IDisposable? _d2;

	public BinaryDisposable(IDisposable d1, IDisposable d2)
	{
		_d1 = d1;
		_d2 = d2;
	}

	public void Dispose()
	{
		IDisposable disposable = Interlocked.Exchange(ref _d1, null);
		if (disposable != null)
		{
			disposable.Dispose();
			Interlocked.Exchange(ref _d2, null)?.Dispose();
		}
	}
}
