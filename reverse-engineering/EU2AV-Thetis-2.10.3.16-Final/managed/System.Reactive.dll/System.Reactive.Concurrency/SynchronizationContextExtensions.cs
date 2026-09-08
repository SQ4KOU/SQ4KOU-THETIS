using System.Threading;

namespace System.Reactive.Concurrency;

internal static class SynchronizationContextExtensions
{
	public static void PostWithStartComplete<T>(this SynchronizationContext context, Action<T> action, T state)
	{
		context.OperationStarted();
		context.Post(delegate(object o)
		{
			try
			{
				action((T)o);
			}
			finally
			{
				context.OperationCompleted();
			}
		}, state);
	}

	public static void PostWithStartComplete(this SynchronizationContext context, Action action)
	{
		context.OperationStarted();
		context.Post(delegate
		{
			try
			{
				action();
			}
			finally
			{
				context.OperationCompleted();
			}
		}, null);
	}
}
