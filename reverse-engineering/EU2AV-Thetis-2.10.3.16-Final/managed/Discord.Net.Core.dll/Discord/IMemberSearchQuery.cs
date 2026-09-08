using System.Collections.Generic;

namespace Discord;

public interface IMemberSearchQuery
{
	MemberSearchV2Range? Range { get; }

	IEnumerable<object> AndQuery { get; }

	IEnumerable<object> OrQuery { get; }
}
