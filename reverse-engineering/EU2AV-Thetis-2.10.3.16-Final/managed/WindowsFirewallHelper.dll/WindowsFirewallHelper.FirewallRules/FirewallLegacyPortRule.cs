using System;
using System.Collections.Generic;
using System.Linq;
using WindowsFirewallHelper.Addresses;
using WindowsFirewallHelper.COMInterop;
using WindowsFirewallHelper.Exceptions;
using WindowsFirewallHelper.InternalHelpers;

namespace WindowsFirewallHelper.FirewallRules;

public class FirewallLegacyPortRule : IFirewallRule, IEquatable<IFirewallRule>, IEquatable<FirewallLegacyPortRule>
{
	public static bool IsLocallySupported => new COMTypeResolver().IsSupported<INetFwOpenPort>();

	public ushort LocalPort
	{
		get
		{
			return (ushort)UnderlyingObjects.Values.SelectMany((INetFwOpenPort[] p) => p).First().Port;
		}
		set
		{
			foreach (INetFwOpenPort item in UnderlyingObjects.Values.SelectMany((INetFwOpenPort[] p) => p))
			{
				item.Port = value;
			}
		}
	}

	public COMTypeResolver TypeResolver { get; }

	private Dictionary<FirewallProfiles, INetFwOpenPort[]> UnderlyingObjects { get; }

	FirewallAction IFirewallRule.Action
	{
		get
		{
			return FirewallAction.Allow;
		}
		set
		{
			throw new FirewallLegacyNotSupportedException();
		}
	}

	string IFirewallRule.ApplicationName
	{
		get
		{
			return string.Empty;
		}
		set
		{
			throw new ArgumentException("You can not change the identity of a port rule. Consider creating another rule.");
		}
	}

	FirewallDirection IFirewallRule.Direction
	{
		get
		{
			return FirewallDirection.Inbound;
		}
		set
		{
			throw new FirewallLegacyNotSupportedException();
		}
	}

	public string FriendlyName => NativeHelper.ResolveStringResource(Name);

	public bool IsEnable
	{
		get
		{
			return UnderlyingObjects.Values.SelectMany((INetFwOpenPort[] p) => p).All((INetFwOpenPort port) => port.Enabled);
		}
		set
		{
			foreach (INetFwOpenPort item in UnderlyingObjects.Values.SelectMany((INetFwOpenPort[] p) => p))
			{
				item.Enabled = value;
			}
		}
	}

	IAddress[] IFirewallRule.LocalAddresses
	{
		get
		{
			return new IAddress[1] { SingleIP.Any };
		}
		set
		{
			throw new FirewallLegacyNotSupportedException();
		}
	}

	ushort[] IFirewallRule.LocalPorts
	{
		get
		{
			return (from port in UnderlyingObjects.Values.SelectMany((INetFwOpenPort[] p) => p)
				select (ushort)port.Port).Distinct().ToArray();
		}
		set
		{
			if (value.Length == 0)
			{
				throw new ArgumentException("You can not change the identity of a port rule. Consider creating another rule.");
			}
			if (value.Length > 1)
			{
				throw new FirewallLegacyNotSupportedException("This property only accept an array of one element length.");
			}
			LocalPort = value[0];
		}
	}

	FirewallPortType IFirewallRule.LocalPortType
	{
		get
		{
			return FirewallPortType.Specific;
		}
		set
		{
			throw new FirewallLegacyNotSupportedException();
		}
	}

	public string Name
	{
		get
		{
			return UnderlyingObjects.Values.SelectMany((INetFwOpenPort[] p) => p).First().Name;
		}
		set
		{
			foreach (INetFwOpenPort item in UnderlyingObjects.Values.SelectMany((INetFwOpenPort[] p) => p))
			{
				item.Name = value;
			}
		}
	}

	public FirewallProfiles Profiles => UnderlyingObjects.Keys.ToArray().Aggregate((FirewallProfiles)0, (FirewallProfiles profiles, FirewallProfiles profile) => profiles | profile);

	public FirewallProtocol Protocol
	{
		get
		{
			return new FirewallProtocol((int)UnderlyingObjects.Values.SelectMany((INetFwOpenPort[] p) => p).First().Protocol);
		}
		set
		{
			if (!value.Equals(FirewallProtocol.Any) && !value.Equals(FirewallProtocol.TCP) && !value.Equals(FirewallProtocol.UDP))
			{
				throw new FirewallLegacyNotSupportedException("Acceptable protocols for Windows Firewall Legacy are UDP, TCP and Any.");
			}
			if (value.Equals(FirewallProtocol.Any) && FirewallWAS.IsSupported(TypeResolver))
			{
				throw new FirewallLegacyNotSupportedException("`Any` protocol is not available with Windows Firewall Legacy in compatibility mode.");
			}
			foreach (INetFwOpenPort item in UnderlyingObjects.Values.SelectMany((INetFwOpenPort[] p) => p))
			{
				item.Protocol = (NetFwIPProtocol)value.ProtocolNumber;
			}
		}
	}

	public IAddress[] RemoteAddresses
	{
		get
		{
			return UnderlyingObjects.Values.SelectMany((INetFwOpenPort[] p) => p).SelectMany((INetFwOpenPort application) => AddressHelper.StringToAddresses(application.RemoteAddresses)).Distinct()
				.ToArray();
		}
		set
		{
			foreach (INetFwOpenPort item in UnderlyingObjects.Values.SelectMany((INetFwOpenPort[] p) => p))
			{
				item.RemoteAddresses = AddressHelper.AddressesToString(value);
			}
		}
	}

	ushort[] IFirewallRule.RemotePorts
	{
		get
		{
			return new ushort[0];
		}
		set
		{
			throw new FirewallLegacyNotSupportedException();
		}
	}

	public FirewallScope Scope
	{
		get
		{
			return (FirewallScope)UnderlyingObjects.Values.SelectMany((INetFwOpenPort[] p) => p).First().Scope;
		}
		set
		{
			switch (value)
			{
			case FirewallScope.Specific:
				throw new ArgumentException("Use the RemoteAddresses property to set the exact remote addresses.");
			case FirewallScope.LocalSubnet:
				RemoteAddresses = new IAddress[1]
				{
					new LocalSubnet()
				};
				{
					foreach (INetFwOpenPort item in UnderlyingObjects.Values.SelectMany((INetFwOpenPort[] p) => p))
					{
						item.Scope = NetFwScope.LocalSubnet;
					}
					break;
				}
			case FirewallScope.All:
				RemoteAddresses = new IAddress[1] { SingleIP.Any };
				{
					foreach (INetFwOpenPort item2 in UnderlyingObjects.Values.SelectMany((INetFwOpenPort[] p) => p))
					{
						item2.Scope = NetFwScope.All;
					}
					break;
				}
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	string IFirewallRule.ServiceName
	{
		get
		{
			return string.Empty;
		}
		set
		{
			throw new ArgumentException("You can not change the identity of a port rule. Consider creating another rule.");
		}
	}

	public FirewallLegacyPortRule(string name, ushort port, FirewallProfiles profiles)
		: this(name, port, profiles, new COMTypeResolver())
	{
	}

	public FirewallLegacyPortRule(string name, ushort port, FirewallProfiles profiles, COMTypeResolver typeResolver)
	{
		TypeResolver = typeResolver;
		if (profiles.HasFlag(FirewallProfiles.Public))
		{
			throw new FirewallLegacyNotSupportedException("Public profile is not supported when working with Windows Firewall Legacy.");
		}
		UnderlyingObjects = new Dictionary<FirewallProfiles, INetFwOpenPort[]>();
		foreach (FirewallProfiles item in Enum.GetValues(typeof(FirewallProfiles)).OfType<FirewallProfiles>())
		{
			if (profiles.HasFlag(item))
			{
				UnderlyingObjects.Add(item, new INetFwOpenPort[1] { typeResolver.CreateInstance<INetFwOpenPort>() });
			}
		}
		if (UnderlyingObjects.Count == 0)
		{
			throw new ArgumentException("At least one profile is required.", "profiles");
		}
		Name = name;
		LocalPort = port;
		IsEnable = true;
		Scope = FirewallScope.All;
		IsEnable = true;
	}

	internal FirewallLegacyPortRule(Dictionary<FirewallProfiles, INetFwOpenPort[]> openPorts, COMTypeResolver typeResolver)
	{
		TypeResolver = typeResolver;
		UnderlyingObjects = openPorts;
	}

	public bool Equals(FirewallLegacyPortRule other)
	{
		if (other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (Profiles != other.Profiles)
		{
			return false;
		}
		if (LocalPort != other.LocalPort || Protocol != other.Protocol)
		{
			return false;
		}
		return true;
	}

	public bool Equals(IFirewallRule other)
	{
		return Equals(other as FirewallLegacyPortRule);
	}

	public static bool operator ==(FirewallLegacyPortRule left, FirewallLegacyPortRule right)
	{
		if (!object.Equals(left, right))
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

	public static bool operator !=(FirewallLegacyPortRule left, FirewallLegacyPortRule right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as IFirewallRule);
	}

	public override int GetHashCode()
	{
		return UnderlyingObjects.Values.Aggregate(0, (int hashCode, INetFwOpenPort[] port) => hashCode + port.GetHashCode());
	}

	public override string ToString()
	{
		return FriendlyName;
	}

	public INetFwOpenPort[] GetCOMObjects(FirewallProfiles profile)
	{
		if (UnderlyingObjects.ContainsKey(profile))
		{
			return UnderlyingObjects[profile];
		}
		return null;
	}
}
