using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace WindowsFirewallHelper.InternalHelpers;

internal static class NetworkInterfaceHelper
{
	public static string[] InterfacesToString(NetworkInterface[] interfaces)
	{
		if (interfaces.Length == 0)
		{
			return null;
		}
		List<string> list = new List<string>();
		foreach (NetworkInterface networkInterface in interfaces)
		{
			list.Add(networkInterface.Name);
		}
		return list.ToArray();
	}

	public static string InterfaceTypesToString(NetworkInterfaceTypes types)
	{
		List<string> list = new List<string>();
		if (types.HasFlag(NetworkInterfaceTypes.Lan) && types.HasFlag(NetworkInterfaceTypes.Wireless) && types.HasFlag(NetworkInterfaceTypes.RemoteAccess))
		{
			return "All";
		}
		if (types.HasFlag(NetworkInterfaceTypes.Lan))
		{
			list.Add("Lan");
		}
		if (types.HasFlag(NetworkInterfaceTypes.Wireless))
		{
			list.Add("Wireless");
		}
		if (types.HasFlag(NetworkInterfaceTypes.RemoteAccess))
		{
			list.Add("RemoteAccess");
		}
		return string.Join(",", list.ToArray());
	}

	public static NetworkInterface[] StringToInterfaces(string[] str)
	{
		if (str == null)
		{
			return new NetworkInterface[0];
		}
		List<NetworkInterface> list = new List<NetworkInterface>();
		NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		foreach (string text in str)
		{
			NetworkInterface[] array = allNetworkInterfaces;
			foreach (NetworkInterface networkInterface in array)
			{
				if (string.Equals(networkInterface.Name.Trim(), text.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					list.Add(networkInterface);
				}
			}
		}
		return list.ToArray();
	}

	public static NetworkInterfaceTypes StringToInterfaceTypes(string str)
	{
		if (string.IsNullOrEmpty(str?.Trim()))
		{
			return NetworkInterfaceTypes.RemoteAccess | NetworkInterfaceTypes.Wireless | NetworkInterfaceTypes.Lan;
		}
		NetworkInterfaceTypes networkInterfaceTypes = (NetworkInterfaceTypes)0;
		string[] array = str.Split(',');
		foreach (string text in array)
		{
			if (string.Equals(text.Trim(), "All", StringComparison.OrdinalIgnoreCase))
			{
				return NetworkInterfaceTypes.RemoteAccess | NetworkInterfaceTypes.Wireless | NetworkInterfaceTypes.Lan;
			}
			if (string.Equals(text.Trim(), "RemoteAccess", StringComparison.OrdinalIgnoreCase))
			{
				networkInterfaceTypes |= NetworkInterfaceTypes.RemoteAccess;
			}
			else if (string.Equals(text.Trim(), "Wireless", StringComparison.OrdinalIgnoreCase))
			{
				networkInterfaceTypes |= NetworkInterfaceTypes.Wireless;
			}
			else if (string.Equals(text.Trim(), "Lan", StringComparison.OrdinalIgnoreCase))
			{
				networkInterfaceTypes |= NetworkInterfaceTypes.Lan;
			}
		}
		if (networkInterfaceTypes == (NetworkInterfaceTypes)0)
		{
			return NetworkInterfaceTypes.RemoteAccess | NetworkInterfaceTypes.Wireless | NetworkInterfaceTypes.Lan;
		}
		return networkInterfaceTypes;
	}
}
