using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
public struct ScriptProperties
{
	[FieldOffset(0)]
	public int IsoScriptCode;

	[FieldOffset(4)]
	public int IsoScriptNumber;

	[FieldOffset(8)]
	public int ClusterLookahead;

	[FieldOffset(12)]
	public int JustificationCharacter;

	[FieldOffset(16)]
	internal int _RestrictCaretToClusters;

	[FieldOffset(16)]
	internal int _UsesWordDividers;

	[FieldOffset(16)]
	internal int _IsDiscreteWriting;

	[FieldOffset(16)]
	internal int _IsBlockWriting;

	[FieldOffset(16)]
	internal int _IsDistributedWithinCluster;

	[FieldOffset(16)]
	internal int _IsConnectedWriting;

	[FieldOffset(16)]
	internal int _IsCursiveWriting;

	[FieldOffset(16)]
	internal int _Reserved;

	public bool RestrictCaretToClusters
	{
		get
		{
			return (_RestrictCaretToClusters & 1) != 0;
		}
		set
		{
			_RestrictCaretToClusters = (_RestrictCaretToClusters & -2) | ((value ? 1 : 0) & 1);
		}
	}

	public bool UsesWordDividers
	{
		get
		{
			return ((_UsesWordDividers >> 1) & 1) != 0;
		}
		set
		{
			_UsesWordDividers = (_UsesWordDividers & -3) | (((value ? 1 : 0) & 1) << 1);
		}
	}

	public bool IsDiscreteWriting
	{
		get
		{
			return ((_IsDiscreteWriting >> 2) & 1) != 0;
		}
		set
		{
			_IsDiscreteWriting = (_IsDiscreteWriting & -5) | (((value ? 1 : 0) & 1) << 2);
		}
	}

	public bool IsBlockWriting
	{
		get
		{
			return ((_IsBlockWriting >> 3) & 1) != 0;
		}
		set
		{
			_IsBlockWriting = (_IsBlockWriting & -9) | (((value ? 1 : 0) & 1) << 3);
		}
	}

	public bool IsDistributedWithinCluster
	{
		get
		{
			return ((_IsDistributedWithinCluster >> 4) & 1) != 0;
		}
		set
		{
			_IsDistributedWithinCluster = (_IsDistributedWithinCluster & -17) | (((value ? 1 : 0) & 1) << 4);
		}
	}

	public bool IsConnectedWriting
	{
		get
		{
			return ((_IsConnectedWriting >> 5) & 1) != 0;
		}
		set
		{
			_IsConnectedWriting = (_IsConnectedWriting & -33) | (((value ? 1 : 0) & 1) << 5);
		}
	}

	public bool IsCursiveWriting
	{
		get
		{
			return ((_IsCursiveWriting >> 6) & 1) != 0;
		}
		set
		{
			_IsCursiveWriting = (_IsCursiveWriting & -65) | (((value ? 1 : 0) & 1) << 6);
		}
	}

	public int Reserved
	{
		get
		{
			return (_Reserved >> 7) & 0x1FFFFFF;
		}
		set
		{
			_Reserved = (_Reserved & 0x7F) | ((value & 0x1FFFFFF) << 7);
		}
	}
}
