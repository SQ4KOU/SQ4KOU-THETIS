using System;

namespace SkiaSharp;

public class SKRuntimeEffectBuilder : IDisposable
{
	public SKRuntimeEffect Effect { get; }

	public SKRuntimeEffectUniforms Uniforms { get; }

	public SKRuntimeEffectChildren Children { get; }

	public SKRuntimeEffectBuilder(SKRuntimeEffect effect)
	{
		Effect = effect;
		Uniforms = new SKRuntimeEffectUniforms(effect);
		Children = new SKRuntimeEffectChildren(effect);
	}

	public void Dispose()
	{
		Uniforms.Dispose();
		Children.Dispose();
		Effect.Dispose();
	}
}
