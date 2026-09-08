namespace RawInput_dll;

public struct DeviceInfoHid
{
	public uint VendorID;

	public uint ProductID;

	public uint VersionNumber;

	public ushort UsagePage;

	public ushort Usage;

	public override string ToString()
	{
		return $"HidInfo\n VendorID: {VendorID}\n ProductID: {ProductID}\n VersionNumber: {VersionNumber}\n UsagePage: {UsagePage}\n Usage: {Usage}\n";
	}
}
