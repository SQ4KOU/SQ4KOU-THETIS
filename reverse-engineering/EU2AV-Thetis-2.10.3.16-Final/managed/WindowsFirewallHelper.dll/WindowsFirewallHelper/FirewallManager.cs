using System;
using System.ServiceProcess;
using WindowsFirewallHelper.COMInterop;
using WindowsFirewallHelper.Collections;
using WindowsFirewallHelper.FirewallRules;

namespace WindowsFirewallHelper;

public static class FirewallManager
{
	public static IFirewall Instance
	{
		get
		{
			switch (Version)
			{
			case FirewallAPIVersion.FirewallLegacy:
				return FirewallLegacy.Instance;
			case FirewallAPIVersion.FirewallWAS:
			case FirewallAPIVersion.FirewallWASWin7:
			case FirewallAPIVersion.FirewallWASWin8:
				return FirewallWAS.Instance;
			default:
				throw new NotSupportedException();
			}
		}
	}

	public static bool IsServiceRunning
	{
		get
		{
			switch (Version)
			{
			case FirewallAPIVersion.FirewallLegacy:
				return new ServiceController("SharedAccess").Status == ServiceControllerStatus.Running;
			case FirewallAPIVersion.FirewallWAS:
			case FirewallAPIVersion.FirewallWASWin7:
			case FirewallAPIVersion.FirewallWASWin8:
				return new ServiceController("MpsSvc").Status == ServiceControllerStatus.Running;
			default:
				return false;
			}
		}
	}

	public static IFirewallProductsCollection RegisteredProducts => GetRegisteredProducts(new COMTypeResolver());

	public static FirewallAPIVersion Version
	{
		get
		{
			if (FirewallWAS.IsLocallySupported)
			{
				if (FirewallWASRuleWin8.IsLocallySupported)
				{
					return FirewallAPIVersion.FirewallWASWin8;
				}
				if (FirewallWASRuleWin7.IsLocallySupported)
				{
					return FirewallAPIVersion.FirewallWASWin7;
				}
				return FirewallAPIVersion.FirewallWAS;
			}
			if (FirewallLegacy.IsLocallySupported)
			{
				return FirewallAPIVersion.FirewallLegacy;
			}
			return FirewallAPIVersion.None;
		}
	}

	public static bool TryGetInstance(out IFirewall instance)
	{
		try
		{
			instance = Instance;
			return true;
		}
		catch
		{
			instance = null;
			return false;
		}
	}

	public static bool TryGetRegisteredProducts(out IFirewallProductsCollection collection)
	{
		try
		{
			collection = RegisteredProducts;
			return true;
		}
		catch
		{
			collection = null;
			return false;
		}
	}

	public static FirewallProductRegistrationHandle RegisterProduct(FirewallProduct product, COMTypeResolver typeResolver)
	{
		return new FirewallProductRegistrationHandle(GetProducts(typeResolver).Register(product.GetCOMObject()));
	}

	public static FirewallProductRegistrationHandle RegisterProduct(FirewallProduct product)
	{
		return RegisterProduct(product, new COMTypeResolver());
	}

	public static IFirewallProductsCollection GetRegisteredProducts(COMTypeResolver typeResolver)
	{
		return new FirewallProductsCollection(GetProducts(typeResolver));
	}

	private static INetFwProducts GetProducts(COMTypeResolver typeResolver)
	{
		if (!FirewallProduct.IsSupported(typeResolver))
		{
			throw new NotSupportedException();
		}
		return typeResolver.CreateInstance<INetFwProducts>();
	}
}
