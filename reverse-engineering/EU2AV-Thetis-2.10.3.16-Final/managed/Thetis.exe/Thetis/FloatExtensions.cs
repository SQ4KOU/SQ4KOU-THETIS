using System.Runtime.CompilerServices;

namespace Thetis;

public static class FloatExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Clamp(this float v, float min, float max)
	{
		if (!(v < min))
		{
			if (!(v > max))
			{
				return v;
			}
			return max;
		}
		return min;
	}
}
