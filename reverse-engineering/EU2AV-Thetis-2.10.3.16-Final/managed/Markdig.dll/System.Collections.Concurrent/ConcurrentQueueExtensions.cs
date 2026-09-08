namespace System.Collections.Concurrent;

internal static class ConcurrentQueueExtensions
{
	public static void Clear<T>(this ConcurrentQueue<T> queue)
	{
		T result;
		while (queue.TryDequeue(out result))
		{
		}
	}
}
