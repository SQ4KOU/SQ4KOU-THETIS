namespace Discord;

public interface IApplicationCommandInteraction : IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	new IApplicationCommandInteractionData Data { get; }
}
