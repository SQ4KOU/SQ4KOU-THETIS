namespace System.Threading.Tasks;

internal static class TaskExt
{
	public static readonly Task<bool> Never;

	public static readonly TaskCompletionSource<bool> True;

	static TaskExt()
	{
		Never = new TaskCompletionSource<bool>().Task;
		True = new TaskCompletionSource<bool>();
		True.SetResult(result: true);
	}
}
