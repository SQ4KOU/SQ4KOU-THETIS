namespace SharpDX.Direct2D1.Effects;

public class LinearTransfer : Effect
{
	public float RedYIntercept
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

	public float RedSlope
	{
		get
		{
			return GetFloatValue(1);
		}
		set
		{
			SetValue(1, value);
		}
	}

	public bool RedDisable
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

	public float GreenYIntercept
	{
		get
		{
			return GetFloatValue(3);
		}
		set
		{
			SetValue(3, value);
		}
	}

	public float GreenSlope
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

	public bool GreenDisable
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

	public float BlueYIntercept
	{
		get
		{
			return GetFloatValue(6);
		}
		set
		{
			SetValue(6, value);
		}
	}

	public float BlueSlope
	{
		get
		{
			return GetFloatValue(7);
		}
		set
		{
			SetValue(7, value);
		}
	}

	public bool BlueDisable
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

	public float AlphaYIntercept
	{
		get
		{
			return GetFloatValue(9);
		}
		set
		{
			SetValue(9, value);
		}
	}

	public float AlphaSlope
	{
		get
		{
			return GetFloatValue(10);
		}
		set
		{
			SetValue(10, value);
		}
	}

	public bool AlphaDisable
	{
		get
		{
			return GetBoolValue(11);
		}
		set
		{
			SetValue(11, value);
		}
	}

	public bool ClampOutput
	{
		get
		{
			return GetBoolValue(12);
		}
		set
		{
			SetValue(12, value);
		}
	}

	public LinearTransfer(DeviceContext context)
		: base(context, Effect.LinearTransfer)
	{
	}
}
