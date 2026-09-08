using System;
using System.Net;
using System.Net.Sockets;

namespace WindowsFirewallHelper.Addresses;

public class NetworkAddress : IAddress, IEquatable<NetworkAddress>
{
	public static readonly IPAddress IPv4SingleHostSubnet = IPAddress.Parse("255.255.255.255");

	public static readonly IPAddress IPv6SingleHostSubnet = IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff");

	private int? _hashCode;

	public IPAddress Address { get; set; }

	public IPAddress EndAddress
	{
		get
		{
			if (SubnetMask.Equals(IPv4SingleHostSubnet) || SubnetMask.Equals(IPv6SingleHostSubnet))
			{
				return Address;
			}
			byte[] addressBytes = Address.GetAddressBytes();
			byte[] addressBytes2 = SubnetMask.GetAddressBytes();
			for (int i = 0; i < addressBytes.Length; i++)
			{
				addressBytes[i] |= (byte)(~addressBytes2[i]);
			}
			return new IPAddress(addressBytes);
		}
	}

	public IPAddress StartAddress
	{
		get
		{
			if (SubnetMask.Equals(IPv4SingleHostSubnet) || SubnetMask.Equals(IPv6SingleHostSubnet))
			{
				return Address;
			}
			byte[] addressBytes = Address.GetAddressBytes();
			byte[] addressBytes2 = SubnetMask.GetAddressBytes();
			for (int i = 0; i < addressBytes.Length; i++)
			{
				addressBytes[i] &= addressBytes2[i];
			}
			return new IPAddress(addressBytes);
		}
	}

	public IPAddress SubnetMask { get; set; }

	public NetworkAddress(IPAddress address)
	{
		switch (address.AddressFamily)
		{
		case AddressFamily.InterNetwork:
			SubnetMask = IPv4SingleHostSubnet;
			break;
		case AddressFamily.InterNetworkV6:
			SubnetMask = IPv6SingleHostSubnet;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		Address = address;
	}

	public NetworkAddress(IPAddress address, IPAddress subnetMask)
	{
		if (address.AddressFamily != subnetMask.AddressFamily)
		{
			throw new ArgumentException("Addresses of different family can not be used.");
		}
		switch (address.AddressFamily)
		{
		case AddressFamily.InterNetwork:
			if (object.Equals(address, IPAddress.Any) && !object.Equals(SubnetMask, IPv4SingleHostSubnet))
			{
				throw new ArgumentException("Any IPAddresses are only allowed with single host sub-nets.");
			}
			break;
		case AddressFamily.InterNetworkV6:
			if (object.Equals(address, IPAddress.IPv6Any) && !object.Equals(SubnetMask, IPv6SingleHostSubnet))
			{
				throw new ArgumentException("Any IPAddresses are only allowed with single host sub-nets.");
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		Address = address;
		SubnetMask = subnetMask;
	}

	public override string ToString()
	{
		if (StartAddress.Equals(EndAddress))
		{
			if (object.Equals(Address, IPAddress.Any) || object.Equals(Address, IPAddress.IPv6Any))
			{
				return "*";
			}
			return Address.ToString();
		}
		return $"{Address}/{SubnetMask}";
	}

	public bool Equals(NetworkAddress other)
	{
		if (other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (Address.Equals(other.Address))
		{
			return SubnetMask.Equals(other.SubnetMask);
		}
		return false;
	}

	public static bool operator ==(NetworkAddress left, NetworkAddress right)
	{
		if (!object.Equals(left, right))
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

	public static bool operator !=(NetworkAddress left, NetworkAddress right)
	{
		return !(left == right);
	}

	public static NetworkAddress Parse(string str)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		if (!TryParse(str, out var addressNetwork))
		{
			throw new FormatException();
		}
		return addressNetwork;
	}

	public static bool TryParse(string str, out NetworkAddress addressNetwork)
	{
		try
		{
			if (str == "*")
			{
				addressNetwork = new NetworkAddress(IPAddress.Any);
				return true;
			}
			string[] array = str.Split('/');
			IPAddress address2;
			if (array.Length == 1)
			{
				if (IPAddress.TryParse(array[0], out var address))
				{
					addressNetwork = new NetworkAddress(address);
					return true;
				}
			}
			else if (array.Length == 2 && IPAddress.TryParse(array[0], out address2))
			{
				if (int.TryParse(array[1], out var result) && result >= 1 && ((address2.AddressFamily == AddressFamily.InterNetwork && result <= 32) || (address2.AddressFamily == AddressFamily.InterNetworkV6 && result <= 128)))
				{
					byte[] array2 = new byte[(address2.AddressFamily == AddressFamily.InterNetworkV6) ? 16 : 4];
					for (byte b = 0; b < result; b++)
					{
						array2[(int)Math.Floor((double)(int)b / 8.0)] |= (byte)(1 << 7 - b % 8);
					}
					addressNetwork = new NetworkAddress(address2, new IPAddress(array2));
					return true;
				}
				if ((array[1].Contains(":") || array[1].Contains(".")) && IPAddress.TryParse(array[1], out var address3))
				{
					addressNetwork = new NetworkAddress(address2, address3);
					return true;
				}
			}
		}
		catch (Exception)
		{
		}
		addressNetwork = null;
		return false;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as NetworkAddress);
	}

	public override int GetHashCode()
	{
		if (!_hashCode.HasValue)
		{
			_hashCode = ((Address?.GetHashCode() ?? 0) * 397) ^ (SubnetMask?.GetHashCode() ?? 0);
		}
		return _hashCode.Value;
	}
}
