using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class Crop : Effect
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

	public Crop(DeviceContext context)
		: base(context, Effect.Crop)
	{
	}
}
