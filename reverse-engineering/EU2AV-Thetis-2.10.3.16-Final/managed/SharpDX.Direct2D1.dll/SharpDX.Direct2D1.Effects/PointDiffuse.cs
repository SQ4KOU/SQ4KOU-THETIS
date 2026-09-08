using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class PointDiffuse : Effect
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

	public float DiffuseConstant
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

	public float SurfaceScale
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

	public RawVector3 Color
	{
		get
		{
			return GetVector3Value(3);
		}
		set
		{
			SetValue(3, value);
		}
	}

	public RawVector2 KernelUnitLength
	{
		get
		{
			return GetVector2Value(4);
		}
		set
		{
			SetValue(4, value);
		}
	}

	public PointDiffuseScaleMode ScaleMode
	{
		get
		{
			return GetEnumValue<PointDiffuseScaleMode>(5);
		}
		set
		{
			SetEnumValue(5, value);
		}
	}

	public PointDiffuse(DeviceContext context)
		: base(context, Effect.PointDiffuse)
	{
	}
}
