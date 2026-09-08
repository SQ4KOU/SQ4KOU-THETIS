using System.Linq;
using Discord.Rest;
using Newtonsoft.Json;

namespace Discord.API;

internal class MediaGalleryComponent : IMessageComponent
{
	[JsonProperty("type")]
	public ComponentType Type { get; set; }

	[JsonProperty("id")]
	public Optional<int> Id { get; set; }

	[JsonProperty("items")]
	public MediaGalleryItem[] Items { get; set; }

	int? IMessageComponent.Id => Id.ToNullable();

	public MediaGalleryComponent()
	{
	}

	public MediaGalleryComponent(Discord.MediaGalleryComponent component)
	{
		Type = component.Type;
		Id = ((Optional<int>?)component.Id) ?? Optional<int>.Unspecified;
		Items = component.Items.Select((Discord.MediaGalleryItem x) => new MediaGalleryItem
		{
			Description = x.Description,
			IsSpoiler = x.IsSpoiler,
			Media = x.Media.ToModel()
		}).ToArray();
	}

	IMessageComponentBuilder IMessageComponent.ToBuilder()
	{
		return null;
	}
}
