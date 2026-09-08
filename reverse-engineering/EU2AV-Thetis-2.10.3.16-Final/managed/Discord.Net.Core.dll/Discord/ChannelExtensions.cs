namespace Discord;

public static class ChannelExtensions
{
	public static ChannelType? GetChannelType(this IChannel channel)
	{
		if (!(channel is IStageChannel))
		{
			if (!(channel is IThreadChannel threadChannel))
			{
				if (!(channel is ICategoryChannel))
				{
					if (!(channel is IDMChannel))
					{
						if (!(channel is IGroupChannel))
						{
							if (!(channel is INewsChannel))
							{
								if (!(channel is IVoiceChannel))
								{
									if (!(channel is ITextChannel))
									{
										if (!(channel is IMediaChannel))
										{
											if (channel is IForumChannel)
											{
												return ChannelType.Forum;
											}
											return null;
										}
										return ChannelType.Media;
									}
									return ChannelType.Text;
								}
								return ChannelType.Voice;
							}
							return ChannelType.News;
						}
						return ChannelType.Group;
					}
					return ChannelType.DM;
				}
				return ChannelType.Category;
			}
			return threadChannel.Type switch
			{
				ThreadType.NewsThread => ChannelType.NewsThread, 
				ThreadType.PrivateThread => ChannelType.PrivateThread, 
				ThreadType.PublicThread => ChannelType.PublicThread, 
				_ => null, 
			};
		}
		return ChannelType.Stage;
	}
}
