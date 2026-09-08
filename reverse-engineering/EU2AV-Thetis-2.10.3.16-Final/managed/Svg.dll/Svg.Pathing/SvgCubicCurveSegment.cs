using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Svg.Pathing;

public sealed class SvgCubicCurveSegment : SvgPathSegment
{
	public PointF FirstControlPoint { get; set; }

	public PointF SecondControlPoint { get; set; }

	public SvgCubicCurveSegment(bool isRelative, PointF firstControlPoint, PointF secondControlPoint, PointF end)
		: base(isRelative, end)
	{
		FirstControlPoint = firstControlPoint;
		SecondControlPoint = secondControlPoint;
	}

	public SvgCubicCurveSegment(bool isRelative, PointF secondControlPoint, PointF end)
		: this(isRelative, SvgPathSegment.NaN, secondControlPoint, end)
	{
	}

	public override string ToString()
	{
		if (float.IsNaN(FirstControlPoint.X) || float.IsNaN(FirstControlPoint.Y))
		{
			return (base.IsRelative ? "s" : "S") + SecondControlPoint.ToSvgString() + " " + base.End.ToSvgString();
		}
		return (base.IsRelative ? "c" : "C") + FirstControlPoint.ToSvgString() + " " + SecondControlPoint.ToSvgString() + " " + base.End.ToSvgString();
	}

	[Obsolete("Use new constructor.")]
	public SvgCubicCurveSegment(PointF start, PointF firstControlPoint, PointF secondControlPoint, PointF end)
		: this(isRelative: false, firstControlPoint, secondControlPoint, end)
	{
		base.Start = start;
	}

	public override PointF AddToPath(GraphicsPath graphicsPath, PointF start, SvgPathSegmentList parent)
	{
		PointF firstControlPoint = FirstControlPoint;
		if (float.IsNaN(firstControlPoint.X) || float.IsNaN(firstControlPoint.Y))
		{
			int num = parent.IndexOf(this) - 1;
			firstControlPoint = ((num < 0 || !(parent[num] is SvgCubicCurveSegment)) ? start : SvgPathSegment.Reflect(graphicsPath.PathPoints[graphicsPath.PointCount - 2], start));
		}
		else
		{
			firstControlPoint = SvgPathSegment.ToAbsolute(firstControlPoint, base.IsRelative, start);
		}
		PointF pointF = SvgPathSegment.ToAbsolute(base.End, base.IsRelative, start);
		graphicsPath.AddBezier(start, firstControlPoint, SvgPathSegment.ToAbsolute(SecondControlPoint, base.IsRelative, start), pointF);
		return pointF;
	}

	[Obsolete("Use new AddToPath.")]
	public override void AddToPath(GraphicsPath graphicsPath)
	{
		AddToPath(graphicsPath, base.Start, null);
	}
}
