namespace Discord;

public interface ISlashCommandInteraction : IApplicationCommandInteraction, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	new IApplicationCommandInteractionData Data { get; }
}
