namespace SharpDX.Direct2D1.Effects;

public class UnPremultiply : Effect
{
	public UnPremultiply(DeviceContext context)
		: base(context, Effect.UnPremultiply)
	{
	}
}
