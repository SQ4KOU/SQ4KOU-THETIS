using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class Brightness : Effect
{
	public RawVector2 WhitePoint
	{
		get
		{
			return GetVector2Value(0);
		}
		set
		{
			SetValue(0, value);
		}
	}

	public RawVector2 BlackPoint
	{
		get
		{
			return GetVector2Value(1);
		}
		set
		{
			SetValue(1, value);
		}
	}

	public Brightness(DeviceContext context)
		: base(context, Effect.Brightness)
	{
	}
}
