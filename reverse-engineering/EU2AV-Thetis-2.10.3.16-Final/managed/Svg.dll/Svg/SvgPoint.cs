using System.ComponentModel;
using System.Drawing;

namespace Svg;

public struct SvgPoint
{
	private SvgUnit x;

	private SvgUnit y;

	public SvgUnit X
	{
		get
		{
			return x;
		}
		set
		{
			x = value;
		}
	}

	public SvgUnit Y
	{
		get
		{
			return y;
		}
		set
		{
			y = value;
		}
	}

	public bool IsEmpty()
	{
		if (X.Value == 0f)
		{
			return Y.Value == 0f;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj.GetType() == typeof(SvgPoint)))
		{
			return false;
		}
		SvgPoint svgPoint = (SvgPoint)obj;
		if (svgPoint.X.Equals(X))
		{
			return svgPoint.Y.Equals(Y);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public SvgPoint(string x, string y)
	{
		TypeConverter converter = TypeDescriptor.GetConverter(typeof(SvgUnit));
		this.x = (SvgUnit)converter.ConvertFrom(x);
		this.y = (SvgUnit)converter.ConvertFrom(y);
	}

	public SvgPoint(SvgUnit x, SvgUnit y)
	{
		this.x = x;
		this.y = y;
	}

	public PointF ToDeviceValue(ISvgRenderer renderer, SvgElement owner)
	{
		return SvgUnit.GetDevicePoint(X, Y, renderer, owner);
	}
}
