using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WindowsFirewallHelper.COMInterop;
using WindowsFirewallHelper.InternalHelpers;

namespace WindowsFirewallHelper;

public class FirewallProduct
{
	public string FriendlyName => NativeHelper.ResolveStringResource(Name);

	public static bool IsLocallySupported => IsSupported(new COMTypeResolver());

	public string Name
	{
		get
		{
			return UnderlyingObject.DisplayName;
		}
		set
		{
			UnderlyingObject.DisplayName = value;
		}
	}

	public COMTypeResolver TypeResolver { get; }

	public FirewallRuleCategory[] RuleCategories
	{
		get
		{
			if (UnderlyingObject.RuleCategories is IEnumerable enumerable)
			{
				List<FirewallRuleCategory> list = new List<FirewallRuleCategory>();
				foreach (int item in enumerable)
				{
					list.Add((FirewallRuleCategory)item);
				}
				return list.ToArray();
			}
			return new FirewallRuleCategory[0];
		}
		set
		{
			object[] ruleCategories = value.Select((FirewallRuleCategory category) => (int)category).Cast<object>().ToArray();
			UnderlyingObject.RuleCategories = ruleCategories;
		}
	}

	public string SignedExecutableFilename => UnderlyingObject.PathToSignedProductExe;

	private INetFwProduct UnderlyingObject { get; }

	public FirewallProduct(string name)
		: this(name, new COMTypeResolver())
	{
	}

	public FirewallProduct(string name, COMTypeResolver typeResolver)
	{
		TypeResolver = typeResolver;
		if (!TypeResolver.IsSupported<INetFwProduct>())
		{
			throw new NotSupportedException();
		}
		UnderlyingObject = TypeResolver.CreateInstance<INetFwProduct>();
		Name = name;
	}

	internal FirewallProduct(INetFwProduct product)
	{
		UnderlyingObject = product;
	}

	public static bool IsSupported(COMTypeResolver typeResolver)
	{
		return typeResolver.IsSupported<INetFwProduct>();
	}

	public INetFwProduct GetCOMObject()
	{
		return UnderlyingObject;
	}
}
