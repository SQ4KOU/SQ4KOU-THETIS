using System;
using System.Net;
using WindowsFirewallHelper.InternalHelpers;

namespace WindowsFirewallHelper.Addresses;

public class IPRange : IAddress, IEquatable<IPRange>
{
	private int? _hashCode;

	public IPAddress EndAddress { get; set; }

	public IPAddress StartAddress { get; set; }

	public IPRange(IPAddress address1, IPAddress address2)
	{
		if (address1.AddressFamily != address2.AddressFamily)
		{
			throw new ArgumentException("Addresses of different family can not be used.");
		}
		if ((address1.Equals(IPAddress.Any) || address1.Equals(IPAddress.IPv6Any) || address2.Equals(IPAddress.Any) || address2.Equals(IPAddress.IPv6Any)) && !address1.Equals(address2))
		{
			throw new ArgumentException("Address ranges starting or ending in `Any` are not supported.");
		}
		StartAddress = AddressHelper.Min(address1, address2);
		EndAddress = AddressHelper.Max(address1, address2);
	}

	public IPRange(IPAddress address)
		: this(address, address)
	{
	}

	public override string ToString()
	{
		if (StartAddress.Equals(EndAddress))
		{
			if (StartAddress.Equals(IPAddress.Any) || StartAddress.Equals(IPAddress.IPv6Any))
			{
				return "*";
			}
			return StartAddress.ToString();
		}
		return $"{StartAddress}-{EndAddress}";
	}

	public bool Equals(IPRange other)
	{
		if (other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (StartAddress.Equals(other.StartAddress))
		{
			return EndAddress.Equals(other.EndAddress);
		}
		return false;
	}

	public static bool operator ==(IPRange left, IPRange right)
	{
		if (!object.Equals(left, right))
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

	public static bool operator !=(IPRange left, IPRange right)
	{
		return !(left == right);
	}

	public static IPRange Parse(string str)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		if (!TryParse(str, out var addressRange))
		{
			throw new FormatException();
		}
		return addressRange;
	}

	public static bool TryParse(string str, out IPRange addressRange)
	{
		try
		{
			if (str == "*")
			{
				addressRange = new IPRange(IPAddress.Any);
				return true;
			}
			string[] array = str.Split('-');
			IPAddress address2;
			IPAddress address3;
			if (array.Length == 1)
			{
				if (IPAddress.TryParse(array[0], out var address))
				{
					addressRange = new IPRange(address);
					return true;
				}
			}
			else if (array.Length == 2 && IPAddress.TryParse(array[0], out address2) && IPAddress.TryParse(array[1], out address3))
			{
				addressRange = new IPRange(address2, address3);
				return true;
			}
		}
		catch (Exception)
		{
		}
		addressRange = null;
		return false;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as IPRange);
	}

	public override int GetHashCode()
	{
		if (!_hashCode.HasValue)
		{
			_hashCode = ((StartAddress?.GetHashCode() ?? 0) * 397) ^ (EndAddress?.GetHashCode() ?? 0);
		}
		return _hashCode.Value;
	}
}
