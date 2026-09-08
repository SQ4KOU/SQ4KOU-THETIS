using System;

namespace SkiaSharp;

internal struct SKRuntimeEffectChildNative : IEquatable<SKRuntimeEffectChildNative>
{
	public unsafe void* fName;

	public IntPtr fNameLength;

	public SKRuntimeEffectChildTypeNative fType;

	public int fIndex;

	public unsafe readonly bool Equals(SKRuntimeEffectChildNative obj)
	{
		if (fName == obj.fName && fNameLength == obj.fNameLength && fType == obj.fType)
		{
			return fIndex == obj.fIndex;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKRuntimeEffectChildNative obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKRuntimeEffectChildNative left, SKRuntimeEffectChildNative right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKRuntimeEffectChildNative left, SKRuntimeEffectChildNative right)
	{
		return !left.Equals(right);
	}

	public unsafe override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fName);
		hashCode.Add(fNameLength);
		hashCode.Add(fType);
		hashCode.Add(fIndex);
		return hashCode.ToHashCode();
	}
}
