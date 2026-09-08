using System.Collections.Generic;

namespace Discord;

public class ContainerComponent : INestedComponent, IMessageComponent
{
	public ComponentType Type => ComponentType.Container;

	public int? Id { get; }

	public IReadOnlyCollection<IMessageComponent> Components { get; }

	public Color? AccentColor { get; }

	public bool? IsSpoiler { get; }

	public ContainerBuilder ToBuilder()
	{
		return new ContainerBuilder(this);
	}

	internal ContainerComponent(IReadOnlyCollection<IMessageComponent> components, Color? accentColor, bool? isSpoiler, int? id = null)
	{
		Components = components;
		AccentColor = accentColor;
		IsSpoiler = isSpoiler;
		Id = id;
	}

	IMessageComponentBuilder IMessageComponent.ToBuilder()
	{
		return ToBuilder();
	}
}
