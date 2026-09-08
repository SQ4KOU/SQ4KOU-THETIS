using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class PerspectiveTransform3D : Effect
{
	public PerspectiveTransform3DInteroplationMode InterpolationMode
	{
		get
		{
			return GetEnumValue<PerspectiveTransform3DInteroplationMode>(0);
		}
		set
		{
			SetEnumValue(0, value);
		}
	}

	public BorderMode BorderMode
	{
		get
		{
			return GetEnumValue<BorderMode>(1);
		}
		set
		{
			SetEnumValue(1, value);
		}
	}

	public float Depth
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

	public RawVector2 PerspectiveOrigin
	{
		get
		{
			return GetVector2Value(3);
		}
		set
		{
			SetValue(3, value);
		}
	}

	public RawVector3 LocalOffset
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

	public RawVector3 GlobalOffset
	{
		get
		{
			return GetVector3Value(5);
		}
		set
		{
			SetValue(5, value);
		}
	}

	public RawVector3 RotationOrigin
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

	public RawVector3 Rotation
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

	public PerspectiveTransform3D(DeviceContext deviceContext)
		: base(deviceContext, Effect.PerspectiveTransform3D)
	{
	}
}
