using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Thetis;

public sealed class RadioDiscoveryService
{
	private sealed class NicIpv4Binding
	{
		public NetworkInterface Nic;

		public IPAddress LocalIp;

		public IPAddress Mask;
	}

	private sealed class DiscoveryParseResult
	{
		public bool IsDiscovery { get; set; }

		public bool IsBusy { get; set; }

		public RadioDiscoveryRadioProtocol Protocol { get; set; }

		public string MacAddress { get; set; }

		public HPSDRHW DeviceType { get; set; }

		public byte CodeVersion { get; set; }

		public byte BetaVersion { get; set; }

		public byte ProtocolSupported { get; set; }

		public byte NumRxs { get; set; }

		public byte MercuryVersion0 { get; set; }

		public byte MercuryVersion1 { get; set; }

		public byte MercuryVersion2 { get; set; }

		public byte MercuryVersion3 { get; set; }

		public byte PennyVersion { get; set; }

		public byte MetisVersion { get; set; }

		public byte HwRev { get; set; }
	}

	private const int P1DefaultPortCount = 1;

	private const int P2DefaultPortCount = 18;

	public List<NicRadioScanResult> DiscoverUsingAllNics(RadioDiscoveryOptions options)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		List<NicRadioScanResult> list = new List<NicRadioScanResult>();
		List<NicIpv4Binding> list2 = enumerateNicIpv4Bindings(options);
		for (int i = 0; i < list2.Count; i++)
		{
			NicIpv4Binding nicIpv4Binding = list2[i];
			NicRadioScanResult nicRadioScanResult = createNicResultSkeleton(nicIpv4Binding);
			hydrateNicNetworkProps(nicIpv4Binding, nicRadioScanResult);
			sanitizeLoopbackNicFields(nicRadioScanResult);
			List<RadioInfo> list3 = discoverOnNic(nicIpv4Binding.LocalIp, nicIpv4Binding.Mask, options, out var diagnostics);
			nicRadioScanResult.Diagnostics = diagnostics;
			for (int j = 0; j < list3.Count; j++)
			{
				nicRadioScanResult.Radios.Add(list3[j]);
			}
			list.Add(nicRadioScanResult);
		}
		return list;
	}

	public NicRadioScanResult DiscoverUsingSingleNic(RadioDiscoveryOptions options, IPAddress localIPv4)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		if (localIPv4 == null)
		{
			throw new ArgumentNullException("localIPv4");
		}
		options.FixedLocalIp = localIPv4;
		List<NicIpv4Binding> list = enumerateNicIpv4Bindings(options);
		if (list.Count == 0)
		{
			return null;
		}
		NicIpv4Binding nicIpv4Binding = list[0];
		NicRadioScanResult nicRadioScanResult = createNicResultSkeleton(nicIpv4Binding);
		hydrateNicNetworkProps(nicIpv4Binding, nicRadioScanResult);
		sanitizeLoopbackNicFields(nicRadioScanResult);
		List<RadioInfo> list2 = discoverOnNic(nicIpv4Binding.LocalIp, nicIpv4Binding.Mask, options, out var diagnostics);
		nicRadioScanResult.Diagnostics = diagnostics;
		for (int i = 0; i < list2.Count; i++)
		{
			nicRadioScanResult.Radios.Add(list2[i]);
		}
		return nicRadioScanResult;
	}

	public List<NicRadioScanResult> ListUsableNics(RadioDiscoveryOptions options)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		options.FixedLocalIp = null;
		List<NicRadioScanResult> list = new List<NicRadioScanResult>();
		List<NicIpv4Binding> list2 = enumerateNicIpv4Bindings(options);
		for (int i = 0; i < list2.Count; i++)
		{
			NicIpv4Binding b = list2[i];
			NicRadioScanResult nicRadioScanResult = createNicResultSkeleton(b);
			hydrateNicNetworkProps(b, nicRadioScanResult);
			sanitizeLoopbackNicFields(nicRadioScanResult);
			nicRadioScanResult.Diagnostics = null;
			list.Add(nicRadioScanResult);
		}
		return list;
	}

	private NicRadioScanResult createNicResultSkeleton(NicIpv4Binding b)
	{
		return new NicRadioScanResult
		{
			NicId = b.Nic.Id,
			NicName = b.Nic.Name,
			NicDescription = b.Nic.Description,
			NicSpeedBitsPerSecond = b.Nic.Speed,
			LocalIPv4 = b.LocalIp,
			LocalMaskIPv4 = b.Mask,
			NicMacAddress = formatNicMac(b.Nic.GetPhysicalAddress()),
			NicInterfaceType = b.Nic.NetworkInterfaceType,
			IsEthernet = (b.Nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet),
			IsWireless = (b.Nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211),
			IsLoopbackLocal = IPAddress.IsLoopback(b.LocalIp),
			IsApipaLocal = isApipa(b.LocalIp),
			NicStatus = b.Nic.OperationalStatus
		};
	}

	private void hydrateNicNetworkProps(NicIpv4Binding b, NicRadioScanResult nicResult)
	{
		if (b == null || b.Nic == null || nicResult == null)
		{
			return;
		}
		IPInterfaceProperties iPProperties = b.Nic.GetIPProperties();
		if (iPProperties == null)
		{
			return;
		}
		if (iPProperties.GatewayAddresses != null)
		{
			for (int i = 0; i < iPProperties.GatewayAddresses.Count; i++)
			{
				IPAddress address = iPProperties.GatewayAddresses[i].Address;
				if (address != null && address.AddressFamily == AddressFamily.InterNetwork)
				{
					nicResult.GatewayIPv4 = address;
					break;
				}
			}
		}
		if (iPProperties.DnsAddresses != null)
		{
			for (int j = 0; j < iPProperties.DnsAddresses.Count; j++)
			{
				IPAddress iPAddress = iPProperties.DnsAddresses[j];
				if (iPAddress != null && iPAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					nicResult.DnsServersIPv4.Add(iPAddress);
				}
			}
		}
		IPv4InterfaceProperties iPv4Properties = iPProperties.GetIPv4Properties();
		if (iPv4Properties != null)
		{
			nicResult.IsDhcpEnabled = iPv4Properties.IsDhcpEnabled;
			nicResult.Mtu = iPv4Properties.Mtu;
		}
	}

	private void sanitizeLoopbackNicFields(NicRadioScanResult nicResult)
	{
		if (nicResult != null && (nicResult.IsLoopbackLocal || nicResult.NicInterfaceType == NetworkInterfaceType.Loopback))
		{
			nicResult.NicSpeedBitsPerSecond = 0L;
			nicResult.Mtu = 0;
			nicResult.GatewayIPv4 = null;
			if (nicResult.DnsServersIPv4 == null)
			{
				nicResult.DnsServersIPv4 = new List<IPAddress>();
			}
			else
			{
				nicResult.DnsServersIPv4.Clear();
			}
		}
	}

	private string formatNicMac(PhysicalAddress pa)
	{
		if (pa == null)
		{
			return "N/A";
		}
		byte[] addressBytes = pa.GetAddressBytes();
		if (addressBytes == null || addressBytes.Length == 0)
		{
			return "N/A";
		}
		return BitConverter.ToString(addressBytes);
	}

	private List<NicIpv4Binding> enumerateNicIpv4Bindings(RadioDiscoveryOptions options)
	{
		List<NicIpv4Binding> list = new List<NicIpv4Binding>();
		NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		foreach (NetworkInterface networkInterface in allNetworkInterfaces)
		{
			if (!isNicCandidate(networkInterface, options))
			{
				continue;
			}
			IPInterfaceProperties iPProperties = networkInterface.GetIPProperties();
			if (iPProperties == null)
			{
				continue;
			}
			UnicastIPAddressInformationCollection unicastAddresses = iPProperties.UnicastAddresses;
			if (unicastAddresses == null)
			{
				continue;
			}
			for (int j = 0; j < unicastAddresses.Count; j++)
			{
				UnicastIPAddressInformation unicastIPAddressInformation = unicastAddresses[j];
				if (unicastIPAddressInformation != null && unicastIPAddressInformation.Address != null && unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork && (options.AllowLoopback || !IPAddress.IsLoopback(unicastIPAddressInformation.Address)) && (options.AllowAPIPA || !isApipa(unicastIPAddressInformation.Address)) && (options.FixedLocalIp == null || unicastIPAddressInformation.Address.Equals(options.FixedLocalIp)))
				{
					IPAddress iPAddress = unicastIPAddressInformation.IPv4Mask;
					if (iPAddress == null)
					{
						iPAddress = IPAddress.Parse("255.255.255.0");
					}
					NicIpv4Binding nicIpv4Binding = new NicIpv4Binding();
					nicIpv4Binding.Nic = networkInterface;
					nicIpv4Binding.LocalIp = unicastIPAddressInformation.Address;
					nicIpv4Binding.Mask = iPAddress;
					list.Add(nicIpv4Binding);
				}
			}
		}
		return list;
	}

	private List<RadioInfo> discoverOnNic(IPAddress localIPv4, IPAddress localMaskIPv4, RadioDiscoveryOptions options, out DiscoveryDiagnostics diagnostics)
	{
		DiscoveryDiagnostics discoveryDiagnostics = new DiscoveryDiagnostics();
		Stopwatch stopwatch = Stopwatch.StartNew();
		List<RadioInfo> list = new List<RadioInfo>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		Socket socket = null;
		try
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.EnableBroadcast = true;
			IPEndPoint localEP = new IPEndPoint(localIPv4, options.BindLocalPort);
			socket.Bind(localEP);
			byte[] buffer = buildDiscoveryPacketP1();
			byte[] buffer2 = buildDiscoveryPacketP2();
			int num = options.AttemptsPerNic;
			if (num < 1)
			{
				num = 1;
			}
			int num2 = options.PollTimeoutMilliseconds;
			if (num2 < 10)
			{
				num2 = 10;
			}
			int num3 = options.QuietPollsBeforeResend;
			if (num3 < 1)
			{
				num3 = 1;
			}
			for (int i = 0; i < num; i++)
			{
				discoveryDiagnostics.AttemptsUsed++;
				List<IPEndPoint> list2 = buildTargets(localIPv4, localMaskIPv4, options);
				for (int j = 0; j < list2.Count; j++)
				{
					IPEndPoint remoteEP = list2[j];
					if (options.ProtocolMode == RadioDiscoveryProtocolMode.Auto || options.ProtocolMode == RadioDiscoveryProtocolMode.P1Only)
					{
						socket.SendTo(buffer, remoteEP);
						discoveryDiagnostics.DiscoverySends++;
					}
					if (options.ProtocolMode == RadioDiscoveryProtocolMode.Auto || options.ProtocolMode == RadioDiscoveryProtocolMode.P2Only)
					{
						socket.SendTo(buffer2, remoteEP);
						discoveryDiagnostics.DiscoverySends++;
					}
				}
				int num4 = 0;
				while (num4 < num3)
				{
					discoveryDiagnostics.Polls++;
					if (!socket.Poll(num2 * 1000, SelectMode.SelectRead))
					{
						num4++;
						discoveryDiagnostics.QuietPolls++;
						continue;
					}
					EndPoint remoteEP2 = new IPEndPoint(IPAddress.Any, 0);
					byte[] array = new byte[256];
					int num5 = socket.ReceiveFrom(array, ref remoteEP2);
					if (num5 <= 0)
					{
						continue;
					}
					discoveryDiagnostics.DiscoveryReceives++;
					if (!(remoteEP2 is IPEndPoint { Address: not null } iPEndPoint))
					{
						continue;
					}
					DiscoveryParseResult discoveryParseResult = parseDiscoveryReply(array, num5, iPEndPoint.Address, options);
					if (!discoveryParseResult.IsDiscovery && !discoveryParseResult.IsBusy)
					{
						continue;
					}
					if (options.ProtocolMode == RadioDiscoveryProtocolMode.P1Only && discoveryParseResult.Protocol != RadioDiscoveryRadioProtocol.P1)
					{
						discoveryDiagnostics.RejectedProtocolModeMismatch++;
						continue;
					}
					if (options.ProtocolMode == RadioDiscoveryProtocolMode.P2Only && discoveryParseResult.Protocol != RadioDiscoveryRadioProtocol.P2)
					{
						discoveryDiagnostics.RejectedProtocolModeMismatch++;
						continue;
					}
					if (!options.IgnoreSubnetCheck && !sameSubnet(iPEndPoint.Address, localIPv4, localMaskIPv4))
					{
						discoveryDiagnostics.RejectedSubnet++;
						continue;
					}
					if (discoveryParseResult.MacAddress == null || discoveryParseResult.MacAddress.Length == 0 || discoveryParseResult.MacAddress.Equals("00-00-00-00-00-00", StringComparison.OrdinalIgnoreCase))
					{
						discoveryDiagnostics.RejectedMacInvalid++;
						continue;
					}
					if (options.FixedTargetIp != null && !iPEndPoint.Address.Equals(options.FixedTargetIp))
					{
						discoveryDiagnostics.RejectedFixedTargetMismatch++;
						continue;
					}
					string item = discoveryParseResult.Protocol.ToString() + "|" + iPEndPoint.Address.ToString() + "|" + discoveryParseResult.MacAddress;
					if (hashSet.Contains(item))
					{
						discoveryDiagnostics.RejectedDuplicate++;
						continue;
					}
					RadioInfo radioInfo = new RadioInfo();
					radioInfo.Protocol = discoveryParseResult.Protocol;
					radioInfo.IpAddress = iPEndPoint.Address;
					radioInfo.MacAddress = discoveryParseResult.MacAddress;
					radioInfo.DeviceType = discoveryParseResult.DeviceType;
					radioInfo.CodeVersion = discoveryParseResult.CodeVersion;
					radioInfo.BetaVersion = discoveryParseResult.BetaVersion;
					radioInfo.Protocol2Supported = discoveryParseResult.ProtocolSupported;
					radioInfo.NumRxs = discoveryParseResult.NumRxs;
					radioInfo.MercuryVersion0 = discoveryParseResult.MercuryVersion0;
					radioInfo.MercuryVersion1 = discoveryParseResult.MercuryVersion1;
					radioInfo.MercuryVersion2 = discoveryParseResult.MercuryVersion2;
					radioInfo.MercuryVersion3 = discoveryParseResult.MercuryVersion3;
					radioInfo.PennyVersion = discoveryParseResult.PennyVersion;
					radioInfo.MetisVersion = discoveryParseResult.MetisVersion;
					radioInfo.HwRev = discoveryParseResult.HwRev;
					radioInfo.IsBusy = discoveryParseResult.IsBusy;
					radioInfo.DiscoveryPortBase = options.DiscoveryPortBase;
					radioInfo.PortCount = ((discoveryParseResult.Protocol != RadioDiscoveryRadioProtocol.P2) ? 1 : 18);
					radioInfo.IsApipaRadio = isApipa(iPEndPoint.Address);
					radioInfo.IsCustom = false;
					radioInfo.CustomGuid = "";
					list.Add(radioInfo);
					hashSet.Add(item);
				}
			}
		}
		catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
		{
			discoveryDiagnostics.SocketError = true;
			diagnostics = discoveryDiagnostics;
			return list;
		}
		catch
		{
			diagnostics = discoveryDiagnostics;
			return list;
		}
		finally
		{
			stopwatch.Stop();
			discoveryDiagnostics.DurationMilliseconds = stopwatch.ElapsedMilliseconds;
			discoveryDiagnostics.UniqueRadios = list.Count;
			if (socket != null)
			{
				try
				{
					socket.Close();
				}
				catch
				{
				}
			}
		}
		diagnostics = discoveryDiagnostics;
		return list;
	}

	private DiscoveryParseResult parseDiscoveryReply(byte[] data, int len, IPAddress senderIp, RadioDiscoveryOptions options)
	{
		DiscoveryParseResult discoveryParseResult = new DiscoveryParseResult();
		discoveryParseResult.IsDiscovery = false;
		discoveryParseResult.IsBusy = false;
		discoveryParseResult.Protocol = RadioDiscoveryRadioProtocol.Unknown;
		if (data == null || len < 24)
		{
			return discoveryParseResult;
		}
		bool flag = data[0] == 239 && data[1] == 254 && (data[2] == 2 || data[2] == 3);
		bool flag2 = data[0] == 0 && data[1] == 0 && data[2] == 0 && data[3] == 0 && (data[4] == 2 || data[4] == 3);
		if (!flag && !flag2)
		{
			return discoveryParseResult;
		}
		discoveryParseResult.IsDiscovery = true;
		if (flag)
		{
			discoveryParseResult.Protocol = RadioDiscoveryRadioProtocol.P1;
			discoveryParseResult.IsBusy = data[2] == 3;
			byte[] array = new byte[6];
			Array.Copy(data, 3, array, 0, 6);
			discoveryParseResult.MacAddress = BitConverter.ToString(array);
			discoveryParseResult.DeviceType = mapP1DeviceType(data[10]);
			discoveryParseResult.ProtocolSupported = 0;
			discoveryParseResult.CodeVersion = data[9];
			discoveryParseResult.BetaVersion = 0;
			if (len > 20)
			{
				discoveryParseResult.MercuryVersion0 = data[14];
				discoveryParseResult.MercuryVersion1 = data[15];
				discoveryParseResult.MercuryVersion2 = data[16];
				discoveryParseResult.MercuryVersion3 = data[17];
				discoveryParseResult.PennyVersion = data[18];
				discoveryParseResult.MetisVersion = data[19];
				discoveryParseResult.NumRxs = data[20];
			}
			return discoveryParseResult;
		}
		if (flag2)
		{
			discoveryParseResult.Protocol = RadioDiscoveryRadioProtocol.P2;
			discoveryParseResult.IsBusy = data[4] == 3;
			byte[] array2 = new byte[6];
			Array.Copy(data, 5, array2, 0, 6);
			discoveryParseResult.MacAddress = BitConverter.ToString(array2);
			discoveryParseResult.DeviceType = (HPSDRHW)data[11];
			discoveryParseResult.ProtocolSupported = data[12];
			discoveryParseResult.CodeVersion = data[13];
			discoveryParseResult.BetaVersion = (byte)((len > 23) ? data[23] : 0);
			discoveryParseResult.HwRev = (byte)((len > 22) ? data[22] : 0);
			if (len > 20)
			{
				discoveryParseResult.MercuryVersion0 = data[14];
				discoveryParseResult.MercuryVersion1 = data[15];
				discoveryParseResult.MercuryVersion2 = data[16];
				discoveryParseResult.MercuryVersion3 = data[17];
				discoveryParseResult.PennyVersion = data[18];
				discoveryParseResult.MetisVersion = data[19];
				discoveryParseResult.NumRxs = data[20];
			}
			return discoveryParseResult;
		}
		return discoveryParseResult;
	}

	private HPSDRHW mapP1DeviceType(byte boardId)
	{
		return boardId switch
		{
			0 => HPSDRHW.Atlas, 
			1 => HPSDRHW.Hermes, 
			2 => HPSDRHW.HermesII, 
			4 => HPSDRHW.Angelia, 
			5 => HPSDRHW.Orion, 
			10 => HPSDRHW.OrionMKII, 
			_ => (HPSDRHW)boardId, 
		};
	}

	private List<IPEndPoint> buildTargets(IPAddress localIPv4, IPAddress mask, RadioDiscoveryOptions options)
	{
		List<IPEndPoint> list = new List<IPEndPoint>();
		int num = options.DiscoveryPortBase;
		if (num < 1)
		{
			num = 1024;
		}
		if (options.FixedTargetIp != null)
		{
			list.Add(new IPEndPoint(options.FixedTargetIp, num));
			if (options.ProtocolMode == RadioDiscoveryProtocolMode.P2Only)
			{
				return list;
			}
		}
		IPAddress broadcastAddress = getBroadcastAddress(localIPv4, mask);
		list.Add(new IPEndPoint(broadcastAddress, num));
		if (options.IncludeGeneralBroadcast)
		{
			IPAddress broadcast = IPAddress.Broadcast;
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Address != null && list[i].Address.Equals(broadcast))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(new IPEndPoint(broadcast, num));
			}
		}
		return list;
	}

	private byte[] buildDiscoveryPacketP1()
	{
		byte[] array = new byte[63];
		Array.Clear(array, 0, array.Length);
		array[0] = 239;
		array[1] = 254;
		array[2] = 2;
		return array;
	}

	private byte[] buildDiscoveryPacketP2()
	{
		byte[] array = new byte[60];
		Array.Clear(array, 0, array.Length);
		array[4] = 2;
		return array;
	}

	private bool isNicCandidate(NetworkInterface nic, RadioDiscoveryOptions options)
	{
		if (nic == null)
		{
			return false;
		}
		if (nic.OperationalStatus != OperationalStatus.Up && nic.OperationalStatus != OperationalStatus.Unknown)
		{
			return false;
		}
		switch (nic.NetworkInterfaceType)
		{
		case NetworkInterfaceType.Loopback:
			return options.AllowLoopback;
		case NetworkInterfaceType.Wireless80211:
			return options.IncludeWireless;
		case NetworkInterfaceType.Ethernet:
			return options.IncludeEthernet;
		default:
			if (options.IncludeOtherInterfaceTypes)
			{
				return true;
			}
			return false;
		}
	}

	private bool sameSubnet(IPAddress a, IPAddress b, IPAddress mask)
	{
		if (a == null || b == null || mask == null)
		{
			return false;
		}
		byte[] addressBytes = a.GetAddressBytes();
		byte[] addressBytes2 = b.GetAddressBytes();
		byte[] addressBytes3 = mask.GetAddressBytes();
		if (addressBytes.Length != 4 || addressBytes2.Length != 4 || addressBytes3.Length != 4)
		{
			return false;
		}
		for (int i = 0; i < 4; i++)
		{
			int num = addressBytes[i] & addressBytes3[i];
			int num2 = addressBytes2[i] & addressBytes3[i];
			if (num != num2)
			{
				return false;
			}
		}
		return true;
	}

	private bool isApipa(IPAddress ip)
	{
		if (ip == null)
		{
			return false;
		}
		byte[] addressBytes = ip.GetAddressBytes();
		if (addressBytes.Length != 4)
		{
			return false;
		}
		if (addressBytes[0] == 169)
		{
			return addressBytes[1] == 254;
		}
		return false;
	}

	private IPAddress getBroadcastAddress(IPAddress address, IPAddress subnetMask)
	{
		if (address == null)
		{
			return IPAddress.Broadcast;
		}
		if (subnetMask == null)
		{
			return IPAddress.Broadcast;
		}
		byte[] addressBytes = address.GetAddressBytes();
		byte[] addressBytes2 = subnetMask.GetAddressBytes();
		if (addressBytes.Length != 4 || addressBytes2.Length != 4)
		{
			return IPAddress.Broadcast;
		}
		byte[] array = new byte[4];
		for (int i = 0; i < 4; i++)
		{
			array[i] = (byte)(addressBytes[i] | (addressBytes2[i] ^ 0xFF));
		}
		return new IPAddress(array);
	}
}
