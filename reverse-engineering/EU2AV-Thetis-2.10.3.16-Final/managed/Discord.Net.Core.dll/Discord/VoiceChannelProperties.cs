namespace Discord;

public class VoiceChannelProperties : TextChannelProperties
{
	public Optional<int> Bitrate { get; set; }

	public Optional<int?> UserLimit { get; set; }

	public Optional<string> RTCRegion { get; set; }

	public Optional<VideoQualityMode> VideoQualityMode { get; set; }

	public new Optional<string> Topic { get; }
}
