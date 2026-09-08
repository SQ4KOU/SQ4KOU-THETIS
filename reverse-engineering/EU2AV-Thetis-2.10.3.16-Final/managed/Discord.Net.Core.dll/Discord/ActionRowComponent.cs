using System.Collections.Generic;

namespace Discord;

public class ActionRowComponent : INestedComponent, IMessageComponent
{
	public ComponentType Type => ComponentType.ActionRow;

	public int? Id { get; internal set; }

	public IReadOnlyCollection<IMessageComponent> Components { get; internal set; }

	public ActionRowBuilder ToBuilder()
	{
		return new ActionRowBuilder(this);
	}

	internal ActionRowComponent()
	{
	}

	internal ActionRowComponent(IReadOnlyCollection<IMessageComponent> components, int? id)
	{
		Components = components;
		Id = id;
	}

	IMessageComponentBuilder IMessageComponent.ToBuilder()
	{
		return ToBuilder();
	}
}
