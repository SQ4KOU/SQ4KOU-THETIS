using System.Diagnostics.CodeAnalysis;
using System.Reactive.PlatformServices;

namespace System.Reactive;

internal static class ExceptionHelpers
{
	private static readonly Lazy<IExceptionServices> Services = new Lazy<IExceptionServices>(Initialize);

	[DoesNotReturn]
	public static void Throw(this Exception exception)
	{
		Services.Value.Rethrow(exception);
	}

	private static IExceptionServices Initialize()
	{
		return PlatformEnlightenmentProvider.Current.GetService<IExceptionServices>(Array.Empty<object>()) ?? new DefaultExceptionServices();
	}
}
