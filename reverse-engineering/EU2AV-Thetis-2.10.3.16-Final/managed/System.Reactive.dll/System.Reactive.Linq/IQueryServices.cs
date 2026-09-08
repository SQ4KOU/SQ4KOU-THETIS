namespace System.Reactive.Linq;

internal interface IQueryServices
{
	T Extend<T>(T baseImpl);
}
