using Newtonsoft.Json;

namespace Discord.API;

internal class SeparatorComponent : IMessageComponent
{
	[JsonProperty("type")]
	public ComponentType Type { get; set; }

	[JsonProperty("id")]
	public Optional<int> Id { get; set; }

	[JsonProperty("divider")]
	public Optional<bool> IsDivider { get; set; }

	[JsonProperty("spacing")]
	public Optional<SeparatorSpacingSize> Spacing { get; set; }

	int? IMessageComponent.Id => Id.ToNullable();

	public SeparatorComponent()
	{
	}

	public SeparatorComponent(Discord.SeparatorComponent component)
	{
		Type = component.Type;
		Id = ((Optional<int>?)component.Id) ?? Optional<int>.Unspecified;
		IsDivider = ((Optional<bool>?)component.IsDivider) ?? Optional<bool>.Unspecified;
		Spacing = ((Optional<SeparatorSpacingSize>?)component.Spacing) ?? Optional<SeparatorSpacingSize>.Unspecified;
	}

	IMessageComponentBuilder IMessageComponent.ToBuilder()
	{
		return null;
	}
}
