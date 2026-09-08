using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Discord.API;
using Discord.Rest;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Attachment : IAttachment, ISnowflakeEntity, IEntity<ulong>
{
	public ulong Id { get; }

	public string Filename { get; }

	public string Url { get; }

	public string ProxyUrl { get; }

	public int Size { get; }

	public int? Height { get; }

	public int? Width { get; }

	public bool Ephemeral { get; }

	public string Description { get; }

	public string ContentType { get; }

	public string Waveform { get; }

	public byte[] WaveformBytes { get; }

	public double? Duration { get; }

	public IReadOnlyCollection<RestUser> ClipParticipants { get; }

	public string Title { get; }

	public DateTimeOffset? ClipCreatedAt { get; }

	public AttachmentFlags Flags { get; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);

	private string DebuggerDisplay => $"{Filename} ({Size} bytes)";

	IReadOnlyCollection<IUser> IAttachment.ClipParticipants => ClipParticipants;

	internal Attachment(ulong id, string filename, string url, string proxyUrl, int size, int? height, int? width, bool? ephemeral, string description, string contentType, double? duration, string waveform, AttachmentFlags flags, string title, IReadOnlyCollection<RestUser> clipParticipants, DateTimeOffset? clipCreatedAt)
	{
		Id = id;
		Filename = filename;
		Url = url;
		ProxyUrl = proxyUrl;
		Size = size;
		Height = height;
		Width = width;
		Ephemeral = ephemeral ?? false;
		Description = description;
		ContentType = contentType;
		Duration = duration;
		Waveform = waveform;
		Flags = flags;
		Title = title;
		ClipParticipants = clipParticipants;
		ClipCreatedAt = clipCreatedAt;
		if (waveform != null)
		{
			WaveformBytes = Convert.FromBase64String(waveform);
		}
	}

	internal static Attachment Create(Discord.API.Attachment model, BaseDiscordClient discord)
	{
		return new Attachment(model.Id, model.Filename, model.Url, model.ProxyUrl, model.Size, model.Height.IsSpecified ? new int?(model.Height.Value) : ((int?)null), model.Width.IsSpecified ? new int?(model.Width.Value) : ((int?)null), model.Ephemeral.ToNullable(), model.Description.GetValueOrDefault(), model.ContentType.GetValueOrDefault(), model.DurationSeconds.IsSpecified ? new double?(model.DurationSeconds.Value) : ((double?)null), model.Waveform.GetValueOrDefault(null), model.Flags.GetValueOrDefault(AttachmentFlags.None), model.Title.GetValueOrDefault(null), (from x in model.ClipParticipants.GetValueOrDefault(Array.Empty<User>())
			select RestUser.Create(discord, x)).ToImmutableArray(), model.ClipCreatedAt.IsSpecified ? new DateTimeOffset?(model.ClipCreatedAt.Value) : ((DateTimeOffset?)null));
	}

	public override string ToString()
	{
		return Filename;
	}
}
