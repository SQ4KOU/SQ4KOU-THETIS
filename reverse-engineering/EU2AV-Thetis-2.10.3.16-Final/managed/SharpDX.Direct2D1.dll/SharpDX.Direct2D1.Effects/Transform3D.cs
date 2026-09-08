using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class Transform3D : Effect
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

	public RawMatrix TransformMatrix
	{
		get
		{
			return GetMatrixValue(2);
		}
		set
		{
			SetValue(2, value);
		}
	}

	public Transform3D(DeviceContext context)
		: base(context, Effect.Transform3D)
	{
	}
}
