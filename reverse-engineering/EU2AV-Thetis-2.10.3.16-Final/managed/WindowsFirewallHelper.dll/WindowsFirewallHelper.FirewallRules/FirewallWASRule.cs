using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using WindowsFirewallHelper.Addresses;
using WindowsFirewallHelper.COMInterop;
using WindowsFirewallHelper.Exceptions;
using WindowsFirewallHelper.InternalHelpers;

namespace WindowsFirewallHelper.FirewallRules;

public class FirewallWASRule : IFirewallRule, IEquatable<IFirewallRule>, IEquatable<FirewallWASRule>
{
	public string Description
	{
		get
		{
			return UnderlyingObject.Description;
		}
		set
		{
			UnderlyingObject.Description = value;
		}
	}

	public bool EdgeTraversal
	{
		get
		{
			return UnderlyingObject.EdgeTraversal;
		}
		set
		{
			UnderlyingObject.EdgeTraversal = value;
		}
	}

	public string FriendlyDescription => NativeHelper.ResolveStringResource(Description);

	public string FriendlyGrouping => NativeHelper.ResolveStringResource(Grouping);

	public string Grouping
	{
		get
		{
			return UnderlyingObject.Grouping;
		}
		set
		{
			UnderlyingObject.Grouping = value;
		}
	}

	public FirewallWASInternetControlMessage[] ICMPTypesAndCodes
	{
		get
		{
			return ICMPHelper.StringToICM(UnderlyingObject.IcmpTypesAndCodes);
		}
		set
		{
			if (value.Length != 0 && !Protocol.Equals(FirewallProtocol.ICMPv4) && !Protocol.Equals(FirewallProtocol.ICMPv6))
			{
				throw new FirewallWASInvalidProtocolException("ICMPTypesAndCodes property can only be specified for the ICMP protocols.");
			}
			UnderlyingObject.IcmpTypesAndCodes = ICMPHelper.ICMToString(value);
		}
	}

	public NetworkInterface[] Interfaces
	{
		get
		{
			if (!(UnderlyingObject.Interfaces is IEnumerable))
			{
				return new NetworkInterface[0];
			}
			return NetworkInterfaceHelper.StringToInterfaces(((IEnumerable)UnderlyingObject.Interfaces).Cast<object>().Select((object o, int i) => o?.ToString()).ToArray());
		}
		set
		{
			UnderlyingObject.Interfaces = NetworkInterfaceHelper.InterfacesToString(value);
		}
	}

	public static bool IsLocallySupported => IsSupported(new COMTypeResolver());

	public NetworkInterfaceTypes NetworkInterfaceTypes
	{
		get
		{
			return NetworkInterfaceHelper.StringToInterfaceTypes(UnderlyingObject.InterfaceTypes);
		}
		set
		{
			UnderlyingObject.InterfaceTypes = NetworkInterfaceHelper.InterfaceTypesToString(value);
		}
	}

	protected INetFwRule UnderlyingObject { get; }

	public FirewallAction Action
	{
		get
		{
			if (UnderlyingObject.Action != NetFwAction.Allow)
			{
				return FirewallAction.Block;
			}
			return FirewallAction.Allow;
		}
		set
		{
			UnderlyingObject.Action = ((value == FirewallAction.Allow) ? NetFwAction.Allow : NetFwAction.Block);
		}
	}

	public string ApplicationName
	{
		get
		{
			return UnderlyingObject.ApplicationName;
		}
		set
		{
			UnderlyingObject.ApplicationName = value;
		}
	}

	public FirewallDirection Direction
	{
		get
		{
			if (UnderlyingObject.Direction != NetFwRuleDirection.Inbound)
			{
				return FirewallDirection.Outbound;
			}
			return FirewallDirection.Inbound;
		}
		set
		{
			UnderlyingObject.Direction = ((value == FirewallDirection.Inbound) ? NetFwRuleDirection.Inbound : NetFwRuleDirection.Outbound);
		}
	}

	public string FriendlyName => NativeHelper.ResolveStringResource(Name);

	public bool IsEnable
	{
		get
		{
			return UnderlyingObject.Enabled;
		}
		set
		{
			UnderlyingObject.Enabled = value;
		}
	}

	public IAddress[] LocalAddresses
	{
		get
		{
			return AddressHelper.StringToAddresses(UnderlyingObject.LocalAddresses);
		}
		set
		{
			try
			{
				UnderlyingObject.LocalAddresses = AddressHelper.AddressesToString(value);
			}
			catch (COMException ex)
			{
				if (ex.ErrorCode == -805306355)
				{
					throw new ArgumentException("An unspecified, multicast, broadcast or loopback IPv6 address was specified.", ex);
				}
				throw;
			}
		}
	}

	public ushort[] LocalPorts
	{
		get
		{
			return PortHelper.StringToPorts(UnderlyingObject.LocalPorts);
		}
		set
		{
			if (value.Length != 0 && !Protocol.Equals(FirewallProtocol.TCP) && !Protocol.Equals(FirewallProtocol.UDP))
			{
				throw new FirewallWASInvalidProtocolException("Port number can only be specified for the UDP and TCP protocols.");
			}
			UnderlyingObject.LocalPorts = PortHelper.PortsToString(value);
		}
	}

	public FirewallPortType LocalPortType
	{
		get
		{
			if (LocalPorts.Length != 0)
			{
				return FirewallPortType.Specific;
			}
			string localPorts = UnderlyingObject.LocalPorts;
			if (localPorts != null && localPorts.StartsWith("RPC,", StringComparison.InvariantCultureIgnoreCase))
			{
				return FirewallPortType.RPCDynamicPorts;
			}
			if (localPorts != null && localPorts.StartsWith("RPC-EPMap,", StringComparison.InvariantCultureIgnoreCase))
			{
				return FirewallPortType.RPCEndpointMapper;
			}
			if (localPorts != null && localPorts.StartsWith("IPHTTPS,", StringComparison.InvariantCultureIgnoreCase))
			{
				return FirewallPortType.IPHTTPS;
			}
			if (localPorts != null && localPorts.StartsWith("Teredo,", StringComparison.InvariantCultureIgnoreCase))
			{
				return FirewallPortType.EdgeTraversal;
			}
			if (localPorts != null && localPorts.StartsWith("Ply2Disc,", StringComparison.InvariantCultureIgnoreCase))
			{
				return FirewallPortType.PlayToDiscovery;
			}
			return FirewallPortType.All;
		}
		set
		{
			switch (value)
			{
			case FirewallPortType.All:
				LocalPorts = new ushort[0];
				break;
			case FirewallPortType.RPCDynamicPorts:
				if (!Protocol.Equals(FirewallProtocol.TCP))
				{
					throw new FirewallWASInvalidProtocolException("RPCDynamicPorts is only valid fot TCP rules. Try setting the protocol to TCP before applying this value.");
				}
				UnderlyingObject.LocalPorts = "RPC,";
				break;
			case FirewallPortType.RPCEndpointMapper:
				if (!Protocol.Equals(FirewallProtocol.TCP))
				{
					throw new FirewallWASInvalidProtocolException("RPCEndpointMapper is only valid fot TCP rules. Try setting the protocol to TCP before applying this value.");
				}
				UnderlyingObject.LocalPorts = "RPC-EPMap,";
				break;
			case FirewallPortType.IPHTTPS:
				if (!Protocol.Equals(FirewallProtocol.TCP))
				{
					throw new FirewallWASInvalidProtocolException("IPHTTPS is only valid fot TCP rules. Try setting the protocol to TCP before applying this value.");
				}
				UnderlyingObject.LocalPorts = "IPHTTPS,";
				break;
			case FirewallPortType.EdgeTraversal:
				if (!Protocol.Equals(FirewallProtocol.UDP))
				{
					throw new FirewallWASInvalidProtocolException("EdgeTraversal is only valid fot UDP rules. Try setting the protocol to TCP before applying this value.");
				}
				UnderlyingObject.LocalPorts = "Teredo,";
				break;
			case FirewallPortType.PlayToDiscovery:
				if (!Protocol.Equals(FirewallProtocol.UDP))
				{
					throw new FirewallWASInvalidProtocolException("PlayToDiscovery is only valid fot UDP rules. Try setting the protocol to TCP before applying this value.");
				}
				UnderlyingObject.LocalPorts = "Ply2Disc,";
				break;
			default:
				throw new ArgumentException("Use the LocalPorts property to set the exact local ports.");
			}
		}
	}

	public string Name
	{
		get
		{
			return UnderlyingObject.Name;
		}
		set
		{
			UnderlyingObject.Name = value;
		}
	}

	public FirewallProfiles Profiles
	{
		get
		{
			return (FirewallProfiles)(UnderlyingObject.Profiles & 7);
		}
		set
		{
			UnderlyingObject.Profiles = (int)(value & (FirewallProfiles.Domain | FirewallProfiles.Private | FirewallProfiles.Public));
		}
	}

	public FirewallProtocol Protocol
	{
		get
		{
			return new FirewallProtocol(UnderlyingObject.Protocol);
		}
		set
		{
			if ((Protocol.Equals(FirewallProtocol.TCP) || Protocol.Equals(FirewallProtocol.UDP)) && !value.Equals(FirewallProtocol.TCP) && !value.Equals(FirewallProtocol.UDP))
			{
				LocalPorts = new ushort[0];
				RemotePorts = new ushort[0];
			}
			if ((Protocol.Equals(FirewallProtocol.ICMPv4) || Protocol.Equals(FirewallProtocol.ICMPv6)) && !value.Equals(FirewallProtocol.ICMPv4) && !value.Equals(FirewallProtocol.ICMPv6))
			{
				ICMPTypesAndCodes = new FirewallWASInternetControlMessage[0];
			}
			UnderlyingObject.Protocol = value.ProtocolNumber;
		}
	}

	public IAddress[] RemoteAddresses
	{
		get
		{
			return AddressHelper.StringToAddresses(UnderlyingObject.RemoteAddresses);
		}
		set
		{
			try
			{
				UnderlyingObject.RemoteAddresses = AddressHelper.AddressesToString(value);
			}
			catch (COMException ex)
			{
				if (ex.ErrorCode == -805306355)
				{
					throw new ArgumentException("An unspecified, multicast, broadcast or loopback IPv6 address was specified.", ex);
				}
				throw;
			}
		}
	}

	public ushort[] RemotePorts
	{
		get
		{
			return PortHelper.StringToPorts(UnderlyingObject.RemotePorts);
		}
		set
		{
			if (value.Length != 0 && !Protocol.Equals(FirewallProtocol.TCP) && !Protocol.Equals(FirewallProtocol.UDP))
			{
				throw new FirewallWASInvalidProtocolException("Port number can only be specified for the UDP and TCP protocols.");
			}
			UnderlyingObject.RemotePorts = PortHelper.PortsToString(value);
		}
	}

	public FirewallScope Scope
	{
		get
		{
			if (RemoteAddresses.Length <= 1)
			{
				IAddress[] remoteAddresses = RemoteAddresses;
				foreach (IAddress address in remoteAddresses)
				{
					if (SingleIP.Any.Equals(address))
					{
						return FirewallScope.All;
					}
					if (address is LocalSubnet)
					{
						return FirewallScope.LocalSubnet;
					}
				}
			}
			return FirewallScope.Specific;
		}
		set
		{
			switch (value)
			{
			case FirewallScope.All:
				RemoteAddresses = new IAddress[1] { SingleIP.Any };
				break;
			case FirewallScope.LocalSubnet:
				RemoteAddresses = new IAddress[1]
				{
					new LocalSubnet()
				};
				break;
			default:
				throw new ArgumentException("Use the RemoteAddresses property to set the exact remote addresses");
			}
		}
	}

	public string ServiceName
	{
		get
		{
			return UnderlyingObject.serviceName;
		}
		set
		{
			UnderlyingObject.serviceName = value;
		}
	}

	public FirewallWASRule(string name, string filename, FirewallAction action, FirewallDirection direction, FirewallProfiles profiles)
		: this(name, filename, action, direction, profiles, new COMTypeResolver())
	{
	}

	public FirewallWASRule(string name, string filename, FirewallAction action, FirewallDirection direction, FirewallProfiles profiles, COMTypeResolver typeResolver)
		: this(name, action, direction, profiles, typeResolver)
	{
		ApplicationName = filename;
	}

	public FirewallWASRule(string name, FirewallAction action, FirewallDirection direction, FirewallProfiles profiles)
		: this(name, action, direction, profiles, new COMTypeResolver())
	{
	}

	public FirewallWASRule(string name, FirewallAction action, FirewallDirection direction, FirewallProfiles profiles, COMTypeResolver typeResolver)
		: this(typeResolver.CreateInstance<INetFwRule>())
	{
		Name = name;
		Action = action;
		Direction = direction;
		IsEnable = true;
		Profiles = profiles;
	}

	public FirewallWASRule(string name, ushort port, FirewallAction action, FirewallDirection direction, FirewallProfiles profiles)
		: this(name, port, action, direction, profiles, new COMTypeResolver())
	{
	}

	public FirewallWASRule(string name, ushort port, FirewallAction action, FirewallDirection direction, FirewallProfiles profiles, COMTypeResolver typeResolver)
		: this(name, action, direction, profiles, typeResolver)
	{
		Protocol = FirewallProtocol.TCP;
		LocalPorts = new ushort[1] { port };
	}

	internal FirewallWASRule(INetFwRule rule)
	{
		UnderlyingObject = rule;
	}

	public static bool IsSupported(COMTypeResolver typeResolver)
	{
		return typeResolver.IsSupported<INetFwRule>();
	}

	public virtual bool Equals(FirewallWASRule other)
	{
		if (other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (UnderlyingObject == other.UnderlyingObject)
		{
			return true;
		}
		if (!string.Equals(UnderlyingObject.Name, other.UnderlyingObject.Name) || UnderlyingObject.Profiles != other.UnderlyingObject.Profiles || UnderlyingObject.Protocol != other.UnderlyingObject.Protocol || UnderlyingObject.Action != other.UnderlyingObject.Action || UnderlyingObject.Enabled != other.UnderlyingObject.Enabled || UnderlyingObject.Direction != other.UnderlyingObject.Direction || !(UnderlyingObject.RemoteAddresses == other.UnderlyingObject.RemoteAddresses) || !(UnderlyingObject.RemotePorts == other.UnderlyingObject.RemotePorts) || !(UnderlyingObject.LocalAddresses == other.UnderlyingObject.LocalAddresses) || !(UnderlyingObject.LocalPorts == other.UnderlyingObject.LocalPorts) || !(UnderlyingObject.ApplicationName == other.UnderlyingObject.ApplicationName))
		{
			return false;
		}
		if (UnderlyingObject.Interfaces == other.UnderlyingObject.Interfaces)
		{
			return true;
		}
		if (UnderlyingObject.Interfaces is IEnumerable != other.UnderlyingObject.Interfaces is IEnumerable)
		{
			return false;
		}
		if (!(UnderlyingObject.Interfaces is IEnumerable))
		{
			return true;
		}
		return ((IEnumerable)UnderlyingObject.Interfaces).Cast<object>().Select((object o, int i) => o?.ToString()).SequenceEqual(((IEnumerable)other.UnderlyingObject.Interfaces).Cast<object>().Select((object o, int i) => o?.ToString()));
	}

	public bool Equals(IFirewallRule other)
	{
		return Equals(other as FirewallWASRule);
	}

	public static bool operator ==(FirewallWASRule left, FirewallWASRule right)
	{
		if (!object.Equals(left, right))
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

	public static bool operator !=(FirewallWASRule left, FirewallWASRule right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as IFirewallRule);
	}

	public override int GetHashCode()
	{
		int num = 132619;
		num = (int)(num * 467 + UnderlyingObject.Action);
		num = (int)(num * 467 + UnderlyingObject.Direction);
		num = num * 467 + (UnderlyingObject.Name?.GetHashCode() ?? 0);
		num = num * 467 + (UnderlyingObject.RemoteAddresses?.GetHashCode() ?? 0);
		num = num * 467 + (UnderlyingObject.RemotePorts?.GetHashCode() ?? 0);
		num = num * 467 + (UnderlyingObject.LocalAddresses?.GetHashCode() ?? 0);
		num = num * 467 + (UnderlyingObject.LocalPorts?.GetHashCode() ?? 0);
		num = num * 467 + (UnderlyingObject.ApplicationName?.GetHashCode() ?? 0);
		num = num * 467 + (UnderlyingObject.Grouping?.GetHashCode() ?? 0);
		num = num * 467 + (UnderlyingObject.Description?.GetHashCode() ?? 0);
		num = num * 467 + (UnderlyingObject.IcmpTypesAndCodes?.GetHashCode() ?? 0);
		num = num * 467 + (UnderlyingObject.InterfaceTypes?.GetHashCode() ?? 0);
		num = num * 467 + (UnderlyingObject.serviceName?.GetHashCode() ?? 0);
		num = num * 467 + UnderlyingObject.Profiles;
		num = num * 467 + UnderlyingObject.Protocol;
		num = num * 467 + UnderlyingObject.Enabled.GetHashCode();
		num = num * 467 + UnderlyingObject.EdgeTraversal.GetHashCode();
		IEnumerable<object> enumerable = (UnderlyingObject.Interfaces as IEnumerable)?.Cast<object>();
		if (enumerable != null)
		{
			int num2 = 260671;
			foreach (object item in enumerable)
			{
				num2 = num2 * 727 + (item?.ToString().GetHashCode() ?? 0);
			}
			return num * 467 + num2.GetHashCode();
		}
		return num * 467;
	}

	public override string ToString()
	{
		return FriendlyName;
	}

	public INetFwRule GetCOMObject()
	{
		return UnderlyingObject;
	}
}
