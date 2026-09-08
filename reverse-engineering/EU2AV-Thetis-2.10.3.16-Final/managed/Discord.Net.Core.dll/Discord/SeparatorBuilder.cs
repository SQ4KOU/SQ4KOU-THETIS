namespace Discord;

public class SeparatorBuilder : IMessageComponentBuilder
{
	public ComponentType Type => ComponentType.Separator;

	public bool? IsDivider { get; set; }

	public SeparatorSpacingSize? Spacing { get; set; }

	public int? Id { get; set; }

	public SeparatorBuilder(bool isDivider = true, SeparatorSpacingSize spacing = SeparatorSpacingSize.Small)
	{
		IsDivider = isDivider;
		Spacing = spacing;
	}

	public SeparatorBuilder(SeparatorComponent separator)
	{
		IsDivider = separator.IsDivider;
		Spacing = separator.Spacing;
		Id = separator.Id;
	}

	public SeparatorBuilder WithIsDivider(bool? isDivider)
	{
		IsDivider = isDivider;
		return this;
	}

	public SeparatorBuilder WithSpacing(SeparatorSpacingSize? spacing)
	{
		Spacing = spacing;
		return this;
	}

	public SeparatorComponent Build()
	{
		return new SeparatorComponent(IsDivider, Spacing, Id);
	}

	IMessageComponent IMessageComponentBuilder.Build()
	{
		return Build();
	}
}
