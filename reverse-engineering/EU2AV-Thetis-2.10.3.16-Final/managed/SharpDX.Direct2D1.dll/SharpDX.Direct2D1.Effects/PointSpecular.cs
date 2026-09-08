using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class PointSpecular : Effect
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

	public float SpecularExponent
	{
		get
		{
			return GetFloatValue(1);
		}
		set
		{
			SetValue(1, value);
		}
	}

	public float SpecularConstant
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

	public float SurfaceScale
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

	public RawVector3 Color
	{
		get
		{
			return GetVector3Value(4);
		}
		set
		{
			SetValue(4, value);
		}
	}

	public RawVector2 KernelUnitLength
	{
		get
		{
			return GetVector2Value(5);
		}
		set
		{
			SetValue(5, value);
		}
	}

	public PointSpecularScaleMode ScaleMode
	{
		get
		{
			return GetEnumValue<PointSpecularScaleMode>(6);
		}
		set
		{
			SetEnumValue(6, value);
		}
	}

	public PointSpecular(DeviceContext context)
		: base(context, Effect.PointSpecular)
	{
	}
}
