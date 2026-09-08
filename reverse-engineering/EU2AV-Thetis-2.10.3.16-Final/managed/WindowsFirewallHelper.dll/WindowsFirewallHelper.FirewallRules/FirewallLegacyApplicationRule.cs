using System;
using System.Collections.Generic;
using System.Linq;
using WindowsFirewallHelper.Addresses;
using WindowsFirewallHelper.COMInterop;
using WindowsFirewallHelper.Exceptions;
using WindowsFirewallHelper.InternalHelpers;

namespace WindowsFirewallHelper.FirewallRules;

public class FirewallLegacyApplicationRule : IFirewallRule, IEquatable<IFirewallRule>, IEquatable<FirewallLegacyApplicationRule>
{
	public static bool IsLocallySupported => new COMTypeResolver().IsSupported<INetFwAuthorizedApplication>();

	private Dictionary<FirewallProfiles, INetFwAuthorizedApplication[]> UnderlyingObjects { get; }

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

	public string ApplicationName
	{
		get
		{
			return UnderlyingObjects.Values.SelectMany((INetFwAuthorizedApplication[] a) => a).First().ProcessImageFileName;
		}
		set
		{
			foreach (INetFwAuthorizedApplication item in UnderlyingObjects.Values.SelectMany((INetFwAuthorizedApplication[] a) => a))
			{
				item.ProcessImageFileName = value;
			}
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
			return UnderlyingObjects.Values.SelectMany((INetFwAuthorizedApplication[] a) => a).All((INetFwAuthorizedApplication port) => port.Enabled);
		}
		set
		{
			foreach (INetFwAuthorizedApplication item in UnderlyingObjects.Values.SelectMany((INetFwAuthorizedApplication[] a) => a))
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
			return new ushort[0];
		}
		set
		{
			throw new FirewallLegacyNotSupportedException();
		}
	}

	FirewallPortType IFirewallRule.LocalPortType
	{
		get
		{
			return FirewallPortType.All;
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
			return UnderlyingObjects.Values.SelectMany((INetFwAuthorizedApplication[] a) => a).First().Name;
		}
		set
		{
			foreach (INetFwAuthorizedApplication item in UnderlyingObjects.Values.SelectMany((INetFwAuthorizedApplication[] a) => a))
			{
				item.Name = value;
			}
		}
	}

	public FirewallProfiles Profiles => UnderlyingObjects.Keys.ToArray().Aggregate((FirewallProfiles)0, (FirewallProfiles profiles, FirewallProfiles profile) => profiles | profile);

	FirewallProtocol IFirewallRule.Protocol
	{
		get
		{
			return FirewallProtocol.Any;
		}
		set
		{
			throw new FirewallLegacyNotSupportedException();
		}
	}

	public IAddress[] RemoteAddresses
	{
		get
		{
			return UnderlyingObjects.Values.SelectMany((INetFwAuthorizedApplication[] a) => a).SelectMany((INetFwAuthorizedApplication application) => AddressHelper.StringToAddresses(application.RemoteAddresses)).Distinct()
				.ToArray();
		}
		set
		{
			foreach (INetFwAuthorizedApplication item in UnderlyingObjects.Values.SelectMany((INetFwAuthorizedApplication[] a) => a))
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
			return (FirewallScope)UnderlyingObjects.Values.SelectMany((INetFwAuthorizedApplication[] a) => a).First().Scope;
		}
		set
		{
			switch (value)
			{
			case FirewallScope.Specific:
				throw new ArgumentException("Use the RemoteAddresses property to set the exact remote addresses");
			case FirewallScope.LocalSubnet:
				RemoteAddresses = new IAddress[1]
				{
					new LocalSubnet()
				};
				{
					foreach (INetFwAuthorizedApplication item in UnderlyingObjects.Values.SelectMany((INetFwAuthorizedApplication[] a) => a))
					{
						item.Scope = NetFwScope.LocalSubnet;
					}
					break;
				}
			case FirewallScope.All:
				RemoteAddresses = new IAddress[1] { SingleIP.Any };
				{
					foreach (INetFwAuthorizedApplication item2 in UnderlyingObjects.Values.SelectMany((INetFwAuthorizedApplication[] a) => a))
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
			throw new ArgumentException("You can not change the identity of an application rule. Consider creating another rule.");
		}
	}

	public FirewallLegacyApplicationRule(string name, string processAddress, FirewallProfiles profiles)
		: this(name, processAddress, profiles, new COMTypeResolver())
	{
	}

	public FirewallLegacyApplicationRule(string name, string processAddress, FirewallProfiles profiles, COMTypeResolver typeResolver)
	{
		if (profiles.HasFlag(FirewallProfiles.Public))
		{
			throw new FirewallLegacyNotSupportedException("Public profile is not supported when working with Windows Firewall Legacy.");
		}
		UnderlyingObjects = new Dictionary<FirewallProfiles, INetFwAuthorizedApplication[]>();
		foreach (FirewallProfiles item in Enum.GetValues(typeof(FirewallProfiles)).OfType<FirewallProfiles>())
		{
			if (profiles.HasFlag(item))
			{
				UnderlyingObjects.Add(item, new INetFwAuthorizedApplication[1] { typeResolver.CreateInstance<INetFwAuthorizedApplication>() });
			}
		}
		if (UnderlyingObjects.Count == 0)
		{
			throw new ArgumentException("At least one profile is required.", "profiles");
		}
		Name = name;
		ApplicationName = processAddress;
		IsEnable = true;
		Scope = FirewallScope.All;
		IsEnable = true;
	}

	internal FirewallLegacyApplicationRule(Dictionary<FirewallProfiles, INetFwAuthorizedApplication[]> authorizedApplications)
	{
		UnderlyingObjects = authorizedApplications;
	}

	public bool Equals(FirewallLegacyApplicationRule other)
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
		if (!string.Equals(ApplicationName, other.ApplicationName))
		{
			return false;
		}
		return true;
	}

	public bool Equals(IFirewallRule other)
	{
		return Equals(other as FirewallLegacyApplicationRule);
	}

	public static bool operator ==(FirewallLegacyApplicationRule left, FirewallLegacyApplicationRule right)
	{
		if (!object.Equals(left, right))
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

	public static bool operator !=(FirewallLegacyApplicationRule left, FirewallLegacyApplicationRule right)
	{
		return !(left == right);
	}

	public override bool Equals(object other)
	{
		return Equals(other as IFirewallRule);
	}

	public override int GetHashCode()
	{
		return UnderlyingObjects.Values.Aggregate(0, (int hashCode, INetFwAuthorizedApplication[] port) => hashCode + port.GetHashCode());
	}

	public override string ToString()
	{
		return FriendlyName;
	}

	public INetFwAuthorizedApplication[] GetCOMObjects(FirewallProfiles profile)
	{
		if (UnderlyingObjects.ContainsKey(profile))
		{
			return UnderlyingObjects[profile];
		}
		return null;
	}
}
