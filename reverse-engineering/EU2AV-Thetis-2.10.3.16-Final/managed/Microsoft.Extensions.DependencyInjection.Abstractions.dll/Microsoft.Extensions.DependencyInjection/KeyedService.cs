namespace Microsoft.Extensions.DependencyInjection;

public static class KeyedService
{
	private sealed class AnyKeyObj
	{
		public override string ToString()
		{
			return "*";
		}
	}

	public static object AnyKey { get; } = new AnyKeyObj();
}
