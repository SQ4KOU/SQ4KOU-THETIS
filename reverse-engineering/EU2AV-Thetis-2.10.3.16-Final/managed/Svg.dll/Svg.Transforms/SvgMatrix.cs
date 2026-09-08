using System.Collections.Generic;
using System.Drawing.Drawing2D;

namespace Svg.Transforms;

public sealed class SvgMatrix : SvgTransform
{
	public List<float> Points { get; set; }

	public override Matrix Matrix => new Matrix(Points[0], Points[1], Points[2], Points[3], Points[4], Points[5]);

	public override string WriteToString()
	{
		return "matrix(" + Points[0].ToSvgString() + ", " + Points[1].ToSvgString() + ", " + Points[2].ToSvgString() + ", " + Points[3].ToSvgString() + ", " + Points[4].ToSvgString() + ", " + Points[5].ToSvgString() + ")";
	}

	public SvgMatrix(List<float> m)
	{
		Points = m;
	}

	public override object Clone()
	{
		return new SvgMatrix(Points);
	}
}
