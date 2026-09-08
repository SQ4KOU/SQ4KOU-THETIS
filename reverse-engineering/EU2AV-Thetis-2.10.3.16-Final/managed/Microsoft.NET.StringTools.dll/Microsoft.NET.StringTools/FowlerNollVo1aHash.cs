namespace Microsoft.NET.StringTools;

public static class FowlerNollVo1aHash
{
	private const uint fnvPrimeA32Bit = 16777619u;

	private const uint fnvOffsetBasisA32Bit = 2166136261u;

	private const long fnvPrimeA64Bit = 1099511628211L;

	private const long fnvOffsetBasisA64Bit = -3750763034362895579L;

	public static int ComputeHash32(string text)
	{
		uint num = 2166136261u;
		foreach (char c in text)
		{
			byte b = (byte)c;
			num ^= b;
			num *= 16777619;
			b = (byte)((int)c >> 8);
			num ^= b;
			num *= 16777619;
		}
		return (int)num;
	}

	public static int ComputeHash32Fast(string text)
	{
		uint num = 2166136261u;
		foreach (char c in text)
		{
			num = (num ^ c) * 16777619;
		}
		return (int)num;
	}

	public static long ComputeHash64Fast(string text)
	{
		long num = -3750763034362895579L;
		foreach (char c in text)
		{
			num = (num ^ c) * 1099511628211L;
		}
		return num;
	}

	public static long ComputeHash64(string text)
	{
		long num = -3750763034362895579L;
		foreach (char c in text)
		{
			byte b = (byte)c;
			num ^= b;
			num *= 1099511628211L;
			b = (byte)((int)c >> 8);
			num ^= b;
			num *= 1099511628211L;
		}
		return num;
	}

	public static long Combine64(long left, long right)
	{
		return (left ^ right) * 1099511628211L;
	}
}
