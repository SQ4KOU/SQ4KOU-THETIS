using System.Drawing;

namespace Thetis;

public class ColorInterpolator
{
	private delegate byte ComponentSelector(Color color);

	private static ComponentSelector _alphaSelector = (Color color) => color.A;

	private static ComponentSelector _redSelector = (Color color) => color.R;

	private static ComponentSelector _greenSelector = (Color color) => color.G;

	private static ComponentSelector _blueSelector = (Color color) => color.B;

	public static Color InterpolateBetween(Color endPoint1, Color endPoint2, double lambda)
	{
		if (lambda < 0.0 || lambda > 1.0)
		{
			return Color.Empty;
		}
		return Color.FromArgb(InterpolateComponent(endPoint1, endPoint2, lambda, _alphaSelector), InterpolateComponent(endPoint1, endPoint2, lambda, _redSelector), InterpolateComponent(endPoint1, endPoint2, lambda, _greenSelector), InterpolateComponent(endPoint1, endPoint2, lambda, _blueSelector));
	}

	private static byte InterpolateComponent(Color endPoint1, Color endPoint2, double lambda, ComponentSelector selector)
	{
		return (byte)((double)(int)selector(endPoint1) + (double)(selector(endPoint2) - selector(endPoint1)) * lambda);
	}
}
