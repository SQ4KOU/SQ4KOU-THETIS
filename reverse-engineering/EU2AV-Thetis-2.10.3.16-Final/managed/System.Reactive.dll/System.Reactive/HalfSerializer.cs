#define TRACE
using System.Diagnostics;
using System.Threading;

namespace System.Reactive;

internal static class HalfSerializer
{
	public static void ForwardOnNext<T>(ISink<T> sink, T item, ref int wip, ref Exception? error)
	{
		if (Interlocked.CompareExchange(ref wip, 1, 0) == 0)
		{
			sink.ForwardOnNext(item);
			if (Interlocked.Decrement(ref wip) != 0)
			{
				Exception ex = error;
				if (ex != ExceptionHelper.Terminated)
				{
					error = ExceptionHelper.Terminated;
					sink.ForwardOnError(ex);
				}
				else
				{
					sink.ForwardOnCompleted();
				}
			}
		}
		else if (error == null)
		{
			Trace.TraceWarning("OnNext called while another OnNext call was in progress on the same Observer.");
		}
	}

	public static void ForwardOnError<T>(ISink<T> sink, Exception ex, ref int wip, ref Exception? error)
	{
		if (ExceptionHelper.TrySetException(ref error, ex) && Interlocked.Increment(ref wip) == 1)
		{
			error = ExceptionHelper.Terminated;
			sink.ForwardOnError(ex);
		}
	}

	public static void ForwardOnCompleted<T>(ISink<T> sink, ref int wip, ref Exception? error)
	{
		if (ExceptionHelper.TrySetException(ref error, ExceptionHelper.Terminated) && Interlocked.Increment(ref wip) == 1)
		{
			sink.ForwardOnCompleted();
		}
	}
}
