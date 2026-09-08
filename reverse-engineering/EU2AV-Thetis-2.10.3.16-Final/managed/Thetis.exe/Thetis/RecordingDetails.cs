using System;

namespace Thetis;

public sealed class RecordingDetails
{
	public DateTime UtcTime { get; set; }

	public string Frequency { get; set; } = "";

	public string Mode { get; set; } = "";

	public string Band { get; set; } = "";

	public string WavFile { get; set; } = "";

	public long? WavFileSizeBytes { get; set; }

	public DateTime? WavFileLastWriteUtc { get; set; }

	public double? PlayDurationSeconds { get; set; }

	public int SampleRate { get; set; }

	public short BitDepth { get; set; }

	public short Channels { get; set; }

	public short FormatTag { get; set; }

	public string Mp3File { get; set; } = "";

	public long? Mp3FileSizeBytes { get; set; }
}
