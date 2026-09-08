using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class SpotDiffuse : Effect
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

	public float DiffuseConstant
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

	public float SurfaceScale
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

	public RawVector3 Color
	{
		get
		{
			return GetVector3Value(6);
		}
		set
		{
			SetValue(6, value);
		}
	}

	public RawVector2 KernelUnitLength
	{
		get
		{
			return GetVector2Value(7);
		}
		set
		{
			SetValue(7, value);
		}
	}

	public SpotDiffuseScaleMode ScaleMode
	{
		get
		{
			return GetEnumValue<SpotDiffuseScaleMode>(8);
		}
		set
		{
			SetEnumValue(8, value);
		}
	}

	public SpotDiffuse(DeviceContext context)
		: base(context, Effect.SpotDiffuse)
	{
	}
}
