namespace SharpDX.Direct2D1.Effects;

public class DpiCompensation : Effect
{
	public DpiCompensationInterpolationMode InterpolationMode
	{
		get
		{
			return GetEnumValue<DpiCompensationInterpolationMode>(0);
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

	public float InputDpi
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

	public DpiCompensation(DeviceContext context)
		: base(context, Effect.DpiCompensation)
	{
	}
}
