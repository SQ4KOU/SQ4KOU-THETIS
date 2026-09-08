using System.Collections.Generic;

namespace Discord;

public struct MemberSearchStringQuery : IMemberSearchQuery
{
	public MemberSearchV2Range? Range { get; set; }

	public IEnumerable<string> AndQuery { get; set; }

	public IEnumerable<string> OrQuery { get; set; }

	IEnumerable<object> IMemberSearchQuery.AndQuery => AndQuery;

	IEnumerable<object> IMemberSearchQuery.OrQuery => OrQuery;
}
