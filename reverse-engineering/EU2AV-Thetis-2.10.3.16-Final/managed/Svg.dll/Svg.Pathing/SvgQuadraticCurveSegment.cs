using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Svg.Pathing;

public sealed class SvgQuadraticCurveSegment : SvgPathSegment
{
	public PointF ControlPoint { get; set; }

	public SvgQuadraticCurveSegment(bool isRelative, PointF controlPoint, PointF end)
		: base(isRelative, end)
	{
		ControlPoint = controlPoint;
	}

	public SvgQuadraticCurveSegment(bool isRelative, PointF end)
		: this(isRelative, SvgPathSegment.NaN, end)
	{
	}

	public override string ToString()
	{
		if (float.IsNaN(ControlPoint.X) || float.IsNaN(ControlPoint.Y))
		{
			return (base.IsRelative ? "t" : "T") + base.End.ToSvgString();
		}
		return (base.IsRelative ? "q" : "Q") + ControlPoint.ToSvgString() + " " + base.End.ToSvgString();
	}

	[Obsolete("Use new constructor.")]
	public SvgQuadraticCurveSegment(PointF start, PointF controlPoint, PointF end)
		: this(isRelative: false, controlPoint, end)
	{
		base.Start = start;
	}

	private static PointF CalculateFirstControlPoint(PointF start, PointF controlPoint)
	{
		float x = start.X + (controlPoint.X - start.X) * 2f / 3f;
		float y = start.Y + (controlPoint.Y - start.Y) * 2f / 3f;
		return new PointF(x, y);
	}

	private static PointF CalculateSecondControlPoint(PointF controlPoint, PointF end)
	{
		float x = controlPoint.X + (end.X - controlPoint.X) / 3f;
		float y = controlPoint.Y + (end.Y - controlPoint.Y) / 3f;
		return new PointF(x, y);
	}

	private static PointF CalculateControlPoint(PointF start, PointF firstControlPoint)
	{
		float x = (firstControlPoint.X * 3f - start.X) / 2f;
		float y = (firstControlPoint.Y * 3f - start.Y) / 2f;
		return new PointF(x, y);
	}

	public override PointF AddToPath(GraphicsPath graphicsPath, PointF start, SvgPathSegmentList parent)
	{
		PointF controlPoint = ControlPoint;
		if (float.IsNaN(controlPoint.X) || float.IsNaN(controlPoint.Y))
		{
			int num = parent.IndexOf(this) - 1;
			if (num >= 0 && parent[num] is SvgQuadraticCurveSegment)
			{
				PointF start2 = graphicsPath.PathPoints[graphicsPath.PointCount - 4];
				PointF firstControlPoint = graphicsPath.PathPoints[graphicsPath.PointCount - 3];
				controlPoint = SvgPathSegment.Reflect(CalculateControlPoint(start2, firstControlPoint), start);
			}
			else
			{
				controlPoint = start;
			}
		}
		else
		{
			controlPoint = SvgPathSegment.ToAbsolute(controlPoint, base.IsRelative, start);
		}
		PointF pointF = SvgPathSegment.ToAbsolute(base.End, base.IsRelative, start);
		graphicsPath.AddBezier(start, CalculateFirstControlPoint(start, controlPoint), CalculateSecondControlPoint(controlPoint, pointF), pointF);
		return pointF;
	}

	[Obsolete("Use new AddToPath.")]
	public override void AddToPath(GraphicsPath graphicsPath)
	{
		AddToPath(graphicsPath, base.Start, null);
	}
}
