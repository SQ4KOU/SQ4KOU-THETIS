using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class Shadow : Effect
{
	public float BlurStandardDeviation
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

	public RawColor4 Color
	{
		get
		{
			return GetColor4Value(1);
		}
		set
		{
			SetValue(1, value);
		}
	}

	public ShadowOptimization Optimization
	{
		get
		{
			return GetEnumValue<ShadowOptimization>(2);
		}
		set
		{
			SetEnumValue(2, value);
		}
	}

	public Shadow(DeviceContext context)
		: base(context, Effect.Shadow)
	{
	}
}
