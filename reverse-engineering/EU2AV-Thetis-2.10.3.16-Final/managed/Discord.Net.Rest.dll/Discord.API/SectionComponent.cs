using System.Linq;
using Discord.Rest;
using Newtonsoft.Json;

namespace Discord.API;

internal class SectionComponent : IMessageComponent
{
	[JsonProperty("type")]
	public ComponentType Type { get; set; }

	[JsonProperty("id")]
	public Optional<int> Id { get; set; }

	[JsonProperty("components")]
	public IMessageComponent[] Components { get; set; }

	[JsonProperty("accessory")]
	public IMessageComponent Accessory { get; set; }

	int? IMessageComponent.Id => Id.ToNullable();

	public SectionComponent()
	{
	}

	public SectionComponent(Discord.SectionComponent component)
	{
		Type = component.Type;
		Id = ((Optional<int>?)component.Id) ?? Optional<int>.Unspecified;
		Components = component.Components.Select((IMessageComponent x) => x.ToModel()).ToArray();
		Accessory = component.Accessory.ToModel();
	}

	IMessageComponentBuilder IMessageComponent.ToBuilder()
	{
		return null;
	}
}
