namespace System.Reactive;

internal static class Stubs<T>
{
	public static readonly Action<T> Ignore = delegate
	{
	};

	public static readonly Func<T, T> I = (T _) => _;
}
internal static class Stubs
{
	public static readonly Action Nop = delegate
	{
	};

	public static readonly Action<Exception> Throw = delegate(Exception ex)
	{
		ex.Throw();
	};
}
