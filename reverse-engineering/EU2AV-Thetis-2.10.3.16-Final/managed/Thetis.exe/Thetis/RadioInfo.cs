using System.Net;

namespace Thetis;

public sealed class RadioInfo
{
	public RadioDiscoveryRadioProtocol Protocol { get; set; }

	public IPAddress IpAddress { get; set; }

	public string MacAddress { get; set; }

	public HPSDRHW DeviceType { get; set; }

	public byte CodeVersion { get; set; }

	public byte BetaVersion { get; set; }

	public byte Protocol2Supported { get; set; }

	public byte NumRxs { get; set; }

	public byte MercuryVersion0 { get; set; }

	public byte MercuryVersion1 { get; set; }

	public byte MercuryVersion2 { get; set; }

	public byte MercuryVersion3 { get; set; }

	public byte PennyVersion { get; set; }

	public byte MetisVersion { get; set; }

	public byte HwRev { get; set; }

	public bool Supports24BitAudio => (HwRev & 0x10) != 0;

	public bool IsBusy { get; set; }

	public int DiscoveryPortBase { get; set; }

	public int PortCount { get; set; }

	public bool IsApipaRadio { get; set; }

	public bool IsCustom { get; set; }

	public string CustomGuid { get; set; }
}
