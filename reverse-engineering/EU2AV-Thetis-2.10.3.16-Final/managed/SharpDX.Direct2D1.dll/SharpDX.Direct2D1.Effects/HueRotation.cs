namespace SharpDX.Direct2D1.Effects;

public class HueRotation : Effect
{
	public float Angle
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

	public HueRotation(DeviceContext context)
		: base(context, Effect.HueRotation)
	{
	}
}
