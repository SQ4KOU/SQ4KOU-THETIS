using System;

namespace NAudio.Wave.SampleProviders;

public class SinPanStrategy : IPanStrategy
{
	private const float HalfPi = MathF.PI / 2f;

	public StereoSamplePair GetMultipliers(float pan)
	{
		float num = (0f - pan + 1f) / 2f;
		float left = (float)Math.Sin(num * (MathF.PI / 2f));
		float right = (float)Math.Cos(num * (MathF.PI / 2f));
		return new StereoSamplePair
		{
			Left = left,
			Right = right
		};
	}
}
