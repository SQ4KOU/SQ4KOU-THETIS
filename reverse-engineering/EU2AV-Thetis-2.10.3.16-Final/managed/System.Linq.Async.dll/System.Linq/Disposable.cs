namespace System.Linq;

internal static class Disposable
{
	public static IDisposable Create(IDisposable d1, IDisposable d2)
	{
		return new BinaryDisposable(d1, d2);
	}

	public static IDisposable Create(Action action)
	{
		return new AnonymousDisposable(action);
	}
}
