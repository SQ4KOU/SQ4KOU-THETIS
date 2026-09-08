using System;
using System.Threading.Tasks;

namespace Discord;

public interface IVoiceChannel : ITextChannel, IMessageChannel, IChannel, ISnowflakeEntity, IEntity<ulong>, IMentionable, INestedChannel, IGuildChannel, IDeletable, IIntegrationChannel, IAudioChannel
{
	int Bitrate { get; }

	int? UserLimit { get; }

	VideoQualityMode VideoQualityMode { get; }

	Task ModifyAsync(Action<VoiceChannelProperties> func, RequestOptions options = null);

	Task SetStatusAsync(string status, RequestOptions options = null);
}
