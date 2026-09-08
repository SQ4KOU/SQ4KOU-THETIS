namespace SharpDX.Direct2D1.Effects;

public class DirectionalBlur : Effect
{
	public float StandardDeviation
	{
		get
		{
			return GetFloatValue(0);
		}
		set
		{
			SetValue(0, value);
		}
	}

	public float Angle
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

	public DirectionalBlurOptimization Optimization
	{
		get
		{
			return GetEnumValue<DirectionalBlurOptimization>(2);
		}
		set
		{
			SetEnumValue(2, value);
		}
	}

	public BorderMode BorderMode
	{
		get
		{
			return GetEnumValue<BorderMode>(3);
		}
		set
		{
			SetEnumValue(3, value);
		}
	}

	public DirectionalBlur(DeviceContext context)
		: base(context, Effect.DirectionalBlur)
	{
	}
}
