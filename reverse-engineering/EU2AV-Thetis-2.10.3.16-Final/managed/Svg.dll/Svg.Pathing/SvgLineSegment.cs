using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Svg.Pathing;

public sealed class SvgLineSegment : SvgPathSegment
{
	public SvgLineSegment(bool isRelative, PointF end)
		: base(isRelative, end)
	{
	}

	public override string ToString()
	{
		if (float.IsNaN(base.End.Y))
		{
			return (base.IsRelative ? "h" : "H") + base.End.X.ToSvgString();
		}
		if (float.IsNaN(base.End.X))
		{
			return (base.IsRelative ? "v" : "V") + base.End.Y.ToSvgString();
		}
		return (base.IsRelative ? "l" : "L") + base.End.ToSvgString();
	}

	[Obsolete("Use new constructor.")]
	public SvgLineSegment(PointF start, PointF end)
		: this(isRelative: false, end)
	{
		base.Start = start;
	}

	public override PointF AddToPath(GraphicsPath graphicsPath, PointF start, SvgPathSegmentList parent)
	{
		PointF pointF = SvgPathSegment.ToAbsolute(base.End, base.IsRelative, start);
		graphicsPath.AddLine(start, pointF);
		return pointF;
	}

	[Obsolete("Use new AddToPath.")]
	public override void AddToPath(GraphicsPath graphicsPath)
	{
		AddToPath(graphicsPath, base.Start, null);
	}
}
