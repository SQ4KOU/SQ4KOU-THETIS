namespace System.Threading.Tasks;

internal static class RoslynValueTaskExtensions
{
	extension(ValueTask)
	{
		public static ValueTask<T> FromResult<T>(T result)
		{
			return new ValueTask<T>(result);
		}

		public static ValueTask CompletedTask => default(ValueTask);
	}
}
