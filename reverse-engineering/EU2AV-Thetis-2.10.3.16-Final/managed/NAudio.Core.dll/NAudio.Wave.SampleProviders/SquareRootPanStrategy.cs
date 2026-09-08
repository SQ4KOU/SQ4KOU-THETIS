using System;

namespace NAudio.Wave.SampleProviders;

public class SquareRootPanStrategy : IPanStrategy
{
	public StereoSamplePair GetMultipliers(float pan)
	{
		float num = (0f - pan + 1f) / 2f;
		float left = (float)Math.Sqrt(num);
		float right = (float)Math.Sqrt(1f - num);
		return new StereoSamplePair
		{
			Left = left,
			Right = right
		};
	}
}
