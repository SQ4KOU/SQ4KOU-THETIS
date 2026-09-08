using System.Collections;
using System.Collections.Generic;

namespace ExCSS;

public interface IRuleList : IEnumerable<IRule>, IEnumerable
{
	IRule this[int index] { get; }

	int Length { get; }
}
