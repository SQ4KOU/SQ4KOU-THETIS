using System;
using Discord.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Discord.Net.Converters;

internal class MessageComponentConverter : JsonConverter
{
	public static MessageComponentConverter Instance => new MessageComponentConverter();

	public override bool CanRead => true;

	public override bool CanWrite => false;

	public override bool CanConvert(Type objectType)
	{
		return true;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		serializer.Serialize(writer, value);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		IMessageComponent messageComponent = null;
		switch ((ComponentType)jObject["type"].Value<int>())
		{
		case ComponentType.ActionRow:
			messageComponent = new Discord.API.ActionRowComponent();
			break;
		case ComponentType.Button:
			messageComponent = new Discord.API.ButtonComponent();
			break;
		case ComponentType.SelectMenu:
		case ComponentType.UserSelect:
		case ComponentType.RoleSelect:
		case ComponentType.MentionableSelect:
		case ComponentType.ChannelSelect:
			messageComponent = new Discord.API.SelectMenuComponent();
			break;
		case ComponentType.TextInput:
			messageComponent = new Discord.API.TextInputComponent();
			break;
		case ComponentType.TextDisplay:
			messageComponent = new Discord.API.TextDisplayComponent();
			break;
		case ComponentType.Thumbnail:
			messageComponent = new Discord.API.ThumbnailComponent();
			break;
		case ComponentType.Section:
			messageComponent = new Discord.API.SectionComponent();
			break;
		case ComponentType.MediaGallery:
			messageComponent = new Discord.API.MediaGalleryComponent();
			break;
		case ComponentType.Separator:
			messageComponent = new Discord.API.SeparatorComponent();
			break;
		case ComponentType.File:
			messageComponent = new Discord.API.FileComponent();
			break;
		case ComponentType.Container:
			messageComponent = new Discord.API.ContainerComponent();
			break;
		}
		serializer.Populate(jObject.CreateReader(), messageComponent);
		return messageComponent;
	}
}
