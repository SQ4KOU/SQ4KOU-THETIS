using System;
using System.Collections.Generic;
using System.Linq;

namespace Discord;

public static class ComponentContainerExtensions
{
	public static int ComponentCount(this IComponentContainer container)
	{
		int num = 0;
		foreach (IMessageComponentBuilder component in container.Components)
		{
			num++;
			if (component is SectionBuilder { Accessory: not null })
			{
				num++;
			}
			if (component is IComponentContainer container2)
			{
				num += container2.ComponentCount();
			}
		}
		return num;
	}

	public static BuilderT WithTextDisplay<BuilderT>(this BuilderT container, TextDisplayBuilder textDisplay) where BuilderT : class, IStaticComponentContainer
	{
		container.AddComponent(textDisplay);
		return container;
	}

	public static BuilderT WithTextDisplay<BuilderT>(this BuilderT container, string content, int? id = null) where BuilderT : class, IStaticComponentContainer
	{
		return container.WithTextDisplay(new TextDisplayBuilder().WithContent(content).WithId(id));
	}

	public static BuilderT WithTextDisplay<BuilderT>(this BuilderT container, Action<TextDisplayBuilder> options) where BuilderT : class, IStaticComponentContainer
	{
		TextDisplayBuilder textDisplayBuilder = new TextDisplayBuilder();
		options(textDisplayBuilder);
		return container.WithTextDisplay(textDisplayBuilder);
	}

	public static BuilderT WithSection<BuilderT>(this BuilderT container, SectionBuilder section) where BuilderT : class, IStaticComponentContainer
	{
		container.AddComponent(section);
		return container;
	}

	public static BuilderT WithSection<BuilderT>(this BuilderT container, Action<SectionBuilder> options) where BuilderT : class, IStaticComponentContainer
	{
		SectionBuilder sectionBuilder = new SectionBuilder();
		options(sectionBuilder);
		return container.WithSection(sectionBuilder);
	}

	public static BuilderT WithSection<BuilderT>(this BuilderT container, IEnumerable<TextDisplayBuilder> components, IMessageComponentBuilder accessory, bool isSpoiler = false, int? id = null) where BuilderT : class, IStaticComponentContainer
	{
		return container.WithSection(new SectionBuilder().WithComponents(components).WithAccessory(accessory).WithId(id));
	}

	public static BuilderT WithMediaGallery<BuilderT>(this BuilderT container, MediaGalleryBuilder mediaGallery) where BuilderT : class, IStaticComponentContainer
	{
		container.AddComponent(mediaGallery);
		return container;
	}

	public static BuilderT WithMediaGallery<BuilderT>(this BuilderT container, IEnumerable<MediaGalleryItemProperties> items, int? id = null) where BuilderT : class, IStaticComponentContainer
	{
		return container.WithMediaGallery(new MediaGalleryBuilder().WithItems(items).WithId(id));
	}

	public static BuilderT WithMediaGallery<BuilderT>(this BuilderT container, IEnumerable<string> urls, int? id = null) where BuilderT : class, IStaticComponentContainer
	{
		return container.WithMediaGallery(new MediaGalleryBuilder().WithItems(urls.Select((string x) => new MediaGalleryItemProperties(new UnfurledMediaItemProperties(x)))).WithId(id));
	}

	public static BuilderT WithMediaGallery<BuilderT>(this BuilderT container, Action<MediaGalleryBuilder> options) where BuilderT : class, IStaticComponentContainer
	{
		MediaGalleryBuilder mediaGalleryBuilder = new MediaGalleryBuilder();
		options(mediaGalleryBuilder);
		return container.WithMediaGallery(mediaGalleryBuilder);
	}

	public static BuilderT WithSeparator<BuilderT>(this BuilderT container, SeparatorBuilder separator) where BuilderT : class, IStaticComponentContainer
	{
		container.AddComponent(separator);
		return container;
	}

	public static BuilderT WithSeparator<BuilderT>(this BuilderT container, SeparatorSpacingSize spacing = SeparatorSpacingSize.Small, bool isDivider = true, int? id = null) where BuilderT : class, IStaticComponentContainer
	{
		return container.WithSeparator(new SeparatorBuilder().WithSpacing(spacing).WithIsDivider(isDivider).WithId(id));
	}

	public static BuilderT WithSeparator<BuilderT>(this BuilderT container, Action<SeparatorBuilder> options) where BuilderT : class, IStaticComponentContainer
	{
		SeparatorBuilder separatorBuilder = new SeparatorBuilder();
		options(separatorBuilder);
		return container.WithSeparator(separatorBuilder);
	}

	public static BuilderT WithFile<BuilderT>(this BuilderT container, FileComponentBuilder file) where BuilderT : class, IStaticComponentContainer
	{
		container.AddComponent(file);
		return container;
	}

	public static BuilderT WithFile<BuilderT>(this BuilderT container, UnfurledMediaItemProperties file, bool isSpoiler = false, int? id = null) where BuilderT : class, IStaticComponentContainer
	{
		return container.WithFile(new FileComponentBuilder().WithFile(file).WithIsSpoiler(isSpoiler).WithId(id));
	}

	public static BuilderT WithFile<BuilderT>(this BuilderT container, Action<FileComponentBuilder> options) where BuilderT : class, IStaticComponentContainer
	{
		FileComponentBuilder fileComponentBuilder = new FileComponentBuilder();
		options(fileComponentBuilder);
		return container.WithFile(fileComponentBuilder);
	}

	public static BuilderT WithContainer<BuilderT>(this BuilderT container, ContainerBuilder containerComponent) where BuilderT : class, IStaticComponentContainer
	{
		container.AddComponent(containerComponent);
		return container;
	}

	public static BuilderT WithContainer<BuilderT>(this BuilderT container, IEnumerable<IMessageComponentBuilder> components, Color? accentColor = null, bool isSpoiler = false, int? id = null) where BuilderT : class, IStaticComponentContainer
	{
		return container.WithContainer(new ContainerBuilder().WithComponents(components).WithAccentColor(accentColor).WithSpoiler(isSpoiler)
			.WithId(id));
	}

	public static BuilderT WithContainer<BuilderT>(this BuilderT container, params IMessageComponentBuilder[] components) where BuilderT : class, IStaticComponentContainer
	{
		return container.WithContainer(new ContainerBuilder().WithComponents(components));
	}

	public static BuilderT WithContainer<BuilderT>(this BuilderT container, Action<ContainerBuilder> options) where BuilderT : class, IStaticComponentContainer
	{
		ContainerBuilder containerBuilder = new ContainerBuilder();
		options(containerBuilder);
		return container.WithContainer(containerBuilder);
	}

	public static BuilderT WithButton<BuilderT>(this BuilderT container, ButtonBuilder button) where BuilderT : class, IInteractableComponentContainer
	{
		container.AddComponent(button);
		return container;
	}

	public static BuilderT WithButton<BuilderT>(this BuilderT container, string label = null, string customId = null, ButtonStyle style = ButtonStyle.Primary, IEmote emote = null, string url = null, bool disabled = false, ulong? skuId = null, int? id = null) where BuilderT : class, IInteractableComponentContainer
	{
		return container.WithButton(new ButtonBuilder().WithLabel(label).WithStyle(style).WithEmote(emote)
			.WithCustomId(customId)
			.WithUrl(url)
			.WithDisabled(disabled)
			.WithSkuId(skuId)
			.WithId(id));
	}

	public static BuilderT WithButton<BuilderT>(this BuilderT container, Action<ButtonBuilder> options) where BuilderT : class, IInteractableComponentContainer
	{
		ButtonBuilder buttonBuilder = new ButtonBuilder();
		options(buttonBuilder);
		return container.WithButton(buttonBuilder);
	}

	public static BuilderT WithSelectMenu<BuilderT>(this BuilderT container, SelectMenuBuilder selectMenu) where BuilderT : class, IInteractableComponentContainer
	{
		container.AddComponent(selectMenu);
		return container;
	}

	public static BuilderT WithSelectMenu<BuilderT>(this BuilderT container, string customId, List<SelectMenuOptionBuilder> options = null, string placeholder = null, int minValues = 1, int maxValues = 1, bool disabled = false, int row = 0, ComponentType type = ComponentType.SelectMenu, ChannelType[] channelTypes = null, SelectMenuDefaultValue[] defaultValues = null, int? id = null) where BuilderT : class, IInteractableComponentContainer
	{
		return container.WithSelectMenu(new SelectMenuBuilder().WithCustomId(customId).WithOptions(options).WithPlaceholder(placeholder)
			.WithMaxValues(maxValues)
			.WithMinValues(minValues)
			.WithDisabled(disabled)
			.WithType(type)
			.WithChannelTypes(channelTypes)
			.WithDefaultValues(defaultValues)
			.WithId(id));
	}

	public static BuilderT WithSelectMenu<BuilderT>(this BuilderT container, Action<SelectMenuBuilder> options) where BuilderT : class, IInteractableComponentContainer
	{
		SelectMenuBuilder selectMenuBuilder = new SelectMenuBuilder();
		options(selectMenuBuilder);
		return container.WithSelectMenu(selectMenuBuilder);
	}

	public static BuilderT WithActionRow<BuilderT>(this BuilderT container, ActionRowBuilder actionRow) where BuilderT : class, IStaticComponentContainer
	{
		container.AddComponent(actionRow);
		return container;
	}

	public static BuilderT WithActionRow<BuilderT>(this BuilderT container, IEnumerable<IMessageComponentBuilder> components, int? id = null) where BuilderT : class, IStaticComponentContainer
	{
		return container.WithActionRow(new ActionRowBuilder().WithComponents(components).WithId(id));
	}

	public static BuilderT WithActionRow<BuilderT>(this BuilderT container, Action<ActionRowBuilder> options) where BuilderT : class, IStaticComponentContainer
	{
		ActionRowBuilder actionRowBuilder = new ActionRowBuilder();
		options(actionRowBuilder);
		return container.WithActionRow(actionRowBuilder);
	}

	public static IMessageComponentBuilder FindComponentById(this IComponentContainer container, int id)
	{
		return container.FindComponentById<IMessageComponentBuilder>(id);
	}

	public static ComponentT FindComponentById<ComponentT>(this IComponentContainer container, int id) where ComponentT : class, IMessageComponentBuilder
	{
		if (container is ComponentT val && val.Id == id)
		{
			return val;
		}
		foreach (IMessageComponentBuilder component in container.Components)
		{
			if (component.Id == id && component is ComponentT result)
			{
				return result;
			}
			if (component is SectionBuilder sectionBuilder && sectionBuilder.Accessory.Id == id && sectionBuilder.Accessory is ComponentT result2)
			{
				return result2;
			}
			if (component is IComponentContainer container2)
			{
				ComponentT val2 = container2.FindComponentById<ComponentT>(id);
				if (val2 != null)
				{
					return val2;
				}
			}
		}
		return null;
	}

	public static IEnumerable<int> GetComponentIds(this IComponentContainer container)
	{
		return (from x in container.Components
			where x.Id.HasValue
			select x.Id.Value).Concat(container.Components.OfType<IComponentContainer>().SelectMany((IComponentContainer x) => x.GetComponentIds()));
	}

	public static IMessageComponent FindComponentById(this INestedComponent container, int id)
	{
		return container.FindComponentById<IMessageComponent>(id);
	}

	public static ComponentT FindComponentById<ComponentT>(this INestedComponent container, int id) where ComponentT : class, IMessageComponent
	{
		if (container is ComponentT val && val.Id == id)
		{
			return val;
		}
		return container.Components.FindComponentById<ComponentT>(id);
	}

	public static ComponentT FindComponentById<ComponentT>(this IEnumerable<IMessageComponent> components, int id) where ComponentT : class, IMessageComponent
	{
		foreach (IMessageComponent component in components)
		{
			if (component.Id == id && component is ComponentT result)
			{
				return result;
			}
			if (component is SectionComponent sectionComponent && sectionComponent.Accessory.Id == id && sectionComponent.Accessory is ComponentT result2)
			{
				return result2;
			}
			if (component is INestedComponent container)
			{
				ComponentT val = container.FindComponentById<ComponentT>(id);
				if (val != null)
				{
					return val;
				}
			}
		}
		return null;
	}

	public static IMessageComponent FindComponentById(this IEnumerable<IMessageComponent> components, int id)
	{
		return components.FindComponentById<IMessageComponent>(id);
	}
}
