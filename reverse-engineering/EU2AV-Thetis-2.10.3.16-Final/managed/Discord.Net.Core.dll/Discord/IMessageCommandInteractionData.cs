namespace Discord;

public interface IMessageCommandInteractionData : IApplicationCommandInteractionData, IDiscordInteractionData
{
	IMessage Message { get; }
}
