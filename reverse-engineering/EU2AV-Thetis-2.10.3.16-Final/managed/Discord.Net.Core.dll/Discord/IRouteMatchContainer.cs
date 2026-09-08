using System.Collections.Generic;

namespace Discord;

public interface IRouteMatchContainer
{
	IEnumerable<IRouteSegmentMatch> SegmentMatches { get; }

	void SetSegmentMatches(IEnumerable<IRouteSegmentMatch> segmentMatches);
}
