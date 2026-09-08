using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class Flood : Effect
{
	public RawColor4 Color
	{
		get
		{
			return GetColor4Value(0);
		}
		set
		{
			SetValue(0, value);
		}
	}

	public Flood(DeviceContext context)
		: base(context, Effect.Flood)
	{
	}
}
