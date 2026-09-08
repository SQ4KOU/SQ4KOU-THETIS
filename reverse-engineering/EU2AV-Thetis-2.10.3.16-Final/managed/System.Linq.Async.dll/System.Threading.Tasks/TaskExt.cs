namespace System.Threading.Tasks;

internal static class TaskExt
{
	public static readonly TaskCompletionSource<bool> True;

	static TaskExt()
	{
		True = new TaskCompletionSource<bool>();
		True.SetResult(result: true);
	}
}
