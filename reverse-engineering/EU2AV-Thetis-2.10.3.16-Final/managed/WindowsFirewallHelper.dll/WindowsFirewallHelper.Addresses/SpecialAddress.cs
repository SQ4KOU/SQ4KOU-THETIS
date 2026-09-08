using System;

namespace WindowsFirewallHelper.Addresses;

public abstract class SpecialAddress : IAddress, IEquatable<SpecialAddress>, IEquatable<string>
{
	protected abstract string AddressString { get; }

	public override string ToString()
	{
		return AddressString;
	}

	public bool Equals(SpecialAddress other)
	{
		if (other == null)
		{
			return false;
		}
		if ((object)this != other)
		{
			return Equals(other.AddressString);
		}
		return true;
	}

	public bool Equals(string other)
	{
		if (other == null)
		{
			return false;
		}
		return AddressString.Equals(other, StringComparison.InvariantCultureIgnoreCase);
	}

	public static bool operator ==(SpecialAddress left, SpecialAddress right)
	{
		if (!object.Equals(left, right))
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

	public static bool operator !=(SpecialAddress left, SpecialAddress right)
	{
		return !(left == right);
	}

	public static SpecialAddress Parse(string str)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		if (!TryParse(str, out var specialAddress))
		{
			throw new FormatException();
		}
		return specialAddress;
	}

	public static bool TryParse(string str, out SpecialAddress specialAddress)
	{
		if (DNSService.TryParse(str, out var service))
		{
			specialAddress = service;
			return true;
		}
		if (DHCPService.TryParse(str, out var service2))
		{
			specialAddress = service2;
			return true;
		}
		if (WINSService.TryParse(str, out var service3))
		{
			specialAddress = service3;
			return true;
		}
		if (LocalSubnet.TryParse(str, out var service4))
		{
			specialAddress = service4;
			return true;
		}
		if (DefaultGateway.TryParse(str, out var address))
		{
			specialAddress = address;
			return true;
		}
		specialAddress = null;
		return false;
	}

	protected static T Parse<T>(string str) where T : SpecialAddress, new()
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		if (!TryParse(str, out T service))
		{
			throw new FormatException();
		}
		return service;
	}

	protected static bool TryParse<T>(string str, out T service) where T : SpecialAddress, new()
	{
		service = new T();
		if (str.Trim().Equals(service.AddressString, StringComparison.InvariantCultureIgnoreCase))
		{
			return true;
		}
		service = null;
		return false;
	}

	public override bool Equals(object obj)
	{
		if (!Equals(obj as SpecialAddress))
		{
			return Equals(obj as string);
		}
		return true;
	}

	public override int GetHashCode()
	{
		return AddressString?.GetHashCode() ?? 0;
	}
}
