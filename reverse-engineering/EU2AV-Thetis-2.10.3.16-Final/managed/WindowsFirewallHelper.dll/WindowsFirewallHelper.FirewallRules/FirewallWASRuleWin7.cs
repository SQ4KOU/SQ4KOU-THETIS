using System;
using WindowsFirewallHelper.COMInterop;

namespace WindowsFirewallHelper.FirewallRules;

public class FirewallWASRuleWin7 : FirewallWASRule, IEquatable<FirewallWASRuleWin7>
{
	public EdgeTraversalAction EdgeTraversalOptions
	{
		get
		{
			if (!Enum.IsDefined(typeof(EdgeTraversalAction), UnderlyingObject.EdgeTraversalOptions))
			{
				throw new ArgumentOutOfRangeException();
			}
			return (EdgeTraversalAction)UnderlyingObject.EdgeTraversalOptions;
		}
		set
		{
			if (!Enum.IsDefined(typeof(EdgeTraversalAction), value))
			{
				throw new ArgumentOutOfRangeException();
			}
			UnderlyingObject.EdgeTraversalOptions = (int)value;
		}
	}

	public new static bool IsLocallySupported => IsSupported(new COMTypeResolver());

	protected new INetFwRule2 UnderlyingObject => base.UnderlyingObject as INetFwRule2;

	public FirewallWASRuleWin7(string name, string filename, FirewallAction action, FirewallDirection direction, FirewallProfiles profiles)
		: base(name, filename, action, direction, profiles)
	{
	}

	public FirewallWASRuleWin7(string name, string filename, FirewallAction action, FirewallDirection direction, FirewallProfiles profiles, COMTypeResolver typeResolver)
		: base(name, filename, action, direction, profiles, typeResolver)
	{
	}

	public FirewallWASRuleWin7(string name, ushort port, FirewallAction action, FirewallDirection direction, FirewallProfiles profiles)
		: base(name, port, action, direction, profiles)
	{
	}

	public FirewallWASRuleWin7(string name, ushort port, FirewallAction action, FirewallDirection direction, FirewallProfiles profiles, COMTypeResolver typeResolver)
		: base(name, port, action, direction, profiles, typeResolver)
	{
	}

	public FirewallWASRuleWin7(string name, FirewallAction action, FirewallDirection direction, FirewallProfiles profiles)
		: base(name, action, direction, profiles)
	{
	}

	public FirewallWASRuleWin7(string name, FirewallAction action, FirewallDirection direction, FirewallProfiles profiles, COMTypeResolver typeResolver)
		: base(name, action, direction, profiles, typeResolver)
	{
	}

	internal FirewallWASRuleWin7(INetFwRule2 rule)
		: base(rule)
	{
	}

	public new static bool IsSupported(COMTypeResolver typeResolver)
	{
		if (FirewallWASRule.IsSupported(typeResolver))
		{
			return typeResolver.IsSupported<INetFwRule2>();
		}
		return false;
	}

	public bool Equals(FirewallWASRuleWin7 other)
	{
		if (other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (!base.Equals(other))
		{
			return false;
		}
		return UnderlyingObject.EdgeTraversalOptions == other.UnderlyingObject.EdgeTraversalOptions;
	}

	public static bool operator ==(FirewallWASRuleWin7 left, FirewallWASRuleWin7 right)
	{
		if (!object.Equals(left, right))
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

	public static bool operator !=(FirewallWASRuleWin7 left, FirewallWASRuleWin7 right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as FirewallWASRuleWin7);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() * 467 + UnderlyingObject.EdgeTraversalOptions;
	}

	public new INetFwRule2 GetCOMObject()
	{
		return UnderlyingObject;
	}
}
