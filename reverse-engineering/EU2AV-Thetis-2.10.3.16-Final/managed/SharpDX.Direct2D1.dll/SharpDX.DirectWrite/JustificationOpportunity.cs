using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
public struct JustificationOpportunity
{
	[FieldOffset(0)]
	public float ExpansionMinimum;

	[FieldOffset(4)]
	public float ExpansionMaximum;

	[FieldOffset(8)]
	public float CompressionMaximum;

	[FieldOffset(12)]
	internal int _ExpansionPriority;

	[FieldOffset(12)]
	internal int _CompressionPriority;

	[FieldOffset(12)]
	internal int _AllowResidualExpansion;

	[FieldOffset(12)]
	internal int _AllowResidualCompression;

	[FieldOffset(12)]
	internal int _ApplyToLeadingEdge;

	[FieldOffset(12)]
	internal int _ApplyToTrailingEdge;

	[FieldOffset(12)]
	internal int _Reserved;

	public int ExpansionPriority
	{
		get
		{
			return _ExpansionPriority & 0xFF;
		}
		set
		{
			_ExpansionPriority = (_ExpansionPriority & -256) | (value & 0xFF);
		}
	}

	public int CompressionPriority
	{
		get
		{
			return (_CompressionPriority >> 8) & 0xFF;
		}
		set
		{
			_CompressionPriority = (_CompressionPriority & -65281) | ((value & 0xFF) << 8);
		}
	}

	public bool AllowResidualExpansion
	{
		get
		{
			return ((_AllowResidualExpansion >> 16) & 1) != 0;
		}
		set
		{
			_AllowResidualExpansion = (_AllowResidualExpansion & -65537) | (((value ? 1 : 0) & 1) << 16);
		}
	}

	public bool AllowResidualCompression
	{
		get
		{
			return ((_AllowResidualCompression >> 17) & 1) != 0;
		}
		set
		{
			_AllowResidualCompression = (_AllowResidualCompression & -131073) | (((value ? 1 : 0) & 1) << 17);
		}
	}

	public bool ApplyToLeadingEdge
	{
		get
		{
			return ((_ApplyToLeadingEdge >> 18) & 1) != 0;
		}
		set
		{
			_ApplyToLeadingEdge = (_ApplyToLeadingEdge & -262145) | (((value ? 1 : 0) & 1) << 18);
		}
	}

	public bool ApplyToTrailingEdge
	{
		get
		{
			return ((_ApplyToTrailingEdge >> 19) & 1) != 0;
		}
		set
		{
			_ApplyToTrailingEdge = (_ApplyToTrailingEdge & -524289) | (((value ? 1 : 0) & 1) << 19);
		}
	}

	public int Reserved
	{
		get
		{
			return (_Reserved >> 20) & 0xFFF;
		}
		set
		{
			_Reserved = (_Reserved & 0xFFFFF) | ((value & 0xFFF) << 20);
		}
	}
}
