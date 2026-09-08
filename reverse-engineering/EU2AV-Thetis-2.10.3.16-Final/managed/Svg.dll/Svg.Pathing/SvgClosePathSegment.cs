using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Svg.Pathing;

public sealed class SvgClosePathSegment : SvgPathSegment
{
	public SvgClosePathSegment(bool isRelative)
		: base(isRelative)
	{
	}

	public override string ToString()
	{
		if (!base.IsRelative)
		{
			return "Z";
		}
		return "z";
	}

	[Obsolete("Use new constructor.")]
	public SvgClosePathSegment()
		: this(isRelative: true)
	{
	}

	public override PointF AddToPath(GraphicsPath graphicsPath, PointF start, SvgPathSegmentList parent)
	{
		graphicsPath.CloseFigure();
		PointF result = start;
		if (graphicsPath.PointCount == 0)
		{
			return result;
		}
		byte[] pathTypes = graphicsPath.PathTypes;
		for (int num = graphicsPath.PointCount - 1; num >= 0; num--)
		{
			if ((pathTypes[num] & 7) == 0)
			{
				result = graphicsPath.PathPoints[num];
				break;
			}
		}
		return result;
	}

	[Obsolete("Use new AddToPath.")]
	public override void AddToPath(GraphicsPath graphicsPath)
	{
		AddToPath(graphicsPath, base.Start, null);
	}
}
