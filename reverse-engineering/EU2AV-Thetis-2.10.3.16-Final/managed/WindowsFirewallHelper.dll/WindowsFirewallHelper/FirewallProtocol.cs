using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WindowsFirewallHelper;

public class FirewallProtocol : IEquatable<FirewallProtocol>, IEquatable<int>
{
	public static readonly FirewallProtocol Any = new FirewallProtocol(256);

	public static readonly FirewallProtocol GRE = new FirewallProtocol(47);

	public static readonly FirewallProtocol HOPOPT = new FirewallProtocol(0);

	public static readonly FirewallProtocol ICMPv4 = new FirewallProtocol(1);

	public static readonly FirewallProtocol ICMPv6 = new FirewallProtocol(58);

	public static readonly FirewallProtocol IGMP = new FirewallProtocol(2);

	public static readonly FirewallProtocol IPv6 = new FirewallProtocol(41);

	public static readonly FirewallProtocol IPv6Frag = new FirewallProtocol(44);

	public static readonly FirewallProtocol IPv6NoNxt = new FirewallProtocol(59);

	public static readonly FirewallProtocol IPv6Opts = new FirewallProtocol(60);

	public static readonly FirewallProtocol IPv6Route = new FirewallProtocol(43);

	public static readonly FirewallProtocol L2TP = new FirewallProtocol(115);

	public static readonly FirewallProtocol PGM = new FirewallProtocol(113);

	public static readonly FirewallProtocol TCP = new FirewallProtocol(6);

	public static readonly FirewallProtocol UDP = new FirewallProtocol(17);

	public static readonly FirewallProtocol VRRP = new FirewallProtocol(112);

	public int ProtocolNumber { get; }

	public FirewallProtocol(byte protocolNumber)
	{
		ProtocolNumber = protocolNumber;
	}

	internal FirewallProtocol(int protocolNumber)
	{
		ProtocolNumber = protocolNumber;
	}

	public bool Equals(FirewallProtocol other)
	{
		if (other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		return Equals(other.ProtocolNumber);
	}

	public bool Equals(int other)
	{
		return ProtocolNumber == other;
	}

	public static bool operator ==(FirewallProtocol left, FirewallProtocol right)
	{
		if (!object.Equals(left, right))
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

	public static bool operator ==(FirewallProtocol left, int right)
	{
		return left?.Equals(right) ?? false;
	}

	public static bool operator ==(int left, FirewallProtocol right)
	{
		return right?.Equals(left) ?? false;
	}

	public static bool operator !=(FirewallProtocol left, FirewallProtocol right)
	{
		return !(left == right);
	}

	public static bool operator !=(FirewallProtocol left, int right)
	{
		return !(left == right);
	}

	public static bool operator !=(int left, FirewallProtocol right)
	{
		return !(left == right);
	}

	public static bool TryParse(string str, out FirewallProtocol firewallProtocol)
	{
		if (int.TryParse(str, out var result) && result >= 0 && result <= 256)
		{
			firewallProtocol = new FirewallProtocol(result);
			return true;
		}
		firewallProtocol = null;
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is int obj2)
		{
			return ProtocolNumber.Equals(obj2);
		}
		return Equals(obj as FirewallProtocol);
	}

	public override int GetHashCode()
	{
		return ProtocolNumber.GetHashCode();
	}

	public override string ToString()
	{
		try
		{
			Dictionary<int, string> dictionary = (from info in typeof(FirewallProtocol).GetFields(BindingFlags.Static | BindingFlags.Public)
				where info.FieldType == typeof(FirewallProtocol)
				select info).ToDictionary((FieldInfo info) => ((FirewallProtocol)info.GetValue(null)).ProtocolNumber, (FieldInfo info) => info.Name);
			if (dictionary.ContainsKey(ProtocolNumber))
			{
				return dictionary[ProtocolNumber];
			}
		}
		catch
		{
		}
		return $"Protocol #{ProtocolNumber}";
	}
}
