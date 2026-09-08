namespace Discord;

public class TextInputComponent : IInteractableComponent, IMessageComponent
{
	public ComponentType Type => ComponentType.TextInput;

	public string CustomId { get; }

	public int? Id { get; }

	public string Label { get; }

	public string Placeholder { get; }

	public int? MinLength { get; }

	public int? MaxLength { get; }

	public TextInputStyle Style { get; }

	public bool? Required { get; }

	public string Value { get; }

	public TextInputBuilder ToBuilder()
	{
		return new TextInputBuilder(this);
	}

	internal TextInputComponent(string customId, string label, string placeholder, int? minLength, int? maxLength, TextInputStyle style, bool? required, string value, int? id)
	{
		CustomId = customId;
		Label = label;
		Placeholder = placeholder;
		MinLength = minLength;
		MaxLength = maxLength;
		Style = style;
		Required = required;
		Value = value;
		Id = id;
	}

	IMessageComponentBuilder IMessageComponent.ToBuilder()
	{
		return ToBuilder();
	}
}
