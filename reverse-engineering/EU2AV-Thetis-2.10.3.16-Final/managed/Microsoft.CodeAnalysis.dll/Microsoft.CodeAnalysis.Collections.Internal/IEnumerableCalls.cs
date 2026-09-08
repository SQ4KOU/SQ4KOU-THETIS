using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Collections.Internal;

internal static class IEnumerableCalls
{
	public static IEnumerator GetEnumerator<TEnumerable>(ref TEnumerable enumerable) where TEnumerable : IEnumerable
	{
		return enumerable.GetEnumerator();
	}
}
internal static class IEnumerableCalls<T>
{
	public static IEnumerator<T> GetEnumerator<TEnumerable>(ref TEnumerable enumerable) where TEnumerable : IEnumerable<T>
	{
		return enumerable.GetEnumerator();
	}
}
