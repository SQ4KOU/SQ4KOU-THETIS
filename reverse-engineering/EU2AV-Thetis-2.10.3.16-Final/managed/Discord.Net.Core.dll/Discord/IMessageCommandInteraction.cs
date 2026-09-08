namespace Discord;

public interface IMessageCommandInteraction : IApplicationCommandInteraction, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	new IMessageCommandInteractionData Data { get; }
}
