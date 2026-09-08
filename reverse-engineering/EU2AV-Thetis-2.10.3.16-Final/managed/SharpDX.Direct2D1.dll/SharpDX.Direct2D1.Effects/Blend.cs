namespace SharpDX.Direct2D1.Effects;

public class Blend : Effect
{
	public BlendMode Mode
	{
		get
		{
			return GetEnumValue<BlendMode>(0);
		}
		set
		{
			SetEnumValue(0, value);
		}
	}

	public Blend(DeviceContext context)
		: base(context, Effect.Blend)
	{
	}
}
