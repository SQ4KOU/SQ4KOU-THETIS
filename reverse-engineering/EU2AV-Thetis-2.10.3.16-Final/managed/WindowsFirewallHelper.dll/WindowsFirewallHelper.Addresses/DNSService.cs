namespace WindowsFirewallHelper.Addresses;

public sealed class DNSService : SpecialAddress
{
	protected override string AddressString => "DNS";

	public new static DNSService Parse(string str)
	{
		return SpecialAddress.Parse<DNSService>(str);
	}

	public static bool TryParse(string str, out DNSService service)
	{
		return SpecialAddress.TryParse(str, out service);
	}
}
