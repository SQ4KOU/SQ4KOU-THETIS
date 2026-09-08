using System;

namespace SkiaSharp;

public struct GRGlTextureInfo : IEquatable<GRGlTextureInfo>
{
	private uint fTarget;

	private uint fID;

	private uint fFormat;

	private byte fProtected;

	public uint Target
	{
		readonly get
		{
			return fTarget;
		}
		set
		{
			fTarget = value;
		}
	}

	public uint Id
	{
		readonly get
		{
			return fID;
		}
		set
		{
			fID = value;
		}
	}

	public uint Format
	{
		readonly get
		{
			return fFormat;
		}
		set
		{
			fFormat = value;
		}
	}

	public bool Protected
	{
		readonly get
		{
			return fProtected > 0;
		}
		set
		{
			fProtected = (value ? ((byte)1) : ((byte)0));
		}
	}

	public GRGlTextureInfo(uint target, uint id)
	{
		fProtected = 0;
		fTarget = target;
		fID = id;
		fFormat = 0u;
	}

	public GRGlTextureInfo(uint target, uint id, uint format)
	{
		fProtected = 0;
		fTarget = target;
		fID = id;
		fFormat = format;
	}

	public readonly bool Equals(GRGlTextureInfo obj)
	{
		if (fTarget == obj.fTarget && fID == obj.fID && fFormat == obj.fFormat)
		{
			return fProtected == obj.fProtected;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is GRGlTextureInfo obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(GRGlTextureInfo left, GRGlTextureInfo right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GRGlTextureInfo left, GRGlTextureInfo right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fTarget);
		hashCode.Add(fID);
		hashCode.Add(fFormat);
		hashCode.Add(fProtected);
		return hashCode.ToHashCode();
	}
}
