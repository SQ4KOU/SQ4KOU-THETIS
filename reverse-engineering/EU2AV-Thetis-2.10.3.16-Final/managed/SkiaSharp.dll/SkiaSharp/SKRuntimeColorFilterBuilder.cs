namespace SkiaSharp;

public class SKRuntimeColorFilterBuilder : SKRuntimeEffectBuilder
{
	public SKRuntimeColorFilterBuilder(SKRuntimeEffect effect)
		: base(effect)
	{
	}

	public SKColorFilter Build()
	{
		return base.Effect.ToColorFilter(base.Uniforms, base.Children);
	}
}
