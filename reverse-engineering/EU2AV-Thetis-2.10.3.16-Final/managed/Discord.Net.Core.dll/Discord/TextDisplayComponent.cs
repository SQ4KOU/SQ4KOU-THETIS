namespace Discord;

public class TextDisplayComponent : IMessageComponent
{
	public ComponentType Type => ComponentType.TextDisplay;

	public int? Id { get; }

	public string Content { get; }

	public TextDisplayBuilder ToBuilder()
	{
		return new TextDisplayBuilder(this);
	}

	internal TextDisplayComponent(string content, int? id = null)
	{
		Id = id;
		Content = content;
	}

	IMessageComponentBuilder IMessageComponent.ToBuilder()
	{
		return ToBuilder();
	}
}
