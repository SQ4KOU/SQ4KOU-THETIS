using System;

namespace SkiaSharp;

internal struct GRD3DTextureResourceInfoNative : IEquatable<GRD3DTextureResourceInfoNative>
{
	public IntPtr fResource;

	public IntPtr fAlloc;

	public uint fResourceState;

	public uint fFormat;

	public uint fSampleCount;

	public uint fLevelCount;

	public uint fSampleQualityPattern;

	public byte fProtected;

	public readonly bool Equals(GRD3DTextureResourceInfoNative obj)
	{
		if (fResource == obj.fResource && fAlloc == obj.fAlloc && fResourceState == obj.fResourceState && fFormat == obj.fFormat && fSampleCount == obj.fSampleCount && fLevelCount == obj.fLevelCount && fSampleQualityPattern == obj.fSampleQualityPattern)
		{
			return fProtected == obj.fProtected;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is GRD3DTextureResourceInfoNative obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(GRD3DTextureResourceInfoNative left, GRD3DTextureResourceInfoNative right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GRD3DTextureResourceInfoNative left, GRD3DTextureResourceInfoNative right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fResource);
		hashCode.Add(fAlloc);
		hashCode.Add(fResourceState);
		hashCode.Add(fFormat);
		hashCode.Add(fSampleCount);
		hashCode.Add(fLevelCount);
		hashCode.Add(fSampleQualityPattern);
		hashCode.Add(fProtected);
		return hashCode.ToHashCode();
	}
}
