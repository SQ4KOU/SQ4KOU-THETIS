using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class Atlas : Effect
{
	public RawVector4 InputRectangle
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

	public RawVector4 InputPaddingRectangle
	{
		get
		{
			return GetVector4Value(1);
		}
		set
		{
			SetValue(1, value);
		}
	}

	public Atlas(DeviceContext context)
		: base(context, Effect.Atlas)
	{
	}
}
