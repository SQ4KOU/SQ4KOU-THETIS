namespace System;

internal static class RoslynMemoryExtensions
{
	public static int BinarySearch<TElement, TValue>(this ReadOnlySpan<TElement> span, TValue value, Func<TElement, TValue, int> comparer)
	{
		int num = 0;
		int num2 = span.Length - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num >> 1);
			int num4 = comparer(span[num3], value);
			if (num4 == 0)
			{
				return num3;
			}
			if (num4 > 0)
			{
				num2 = num3 - 1;
			}
			else
			{
				num = num3 + 1;
			}
		}
		return ~num;
	}
}
