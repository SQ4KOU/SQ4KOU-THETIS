using System;

namespace SharpDX.Direct2D1.Effects;

public class TableTransfer : Effect
{
	public unsafe float[] RedTable
	{
		get
		{
			float[] array = new float[256];
			fixed (float* value = &array[0])
			{
				GetValue(0, PropertyType.Blob, new IntPtr(value), 1024);
				return array;
			}
		}
		set
		{
			if (value.Length != 256)
			{
				throw new ArgumentException("Invalid table size. Excepting Length 256.");
			}
			fixed (float* value2 = &value[0])
			{
				SetValue(0, PropertyType.Blob, new IntPtr(value2), 1024);
			}
		}
	}

	public bool RedDisable
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

	public unsafe float[] GreenTable
	{
		get
		{
			float[] array = new float[256];
			fixed (float* value = &array[0])
			{
				GetValue(2, PropertyType.Blob, new IntPtr(value), 1024);
				return array;
			}
		}
		set
		{
			if (value.Length != 256)
			{
				throw new ArgumentException("Invalid table size. Excepting Length 256.");
			}
			fixed (float* value2 = &value[0])
			{
				SetValue(2, PropertyType.Blob, new IntPtr(value2), 1024);
			}
		}
	}

	public bool GreenDisable
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

	public unsafe float[] BlueTable
	{
		get
		{
			float[] array = new float[256];
			fixed (float* value = &array[0])
			{
				GetValue(4, PropertyType.Blob, new IntPtr(value), 1024);
				return array;
			}
		}
		set
		{
			if (value.Length != 256)
			{
				throw new ArgumentException("Invalid table size. Excepting Length 256.");
			}
			fixed (float* value2 = &value[0])
			{
				SetValue(4, PropertyType.Blob, new IntPtr(value2), 1024);
			}
		}
	}

	public bool BlueDisable
	{
		get
		{
			return GetBoolValue(5);
		}
		set
		{
			SetValue(5, value);
		}
	}

	public unsafe float[] AlphaTable
	{
		get
		{
			float[] array = new float[256];
			fixed (float* value = &array[0])
			{
				GetValue(6, PropertyType.Blob, new IntPtr(value), 1024);
				return array;
			}
		}
		set
		{
			if (value.Length != 256)
			{
				throw new ArgumentException("Invalid table size. Excepting Length 256.");
			}
			fixed (float* value2 = &value[0])
			{
				SetValue(6, PropertyType.Blob, new IntPtr(value2), 1024);
			}
		}
	}

	public bool AlphaDisable
	{
		get
		{
			return GetBoolValue(7);
		}
		set
		{
			SetValue(7, value);
		}
	}

	public bool ClampOutput
	{
		get
		{
			return GetBoolValue(8);
		}
		set
		{
			SetValue(8, value);
		}
	}

	public TableTransfer(DeviceContext context)
		: base(context, Effect.TableTransfer)
	{
	}
}
