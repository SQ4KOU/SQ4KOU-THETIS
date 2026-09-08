using System;
using WindowsFirewallHelper.InternalHelpers;

namespace WindowsFirewallHelper;

public class FirewallWASRuleGroup : IEquatable<FirewallWASRuleGroup>
{
	private readonly FirewallWAS _firewall;

	public string FriendlyName => NativeHelper.ResolveStringResource(Name);

	public string Name { get; }

	internal FirewallWASRuleGroup(FirewallWAS firewall, string name)
	{
		_firewall = firewall;
		Name = name;
	}

	public bool Equals(FirewallWASRuleGroup other)
	{
		if (other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		return string.Equals(Name, other.Name);
	}

	public static bool operator ==(FirewallWASRuleGroup left, FirewallWASRuleGroup right)
	{
		if (!object.Equals(left, right))
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

	public static bool operator !=(FirewallWASRuleGroup left, FirewallWASRuleGroup right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as FirewallWASRuleGroup);
	}

	public override int GetHashCode()
	{
		if (Name == null)
		{
			return 0;
		}
		return Name.GetHashCode();
	}

	public override string ToString()
	{
		return FriendlyName;
	}

	public void DisableRuleGroup(FirewallProfiles profiles)
	{
		_firewall.UnderlyingObject.EnableRuleGroup((int)profiles, Name, enable: false);
	}

	public void EnableRuleGroup(FirewallProfiles profiles)
	{
		_firewall.UnderlyingObject.EnableRuleGroup((int)profiles, Name, enable: true);
	}

	public bool IsRuleGroupEnable(FirewallProfiles profiles)
	{
		return _firewall.UnderlyingObject.IsRuleGroupEnabled((int)profiles, Name);
	}
}
