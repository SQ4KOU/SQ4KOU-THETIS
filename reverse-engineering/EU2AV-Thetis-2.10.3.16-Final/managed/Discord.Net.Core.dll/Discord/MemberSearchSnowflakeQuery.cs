using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord;

public struct MemberSearchSnowflakeQuery : IMemberSearchQuery
{
	public MemberSearchV2Range? Range { get; set; }

	public IEnumerable<ulong> AndQuery { get; set; }

	public IEnumerable<ulong> OrQuery { get; set; }

	IEnumerable<object> IMemberSearchQuery.AndQuery => AndQuery?.Select((Func<ulong, object>)((ulong x) => x));

	IEnumerable<object> IMemberSearchQuery.OrQuery => OrQuery?.Select((Func<ulong, object>)((ulong x) => x));
}
