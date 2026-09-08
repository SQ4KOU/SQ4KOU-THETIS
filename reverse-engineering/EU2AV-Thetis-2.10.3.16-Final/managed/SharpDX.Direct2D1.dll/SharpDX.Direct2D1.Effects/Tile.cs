using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class Tile : Effect
{
	public RawVector4 Rectangle
	{
		get
		{
			return GetVector4Value(0);
		}
		set
		{
			SetValue(0, value);
		}
	}

	public Tile(DeviceContext context)
		: base(context, Effect.Tile)
	{
	}
}
