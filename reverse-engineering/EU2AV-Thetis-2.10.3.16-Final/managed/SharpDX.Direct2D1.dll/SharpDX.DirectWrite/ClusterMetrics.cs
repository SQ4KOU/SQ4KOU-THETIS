using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
public struct ClusterMetrics
{
	[FieldOffset(0)]
	public float Width;

	[FieldOffset(4)]
	public short Length;

	[FieldOffset(6)]
	internal short _CanWrapLineAfter;

	[FieldOffset(6)]
	internal short _IsWhitespace;

	[FieldOffset(6)]
	internal short _IsNewline;

	[FieldOffset(6)]
	internal short _IsSoftHyphen;

	[FieldOffset(6)]
	internal short _IsRightToLeft;

	[FieldOffset(6)]
	internal short _Padding;

	public bool CanWrapLineAfter
	{
		get
		{
			return (_CanWrapLineAfter & 1) != 0;
		}
		set
		{
			_CanWrapLineAfter = (short)((_CanWrapLineAfter & -2) | ((value ? 1 : 0) & 1));
		}
	}

	public bool IsWhitespace
	{
		get
		{
			return ((_IsWhitespace >> 1) & 1) != 0;
		}
		set
		{
			_IsWhitespace = (short)((_IsWhitespace & -3) | (((value ? 1 : 0) & 1) << 1));
		}
	}

	public bool IsNewline
	{
		get
		{
			return ((_IsNewline >> 2) & 1) != 0;
		}
		set
		{
			_IsNewline = (short)((_IsNewline & -5) | (((value ? 1 : 0) & 1) << 2));
		}
	}

	public bool IsSoftHyphen
	{
		get
		{
			return ((_IsSoftHyphen >> 3) & 1) != 0;
		}
		set
		{
			_IsSoftHyphen = (short)((_IsSoftHyphen & -9) | (((value ? 1 : 0) & 1) << 3));
		}
	}

	public bool IsRightToLeft
	{
		get
		{
			return ((_IsRightToLeft >> 4) & 1) != 0;
		}
		set
		{
			_IsRightToLeft = (short)((_IsRightToLeft & -17) | (((value ? 1 : 0) & 1) << 4));
		}
	}

	internal short Padding
	{
		get
		{
			return (short)((_Padding >> 5) & 0x7FF);
		}
		set
		{
			_Padding = (short)((_Padding & -65505) | ((value & 0x7FF) << 5));
		}
	}
}
