using System;
using System.ComponentModel;
using System.Net;

namespace WindowsFirewallHelper.Addresses;

public class SingleIP : IPAddress, IAddress, IEquatable<SingleIP>, IEquatable<IPAddress>
{
	public new static readonly SingleIP Any = new SingleIP(IPAddress.Any);

	public new static readonly SingleIP Broadcast = new SingleIP(IPAddress.Broadcast);

	public new static readonly SingleIP IPv6Any = new SingleIP(IPAddress.IPv6Any);

	public new static readonly SingleIP IPv6Loopback = new SingleIP(IPAddress.IPv6Loopback);

	[Obsolete("Unrelated", true)]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new static readonly SingleIP IPv6None = new SingleIP(IPAddress.IPv6None);

	public new static readonly SingleIP Loopback = new SingleIP(IPAddress.Loopback);

	[Obsolete("Unrelated", true)]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new static readonly SingleIP None = new SingleIP(IPAddress.None);

	public SingleIP(long newAddress)
		: base(newAddress)
	{
	}

	public SingleIP(byte[] address)
		: base(address)
	{
	}

	public SingleIP(IPAddress ip)
		: this(ip.GetAddressBytes())
	{
	}

	public override string ToString()
	{
		if (Equals(Any) || Equals(IPv6Any))
		{
			return "*";
		}
		return base.ToString();
	}

	public bool Equals(IPAddress other)
	{
		if (other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		return base.Equals((object)other);
	}

	public bool Equals(SingleIP other)
	{
		if (other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		return Equals(other.ToIPAddress());
	}

	public static bool IsLoopback(SingleIP address)
	{
		return IPAddress.IsLoopback(address.ToIPAddress());
	}

	public static bool operator ==(SingleIP left, SingleIP right)
	{
		if (!object.Equals(left, right))
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

	public static bool operator ==(SingleIP left, IPAddress right)
	{
		if (!object.Equals(left, right))
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

	public static bool operator ==(IPAddress left, SingleIP right)
	{
		if (!object.Equals(left, right))
		{
			return right?.Equals(left) ?? false;
		}
		return true;
	}

	public static bool operator !=(SingleIP left, SingleIP right)
	{
		return !(left == right);
	}

	public static bool operator !=(SingleIP left, IPAddress right)
	{
		return !(left == right);
	}

	public static bool operator !=(IPAddress left, SingleIP right)
	{
		return !(left == right);
	}

	public new static SingleIP Parse(string ipString)
	{
		if (ipString == null)
		{
			throw new ArgumentNullException("ipString");
		}
		if (!TryParse(ipString, out SingleIP address))
		{
			throw new FormatException();
		}
		return address;
	}

	public new static bool TryParse(string ipString, out IPAddress address)
	{
		throw new NotSupportedException();
	}

	public static bool TryParse(string ipString, out SingleIP address)
	{
		if (ipString.Trim() == "*")
		{
			address = Any;
			return true;
		}
		if (IPAddress.TryParse(ipString, out var address2))
		{
			address = new SingleIP(address2);
			return true;
		}
		if (IPRange.TryParse(ipString, out var addressRange) && addressRange.StartAddress.Equals(addressRange.EndAddress))
		{
			address = new SingleIP(addressRange.StartAddress);
			return true;
		}
		if (NetworkAddress.TryParse(ipString, out var addressNetwork) && addressNetwork.StartAddress.Equals(addressNetwork.EndAddress))
		{
			address = new SingleIP(addressNetwork.Address);
			return true;
		}
		address = null;
		return false;
	}

	public override bool Equals(object other)
	{
		if (!Equals(other as SingleIP))
		{
			return Equals(other as IPAddress);
		}
		return true;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public IPAddress ToIPAddress()
	{
		return this;
	}
}
