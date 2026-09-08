namespace Discord;

public class SeparatorComponent : IMessageComponent
{
	public ComponentType Type => ComponentType.Separator;

	public int? Id { get; }

	public bool? IsDivider { get; }

	public SeparatorSpacingSize? Spacing { get; }

	public SeparatorBuilder ToBuilder()
	{
		return new SeparatorBuilder(this);
	}

	internal SeparatorComponent(bool? isDivider, SeparatorSpacingSize? spacing, int? id = null)
	{
		IsDivider = isDivider;
		Spacing = spacing;
		Id = id;
	}

	IMessageComponentBuilder IMessageComponent.ToBuilder()
	{
		return ToBuilder();
	}
}
