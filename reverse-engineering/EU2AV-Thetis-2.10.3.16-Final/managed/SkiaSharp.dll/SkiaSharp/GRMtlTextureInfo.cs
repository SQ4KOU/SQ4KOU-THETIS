using System;

namespace SkiaSharp;

public struct GRMtlTextureInfo
{
	private IntPtr _textureHandle = default(IntPtr);

	public IntPtr TextureHandle
	{
		readonly get
		{
			return _textureHandle;
		}
		set
		{
			_textureHandle = value;
		}
	}

	public GRMtlTextureInfo(IntPtr textureHandle)
	{
		TextureHandle = textureHandle;
	}

	internal unsafe GRMtlTextureInfoNative ToNative()
	{
		return new GRMtlTextureInfoNative
		{
			fTexture = (void*)TextureHandle
		};
	}

	public readonly bool Equals(GRMtlTextureInfo obj)
	{
		return TextureHandle == obj.TextureHandle;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is GRMtlTextureInfo obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(GRMtlTextureInfo left, GRMtlTextureInfo right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GRMtlTextureInfo left, GRMtlTextureInfo right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(TextureHandle);
		return hashCode.ToHashCode();
	}
}
