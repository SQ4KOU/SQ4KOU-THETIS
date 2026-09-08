using System.Net;

namespace Thetis;

public sealed class RadioDiscoveryOptions
{
	private ScanPerformanceProfile _scanPerformance = ScanPerformanceProfile.Balanced;

	public bool IgnoreSubnetCheck { get; set; }

	public bool IncludeWireless { get; set; }

	public bool IncludeEthernet { get; set; }

	public bool IncludeOtherInterfaceTypes { get; set; }

	public bool AllowLoopback { get; set; }

	public bool AllowAPIPA { get; set; }

	public int DiscoveryPortBase { get; set; }

	public int BindLocalPort { get; set; }

	public int AttemptsPerNic { get; set; }

	public int PollTimeoutMilliseconds { get; set; }

	public int QuietPollsBeforeResend { get; set; }

	public bool IncludeGeneralBroadcast { get; set; }

	public ScanPerformanceProfile ScanPerformance
	{
		get
		{
			return _scanPerformance;
		}
		set
		{
			_scanPerformance = value;
			applyScanPerformanceProfile(value);
		}
	}

	public RadioDiscoveryProtocolMode ProtocolMode { get; set; }

	public IPAddress FixedTargetIp { get; set; }

	public IPAddress FixedLocalIp { get; set; }

	public RadioDiscoveryOptions()
	{
		IgnoreSubnetCheck = false;
		IncludeWireless = true;
		IncludeEthernet = true;
		IncludeOtherInterfaceTypes = false;
		AllowLoopback = false;
		AllowAPIPA = true;
		DiscoveryPortBase = 1024;
		BindLocalPort = 0;
		IncludeGeneralBroadcast = true;
		ScanPerformance = ScanPerformanceProfile.Balanced;
		ProtocolMode = RadioDiscoveryProtocolMode.Auto;
		FixedTargetIp = null;
		FixedLocalIp = null;
	}

	private void applyScanPerformanceProfile(ScanPerformanceProfile profile)
	{
		switch (profile)
		{
		case ScanPerformanceProfile.UltraFast:
			AttemptsPerNic = 1;
			QuietPollsBeforeResend = 2;
			PollTimeoutMilliseconds = 40;
			break;
		case ScanPerformanceProfile.VeryFast:
			AttemptsPerNic = 1;
			QuietPollsBeforeResend = 3;
			PollTimeoutMilliseconds = 60;
			break;
		case ScanPerformanceProfile.Fast:
			AttemptsPerNic = 2;
			QuietPollsBeforeResend = 3;
			PollTimeoutMilliseconds = 80;
			break;
		case ScanPerformanceProfile.Balanced:
			AttemptsPerNic = 2;
			QuietPollsBeforeResend = 4;
			PollTimeoutMilliseconds = 100;
			break;
		case ScanPerformanceProfile.Safe:
			AttemptsPerNic = 3;
			QuietPollsBeforeResend = 4;
			PollTimeoutMilliseconds = 150;
			break;
		case ScanPerformanceProfile.VeryTolerant:
			AttemptsPerNic = 3;
			QuietPollsBeforeResend = 6;
			PollTimeoutMilliseconds = 300;
			break;
		default:
			AttemptsPerNic = 2;
			QuietPollsBeforeResend = 4;
			PollTimeoutMilliseconds = 100;
			break;
		}
	}
}
