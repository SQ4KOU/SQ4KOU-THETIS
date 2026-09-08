namespace SkiaSharp;

public class SKRuntimeShaderBuilder : SKRuntimeEffectBuilder
{
	public SKRuntimeShaderBuilder(SKRuntimeEffect effect)
		: base(effect)
	{
	}

	public SKShader Build()
	{
		return base.Effect.ToShader(base.Uniforms, base.Children);
	}

	public SKShader Build(SKMatrix localMatrix)
	{
		return base.Effect.ToShader(base.Uniforms, base.Children, localMatrix);
	}
}
