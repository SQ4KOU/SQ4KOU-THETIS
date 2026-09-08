namespace SharpDX.Direct2D1.Effects;

public class Composite : Effect
{
	public CompositeMode Mode
	{
		get
		{
			return GetEnumValue<CompositeMode>(0);
		}
		set
		{
			SetEnumValue(0, value);
		}
	}

	public Composite(DeviceContext context)
		: base(context, Effect.Composite)
	{
	}
}
