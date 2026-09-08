using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord;

public interface IUserMessage : IMessage, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	MessageResolvedData ResolvedData { get; }

	IUserMessage ReferencedMessage { get; }

	IMessageInteractionMetadata InteractionMetadata { get; }

	IReadOnlyCollection<MessageSnapshot> ForwardedMessages { get; }

	Poll? Poll { get; }

	Task ModifyAsync(Action<MessageProperties> func, RequestOptions options = null);

	Task PinAsync(RequestOptions options = null);

	Task UnpinAsync(RequestOptions options = null);

	Task CrosspostAsync(RequestOptions options = null);

	string Resolve(TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name, TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name);

	Task EndPollAsync(RequestOptions options);

	IAsyncEnumerable<IReadOnlyCollection<IUser>> GetPollAnswerVotersAsync(uint answerId, int? limit = null, ulong? afterId = null, RequestOptions options = null);
}
