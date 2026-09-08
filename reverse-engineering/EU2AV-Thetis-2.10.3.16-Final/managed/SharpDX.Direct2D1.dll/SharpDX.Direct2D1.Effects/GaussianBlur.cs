namespace SharpDX.Direct2D1.Effects;

public class GaussianBlur : Effect
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

	public GaussianBlurOptimization Optimization
	{
		get
		{
			return GetEnumValue<GaussianBlurOptimization>(1);
		}
		set
		{
			SetEnumValue(1, value);
		}
	}

	public BorderMode BorderMode
	{
		get
		{
			return GetEnumValue<BorderMode>(2);
		}
		set
		{
			SetEnumValue(2, value);
		}
	}

	public GaussianBlur(DeviceContext context)
		: base(context, Effect.GaussianBlur)
	{
	}
}
