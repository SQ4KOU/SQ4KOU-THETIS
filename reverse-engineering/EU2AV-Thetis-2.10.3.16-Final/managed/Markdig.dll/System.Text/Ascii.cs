namespace System.Text;

internal static class Ascii
{
	public static bool IsValid(this string value)
	{
		return value.AsSpan().IsValid();
	}

	public static bool IsValid(this ReadOnlySpan<char> value)
	{
		for (int i = 0; i < value.Length; i++)
		{
			if (value[i] > '\u007f')
			{
				return false;
			}
		}
		return true;
	}
}
