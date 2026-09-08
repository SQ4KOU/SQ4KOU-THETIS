using System;
using System.Collections.Generic;
using System.Net;
using WindowsFirewallHelper.Addresses;

namespace WindowsFirewallHelper.InternalHelpers;

internal static class AddressHelper
{
	public static string AddressesToString(IAddress[] rules)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < rules.Length; i++)
		{
			string text = rules[i].ToString();
			if (text == "*")
			{
				return "*";
			}
			list.Add(text);
		}
		return string.Join(",", list.ToArray());
	}

	public static IPAddress Max(IPAddress val1, IPAddress val2)
	{
		if (val1.AddressFamily != val2.AddressFamily)
		{
			throw new ArgumentException("Addresses of different family can not be compared.");
		}
		byte[] addressBytes = val1.GetAddressBytes();
		byte[] addressBytes2 = val2.GetAddressBytes();
		for (int i = 0; i < addressBytes.Length; i++)
		{
			if (addressBytes[i] > addressBytes2[i])
			{
				return val1;
			}
			if (addressBytes2[i] > addressBytes[i])
			{
				return val2;
			}
		}
		return val1;
	}

	public static IPAddress Min(IPAddress val1, IPAddress val2)
	{
		if (val1.AddressFamily != val2.AddressFamily)
		{
			throw new ArgumentException("Addresses of different family can not be compared.");
		}
		byte[] addressBytes = val1.GetAddressBytes();
		byte[] addressBytes2 = val2.GetAddressBytes();
		for (int i = 0; i < addressBytes.Length; i++)
		{
			if (addressBytes[i] < addressBytes2[i])
			{
				return val1;
			}
			if (addressBytes2[i] < addressBytes[i])
			{
				return val2;
			}
		}
		return val1;
	}

	public static IAddress[] StringToAddresses(string str)
	{
		List<IAddress> list = new List<IAddress>();
		string[] array = str.Split(',');
		foreach (string text in array)
		{
			SingleIP address;
			IPRange addressRange;
			NetworkAddress addressNetwork;
			if (SpecialAddress.TryParse(text, out var specialAddress))
			{
				list.Add(specialAddress);
			}
			else if (SingleIP.TryParse(text, out address))
			{
				list.Add(address);
			}
			else if (IPRange.TryParse(text, out addressRange))
			{
				list.Add(addressRange);
			}
			else if (NetworkAddress.TryParse(text, out addressNetwork))
			{
				list.Add(addressNetwork);
			}
		}
		return list.ToArray();
	}
}
