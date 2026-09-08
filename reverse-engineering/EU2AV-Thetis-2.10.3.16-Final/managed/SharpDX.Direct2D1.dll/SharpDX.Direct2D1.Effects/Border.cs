namespace SharpDX.Direct2D1.Effects;

public class Border : Effect
{
	public BorderEdgeMode EdgeModeX
	{
		get
		{
			return GetEnumValue<BorderEdgeMode>(0);
		}
		set
		{
			SetEnumValue(0, value);
		}
	}

	public BorderEdgeMode EdgeModeY
	{
		get
		{
			return GetEnumValue<BorderEdgeMode>(1);
		}
		set
		{
			SetEnumValue(1, value);
		}
	}

	public Border(DeviceContext context)
		: base(context, Effect.Border)
	{
	}
}
