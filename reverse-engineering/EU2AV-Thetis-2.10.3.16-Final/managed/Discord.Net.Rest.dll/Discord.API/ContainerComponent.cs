using System.Linq;
using Discord.Rest;
using Newtonsoft.Json;

namespace Discord.API;

internal class ContainerComponent : IMessageComponent
{
	[JsonProperty("type")]
	public ComponentType Type { get; set; }

	[JsonProperty("id")]
	public Optional<int> Id { get; set; }

	[JsonProperty("accent_color")]
	public Optional<uint?> AccentColor { get; set; }

	[JsonProperty("spoiler")]
	public Optional<bool> IsSpoiler { get; set; }

	[JsonProperty("components")]
	public IMessageComponent[] Components { get; set; }

	int? IMessageComponent.Id => Id.ToNullable();

	public ContainerComponent()
	{
	}

	public ContainerComponent(Discord.ContainerComponent component)
	{
		Type = component.Type;
		Id = ((Optional<int>?)component.Id) ?? Optional<int>.Unspecified;
		uint? num = component.AccentColor?.RawValue;
		AccentColor = (num.HasValue ? ((Optional<uint?>)num.GetValueOrDefault()) : Optional<uint?>.Unspecified);
		IsSpoiler = ((Optional<bool>?)component.IsSpoiler) ?? Optional<bool>.Unspecified;
		Components = component.Components.Select((IMessageComponent x) => x.ToModel()).ToArray();
	}

	IMessageComponentBuilder IMessageComponent.ToBuilder()
	{
		return null;
	}
}
