using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace Thetis;

public sealed class NicRadioScanResult
{
	public string NicId { get; set; }

	public string NicName { get; set; }

	public string NicDescription { get; set; }

	public long NicSpeedBitsPerSecond { get; set; }

	public NetworkInterfaceType NicInterfaceType { get; set; }

	public string NicInterfaceTypeString
	{
		get
		{
			int nicInterfaceType = (int)NicInterfaceType;
			if (Enum.IsDefined(typeof(NetworkInterfaceType), nicInterfaceType))
			{
				NetworkInterfaceType networkInterfaceType = (NetworkInterfaceType)nicInterfaceType;
				return networkInterfaceType.ToString();
			}
			if (Enum.IsDefined(typeof(IfTypeSyntax), nicInterfaceType))
			{
				IfTypeSyntax ifTypeSyntax = (IfTypeSyntax)nicInterfaceType;
				return ifTypeSyntax.ToString();
			}
			return nicInterfaceType.ToString();
		}
	}

	public bool IsEthernet { get; set; }

	public bool IsWireless { get; set; }

	public IPAddress LocalIPv4 { get; set; }

	public IPAddress LocalMaskIPv4 { get; set; }

	public string NicMacAddress { get; set; }

	public bool IsApipaLocal { get; set; }

	public bool IsLoopbackLocal { get; set; }

	public List<RadioInfo> Radios { get; set; }

	public IPAddress GatewayIPv4 { get; set; }

	public List<IPAddress> DnsServersIPv4 { get; set; }

	public bool IsDhcpEnabled { get; set; }

	public OperationalStatus NicStatus { get; set; }

	public int Mtu { get; set; }

	public DiscoveryDiagnostics Diagnostics { get; set; }

	public string DisplayText
	{
		get
		{
			string text = (IsEthernet ? "Ethernet" : (IsWireless ? "WiFi" : NicInterfaceType.ToString()));
			string text2 = (IsApipaLocal ? " APIPA" : "");
			string text3 = ((LocalIPv4 != null) ? LocalIPv4.ToString() : "");
			return NicDescription + " [" + text + text2 + "] " + text3;
		}
	}

	public NicRadioScanResult()
	{
		Radios = new List<RadioInfo>();
		DnsServersIPv4 = new List<IPAddress>();
	}

	public override string ToString()
	{
		return DisplayText;
	}
}
