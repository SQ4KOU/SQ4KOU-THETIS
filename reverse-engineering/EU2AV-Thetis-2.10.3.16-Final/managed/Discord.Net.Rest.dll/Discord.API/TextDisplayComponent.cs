using Newtonsoft.Json;

namespace Discord.API;

internal class TextDisplayComponent : IMessageComponent
{
	[JsonProperty("type")]
	public ComponentType Type { get; set; }

	[JsonProperty("id")]
	public Optional<int> Id { get; set; }

	[JsonProperty("content")]
	public string Content { get; set; }

	int? IMessageComponent.Id => Id.ToNullable();

	public TextDisplayComponent()
	{
	}

	public TextDisplayComponent(Discord.TextDisplayComponent component)
	{
		Type = component.Type;
		Id = ((Optional<int>?)component.Id) ?? Optional<int>.Unspecified;
		Content = component.Content;
	}

	IMessageComponentBuilder IMessageComponent.ToBuilder()
	{
		return null;
	}
}
