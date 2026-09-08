namespace Thetis;

public sealed class DiscoveryDiagnostics
{
	public long DurationMilliseconds { get; set; }

	public int AttemptsUsed { get; set; }

	public int Polls { get; set; }

	public int QuietPolls { get; set; }

	public int DiscoverySends { get; set; }

	public int DiscoveryReceives { get; set; }

	public int UniqueRadios { get; set; }

	public int RejectedSubnet { get; set; }

	public int RejectedDuplicate { get; set; }

	public int RejectedMacInvalid { get; set; }

	public int RejectedFixedTargetMismatch { get; set; }

	public int RejectedProtocolModeMismatch { get; set; }

	public bool SocketError { get; set; }
}
