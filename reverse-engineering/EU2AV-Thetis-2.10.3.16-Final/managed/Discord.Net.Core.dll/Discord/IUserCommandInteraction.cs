namespace Discord;

public interface IUserCommandInteraction : IApplicationCommandInteraction, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	new IUserCommandInteractionData Data { get; }
}
