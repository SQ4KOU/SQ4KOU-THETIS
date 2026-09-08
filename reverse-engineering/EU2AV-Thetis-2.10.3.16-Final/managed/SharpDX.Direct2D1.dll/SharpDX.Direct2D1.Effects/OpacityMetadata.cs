using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1.Effects;

public class OpacityMetadata : Effect
{
	public RawVector4 OpaqueRectangle
	{
		get
		{
			return GetVector4Value(0);
		}
		set
		{
			SetValue(0, value);
		}
	}

	public OpacityMetadata(DeviceContext deviceContext)
		: base(deviceContext, Effect.OpacityMetadata)
	{
	}
}
