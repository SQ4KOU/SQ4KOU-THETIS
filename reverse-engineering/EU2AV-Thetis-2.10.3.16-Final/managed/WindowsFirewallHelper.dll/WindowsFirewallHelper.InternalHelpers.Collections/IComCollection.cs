using System.Collections;
using System.Collections.Generic;

namespace WindowsFirewallHelper.InternalHelpers.Collections;

public interface IComCollection<in TKey, TValue> : ICollection<TValue>, IEnumerable<TValue>, IEnumerable, ICollection where TValue : class
{
	TValue this[TKey key] { get; }

	bool Contains(TKey key);

	bool Remove(TKey key);
}
