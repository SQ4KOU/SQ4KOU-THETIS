using System;
using System.Threading.Tasks;
using Discord.Audio;

namespace Discord;

public interface IAudioChannel : IChannel, ISnowflakeEntity, IEntity<ulong>
{
	string RTCRegion { get; }

	Task<IAudioClient> ConnectAsync(bool selfDeaf = false, bool selfMute = false, bool external = false, bool disconnect = true);

	Task DisconnectAsync();

	Task ModifyAsync(Action<AudioChannelProperties> func, RequestOptions options = null);
}
