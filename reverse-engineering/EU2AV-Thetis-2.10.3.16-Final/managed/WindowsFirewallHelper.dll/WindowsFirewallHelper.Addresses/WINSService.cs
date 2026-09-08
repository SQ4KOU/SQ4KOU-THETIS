namespace WindowsFirewallHelper.Addresses;

public sealed class WINSService : SpecialAddress
{
	protected override string AddressString => "WINS";

	public new static WINSService Parse(string str)
	{
		return SpecialAddress.Parse<WINSService>(str);
	}

	public static bool TryParse(string str, out WINSService service)
	{
		return SpecialAddress.TryParse(str, out service);
	}
}
