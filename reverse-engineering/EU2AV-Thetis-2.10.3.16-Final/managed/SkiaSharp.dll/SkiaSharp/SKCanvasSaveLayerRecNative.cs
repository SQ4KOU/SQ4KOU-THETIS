using System;

namespace SkiaSharp;

internal struct SKCanvasSaveLayerRecNative : IEquatable<SKCanvasSaveLayerRecNative>
{
	public unsafe SKRect* fBounds;

	public IntPtr fPaint;

	public IntPtr fBackdrop;

	public SKCanvasSaveLayerRecFlags fFlags;

	public unsafe readonly bool Equals(SKCanvasSaveLayerRecNative obj)
	{
		if (fBounds == obj.fBounds && fPaint == obj.fPaint && fBackdrop == obj.fBackdrop)
		{
			return fFlags == obj.fFlags;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKCanvasSaveLayerRecNative obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKCanvasSaveLayerRecNative left, SKCanvasSaveLayerRecNative right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKCanvasSaveLayerRecNative left, SKCanvasSaveLayerRecNative right)
	{
		return !left.Equals(right);
	}

	public unsafe override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add((void*)fBounds);
		hashCode.Add(fPaint);
		hashCode.Add(fBackdrop);
		hashCode.Add(fFlags);
		return hashCode.ToHashCode();
	}
}
