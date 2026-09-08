using System.Drawing;
using System.Globalization;

namespace Svg;

public static class PointFExtensions
{
	public static string ToSvgString(this float value)
	{
		return value.ToString("G7", CultureInfo.InvariantCulture);
	}

	public static string ToSvgString(this PointF p)
	{
		return p.X.ToSvgString() + " " + p.Y.ToSvgString();
	}
}
