namespace Discord;

public class ButtonComponent : IInteractableComponent, IMessageComponent
{
	public ComponentType Type => ComponentType.Button;

	public int? Id { get; }

	public ButtonStyle Style { get; }

	public string Label { get; }

	public IEmote Emote { get; }

	public string CustomId { get; }

	public string Url { get; }

	public bool IsDisabled { get; }

	public ulong? SkuId { get; }

	public ButtonBuilder ToBuilder()
	{
		return new ButtonBuilder(this);
	}

	internal ButtonComponent(ButtonStyle style, string label, IEmote emote, string customId, string url, bool isDisabled, ulong? skuId, int? id)
	{
		Style = style;
		Id = id;
		Label = label;
		Emote = emote;
		CustomId = customId;
		Url = url;
		IsDisabled = isDisabled;
		SkuId = skuId;
	}

	IMessageComponentBuilder IMessageComponent.ToBuilder()
	{
		return ToBuilder();
	}
}
