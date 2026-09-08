namespace WindowsFirewallHelper.Addresses;

public sealed class LocalSubnet : SpecialAddress
{
	protected override string AddressString => "LocalSubnet";

	public new static LocalSubnet Parse(string str)
	{
		return SpecialAddress.Parse<LocalSubnet>(str);
	}

	public static bool TryParse(string str, out LocalSubnet service)
	{
		return SpecialAddress.TryParse(str, out service);
	}
}
