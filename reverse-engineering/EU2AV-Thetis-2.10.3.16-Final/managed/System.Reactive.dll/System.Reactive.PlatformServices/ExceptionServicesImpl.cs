using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

namespace System.Reactive.PlatformServices;

internal sealed class ExceptionServicesImpl : IExceptionServices
{
	[DoesNotReturn]
	public void Rethrow(Exception exception)
	{
		ExceptionDispatchInfo.Capture(exception).Throw();
	}
}
