using System;
using System.Collections.Generic;

namespace SkiaSharp;

public class SKBlender : SKObject, ISKReferenceCounted
{
	private sealed class SKBlenderStatic : SKBlender
	{
		internal SKBlenderStatic(IntPtr x)
			: base(x, owns: false)
		{
		}

		protected override void Dispose(bool disposing)
		{
		}
	}

	private static readonly Dictionary<SKBlendMode, SKBlender> blendModeBlenders;

	static SKBlender()
	{
		Array values = Enum.GetValues(typeof(SKBlendMode));
		blendModeBlenders = new Dictionary<SKBlendMode, SKBlender>(values.Length);
		foreach (SKBlendMode item in values)
		{
			blendModeBlenders[item] = new SKBlenderStatic(SkiaApi.sk_blender_new_mode(item));
		}
	}

	internal static void EnsureStaticInstanceAreInitialized()
	{
	}

	internal SKBlender(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public static SKBlender CreateBlendMode(SKBlendMode mode)
	{
		if (!blendModeBlenders.TryGetValue(mode, out SKBlender value))
		{
			throw new ArgumentOutOfRangeException("mode");
		}
		return value;
	}

	public static SKBlender CreateArithmetic(float k1, float k2, float k3, float k4, bool enforcePMColor)
	{
		return GetObject(SkiaApi.sk_blender_new_arithmetic(k1, k2, k3, k4, enforcePMColor));
	}

	internal static SKBlender GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKBlender(h, o));
	}
}
