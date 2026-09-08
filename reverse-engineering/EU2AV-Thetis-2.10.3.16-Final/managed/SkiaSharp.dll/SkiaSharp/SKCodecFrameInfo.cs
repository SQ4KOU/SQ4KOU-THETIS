using System;

namespace SkiaSharp;

public struct SKCodecFrameInfo : IEquatable<SKCodecFrameInfo>
{
	private int fRequiredFrame;

	private int fDuration;

	private byte fFullyReceived;

	private SKAlphaType fAlphaType;

	private byte fHasAlphaWithinBounds;

	private SKCodecAnimationDisposalMethod fDisposalMethod;

	private SKCodecAnimationBlend fBlend;

	private SKRectI fFrameRect;

	public int RequiredFrame
	{
		readonly get
		{
			return fRequiredFrame;
		}
		set
		{
			fRequiredFrame = value;
		}
	}

	public int Duration
	{
		readonly get
		{
			return fDuration;
		}
		set
		{
			fDuration = value;
		}
	}

	public bool FullyRecieved
	{
		readonly get
		{
			return fFullyReceived > 0;
		}
		set
		{
			fFullyReceived = (value ? ((byte)1) : ((byte)0));
		}
	}

	public SKAlphaType AlphaType
	{
		readonly get
		{
			return fAlphaType;
		}
		set
		{
			fAlphaType = value;
		}
	}

	public bool HasAlphaWithinBounds
	{
		readonly get
		{
			return fHasAlphaWithinBounds > 0;
		}
		set
		{
			fHasAlphaWithinBounds = (value ? ((byte)1) : ((byte)0));
		}
	}

	public SKCodecAnimationDisposalMethod DisposalMethod
	{
		readonly get
		{
			return fDisposalMethod;
		}
		set
		{
			fDisposalMethod = value;
		}
	}

	public SKCodecAnimationBlend Blend
	{
		readonly get
		{
			return fBlend;
		}
		set
		{
			fBlend = value;
		}
	}

	public SKRectI FrameRect
	{
		readonly get
		{
			return fFrameRect;
		}
		set
		{
			fFrameRect = value;
		}
	}

	public readonly bool Equals(SKCodecFrameInfo obj)
	{
		if (fRequiredFrame == obj.fRequiredFrame && fDuration == obj.fDuration && fFullyReceived == obj.fFullyReceived && fAlphaType == obj.fAlphaType && fHasAlphaWithinBounds == obj.fHasAlphaWithinBounds && fDisposalMethod == obj.fDisposalMethod && fBlend == obj.fBlend)
		{
			return fFrameRect == obj.fFrameRect;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKCodecFrameInfo obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKCodecFrameInfo left, SKCodecFrameInfo right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKCodecFrameInfo left, SKCodecFrameInfo right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fRequiredFrame);
		hashCode.Add(fDuration);
		hashCode.Add(fFullyReceived);
		hashCode.Add(fAlphaType);
		hashCode.Add(fHasAlphaWithinBounds);
		hashCode.Add(fDisposalMethod);
		hashCode.Add(fBlend);
		hashCode.Add(fFrameRect);
		return hashCode.ToHashCode();
	}
}
