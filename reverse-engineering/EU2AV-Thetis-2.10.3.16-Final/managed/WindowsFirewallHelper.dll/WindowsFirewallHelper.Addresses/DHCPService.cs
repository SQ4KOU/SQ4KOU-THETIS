namespace WindowsFirewallHelper.Addresses;

public sealed class DHCPService : SpecialAddress
{
	protected override string AddressString => "DHCP";

	public new static DHCPService Parse(string str)
	{
		return SpecialAddress.Parse<DHCPService>(str);
	}

	public static bool TryParse(string str, out DHCPService service)
	{
		return SpecialAddress.TryParse(str, out service);
	}
}
