using System;

namespace SharpDX.Direct2D1.Effects;

public class Histogram : Effect
{
	public int NumBins
	{
		get
		{
			return (int)GetUIntValue(0);
		}
		set
		{
			SetValue(0, (uint)value);
		}
	}

	public ChannelSelector ChannelSelect
	{
		get
		{
			return GetEnumValue<ChannelSelector>(1);
		}
		set
		{
			SetEnumValue(1, value);
		}
	}

	public unsafe float[] HistogramOutput
	{
		get
		{
			float[] array = new float[NumBins];
			fixed (float* ptr = array)
			{
				void* ptr2 = ptr;
				GetValue(2, PropertyType.Blob, (IntPtr)ptr2, 4 * array.Length);
			}
			return array;
		}
	}

	public Histogram(DeviceContext context)
		: base(context, Effect.Histogram)
	{
	}
}
