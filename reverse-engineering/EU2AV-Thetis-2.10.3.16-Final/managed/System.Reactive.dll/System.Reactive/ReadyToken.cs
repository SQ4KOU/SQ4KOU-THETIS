namespace System.Reactive;

internal static class ReadyToken
{
	private sealed class ReadyDisposable : IDisposable
	{
		public void Dispose()
		{
		}
	}

	internal static readonly IDisposable Ready = new ReadyDisposable();
}
