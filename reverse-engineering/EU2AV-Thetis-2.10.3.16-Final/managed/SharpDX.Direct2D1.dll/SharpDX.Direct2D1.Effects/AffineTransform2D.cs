using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class AffineTransform2D : Effect
{
	public InterpolationMode InterpolationMode
	{
		get
		{
			return GetEnumValue<InterpolationMode>(0);
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

	public RawMatrix3x2 TransformMatrix
	{
		get
		{
			return GetMatrix3x2Value(2);
		}
		set
		{
			SetValue(2, value);
		}
	}

	public float Sharpness
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

	public AffineTransform2D(DeviceContext context)
		: base(context, Effect.AffineTransform2D)
	{
	}
}
