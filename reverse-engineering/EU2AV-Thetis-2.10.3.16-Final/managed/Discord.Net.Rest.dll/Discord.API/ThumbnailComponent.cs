using Discord.Rest;
using Newtonsoft.Json;

namespace Discord.API;

internal class ThumbnailComponent : IMessageComponent
{
	[JsonProperty("type")]
	public ComponentType Type { get; set; }

	[JsonProperty("id")]
	public Optional<int> Id { get; set; }

	[JsonProperty("media")]
	public UnfurledMediaItem Media { get; set; }

	[JsonProperty("description")]
	public Optional<string> Description { get; set; }

	[JsonProperty("spoiler")]
	public Optional<bool> IsSpoiler { get; set; }

	int? IMessageComponent.Id => Id.ToNullable();

	public ThumbnailComponent()
	{
	}

	public ThumbnailComponent(Discord.ThumbnailComponent component)
	{
		Type = component.Type;
		Id = ((Optional<int>?)component.Id) ?? Optional<int>.Unspecified;
		Media = component.Media.ToModel();
		Description = component.Description;
		IsSpoiler = component.IsSpoiler;
	}

	IMessageComponentBuilder IMessageComponent.ToBuilder()
	{
		return null;
	}
}
