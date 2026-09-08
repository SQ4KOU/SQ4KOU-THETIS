using System;

namespace SkiaSharp;

internal struct SKManagedDrawableDelegates : IEquatable<SKManagedDrawableDelegates>
{
	public SKManagedDrawableDrawProxyDelegate fDraw;

	public SKManagedDrawableGetBoundsProxyDelegate fGetBounds;

	public SKManagedDrawableApproximateBytesUsedProxyDelegate fApproximateBytesUsed;

	public SKManagedDrawableMakePictureSnapshotProxyDelegate fMakePictureSnapshot;

	public SKManagedDrawableDestroyProxyDelegate fDestroy;

	public readonly bool Equals(SKManagedDrawableDelegates obj)
	{
		if (fDraw == obj.fDraw && fGetBounds == obj.fGetBounds && fApproximateBytesUsed == obj.fApproximateBytesUsed && fMakePictureSnapshot == obj.fMakePictureSnapshot)
		{
			return fDestroy == obj.fDestroy;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKManagedDrawableDelegates obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKManagedDrawableDelegates left, SKManagedDrawableDelegates right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKManagedDrawableDelegates left, SKManagedDrawableDelegates right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fDraw);
		hashCode.Add(fGetBounds);
		hashCode.Add(fApproximateBytesUsed);
		hashCode.Add(fMakePictureSnapshot);
		hashCode.Add(fDestroy);
		return hashCode.ToHashCode();
	}
}
