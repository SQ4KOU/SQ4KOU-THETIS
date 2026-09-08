namespace SharpDX.Direct2D1.Effects;

public class Premultiply : Effect
{
	public Premultiply(DeviceContext context)
		: base(context, Effect.Premultiply)
	{
	}
}
