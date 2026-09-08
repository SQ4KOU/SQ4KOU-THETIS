using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace System.Reactive.PlatformServices;

[EditorBrowsable(EditorBrowsableState.Never)]
public class CurrentPlatformEnlightenmentProvider : IPlatformEnlightenmentProvider
{
	public virtual T? GetService<T>(object[] args) where T : class
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle == typeof(IExceptionServices))
		{
			return (T)(object)new ExceptionServicesImpl();
		}
		if (typeFromHandle == typeof(IConcurrencyAbstractionLayer))
		{
			return (T)(object)new ConcurrencyAbstractionLayerImpl();
		}
		if (typeFromHandle == typeof(IScheduler) && args != null)
		{
			switch ((string)args[0])
			{
			case "ThreadPool":
				return (T)(object)ThreadPoolScheduler.Instance;
			case "TaskPool":
				return (T)(object)TaskPoolScheduler.Default;
			case "NewThread":
				return (T)(object)NewThreadScheduler.Default;
			}
		}
		if (typeFromHandle == typeof(IQueryServices) && Debugger.IsAttached)
		{
			return (T)(object)new QueryDebugger();
		}
		return null;
	}
}
