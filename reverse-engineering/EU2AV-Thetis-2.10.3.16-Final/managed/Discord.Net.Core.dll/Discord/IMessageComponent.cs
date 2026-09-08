namespace Discord;

public interface IMessageComponent
{
	ComponentType Type { get; }

	int? Id { get; }

	IMessageComponentBuilder ToBuilder();
}
