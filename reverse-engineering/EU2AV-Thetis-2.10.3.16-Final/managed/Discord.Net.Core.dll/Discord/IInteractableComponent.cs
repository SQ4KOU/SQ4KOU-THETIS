namespace Discord;

public interface IInteractableComponent : IMessageComponent
{
	string CustomId { get; }
}
