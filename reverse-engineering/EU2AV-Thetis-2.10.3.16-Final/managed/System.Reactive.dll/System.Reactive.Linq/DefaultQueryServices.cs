namespace System.Reactive.Linq;

internal sealed class DefaultQueryServices : IQueryServices
{
	public T Extend<T>(T baseImpl)
	{
		return baseImpl;
	}
}
