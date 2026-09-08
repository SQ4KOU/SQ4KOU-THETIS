namespace Thetis;

public class GripperEventArgs
{
	public int DBM { get; }

	public float Percent { get; }

	public GripperEventArgs(int dBm, float percent)
	{
		DBM = dBm;
		Percent = percent;
	}
}
