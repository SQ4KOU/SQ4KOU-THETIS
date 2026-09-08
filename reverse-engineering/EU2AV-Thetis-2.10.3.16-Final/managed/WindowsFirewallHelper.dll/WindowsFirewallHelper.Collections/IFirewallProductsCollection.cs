using System.Collections;
using System.Collections.Generic;

namespace WindowsFirewallHelper.Collections;

public interface IFirewallProductsCollection : ICollection<FirewallProduct>, IEnumerable<FirewallProduct>, IEnumerable
{
	FirewallProduct this[int index] { get; }

	int IndexOf(FirewallProduct product);
}
