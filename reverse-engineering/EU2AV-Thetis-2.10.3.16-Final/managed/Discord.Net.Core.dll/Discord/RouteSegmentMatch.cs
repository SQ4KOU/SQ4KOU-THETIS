namespace Discord;

internal record RouteSegmentMatch : IRouteSegmentMatch
{
	public string Value { get; }

	public RouteSegmentMatch(string value)
	{
		Value = value;
	}
}
