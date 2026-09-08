using System.Collections.Generic;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Collections.Internal;

namespace System.Linq;

internal static class RoslynEnumerable
{
	public static SegmentedList<TSource> ToSegmentedList<TSource>(this IEnumerable<TSource> source)
	{
		if (source == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
		}
		return new SegmentedList<TSource>(source);
	}
}
