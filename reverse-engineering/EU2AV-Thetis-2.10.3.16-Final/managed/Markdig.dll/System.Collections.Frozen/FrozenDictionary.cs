using System.Collections.Generic;

namespace System.Collections.Frozen;

internal sealed class FrozenDictionary<TKey, TValue> : Dictionary<TKey, TValue>
{
	public FrozenDictionary(Dictionary<TKey, TValue> dictionary)
		: base((IDictionary<TKey, TValue>)dictionary)
	{
	}
}
