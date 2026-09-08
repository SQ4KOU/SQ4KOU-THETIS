namespace System.Reactive;

internal static class Helpers
{
	public static bool All(this bool[] values)
	{
		for (int i = 0; i < values.Length; i++)
		{
			if (!values[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool AllExcept(this bool[] values, int index)
	{
		for (int i = 0; i < values.Length; i++)
		{
			if (i != index && !values[i])
			{
				return false;
			}
		}
		return true;
	}
}
