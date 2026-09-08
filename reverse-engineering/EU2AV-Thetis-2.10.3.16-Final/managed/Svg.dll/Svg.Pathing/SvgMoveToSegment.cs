using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Svg.Pathing;

public sealed class SvgMoveToSegment : SvgPathSegment
{
	public SvgMoveToSegment(bool isRelative, PointF moveTo)
		: base(isRelative, moveTo)
	{
	}

	public override string ToString()
	{
		return (base.IsRelative ? "m" : "M") + base.End.ToSvgString();
	}

	[Obsolete("Use new constructor.")]
	public SvgMoveToSegment(PointF moveTo)
		: this(isRelative: false, moveTo)
	{
		base.Start = moveTo;
	}

	public override PointF AddToPath(GraphicsPath graphicsPath, PointF start, SvgPathSegmentList parent)
	{
		graphicsPath.StartFigure();
		return SvgPathSegment.ToAbsolute(base.End, base.IsRelative, start);
	}

	[Obsolete("Use new AddToPath.")]
	public override void AddToPath(GraphicsPath graphicsPath)
	{
		AddToPath(graphicsPath, base.Start, null);
	}
}
