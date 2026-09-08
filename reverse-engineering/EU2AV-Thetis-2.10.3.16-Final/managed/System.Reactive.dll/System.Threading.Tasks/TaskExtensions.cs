namespace System.Threading.Tasks;

internal static class TaskExtensions
{
	public static Task ContinueWithState<TState>(this Task task, Action<Task, TState> continuationAction, TState state, TaskContinuationOptions continuationOptions, CancellationToken cancellationToken)
	{
		return task.ContinueWith(delegate(Task t, object tupleObject)
		{
			var (action, arg) = ((Action<Task, TState>, TState))tupleObject;
			action(t, arg);
		}, (continuationAction, state), cancellationToken, continuationOptions, TaskScheduler.Default);
	}

	public static Task ContinueWithState<TResult, TState>(this Task<TResult> task, Action<Task<TResult>, TState> continuationAction, TState state, CancellationToken cancellationToken)
	{
		return task.ContinueWith(delegate(Task<TResult> t, object tupleObject)
		{
			var (action, arg) = ((Action<Task<TResult>, TState>, TState))tupleObject;
			action(t, arg);
		}, (continuationAction, state), cancellationToken);
	}

	public static Task ContinueWithState<TResult, TState>(this Task<TResult> task, Action<Task<TResult>, TState> continuationAction, TState state, TaskContinuationOptions continuationOptions, CancellationToken cancellationToken)
	{
		return task.ContinueWith(delegate(Task<TResult> t, object tupleObject)
		{
			var (action, arg) = ((Action<Task<TResult>, TState>, TState))tupleObject;
			action(t, arg);
		}, (continuationAction, state), cancellationToken, continuationOptions, TaskScheduler.Default);
	}
}
