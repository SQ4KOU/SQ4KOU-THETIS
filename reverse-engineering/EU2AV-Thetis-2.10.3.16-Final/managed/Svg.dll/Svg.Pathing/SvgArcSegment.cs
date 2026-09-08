using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Svg.Pathing;

public sealed class SvgArcSegment : SvgPathSegment
{
	private const double RadiansPerDegree = Math.PI / 180.0;

	private const double DoublePI = Math.PI * 2.0;

	public float RadiusX { get; set; }

	public float RadiusY { get; set; }

	public float Angle { get; set; }

	public SvgArcSweep Sweep { get; set; }

	public SvgArcSize Size { get; set; }

	public SvgArcSegment(float radiusX, float radiusY, float angle, SvgArcSize size, SvgArcSweep sweep, bool isRelative, PointF end)
		: base(isRelative, end)
	{
		RadiusX = Math.Abs(radiusX);
		RadiusY = Math.Abs(radiusY);
		Angle = angle;
		Sweep = sweep;
		Size = size;
	}

	public override string ToString()
	{
		string text = ((Size == SvgArcSize.Large) ? "1" : "0");
		string text2 = ((Sweep == SvgArcSweep.Positive) ? "1" : "0");
		return (base.IsRelative ? "a" : "A") + RadiusX.ToSvgString() + " " + RadiusY.ToSvgString() + " " + Angle.ToSvgString() + " " + text + " " + text2 + " " + base.End.ToSvgString();
	}

	[Obsolete("Use new constructor.")]
	public SvgArcSegment(PointF start, float radiusX, float radiusY, float angle, SvgArcSize size, SvgArcSweep sweep, PointF end)
		: this(radiusX, radiusY, angle, size, sweep, isRelative: false, end)
	{
		base.Start = start;
	}

	private static double CalculateVectorAngle(double ux, double uy, double vx, double vy)
	{
		double num = Math.Atan2(uy, ux);
		double num2 = Math.Atan2(vy, vx);
		if (num2 >= num)
		{
			return num2 - num;
		}
		return Math.PI * 2.0 - (num - num2);
	}

	public override PointF AddToPath(GraphicsPath graphicsPath, PointF start, SvgPathSegmentList parent)
	{
		PointF pointF = SvgPathSegment.ToAbsolute(base.End, base.IsRelative, start);
		if (start == pointF)
		{
			return pointF;
		}
		if (RadiusX == 0f && RadiusY == 0f)
		{
			graphicsPath.AddLine(start, pointF);
			return pointF;
		}
		double num = Math.Sin((double)Angle * (Math.PI / 180.0));
		double num2 = Math.Cos((double)Angle * (Math.PI / 180.0));
		double num3 = num2 * (double)(start.X - pointF.X) / 2.0 + num * (double)(start.Y - pointF.Y) / 2.0;
		double num4 = (0.0 - num) * (double)(start.X - pointF.X) / 2.0 + num2 * (double)(start.Y - pointF.Y) / 2.0;
		double num5 = (double)(RadiusX * RadiusX * RadiusY * RadiusY) - (double)(RadiusX * RadiusX) * num4 * num4 - (double)(RadiusY * RadiusY) * num3 * num3;
		float num6 = RadiusX;
		float num7 = RadiusY;
		double num9;
		if (num5 < 0.0)
		{
			float num8 = (float)Math.Sqrt(1.0 - num5 / (double)(RadiusX * RadiusX * RadiusY * RadiusY));
			num6 *= num8;
			num7 *= num8;
			num9 = 0.0;
		}
		else
		{
			num9 = (((Size == SvgArcSize.Large && Sweep == SvgArcSweep.Positive) || (Size == SvgArcSize.Small && Sweep == SvgArcSweep.Negative)) ? (-1.0) : 1.0) * Math.Sqrt(num5 / ((double)(RadiusX * RadiusX) * num4 * num4 + (double)(RadiusY * RadiusY) * num3 * num3));
		}
		double num10 = num9 * (double)num6 * num4 / (double)num7;
		double num11 = (0.0 - num9) * (double)num7 * num3 / (double)num6;
		double num12 = num2 * num10 - num * num11 + (double)(start.X + pointF.X) / 2.0;
		double num13 = num * num10 + num2 * num11 + (double)(start.Y + pointF.Y) / 2.0;
		double num14 = CalculateVectorAngle(1.0, 0.0, (num3 - num10) / (double)num6, (num4 - num11) / (double)num7);
		double num15 = CalculateVectorAngle((num3 - num10) / (double)num6, (num4 - num11) / (double)num7, (0.0 - num3 - num10) / (double)num6, (0.0 - num4 - num11) / (double)num7);
		if (Sweep == SvgArcSweep.Negative && num15 > 0.0)
		{
			num15 -= Math.PI * 2.0;
		}
		else if (Sweep == SvgArcSweep.Positive && num15 < 0.0)
		{
			num15 += Math.PI * 2.0;
		}
		int num16 = (int)Math.Ceiling(Math.Abs(num15 / (Math.PI / 2.0)));
		double num17 = num15 / (double)num16;
		double num18 = 2.6666666666666665 * Math.Sin(num17 / 4.0) * Math.Sin(num17 / 4.0) / Math.Sin(num17 / 2.0);
		float num19 = start.X;
		float num20 = start.Y;
		for (int i = 0; i < num16; i++)
		{
			double num21 = Math.Cos(num14);
			double num22 = Math.Sin(num14);
			double num23 = num14 + num17;
			double num24 = Math.Cos(num23);
			double num25 = Math.Sin(num23);
			double num26 = num2 * (double)num6 * num24 - num * (double)num7 * num25 + num12;
			double num27 = num * (double)num6 * num24 + num2 * (double)num7 * num25 + num13;
			double num28 = num18 * ((0.0 - num2) * (double)num6 * num22 - num * (double)num7 * num21);
			double num29 = num18 * ((0.0 - num) * (double)num6 * num22 + num2 * (double)num7 * num21);
			double num30 = num18 * (num2 * (double)num6 * num25 + num * (double)num7 * num24);
			double num31 = num18 * (num * (double)num6 * num25 - num2 * (double)num7 * num24);
			graphicsPath.AddBezier(num19, num20, (float)((double)num19 + num28), (float)((double)num20 + num29), (float)(num26 + num30), (float)(num27 + num31), (float)num26, (float)num27);
			num14 = num23;
			num19 = (float)num26;
			num20 = (float)num27;
		}
		return pointF;
	}

	[Obsolete("Use new AddToPath.")]
	public override void AddToPath(GraphicsPath graphicsPath)
	{
		AddToPath(graphicsPath, base.Start, null);
	}
}
