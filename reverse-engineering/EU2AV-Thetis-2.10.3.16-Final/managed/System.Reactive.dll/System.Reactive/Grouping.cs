using System.Collections.Generic;
using System.Reactive.Subjects;

namespace System.Reactive;

internal sealed class Grouping<TKey, TElement> : Dictionary<TKey, Subject<TElement>>
{
	public Grouping(IEqualityComparer<TKey> comparer)
		: base(comparer)
	{
	}

	public Grouping(int capacity, IEqualityComparer<TKey> comparer)
		: base(capacity, comparer)
	{
	}
}
