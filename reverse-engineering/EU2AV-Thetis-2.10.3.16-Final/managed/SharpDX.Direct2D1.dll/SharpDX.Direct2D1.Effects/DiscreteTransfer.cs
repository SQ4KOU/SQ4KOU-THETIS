using System;

namespace SharpDX.Direct2D1.Effects;

public class DiscreteTransfer : Effect
{
	public float[] RedTable
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
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

	public float[] GreenTable
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
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

	public float[] BlueTable
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
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

	public float[] AlphaTable
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
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

	public DiscreteTransfer(DeviceContext context)
		: base(context, Effect.DiscreteTransfer)
	{
	}
}
