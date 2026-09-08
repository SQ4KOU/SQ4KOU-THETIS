using System.Reactive.PlatformServices;

namespace System.Reactive.Concurrency;

internal static class ConcurrencyAbstractionLayer
{
	public static IConcurrencyAbstractionLayer Current { get; } = Initialize();

	private static IConcurrencyAbstractionLayer Initialize()
	{
		return PlatformEnlightenmentProvider.Current.GetService<IConcurrencyAbstractionLayer>(Array.Empty<object>());
	}
}
