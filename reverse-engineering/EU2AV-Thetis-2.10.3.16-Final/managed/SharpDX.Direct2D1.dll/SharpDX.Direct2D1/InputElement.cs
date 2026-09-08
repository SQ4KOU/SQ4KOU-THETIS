using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct2D1;

public struct InputElement
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public IntPtr SemanticName;

		public int SemanticIndex;

		public Format Format;

		public int Slot;

		public int AlignedByteOffset;
	}

	public string SemanticName;

	public int SemanticIndex;

	public Format Format;

	public int Slot;

	public int AlignedByteOffset;

	public static int AppendAligned => -1;

	public InputElement(string name, int index, Format format, int slot)
	{
		SemanticName = name;
		SemanticIndex = index;
		Format = format;
		Slot = slot;
		AlignedByteOffset = AppendAligned;
	}

	public InputElement(string name, int index, Format format, int offset, int slot)
	{
		SemanticName = name;
		SemanticIndex = index;
		Format = format;
		Slot = slot;
		AlignedByteOffset = offset;
	}

	public bool Equals(InputElement other)
	{
		if (object.Equals(other.SemanticName, SemanticName) && other.SemanticIndex == SemanticIndex && object.Equals(other.Format, Format) && other.Slot == Slot)
		{
			return other.AlignedByteOffset == AlignedByteOffset;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.GetType() != typeof(InputElement))
		{
			return false;
		}
		return Equals((InputElement)obj);
	}

	public override int GetHashCode()
	{
		return (((((((SemanticName.GetHashCode() * 397) ^ SemanticIndex.GetHashCode()) * 397) ^ Format.GetHashCode()) * 397) ^ Slot.GetHashCode()) * 397) ^ AlignedByteOffset.GetHashCode();
	}

	public static bool operator ==(InputElement left, InputElement right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(InputElement left, InputElement right)
	{
		return !left.Equals(right);
	}

	internal void __MarshalFree(ref __Native @ref)
	{
		Marshal.FreeHGlobal(@ref.SemanticName);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		SemanticName = Marshal.PtrToStringAnsi(@ref.SemanticName);
		SemanticIndex = @ref.SemanticIndex;
		Format = @ref.Format;
		Slot = @ref.Slot;
		AlignedByteOffset = @ref.AlignedByteOffset;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.SemanticName = Marshal.StringToHGlobalAnsi(SemanticName);
		@ref.SemanticIndex = SemanticIndex;
		@ref.Format = Format;
		@ref.Slot = Slot;
		@ref.AlignedByteOffset = AlignedByteOffset;
	}
}
