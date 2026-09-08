namespace SharpDX.Direct2D1.Effects;

public class Saturation : Effect
{
	public float Value
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

	public Saturation(DeviceContext context)
		: base(context, Effect.Saturation)
	{
	}
}
