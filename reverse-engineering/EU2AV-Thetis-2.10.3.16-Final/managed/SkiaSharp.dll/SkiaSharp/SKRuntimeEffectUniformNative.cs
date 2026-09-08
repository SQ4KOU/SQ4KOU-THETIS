using System;

namespace SkiaSharp;

internal struct SKRuntimeEffectUniformNative : IEquatable<SKRuntimeEffectUniformNative>
{
	public unsafe void* fName;

	public IntPtr fNameLength;

	public IntPtr fOffset;

	public SKRuntimeEffectUniformTypeNative fType;

	public int fCount;

	public SKRuntimeEffectUniformFlagsNative fFlags;

	public unsafe readonly bool Equals(SKRuntimeEffectUniformNative obj)
	{
		if (fName == obj.fName && fNameLength == obj.fNameLength && fOffset == obj.fOffset && fType == obj.fType && fCount == obj.fCount)
		{
			return fFlags == obj.fFlags;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKRuntimeEffectUniformNative obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKRuntimeEffectUniformNative left, SKRuntimeEffectUniformNative right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKRuntimeEffectUniformNative left, SKRuntimeEffectUniformNative right)
	{
		return !left.Equals(right);
	}

	public unsafe override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fName);
		hashCode.Add(fNameLength);
		hashCode.Add(fOffset);
		hashCode.Add(fType);
		hashCode.Add(fCount);
		hashCode.Add(fFlags);
		return hashCode.ToHashCode();
	}
}
