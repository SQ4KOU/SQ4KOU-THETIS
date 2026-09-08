using SharpDX.Mathematics.Interop;
using SharpDX.WIC;

namespace SharpDX.Direct2D1.Effects;

public class BitmapSource : Effect
{
	private SharpDX.WIC.BitmapSource wicBitmapSource;

	public SharpDX.WIC.BitmapSource WicBitmapSource
	{
		get
		{
			if (wicBitmapSource == null)
			{
				wicBitmapSource = GetComObjectValue<SharpDX.WIC.BitmapSource>(0);
			}
			return wicBitmapSource;
		}
		set
		{
			wicBitmapSource = value;
			SetValue(0, wicBitmapSource);
		}
	}

	public RawVector2 ScaleSource
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

	public bool EnableDpiCorrection
	{
		get
		{
			return GetBoolValue(3);
		}
		set
		{
			SetValue(3, value);
		}
	}

	public AlphaMode AlphaMode
	{
		get
		{
			return GetEnumValue<AlphaMode>(4);
		}
		set
		{
			SetEnumValue(4, value);
		}
	}

	public BitmapSourceOrientation Orientation
	{
		get
		{
			return GetEnumValue<BitmapSourceOrientation>(5);
		}
		set
		{
			SetEnumValue(5, value);
		}
	}

	public BitmapSource(DeviceContext context)
		: base(context, Effect.BitmapSource)
	{
	}
}
