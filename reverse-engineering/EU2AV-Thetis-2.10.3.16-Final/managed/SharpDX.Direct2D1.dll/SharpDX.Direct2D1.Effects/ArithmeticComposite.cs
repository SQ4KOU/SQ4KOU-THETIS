using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class ArithmeticComposite : Effect
{
	public RawVector4 Coefficients
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

	public bool ClampOutput
	{
		get
		{
			return GetBoolValue(1);
		}
		set
		{
			SetValue(1, value);
		}
	}

	public ArithmeticComposite(DeviceContext context)
		: base(context, Effect.ArithmeticComposite)
	{
	}
}
