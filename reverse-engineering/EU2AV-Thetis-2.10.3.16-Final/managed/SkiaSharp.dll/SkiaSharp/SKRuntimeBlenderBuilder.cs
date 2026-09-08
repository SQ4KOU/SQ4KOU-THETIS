namespace SkiaSharp;

public class SKRuntimeBlenderBuilder : SKRuntimeEffectBuilder
{
	public SKRuntimeBlenderBuilder(SKRuntimeEffect effect)
		: base(effect)
	{
	}

	public SKBlender Build()
	{
		return base.Effect.ToBlender(base.Uniforms, base.Children);
	}
}
