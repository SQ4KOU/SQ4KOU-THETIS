namespace SkiaSharp;

public readonly struct SKRuntimeEffectChild
{
	private readonly SKObject value;

	public SKObject Value => value;

	public SKShader Shader => value as SKShader;

	public SKColorFilter ColorFilter => value as SKColorFilter;

	public SKBlender Blender => value as SKBlender;

	public SKRuntimeEffectChild(SKShader shader)
	{
		value = shader;
	}

	public SKRuntimeEffectChild(SKColorFilter colorFilter)
	{
		value = colorFilter;
	}

	public SKRuntimeEffectChild(SKBlender blender)
	{
		value = blender;
	}

	public static implicit operator SKRuntimeEffectChild(SKShader shader)
	{
		return new SKRuntimeEffectChild(shader);
	}

	public static implicit operator SKRuntimeEffectChild(SKColorFilter colorFilter)
	{
		return new SKRuntimeEffectChild(colorFilter);
	}

	public static implicit operator SKRuntimeEffectChild(SKBlender blender)
	{
		return new SKRuntimeEffectChild(blender);
	}
}
