using System;
using WindowsFirewallHelper.COMInterop;

namespace WindowsFirewallHelper.FirewallRules;

public class FirewallWASRuleWin8 : FirewallWASRuleWin7, IEquatable<FirewallWASRuleWin8>
{
	public string ApplicationPackageId
	{
		get
		{
			return UnderlyingObject.LocalAppPackageId;
		}
		set
		{
			UnderlyingObject.LocalAppPackageId = value;
		}
	}

	public IPSecSecurityLevel IPSecSecurityLevel
	{
		get
		{
			if (!Enum.IsDefined(typeof(IPSecSecurityLevel), UnderlyingObject.SecureFlags))
			{
				throw new ArgumentOutOfRangeException();
			}
			return (IPSecSecurityLevel)UnderlyingObject.SecureFlags;
		}
		set
		{
			if (!Enum.IsDefined(typeof(IPSecSecurityLevel), value))
			{
				throw new ArgumentOutOfRangeException();
			}
			UnderlyingObject.SecureFlags = (int)value;
		}
	}

	public new static bool IsLocallySupported => IsSupported(new COMTypeResolver());

	public string LocalUserAuthorizedList
	{
		get
		{
			return UnderlyingObject.LocalUserAuthorizedList;
		}
		set
		{
			UnderlyingObject.LocalUserAuthorizedList = value;
		}
	}

	public string RemoteMachineAuthorizedList
	{
		get
		{
			return UnderlyingObject.RemoteMachineAuthorizedList;
		}
		set
		{
			UnderlyingObject.RemoteMachineAuthorizedList = value;
		}
	}

	public string RemoteUserAuthorizedList
	{
		get
		{
			return UnderlyingObject.RemoteUserAuthorizedList;
		}
		set
		{
			UnderlyingObject.RemoteUserAuthorizedList = value;
		}
	}

	protected new INetFwRule3 UnderlyingObject => base.UnderlyingObject as INetFwRule3;

	public string UserOwner
	{
		get
		{
			return UnderlyingObject.LocalUserOwner;
		}
		set
		{
			UnderlyingObject.LocalUserOwner = value;
		}
	}

	public FirewallWASRuleWin8(string name, string filename, FirewallAction action, FirewallDirection direction, FirewallProfiles profiles)
		: base(name, filename, action, direction, profiles)
	{
	}

	public FirewallWASRuleWin8(string name, string filename, FirewallAction action, FirewallDirection direction, FirewallProfiles profiles, COMTypeResolver typeResolver)
		: base(name, filename, action, direction, profiles, typeResolver)
	{
	}

	public FirewallWASRuleWin8(string name, ushort port, FirewallAction action, FirewallDirection direction, FirewallProfiles profiles)
		: base(name, port, action, direction, profiles)
	{
	}

	public FirewallWASRuleWin8(string name, ushort port, FirewallAction action, FirewallDirection direction, FirewallProfiles profiles, COMTypeResolver typeResolver)
		: base(name, port, action, direction, profiles, typeResolver)
	{
	}

	public FirewallWASRuleWin8(string name, FirewallAction action, FirewallDirection direction, FirewallProfiles profiles)
		: base(name, action, direction, profiles)
	{
	}

	public FirewallWASRuleWin8(string name, FirewallAction action, FirewallDirection direction, FirewallProfiles profiles, COMTypeResolver typeResolver)
		: base(name, action, direction, profiles, typeResolver)
	{
	}

	internal FirewallWASRuleWin8(INetFwRule3 rule)
		: base(rule)
	{
	}

	public new static bool IsSupported(COMTypeResolver typeResolver)
	{
		if (FirewallWASRuleWin7.IsSupported(typeResolver))
		{
			return typeResolver.IsSupported<INetFwRule3>();
		}
		return false;
	}

	public bool Equals(FirewallWASRuleWin8 other)
	{
		if (other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (!Equals((FirewallWASRuleWin7)other))
		{
			return false;
		}
		if (string.Equals(UnderlyingObject.LocalAppPackageId, other.UnderlyingObject.LocalAppPackageId) && string.Equals(UnderlyingObject.LocalUserAuthorizedList, other.UnderlyingObject.LocalUserAuthorizedList) && string.Equals(UnderlyingObject.RemoteMachineAuthorizedList, other.UnderlyingObject.RemoteMachineAuthorizedList) && string.Equals(UnderlyingObject.RemoteUserAuthorizedList, other.UnderlyingObject.RemoteUserAuthorizedList) && string.Equals(UnderlyingObject.LocalUserOwner, other.UnderlyingObject.LocalUserOwner))
		{
			return UnderlyingObject.SecureFlags == other.UnderlyingObject.SecureFlags;
		}
		return false;
	}

	public static bool operator ==(FirewallWASRuleWin8 left, FirewallWASRuleWin8 right)
	{
		if (!object.Equals(left, right))
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

	public static bool operator !=(FirewallWASRuleWin8 left, FirewallWASRuleWin8 right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as FirewallWASRuleWin8);
	}

	public override int GetHashCode()
	{
		return (((((base.GetHashCode() * 467 + (UnderlyingObject.LocalAppPackageId?.GetHashCode() ?? 0)) * 467 + (UnderlyingObject.LocalUserAuthorizedList?.GetHashCode() ?? 0)) * 467 + (UnderlyingObject.RemoteMachineAuthorizedList?.GetHashCode() ?? 0)) * 467 + (UnderlyingObject.RemoteUserAuthorizedList?.GetHashCode() ?? 0)) * 467 + (UnderlyingObject.LocalUserOwner?.GetHashCode() ?? 0)) * 467 + UnderlyingObject.SecureFlags;
	}

	public new INetFwRule3 GetCOMObject()
	{
		return UnderlyingObject;
	}
}
