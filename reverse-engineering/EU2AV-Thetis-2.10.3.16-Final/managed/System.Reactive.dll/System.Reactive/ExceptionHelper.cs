using System.IO;
using System.Threading;

namespace System.Reactive;

internal static class ExceptionHelper
{
	public static Exception Terminated { get; } = new EndOfStreamException("On[Error|Completed]");

	public static bool TrySetException(ref Exception? field, Exception ex)
	{
		return Interlocked.CompareExchange(ref field, ex, null) == null;
	}
}
