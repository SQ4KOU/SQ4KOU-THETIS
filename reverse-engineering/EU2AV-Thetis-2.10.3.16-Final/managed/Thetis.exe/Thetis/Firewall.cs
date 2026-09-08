using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using WindowsFirewallHelper;

namespace Thetis;

internal class Firewall
{
	private const string _IN_TCP = "Thetis Allow IN TCP";

	private const string _OUT_TCP = "Thetis Allow OUT TCP";

	private const string _IN_UDP = "Thetis Allow IN UDP";

	private const string _OUT_UDP = "Thetis Allow OUT UDP";

	public static void Setup()
	{
		if (!FirewallManager.IsServiceRunning)
		{
			MessageBox.Show("Firewall service is not running.", "Firewall - No Service", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			return;
		}
		string sExe = Assembly.GetEntryAssembly().Location;
		if (sExe == "")
		{
			MessageBox.Show("Could not obtain EXE path.", "Firewall - Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			return;
		}
		if (!Common.IsAdministrator())
		{
			MessageBox.Show("To reset Thetis firewall entries please run Thetis 'As Administrator'.", "Firewall - No Administrator Rights", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			return;
		}
		try
		{
			foreach (IFirewallRule item in FirewallManager.Instance.Rules.Where((IFirewallRule a) => a.ApplicationName != null && a.ApplicationName.Equals(sExe, StringComparison.InvariantCultureIgnoreCase)))
			{
				FirewallManager.Instance.Rules.Remove(item);
			}
		}
		catch
		{
			MessageBox.Show("There was a problem removing existing firewall entries. Please configure manually.\nThetis.exe needs UDP/TCP in/out, all ports", "Firewall - Problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			return;
		}
		bool num = addApplicationRule(FirewallDirection.Outbound, FirewallProtocol.TCP, sExe, "Thetis Allow OUT TCP");
		bool flag = addApplicationRule(FirewallDirection.Outbound, FirewallProtocol.UDP, sExe, "Thetis Allow OUT UDP");
		bool flag2 = addApplicationRule(FirewallDirection.Inbound, FirewallProtocol.TCP, sExe, "Thetis Allow IN TCP");
		bool flag3 = addApplicationRule(FirewallDirection.Inbound, FirewallProtocol.UDP, sExe, "Thetis Allow IN UDP");
		if (num & flag & flag2 & flag3)
		{
			MessageBox.Show("Firewall is configured correctly.", "Firewall - Success", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
		}
		else
		{
			MessageBox.Show("There was a problem configuring the firewall. Please configure manually.\nThetis.exe needs UDP/TCP in/out, all ports", "Firewall - Problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
		}
	}

	private static IFirewallRule findRule(string sName)
	{
		IFirewallRule result = null;
		ICollection<IFirewallRule> rules = FirewallManager.Instance.Rules;
		if (rules != null)
		{
			try
			{
				result = rules.First((IFirewallRule a) => a.Name != null && a.Name == sName);
			}
			catch
			{
			}
		}
		return result;
	}

	private static bool addApplicationRule(FirewallDirection direction, FirewallProtocol protocol, string sPath, string ruleName)
	{
		bool result = false;
		try
		{
			IFirewallRule firewallRule = findRule(ruleName);
			if (firewallRule != null)
			{
				FirewallManager.Instance.Rules.Remove(firewallRule);
			}
			firewallRule = FirewallManager.Instance.CreateApplicationRule(FirewallProfiles.Domain | FirewallProfiles.Private | FirewallProfiles.Public, ruleName, FirewallAction.Allow, sPath);
			if (firewallRule != null)
			{
				firewallRule.Direction = direction;
				firewallRule.Protocol = protocol;
				firewallRule.Scope = FirewallScope.All;
				FirewallManager.Instance.Rules.Add(firewallRule);
				result = true;
			}
		}
		catch
		{
		}
		return result;
	}
}
