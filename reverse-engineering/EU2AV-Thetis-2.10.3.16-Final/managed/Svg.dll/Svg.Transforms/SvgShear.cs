using System.Drawing.Drawing2D;

namespace Svg.Transforms;

public sealed class SvgShear : SvgTransform
{
	public float X { get; set; }

	public float Y { get; set; }

	public override Matrix Matrix
	{
		get
		{
			Matrix matrix = new Matrix();
			matrix.Shear(X, Y);
			return matrix;
		}
	}

	public override string WriteToString()
	{
		return "shear(" + X.ToSvgString() + ", " + Y.ToSvgString() + ")";
	}

	public SvgShear(float x)
		: this(x, x)
	{
	}

	public SvgShear(float x, float y)
	{
		X = x;
		Y = y;
	}

	public override object Clone()
	{
		return new SvgShear(X, Y);
	}
}
