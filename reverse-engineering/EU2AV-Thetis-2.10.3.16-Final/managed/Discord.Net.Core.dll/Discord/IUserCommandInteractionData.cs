namespace Discord;

public interface IUserCommandInteractionData : IApplicationCommandInteractionData, IDiscordInteractionData
{
	IUser User { get; }
}
