using System.Collections.Immutable;
using Discord.API;

namespace Discord.Rest;

internal static class InteractionMetadataExtensions
{
	public static IMessageInteractionMetadata ToInteractionMetadata(this MessageInteractionMetadata metadata, BaseDiscordClient discord)
	{
		return metadata.Type switch
		{
			InteractionType.ApplicationCommand => new ApplicationCommandInteractionMetadata(metadata.Id, metadata.Type, metadata.User.Id, metadata.IntegrationOwners.ToImmutableDictionary(), metadata.OriginalResponseMessageId.IsSpecified ? new ulong?(metadata.OriginalResponseMessageId.Value) : ((ulong?)null), metadata.Name.GetValueOrDefault(null), RestUser.Create(discord, metadata.User)), 
			InteractionType.MessageComponent => new MessageComponentInteractionMetadata(metadata.Id, metadata.Type, metadata.User.Id, metadata.IntegrationOwners.ToImmutableDictionary(), metadata.OriginalResponseMessageId.IsSpecified ? new ulong?(metadata.OriginalResponseMessageId.Value) : ((ulong?)null), metadata.InteractedMessageId.GetValueOrDefault(0uL), RestUser.Create(discord, metadata.User)), 
			InteractionType.ModalSubmit => new ModalSubmitInteractionMetadata(metadata.Id, metadata.Type, metadata.User.Id, metadata.IntegrationOwners.ToImmutableDictionary(), metadata.OriginalResponseMessageId.IsSpecified ? new ulong?(metadata.OriginalResponseMessageId.Value) : ((ulong?)null), metadata.TriggeringInteractionMetadata.GetValueOrDefault(null)?.ToInteractionMetadata(discord), RestUser.Create(discord, metadata.User)), 
			_ => null, 
		};
	}
}
