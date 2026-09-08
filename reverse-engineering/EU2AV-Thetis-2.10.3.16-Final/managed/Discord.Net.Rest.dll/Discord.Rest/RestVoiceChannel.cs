using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Audio;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class RestVoiceChannel : RestTextChannel, IVoiceChannel, ITextChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IMentionable, INestedChannel, IGuildChannel, IDeletable, IIntegrationChannel, IAudioChannel, IRestAudioChannel
{
	[Obsolete("This property is no longer used because Discord enabled text-in-voice for all channels.")]
	public virtual bool IsTextInVoice => true;

	public int Bitrate { get; private set; }

	public int? UserLimit { get; private set; }

	public string RTCRegion { get; private set; }

	public VideoQualityMode VideoQualityMode { get; private set; }

	private string DebuggerDisplay => $"{base.Name} ({base.Id}, Voice)";

	internal RestVoiceChannel(BaseDiscordClient discord, IGuild guild, ulong id, ulong guildId)
		: base(discord, guild, id, guildId)
	{
	}

	internal new static RestVoiceChannel Create(BaseDiscordClient discord, IGuild guild, Channel model)
	{
		RestVoiceChannel restVoiceChannel = new RestVoiceChannel(discord, guild, model.Id, guild?.Id ?? model.GuildId.Value);
		restVoiceChannel.Update(model);
		return restVoiceChannel;
	}

	internal override void Update(Channel model)
	{
		base.Update(model);
		if (model.Bitrate.IsSpecified)
		{
			Bitrate = model.Bitrate.Value;
		}
		if (model.UserLimit.IsSpecified)
		{
			UserLimit = ((model.UserLimit.Value != 0) ? new int?(model.UserLimit.Value) : ((int?)null));
		}
		VideoQualityMode = model.VideoQualityMode.GetValueOrDefault(VideoQualityMode.Auto);
		RTCRegion = model.RTCRegion.GetValueOrDefault(null);
	}

	public async Task ModifyAsync(Action<VoiceChannelProperties> func, RequestOptions options = null)
	{
		Update(await ChannelHelper.ModifyAsync(this, base.Discord, func, options).ConfigureAwait(continueOnCapturedContext: false));
	}

	public override Task<RestThreadChannel> CreateThreadAsync(string name, ThreadType type = ThreadType.PublicThread, ThreadArchiveDuration autoArchiveDuration = ThreadArchiveDuration.OneDay, IMessage message = null, bool? invitable = null, int? slowmode = null, RequestOptions options = null)
	{
		throw new InvalidOperationException("Cannot create a thread within a voice channel");
	}

	public virtual Task SetStatusAsync(string status, RequestOptions options = null)
	{
		return ChannelHelper.ModifyVoiceChannelStatusAsync(this, status, base.Discord, options);
	}

	public override Task<IReadOnlyCollection<RestThreadChannel>> GetActiveThreadsAsync(RequestOptions options = null)
	{
		throw new NotSupportedException("Threads are not supported in voice channels");
	}

	Task<IAudioClient> IAudioChannel.ConnectAsync(bool selfDeaf, bool selfMute, bool external, bool disconnect)
	{
		throw new NotSupportedException();
	}

	Task IAudioChannel.DisconnectAsync()
	{
		throw new NotSupportedException();
	}

	Task IAudioChannel.ModifyAsync(Action<AudioChannelProperties> func, RequestOptions options)
	{
		throw new NotSupportedException();
	}

	Task<IGuildUser> IGuildChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult<IGuildUser>(null);
	}

	IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
	{
		return AsyncEnumerable.Empty<IReadOnlyCollection<IGuildUser>>();
	}

	async Task<ICategoryChannel> INestedChannel.GetCategoryAsync(CacheMode mode, RequestOptions options)
	{
		if (base.CategoryId.HasValue && mode == CacheMode.AllowDownload)
		{
			return (await base.Guild.GetChannelAsync(base.CategoryId.Value, mode, options).ConfigureAwait(continueOnCapturedContext: false)) as ICategoryChannel;
		}
		return null;
	}
}
