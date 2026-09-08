using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class YCbCr : Effect
{
	public YcbcrChromaSubSampling ChromaSubSampling
	{
		get
		{
			return GetEnumValue<YcbcrChromaSubSampling>(0);
		}
		set
		{
			SetEnumValue(0, value);
		}
	}

	public RawMatrix3x2 Transform
	{
		get
		{
			return GetMatrix3x2Value(1);
		}
		set
		{
			SetValue(1, value);
		}
	}

	public YcbcrInterpolationMode InterpolationMode
	{
		get
		{
			return GetEnumValue<YcbcrInterpolationMode>(2);
		}
		set
		{
			SetEnumValue(2, value);
		}
	}

	public YCbCr(DeviceContext context)
		: base(context, Effect.YCbCr)
	{
	}
}
