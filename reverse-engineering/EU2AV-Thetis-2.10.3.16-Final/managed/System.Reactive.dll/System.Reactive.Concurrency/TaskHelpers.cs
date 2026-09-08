using System.Threading;
using System.Threading.Tasks;

namespace System.Reactive.Concurrency;

internal static class TaskHelpers
{
	private const int MaxDelay = int.MaxValue;

	public static Task Delay(TimeSpan delay, CancellationToken token)
	{
		if ((long)delay.TotalMilliseconds > int.MaxValue)
		{
			TimeSpan remainder = delay - TimeSpan.FromMilliseconds(2147483647.0);
			return Task.Delay(int.MaxValue, token).ContinueWith((Task _) => Delay(remainder, token), TaskContinuationOptions.ExecuteSynchronously).Unwrap();
		}
		return Task.Delay(delay, token);
	}

	public static Exception GetSingleException(this Task t)
	{
		if (t.Exception.InnerException != null)
		{
			return t.Exception.InnerException;
		}
		return t.Exception;
	}
}
