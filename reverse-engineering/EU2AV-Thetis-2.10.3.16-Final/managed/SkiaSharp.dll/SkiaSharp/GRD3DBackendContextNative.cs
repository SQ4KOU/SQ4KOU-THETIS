using System;

namespace SkiaSharp;

internal struct GRD3DBackendContextNative : IEquatable<GRD3DBackendContextNative>
{
	public IntPtr fAdapter;

	public IntPtr fDevice;

	public IntPtr fQueue;

	public IntPtr fMemoryAllocator;

	public byte fProtectedContext;

	public readonly bool Equals(GRD3DBackendContextNative obj)
	{
		if (fAdapter == obj.fAdapter && fDevice == obj.fDevice && fQueue == obj.fQueue && fMemoryAllocator == obj.fMemoryAllocator)
		{
			return fProtectedContext == obj.fProtectedContext;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is GRD3DBackendContextNative obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(GRD3DBackendContextNative left, GRD3DBackendContextNative right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GRD3DBackendContextNative left, GRD3DBackendContextNative right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fAdapter);
		hashCode.Add(fDevice);
		hashCode.Add(fQueue);
		hashCode.Add(fMemoryAllocator);
		hashCode.Add(fProtectedContext);
		return hashCode.ToHashCode();
	}
}
