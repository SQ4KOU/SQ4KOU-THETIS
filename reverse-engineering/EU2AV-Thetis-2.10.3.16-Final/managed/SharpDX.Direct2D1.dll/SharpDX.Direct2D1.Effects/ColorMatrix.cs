using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class ColorMatrix : Effect
{
	public RawMatrix5x4 Matrix
	{
		get
		{
			return GetMatrix5x4Value(0);
		}
		set
		{
			SetValue(0, value);
		}
	}

	public AlphaMode AlphaMode
	{
		get
		{
			return GetEnumValue<AlphaMode>(1);
		}
		set
		{
			SetEnumValue(1, value);
		}
	}

	public bool ClampOutput
	{
		get
		{
			return GetBoolValue(2);
		}
		set
		{
			SetValue(2, value);
		}
	}

	public ColorMatrix(DeviceContext context)
		: base(context, Effect.ColorMatrix)
	{
	}
}
