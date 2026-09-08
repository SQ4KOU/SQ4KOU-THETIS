using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord;

public struct MemberSearchIntQuery : IMemberSearchQuery
{
	public MemberSearchV2Range? Range { get; set; }

	public IEnumerable<int> AndQuery { get; set; }

	public IEnumerable<int> OrQuery { get; set; }

	IEnumerable<object> IMemberSearchQuery.AndQuery => AndQuery?.Select((Func<int, object>)((int x) => x));

	IEnumerable<object> IMemberSearchQuery.OrQuery => OrQuery?.Select((Func<int, object>)((int x) => x));
}
