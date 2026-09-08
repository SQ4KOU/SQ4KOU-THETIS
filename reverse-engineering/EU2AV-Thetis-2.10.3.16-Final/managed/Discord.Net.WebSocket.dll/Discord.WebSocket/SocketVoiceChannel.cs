using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Audio;
using Discord.Rest;

namespace Discord.WebSocket;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class SocketVoiceChannel : SocketTextChannel, IVoiceChannel, ITextChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IMentionable, INestedChannel, IGuildChannel, IDeletable, IIntegrationChannel, IAudioChannel, ISocketAudioChannel
{
	[Obsolete("This property is no longer used because Discord enabled text-in-voice for all channels.")]
	public virtual bool IsTextInVoice => true;

	public int Bitrate { get; private set; }

	public int? UserLimit { get; private set; }

	public string RTCRegion { get; private set; }

	public VideoQualityMode VideoQualityMode { get; private set; }

	public virtual string Status { get; private set; }

	public IReadOnlyCollection<SocketGuildUser> ConnectedUsers => base.Guild.Users.Where((SocketGuildUser x) => x.VoiceChannel?.Id == base.Id).ToImmutableArray();

	private string DebuggerDisplay => $"{base.Name} ({base.Id}, Voice)";

	internal SocketVoiceChannel(DiscordSocketClient discord, ulong id, SocketGuild guild)
		: base(discord, id, guild)
	{
	}

	internal new static SocketVoiceChannel Create(SocketGuild guild, ClientState state, Channel model)
	{
		SocketVoiceChannel socketVoiceChannel = new SocketVoiceChannel(guild?.Discord, model.Id, guild);
		socketVoiceChannel.Update(state, model);
		return socketVoiceChannel;
	}

	internal void UpdateVoiceStatus(string status)
	{
		Status = status;
	}

	internal override void Update(ClientState state, Channel model)
	{
		base.Update(state, model);
		Bitrate = model.Bitrate.GetValueOrDefault(64000);
		UserLimit = ((model.UserLimit.GetValueOrDefault() != 0) ? new int?(model.UserLimit.Value) : ((int?)null));
		VideoQualityMode = model.VideoQualityMode.GetValueOrDefault(VideoQualityMode.Auto);
		RTCRegion = model.RTCRegion.GetValueOrDefault(null);
		Status = model.Status.GetValueOrDefault(null);
	}

	public virtual Task SetStatusAsync(string status, RequestOptions options = null)
	{
		return ChannelHelper.ModifyVoiceChannelStatusAsync(this, status, base.Discord, options);
	}

	public Task ModifyAsync(Action<VoiceChannelProperties> func, RequestOptions options = null)
	{
		return ChannelHelper.ModifyAsync(this, base.Discord, func, options);
	}

	public Task<IAudioClient> ConnectAsync(bool selfDeaf = false, bool selfMute = false, bool external = false, bool disconnect = true)
	{
		return base.Guild.ConnectAudioAsync(base.Id, selfDeaf, selfMute, external, disconnect);
	}

	public Task DisconnectAsync()
	{
		return base.Guild.DisconnectAudioAsync();
	}

	public Task ModifyAsync(Action<AudioChannelProperties> func, RequestOptions options = null)
	{
		return base.Guild.ModifyAudioAsync(base.Id, func, options);
	}

	public override SocketGuildUser GetUser(ulong id)
	{
		SocketGuildUser user = base.Guild.GetUser(id);
		if (user?.VoiceChannel?.Id == base.Id)
		{
			return user;
		}
		return null;
	}

	public override Task<SocketThreadChannel> CreateThreadAsync(string name, ThreadType type = ThreadType.PublicThread, ThreadArchiveDuration autoArchiveDuration = ThreadArchiveDuration.OneDay, IMessage message = null, bool? invitable = null, int? slowmode = null, RequestOptions options = null)
	{
		throw new InvalidOperationException("Voice channels cannot contain threads.");
	}

	public override Task<IReadOnlyCollection<RestThreadChannel>> GetActiveThreadsAsync(RequestOptions options = null)
	{
		throw new NotSupportedException("Threads are not supported in voice channels");
	}

	internal new SocketVoiceChannel Clone()
	{
		return MemberwiseClone() as SocketVoiceChannel;
	}

	Task<IGuildUser> IGuildChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
	{
		return Task.FromResult((IGuildUser)GetUser(id));
	}

	IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
	{
		return ImmutableArray.Create((IReadOnlyCollection<IGuildUser>)Users).ToAsyncEnumerable();
	}

	Task<ICategoryChannel> INestedChannel.GetCategoryAsync(CacheMode mode, RequestOptions options)
	{
		return Task.FromResult(base.Category);
	}
}
