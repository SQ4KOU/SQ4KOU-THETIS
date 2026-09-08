namespace SharpDX.Direct2D1.Effects;

public class GammaTransfer : Effect
{
	public float RedAmplitude
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

	public float RedExponent
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

	public float RedOffset
	{
		get
		{
			return GetFloatValue(2);
		}
		set
		{
			SetValue(2, value);
		}
	}

	public bool RedDisable
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

	public float GreenAmplitude
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

	public float GreenExponent
	{
		get
		{
			return GetFloatValue(5);
		}
		set
		{
			SetValue(5, value);
		}
	}

	public float GreenOffset
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

	public bool GreenDisable
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

	public float BlueAmplitude
	{
		get
		{
			return GetFloatValue(8);
		}
		set
		{
			SetValue(8, value);
		}
	}

	public float BlueExponent
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

	public float BlueOffset
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

	public bool BlueDisable
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

	public float AlphaAmplitude
	{
		get
		{
			return GetFloatValue(12);
		}
		set
		{
			SetValue(12, value);
		}
	}

	public float AlphaExponent
	{
		get
		{
			return GetFloatValue(13);
		}
		set
		{
			SetValue(13, value);
		}
	}

	public float AlphaOffset
	{
		get
		{
			return GetFloatValue(14);
		}
		set
		{
			SetValue(14, value);
		}
	}

	public bool AlphaDisable
	{
		get
		{
			return GetBoolValue(15);
		}
		set
		{
			SetValue(15, value);
		}
	}

	public bool ClampOutput
	{
		get
		{
			return GetBoolValue(16);
		}
		set
		{
			SetValue(16, value);
		}
	}

	public GammaTransfer(DeviceContext context)
		: base(context, Effect.GammaTransfer)
	{
	}
}
