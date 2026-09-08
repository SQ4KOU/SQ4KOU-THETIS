using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class Turbulence : Effect
{
	public RawVector2 Offset
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

	public RawVector2 BaseFrequency
	{
		get
		{
			return GetVector2Value(2);
		}
		set
		{
			SetValue(2, value);
		}
	}

	public int OctaveCount
	{
		get
		{
			return (int)GetUIntValue(3);
		}
		set
		{
			SetValue(3, (uint)value);
		}
	}

	public int Seed
	{
		get
		{
			return (int)GetUIntValue(4);
		}
		set
		{
			SetValue(4, (uint)value);
		}
	}

	public TurbulenceNoise Noise
	{
		get
		{
			return GetEnumValue<TurbulenceNoise>(5);
		}
		set
		{
			SetEnumValue(5, value);
		}
	}

	public bool Stitchable
	{
		get
		{
			return GetBoolValue(6);
		}
		set
		{
			SetValue(6, value);
		}
	}

	public Turbulence(DeviceContext context)
		: base(context, Effect.Turbulence)
	{
	}
}
