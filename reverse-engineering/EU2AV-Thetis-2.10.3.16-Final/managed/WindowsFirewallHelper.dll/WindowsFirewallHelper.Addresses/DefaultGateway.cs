namespace WindowsFirewallHelper.Addresses;

public sealed class DefaultGateway : SpecialAddress
{
	protected override string AddressString => "Defaultgateway";

	public new static DefaultGateway Parse(string str)
	{
		return SpecialAddress.Parse<DefaultGateway>(str);
	}

	public static bool TryParse(string str, out DefaultGateway address)
	{
		return SpecialAddress.TryParse(str, out address);
	}
}
