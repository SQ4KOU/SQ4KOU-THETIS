using System.Reactive.PlatformServices;

namespace System.Reactive.Linq;

internal static class QueryServices
{
	private static readonly IQueryServices Services = Initialize();

	public static T GetQueryImpl<T>(T defaultInstance)
	{
		return Services.Extend(defaultInstance);
	}

	private static IQueryServices Initialize()
	{
		return PlatformEnlightenmentProvider.Current.GetService<IQueryServices>(Array.Empty<object>()) ?? new DefaultQueryServices();
	}
}
