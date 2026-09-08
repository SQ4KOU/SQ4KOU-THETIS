namespace WindowsFirewallHelper;

public enum IPSecSecurityLevel
{
	None,
	IPSecNoEncapsulation,
	IPSecWithIntegrityProtection,
	IPSecWithEncryptionNegotiation,
	IPSecWithFullEncryption
}
