using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class Scale : Effect
{
	public RawVector2 ScaleAmount
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

	public RawVector2 CenterPoint
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

	public BorderMode BorderMode
	{
		get
		{
			return GetEnumValue<BorderMode>(3);
		}
		set
		{
			SetEnumValue(3, value);
		}
	}

	public float Sharpness
	{
		get
		{
			return GetFloatValue(4);
		}
		set
		{
			SetValue(4, value);
		}
	}

	public InterpolationMode InterpolationMode
	{
		get
		{
			return GetEnumValue<InterpolationMode>(2);
		}
		set
		{
			SetEnumValue(2, value);
		}
	}

	public Scale(DeviceContext context)
		: base(context, Effect.Scale)
	{
	}
}
