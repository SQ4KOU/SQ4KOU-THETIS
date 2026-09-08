using System;
using System.Collections.Generic;

namespace Discord;

public interface IAttachment : ISnowflakeEntity, IEntity<ulong>
{
	string Filename { get; }

	string Url { get; }

	string ProxyUrl { get; }

	int Size { get; }

	int? Height { get; }

	int? Width { get; }

	bool Ephemeral { get; }

	string Description { get; }

	string ContentType { get; }

	double? Duration { get; }

	string Waveform { get; }

	byte[] WaveformBytes { get; }

	AttachmentFlags Flags { get; }

	IReadOnlyCollection<IUser> ClipParticipants { get; }

	string Title { get; }

	DateTimeOffset? ClipCreatedAt { get; }
}
