using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp;

public class SKRuntimeEffect : SKObject, ISKReferenceCounted
{
	private string[] children;

	private string[] uniforms;

	public int UniformSize => (int)SkiaApi.sk_runtimeeffect_get_uniform_byte_size(Handle);

	public IReadOnlyList<string> Children => children ?? (children = GetChildrenNames().ToArray());

	public IReadOnlyList<string> Uniforms => uniforms ?? (uniforms = GetUniformNames().ToArray());

	internal SKRuntimeEffect(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public static SKRuntimeEffect CreateShader(string sksl, out string errors)
	{
		using SKString sKString = new SKString(sksl);
		using SKString sKString2 = new SKString();
		SKRuntimeEffect result = GetObject(SkiaApi.sk_runtimeeffect_make_for_shader(sKString.Handle, sKString2.Handle));
		errors = sKString2?.ToString();
		string obj = errors;
		if (obj != null && obj.Length == 0)
		{
			errors = null;
		}
		return result;
	}

	public static SKRuntimeEffect CreateColorFilter(string sksl, out string errors)
	{
		using SKString sKString = new SKString(sksl);
		using SKString sKString2 = new SKString();
		SKRuntimeEffect result = GetObject(SkiaApi.sk_runtimeeffect_make_for_color_filter(sKString.Handle, sKString2.Handle));
		errors = sKString2?.ToString();
		string obj = errors;
		if (obj != null && obj.Length == 0)
		{
			errors = null;
		}
		return result;
	}

	public static SKRuntimeEffect CreateBlender(string sksl, out string errors)
	{
		using SKString sKString = new SKString(sksl);
		using SKString sKString2 = new SKString();
		SKRuntimeEffect result = GetObject(SkiaApi.sk_runtimeeffect_make_for_blender(sKString.Handle, sKString2.Handle));
		errors = sKString2?.ToString();
		string obj = errors;
		if (obj != null && obj.Length == 0)
		{
			errors = null;
		}
		return result;
	}

	public static SKRuntimeShaderBuilder BuildShader(string sksl)
	{
		SKRuntimeEffect effect = CreateShader(sksl, out var errors);
		ValidateResult(effect, errors);
		return new SKRuntimeShaderBuilder(effect);
	}

	public static SKRuntimeColorFilterBuilder BuildColorFilter(string sksl)
	{
		SKRuntimeEffect effect = CreateColorFilter(sksl, out var errors);
		ValidateResult(effect, errors);
		return new SKRuntimeColorFilterBuilder(effect);
	}

	public static SKRuntimeBlenderBuilder BuildBlender(string sksl)
	{
		SKRuntimeEffect effect = CreateBlender(sksl, out var errors);
		ValidateResult(effect, errors);
		return new SKRuntimeBlenderBuilder(effect);
	}

	private static void ValidateResult(SKRuntimeEffect effect, string errors)
	{
		if (effect == null)
		{
			if (string.IsNullOrEmpty(errors))
			{
				throw new SKRuntimeEffectBuilderException("Failed to compile the runtime effect. There was an unknown error.");
			}
			throw new SKRuntimeEffectBuilderException("Failed to compile the runtime effect. There was an error: " + errors);
		}
	}

	private IEnumerable<string> GetChildrenNames()
	{
		int count = (int)SkiaApi.sk_runtimeeffect_get_children_size(Handle);
		using SKString str = new SKString();
		for (int i = 0; i < count; i++)
		{
			SkiaApi.sk_runtimeeffect_get_child_name(Handle, i, str.Handle);
			yield return str.ToString();
		}
	}

	private IEnumerable<string> GetUniformNames()
	{
		int count = (int)SkiaApi.sk_runtimeeffect_get_uniforms_size(Handle);
		using SKString str = new SKString();
		for (int i = 0; i < count; i++)
		{
			SkiaApi.sk_runtimeeffect_get_uniform_name(Handle, i, str.Handle);
			yield return str.ToString();
		}
	}

	public unsafe SKShader ToShader()
	{
		return ToShader(null, null, null);
	}

	public unsafe SKShader ToShader(SKRuntimeEffectUniforms uniforms)
	{
		return ToShader(uniforms.ToData(), null, null);
	}

	public unsafe SKShader ToShader(SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children)
	{
		return ToShader(uniforms.ToData(), children.ToArray(), null);
	}

	public unsafe SKShader ToShader(SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children, SKMatrix localMatrix)
	{
		return ToShader(uniforms.ToData(), children.ToArray(), &localMatrix);
	}

	private unsafe SKShader ToShader(SKData uniforms, SKObject[] children, SKMatrix* localMatrix)
	{
		IntPtr intPtr = uniforms?.Handle ?? IntPtr.Zero;
		Utils.RentedArray<IntPtr> rentedArray = Utils.RentHandlesArray(children, nullIfEmpty: true);
		try
		{
			fixed (IntPtr* ptr = rentedArray)
			{
				return SKShader.GetObject(SkiaApi.sk_runtimeeffect_make_shader(Handle, intPtr, ptr, (IntPtr)rentedArray.Length, localMatrix));
			}
		}
		finally
		{
			rentedArray.Dispose();
		}
	}

	public SKColorFilter ToColorFilter()
	{
		return ToColorFilter((SKData)null, (SKObject[])null);
	}

	public SKColorFilter ToColorFilter(SKRuntimeEffectUniforms uniforms)
	{
		return ToColorFilter(uniforms.ToData(), null);
	}

	private SKColorFilter ToColorFilter(SKData uniforms)
	{
		return ToColorFilter(uniforms, null);
	}

	public SKColorFilter ToColorFilter(SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children)
	{
		return ToColorFilter(uniforms.ToData(), children.ToArray());
	}

	private unsafe SKColorFilter ToColorFilter(SKData uniforms, SKObject[] children)
	{
		IntPtr intPtr = uniforms?.Handle ?? IntPtr.Zero;
		Utils.RentedArray<IntPtr> rentedArray = Utils.RentHandlesArray(children, nullIfEmpty: true);
		try
		{
			fixed (IntPtr* ptr = rentedArray)
			{
				return SKColorFilter.GetObject(SkiaApi.sk_runtimeeffect_make_color_filter(Handle, intPtr, ptr, (IntPtr)rentedArray.Length));
			}
		}
		finally
		{
			rentedArray.Dispose();
		}
	}

	public SKBlender ToBlender()
	{
		return ToBlender((SKData)null, (SKObject[])null);
	}

	public SKBlender ToBlender(SKRuntimeEffectUniforms uniforms)
	{
		return ToBlender(uniforms.ToData(), null);
	}

	private SKBlender ToBlender(SKData uniforms)
	{
		return ToBlender(uniforms, null);
	}

	public SKBlender ToBlender(SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children)
	{
		return ToBlender(uniforms.ToData(), children.ToArray());
	}

	private unsafe SKBlender ToBlender(SKData uniforms, SKObject[] children)
	{
		IntPtr intPtr = uniforms?.Handle ?? IntPtr.Zero;
		Utils.RentedArray<IntPtr> rentedArray = Utils.RentHandlesArray(children, nullIfEmpty: true);
		try
		{
			fixed (IntPtr* ptr = rentedArray)
			{
				return SKBlender.GetObject(SkiaApi.sk_runtimeeffect_make_blender(Handle, intPtr, ptr, (IntPtr)rentedArray.Length));
			}
		}
		finally
		{
			rentedArray.Dispose();
		}
	}

	internal static SKRuntimeEffect GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKRuntimeEffect(h, o));
	}
}
