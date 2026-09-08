using System;
using WindowsFirewallHelper.COMInterop;

namespace WindowsFirewallHelper.Collections;

internal class FirewallLegacyPortCollectionKey : IEquatable<FirewallLegacyPortCollectionKey>
{
	public int PortNumber { get; }

	public NetFwIPProtocol ProtocolType { get; }

	public FirewallLegacyPortCollectionKey(int portNumber, NetFwIPProtocol protocolType)
	{
		PortNumber = portNumber;
		ProtocolType = protocolType;
	}

	public bool Equals(FirewallLegacyPortCollectionKey other)
	{
		if (other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (PortNumber == other.PortNumber)
		{
			return ProtocolType == other.ProtocolType;
		}
		return false;
	}

	public static bool operator ==(FirewallLegacyPortCollectionKey left, FirewallLegacyPortCollectionKey right)
	{
		if (!object.Equals(left, right))
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

	public static bool operator !=(FirewallLegacyPortCollectionKey left, FirewallLegacyPortCollectionKey right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as FirewallLegacyPortCollectionKey);
	}

	public override int GetHashCode()
	{
		return (PortNumber * 397) ^ (int)ProtocolType;
	}
}
