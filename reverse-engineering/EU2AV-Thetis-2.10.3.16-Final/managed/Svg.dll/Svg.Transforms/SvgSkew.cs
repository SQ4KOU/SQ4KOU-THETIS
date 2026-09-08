using System;
using System.Drawing.Drawing2D;

namespace Svg.Transforms;

public sealed class SvgSkew : SvgTransform
{
	public float AngleX { get; set; }

	public float AngleY { get; set; }

	public override Matrix Matrix
	{
		get
		{
			Matrix matrix = new Matrix();
			matrix.Shear((float)Math.Tan((double)(AngleX / 180f) * Math.PI), (float)Math.Tan((double)(AngleY / 180f) * Math.PI));
			return matrix;
		}
	}

	public override string WriteToString()
	{
		if (AngleY == 0f)
		{
			return "skewX(" + AngleX.ToSvgString() + ")";
		}
		return "skewY(" + AngleY.ToSvgString() + ")";
	}

	public SvgSkew(float x, float y)
	{
		AngleX = x;
		AngleY = y;
	}

	public override object Clone()
	{
		return new SvgSkew(AngleX, AngleY);
	}
}
