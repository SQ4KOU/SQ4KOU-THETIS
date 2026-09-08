using System.ComponentModel;
using Svg.DataTypes;

namespace Svg;

[TypeConverter(typeof(SvgOrientConverter))]
public class SvgOrient
{
	private bool _isAuto;

	private float _angle;

	public float Angle
	{
		get
		{
			return _angle;
		}
		set
		{
			_angle = value;
			_isAuto = false;
		}
	}

	public bool IsAuto
	{
		get
		{
			return _isAuto;
		}
		set
		{
			_isAuto = value;
			_angle = 0f;
		}
	}

	public bool IsAutoStartReverse { get; set; }

	public SvgOrient()
		: this(0f)
	{
	}

	public SvgOrient(bool isAuto)
	{
		IsAuto = isAuto;
	}

	public SvgOrient(bool isAuto, bool isAutoStartReverse)
	{
		IsAuto = isAuto;
		IsAutoStartReverse = isAutoStartReverse;
	}

	public SvgOrient(float angle)
	{
		Angle = angle;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SvgOrient))
		{
			return false;
		}
		SvgOrient svgOrient = (SvgOrient)obj;
		if (svgOrient.IsAuto == IsAuto)
		{
			return svgOrient.Angle == Angle;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		if (IsAuto)
		{
			if (!IsAutoStartReverse)
			{
				return "auto";
			}
			return "auto-start-reverse";
		}
		return Angle.ToSvgString();
	}

	public static implicit operator SvgOrient(float value)
	{
		return new SvgOrient(value);
	}
}
