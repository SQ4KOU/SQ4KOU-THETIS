namespace SharpDX.Direct2D1.Effects;

public class LuminanceToAlpha : Effect
{
	public LuminanceToAlpha(DeviceContext context)
		: base(context, Effect.LuminanceToAlpha)
	{
	}
}
