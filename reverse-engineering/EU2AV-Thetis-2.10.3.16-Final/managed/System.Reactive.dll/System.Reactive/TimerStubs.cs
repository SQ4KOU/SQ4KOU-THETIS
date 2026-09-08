using System.Threading;

namespace System.Reactive;

internal static class TimerStubs
{
	public static readonly Timer Never = new Timer(delegate
	{
	});
}
