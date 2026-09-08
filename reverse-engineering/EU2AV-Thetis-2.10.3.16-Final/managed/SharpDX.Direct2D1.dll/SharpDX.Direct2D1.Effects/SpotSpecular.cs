using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class SpotSpecular : Effect
{
	public RawVector3 LightPosition
	{
		get
		{
			return GetVector3Value(0);
		}
		set
		{
			SetValue(0, value);
		}
	}

	public RawVector3 PointsAt
	{
		get
		{
			return GetVector3Value(1);
		}
		set
		{
			SetValue(1, value);
		}
	}

	public float Focus
	{
		get
		{
			return GetFloatValue(2);
		}
		set
		{
			SetValue(2, value);
		}
	}

	public float LimitingConeAngle
	{
		get
		{
			return GetFloatValue(3);
		}
		set
		{
			SetValue(3, value);
		}
	}

	public float SpecularExponent
	{
		get
		{
			return GetFloatValue(4);
		}
		set
		{
			SetValue(4, value);
		}
	}

	public float SpecularConstant
	{
		get
		{
			return GetFloatValue(5);
		}
		set
		{
			SetValue(5, value);
		}
	}

	public float SurfaceScale
	{
		get
		{
			return GetFloatValue(6);
		}
		set
		{
			SetValue(6, value);
		}
	}

	public RawVector3 Color
	{
		get
		{
			return GetVector3Value(7);
		}
		set
		{
			SetValue(7, value);
		}
	}

	public RawVector2 KernelUnitLength
	{
		get
		{
			return GetVector2Value(8);
		}
		set
		{
			SetValue(8, value);
		}
	}

	public SpotSpecularScaleMode ScaleMode
	{
		get
		{
			return GetEnumValue<SpotSpecularScaleMode>(9);
		}
		set
		{
			SetEnumValue(9, value);
		}
	}

	public SpotSpecular(DeviceContext context)
		: base(context, Effect.SpotSpecular)
	{
	}
}
