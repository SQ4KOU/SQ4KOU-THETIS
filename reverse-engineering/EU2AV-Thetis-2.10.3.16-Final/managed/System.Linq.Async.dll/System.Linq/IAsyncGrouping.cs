using System.Collections.Generic;

namespace System.Linq;

public interface IAsyncGrouping<out TKey, out TElement> : IAsyncEnumerable<TElement>
{
	TKey Key { get; }
}
