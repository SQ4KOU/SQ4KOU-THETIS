using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal static class MessageComponentExtension
{
	internal static IMessageComponent ToModel(this IMessageComponent component)
	{
		if (!(component is ActionRowComponent c))
		{
			if (!(component is ButtonComponent c2))
			{
				if (!(component is SelectMenuComponent component2))
				{
					if (!(component is TextInputComponent component3))
					{
						if (!(component is TextDisplayComponent component4))
						{
							if (!(component is SectionComponent component5))
							{
								if (!(component is ThumbnailComponent component6))
								{
									if (!(component is MediaGalleryComponent component7))
									{
										if (!(component is SeparatorComponent component8))
										{
											if (!(component is FileComponent component9))
											{
												if (component is ContainerComponent component10)
												{
													return new Discord.API.ContainerComponent(component10);
												}
												return null;
											}
											return new Discord.API.FileComponent(component9);
										}
										return new Discord.API.SeparatorComponent(component8);
									}
									return new Discord.API.MediaGalleryComponent(component7);
								}
								return new Discord.API.ThumbnailComponent(component6);
							}
							return new Discord.API.SectionComponent(component5);
						}
						return new Discord.API.TextDisplayComponent(component4);
					}
					return new Discord.API.TextInputComponent(component3);
				}
				return new Discord.API.SelectMenuComponent(component2);
			}
			return new Discord.API.ButtonComponent(c2);
		}
		return new Discord.API.ActionRowComponent(c);
	}

	internal static IMessageComponent ToEntity(this IMessageComponent component)
	{
		switch (component.Type)
		{
		case ComponentType.ActionRow:
		{
			Discord.API.ActionRowComponent actionRowComponent = (Discord.API.ActionRowComponent)component;
			return new ActionRowComponent
			{
				Id = component.Id,
				Components = actionRowComponent.Components.Select((IMessageComponent x) => x.ToEntity()).ToImmutableArray()
			};
		}
		case ComponentType.Button:
		{
			Discord.API.ButtonComponent buttonComponent = (Discord.API.ButtonComponent)component;
			ButtonStyle style = buttonComponent.Style;
			string valueOrDefault = buttonComponent.Label.GetValueOrDefault();
			object emote;
			if (!buttonComponent.Emote.IsSpecified)
			{
				emote = null;
			}
			else if (!buttonComponent.Emote.Value.Id.HasValue)
			{
				IEmote emote2 = new Emoji(buttonComponent.Emote.Value.Name);
				emote = emote2;
			}
			else
			{
				IEmote emote2 = new Emote(buttonComponent.Emote.Value.Id.Value, buttonComponent.Emote.Value.Name, buttonComponent.Emote.Value.Animated.GetValueOrDefault());
				emote = emote2;
			}
			return new ButtonComponent(style, valueOrDefault, (IEmote)emote, buttonComponent.CustomId.GetValueOrDefault(), buttonComponent.Url.GetValueOrDefault(), buttonComponent.Disabled.GetValueOrDefault(), buttonComponent.SkuId.ToNullable(), buttonComponent.Id.ToNullable());
		}
		case ComponentType.SelectMenu:
		case ComponentType.UserSelect:
		case ComponentType.RoleSelect:
		case ComponentType.MentionableSelect:
		case ComponentType.ChannelSelect:
		{
			Discord.API.SelectMenuComponent selectMenuComponent = (Discord.API.SelectMenuComponent)component;
			string customId = selectMenuComponent.CustomId;
			List<SelectMenuOption> options = selectMenuComponent.Options?.Select(delegate(Discord.API.SelectMenuOption z)
			{
				string label = z.Label;
				string value = z.Value;
				string valueOrDefault4 = z.Description.GetValueOrDefault();
				object emote3;
				if (!z.Emoji.IsSpecified)
				{
					emote3 = null;
				}
				else if (!z.Emoji.Value.Id.HasValue)
				{
					IEmote emote4 = new Emoji(z.Emoji.Value.Name);
					emote3 = emote4;
				}
				else
				{
					IEmote emote4 = new Emote(z.Emoji.Value.Id.Value, z.Emoji.Value.Name, z.Emoji.Value.Animated.GetValueOrDefault());
					emote3 = emote4;
				}
				return new SelectMenuOption(label, value, valueOrDefault4, (IEmote)emote3, z.Default.ToNullable());
			}).ToList();
			string valueOrDefault2 = selectMenuComponent.Placeholder.GetValueOrDefault();
			int minValues = selectMenuComponent.MinValues;
			int maxValues = selectMenuComponent.MaxValues;
			bool disabled = selectMenuComponent.Disabled;
			ComponentType type = selectMenuComponent.Type;
			int? id = selectMenuComponent.Id.ToNullable();
			ChannelType[] valueOrDefault3 = selectMenuComponent.ChannelTypes.GetValueOrDefault();
			IEnumerable<SelectMenuDefaultValue> defaultValues;
			if (!selectMenuComponent.DefaultValues.IsSpecified)
			{
				IEnumerable<SelectMenuDefaultValue> enumerable = Array.Empty<SelectMenuDefaultValue>();
				defaultValues = enumerable;
			}
			else
			{
				defaultValues = selectMenuComponent.DefaultValues.Value.Select((Discord.API.SelectMenuDefaultValue x) => new SelectMenuDefaultValue(x.Id, x.Type));
			}
			return new SelectMenuComponent(customId, options, valueOrDefault2, minValues, maxValues, disabled, type, id, valueOrDefault3, defaultValues);
		}
		case ComponentType.TextInput:
		{
			Discord.API.TextInputComponent textInputComponent = (Discord.API.TextInputComponent)component;
			return new TextInputComponent(textInputComponent.CustomId, textInputComponent.Label, textInputComponent.Placeholder.GetValueOrDefault(null), textInputComponent.MinLength.ToNullable(), textInputComponent.MaxLength.ToNullable(), textInputComponent.Style, textInputComponent.Required.ToNullable(), textInputComponent.Value.GetValueOrDefault(null), textInputComponent.Id.ToNullable());
		}
		case ComponentType.TextDisplay:
		{
			Discord.API.TextDisplayComponent textDisplayComponent = (Discord.API.TextDisplayComponent)component;
			return new TextDisplayComponent(textDisplayComponent.Content, textDisplayComponent.Id.ToNullable());
		}
		case ComponentType.Section:
		{
			Discord.API.SectionComponent sectionComponent = (Discord.API.SectionComponent)component;
			return new SectionComponent(sectionComponent.Id.ToNullable(), sectionComponent.Components.Select((IMessageComponent x) => x.ToEntity()).ToImmutableArray(), sectionComponent.Accessory.ToEntity());
		}
		case ComponentType.Thumbnail:
		{
			Discord.API.ThumbnailComponent thumbnailComponent = (Discord.API.ThumbnailComponent)component;
			return new ThumbnailComponent(thumbnailComponent.Id.ToNullable(), thumbnailComponent.Media.ToEntity(), thumbnailComponent.Description.GetValueOrDefault(null), thumbnailComponent.IsSpoiler.ToNullable());
		}
		case ComponentType.MediaGallery:
		{
			Discord.API.MediaGalleryComponent mediaGalleryComponent = (Discord.API.MediaGalleryComponent)component;
			return new MediaGalleryComponent(mediaGalleryComponent.Items.Select((Discord.API.MediaGalleryItem x) => new MediaGalleryItem(x.Media.ToEntity(), x.Description.GetValueOrDefault(null), x.IsSpoiler.GetValueOrDefault(defaultValue: false))).ToList(), mediaGalleryComponent.Id.ToNullable());
		}
		case ComponentType.Separator:
		{
			Discord.API.SeparatorComponent separatorComponent = (Discord.API.SeparatorComponent)component;
			return new SeparatorComponent(separatorComponent.IsDivider.ToNullable(), separatorComponent.Spacing.ToNullable(), separatorComponent.Id.ToNullable());
		}
		case ComponentType.File:
		{
			Discord.API.FileComponent fileComponent = (Discord.API.FileComponent)component;
			return new FileComponent(fileComponent.File.ToEntity(), fileComponent.IsSpoiler.ToNullable(), fileComponent.Id.ToNullable());
		}
		case ComponentType.Container:
		{
			Discord.API.ContainerComponent containerComponent = (Discord.API.ContainerComponent)component;
			return new ContainerComponent(containerComponent.Components.Select((IMessageComponent x) => x.ToEntity()).ToImmutableArray(), containerComponent.AccentColor.GetValueOrDefault(null), containerComponent.IsSpoiler.ToNullable(), containerComponent.Id.ToNullable());
		}
		default:
			return null;
		}
	}

	internal static UnfurledMediaItem ToEntity(this Discord.API.UnfurledMediaItem mediaItem)
	{
		return new ResolvedUnfurledMediaItem(mediaItem.Url, mediaItem.ProxyUrl.GetValueOrDefault(null), mediaItem.Height.GetValueOrDefault(0) ?? 0, mediaItem.Width.GetValueOrDefault(0) ?? 0, mediaItem.ContentType.GetValueOrDefault(null), mediaItem.LoadingState.GetValueOrDefault(UnfurledMediaItemLoadingState.Unknown));
	}

	internal static Discord.API.UnfurledMediaItem ToModel(this UnfurledMediaItem mediaItem)
	{
		return new Discord.API.UnfurledMediaItem
		{
			Url = mediaItem.Url
		};
	}
}
