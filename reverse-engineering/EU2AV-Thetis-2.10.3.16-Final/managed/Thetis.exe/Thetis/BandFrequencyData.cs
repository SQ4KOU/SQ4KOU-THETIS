namespace Thetis;

public struct BandFrequencyData(double low, double high, Band band, BandType bandType, bool lowOnly, FRSRegion region)
{
	public double low = low;

	public double high = (lowOnly ? 0.0 : high);

	public Band band = band;

	public bool lowOnly = lowOnly;

	public FRSRegion region = region;

	public BandType bandType = bandType;

	public BandFrequencyData Copy()
	{
		return new BandFrequencyData
		{
			low = low,
			high = high,
			band = band,
			lowOnly = lowOnly,
			region = region,
			bandType = bandType
		};
	}
}
