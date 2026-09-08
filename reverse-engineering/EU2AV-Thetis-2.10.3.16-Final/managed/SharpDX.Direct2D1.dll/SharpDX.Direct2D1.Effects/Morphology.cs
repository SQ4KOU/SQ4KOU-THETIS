namespace SharpDX.Direct2D1.Effects;

public class Morphology : Effect
{
	public MorphologyMode Mode
	{
		get
		{
			return GetEnumValue<MorphologyMode>(0);
		}
		set
		{
			SetEnumValue(0, value);
		}
	}

	public int Width
	{
		get
		{
			return (int)GetUIntValue(1);
		}
		set
		{
			SetValue(1, (uint)value);
		}
	}

	public int Height
	{
		get
		{
			return (int)GetUIntValue(2);
		}
		set
		{
			SetValue(2, (uint)value);
		}
	}

	public Morphology(DeviceContext context)
		: base(context, Effect.Morphology)
	{
	}
}
