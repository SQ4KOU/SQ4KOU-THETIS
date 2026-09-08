namespace SharpDX.Direct2D1.Effects;

public class DisplacementMap : Effect
{
	public new float Scale
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

	public ChannelSelector XChannelSelect
	{
		get
		{
			return GetEnumValue<ChannelSelector>(1);
		}
		set
		{
			SetEnumValue(1, value);
		}
	}

	public ChannelSelector YChannelSelect
	{
		get
		{
			return GetEnumValue<ChannelSelector>(2);
		}
		set
		{
			SetEnumValue(2, value);
		}
	}

	public DisplacementMap(DeviceContext context)
		: base(context, Effect.DisplacementMap)
	{
	}
}
