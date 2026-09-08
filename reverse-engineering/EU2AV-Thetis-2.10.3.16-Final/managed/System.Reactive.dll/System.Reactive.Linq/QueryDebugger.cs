namespace System.Reactive.Linq;

public class QueryDebugger : IQueryServices
{
	T IQueryServices.Extend<T>(T baseImpl)
	{
		return baseImpl;
	}
}
