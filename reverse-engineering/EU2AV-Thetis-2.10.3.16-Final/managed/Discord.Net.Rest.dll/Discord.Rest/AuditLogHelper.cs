using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Discord.API;

namespace Discord.Rest;

internal static class AuditLogHelper
{
	private static readonly Dictionary<ActionType, Func<BaseDiscordClient, AuditLogEntry, AuditLog, IAuditLogData>> CreateMapping = new Dictionary<ActionType, Func<BaseDiscordClient, AuditLogEntry, AuditLog, IAuditLogData>>
	{
		[ActionType.GuildUpdated] = GuildUpdateAuditLogData.Create,
		[ActionType.ChannelCreated] = ChannelCreateAuditLogData.Create,
		[ActionType.ChannelUpdated] = ChannelUpdateAuditLogData.Create,
		[ActionType.ChannelDeleted] = ChannelDeleteAuditLogData.Create,
		[ActionType.OverwriteCreated] = OverwriteCreateAuditLogData.Create,
		[ActionType.OverwriteUpdated] = OverwriteUpdateAuditLogData.Create,
		[ActionType.OverwriteDeleted] = OverwriteDeleteAuditLogData.Create,
		[ActionType.Kick] = KickAuditLogData.Create,
		[ActionType.Prune] = PruneAuditLogData.Create,
		[ActionType.Ban] = BanAuditLogData.Create,
		[ActionType.Unban] = UnbanAuditLogData.Create,
		[ActionType.MemberUpdated] = MemberUpdateAuditLogData.Create,
		[ActionType.MemberRoleUpdated] = MemberRoleAuditLogData.Create,
		[ActionType.MemberMoved] = MemberMoveAuditLogData.Create,
		[ActionType.MemberDisconnected] = MemberDisconnectAuditLogData.Create,
		[ActionType.BotAdded] = BotAddAuditLogData.Create,
		[ActionType.RoleCreated] = RoleCreateAuditLogData.Create,
		[ActionType.RoleUpdated] = RoleUpdateAuditLogData.Create,
		[ActionType.RoleDeleted] = RoleDeleteAuditLogData.Create,
		[ActionType.InviteCreated] = InviteCreateAuditLogData.Create,
		[ActionType.InviteUpdated] = InviteUpdateAuditLogData.Create,
		[ActionType.InviteDeleted] = InviteDeleteAuditLogData.Create,
		[ActionType.WebhookCreated] = WebhookCreateAuditLogData.Create,
		[ActionType.WebhookUpdated] = WebhookUpdateAuditLogData.Create,
		[ActionType.WebhookDeleted] = WebhookDeleteAuditLogData.Create,
		[ActionType.EmojiCreated] = EmoteCreateAuditLogData.Create,
		[ActionType.EmojiUpdated] = EmoteUpdateAuditLogData.Create,
		[ActionType.EmojiDeleted] = EmoteDeleteAuditLogData.Create,
		[ActionType.MessageDeleted] = MessageDeleteAuditLogData.Create,
		[ActionType.MessageBulkDeleted] = MessageBulkDeleteAuditLogData.Create,
		[ActionType.MessagePinned] = MessagePinAuditLogData.Create,
		[ActionType.MessageUnpinned] = MessageUnpinAuditLogData.Create,
		[ActionType.EventCreate] = ScheduledEventCreateAuditLogData.Create,
		[ActionType.EventUpdate] = ScheduledEventUpdateAuditLogData.Create,
		[ActionType.EventDelete] = ScheduledEventDeleteAuditLogData.Create,
		[ActionType.ThreadCreate] = ThreadCreateAuditLogData.Create,
		[ActionType.ThreadUpdate] = ThreadUpdateAuditLogData.Create,
		[ActionType.ThreadDelete] = ThreadDeleteAuditLogData.Create,
		[ActionType.ApplicationCommandPermissionUpdate] = CommandPermissionUpdateAuditLogData.Create,
		[ActionType.IntegrationCreated] = IntegrationCreatedAuditLogData.Create,
		[ActionType.IntegrationUpdated] = IntegrationUpdatedAuditLogData.Create,
		[ActionType.IntegrationDeleted] = IntegrationDeletedAuditLogData.Create,
		[ActionType.StageInstanceCreated] = StageInstanceCreateAuditLogData.Create,
		[ActionType.StageInstanceUpdated] = StageInstanceUpdatedAuditLogData.Create,
		[ActionType.StageInstanceDeleted] = StageInstanceDeleteAuditLogData.Create,
		[ActionType.StickerCreated] = StickerCreatedAuditLogData.Create,
		[ActionType.StickerUpdated] = StickerUpdatedAuditLogData.Create,
		[ActionType.StickerDeleted] = StickerDeletedAuditLogData.Create,
		[ActionType.AutoModerationRuleCreate] = AutoModRuleCreatedAuditLogData.Create,
		[ActionType.AutoModerationRuleUpdate] = AutoModRuleUpdatedAuditLogData.Create,
		[ActionType.AutoModerationRuleDelete] = AutoModRuleDeletedAuditLogData.Create,
		[ActionType.AutoModerationBlockMessage] = AutoModBlockedMessageAuditLogData.Create,
		[ActionType.AutoModerationFlagToChannel] = AutoModFlaggedMessageAuditLogData.Create,
		[ActionType.AutoModerationUserCommunicationDisabled] = AutoModTimeoutUserAuditLogData.Create,
		[ActionType.OnboardingQuestionCreated] = OnboardingPromptCreatedAuditLogData.Create,
		[ActionType.OnboardingQuestionUpdated] = OnboardingPromptUpdatedAuditLogData.Create,
		[ActionType.OnboardingUpdated] = OnboardingUpdatedAuditLogData.Create,
		[ActionType.VoiceChannelStatusUpdated] = VoiceChannelStatusUpdateAuditLogData.Create,
		[ActionType.VoiceChannelStatusDeleted] = VoiceChannelStatusDeletedAuditLogData.Create
	};

	public static IAuditLogData CreateData(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		if (CreateMapping.TryGetValue(entry.Action, out var value))
		{
			return value(discord, entry, log);
		}
		return null;
	}

	internal static (T, T) CreateAuditLogEntityInfo<T>(AuditLogChange[] changes, BaseDiscordClient discord) where T : IAuditLogInfoModel
	{
		T val = (T)Activator.CreateInstance(typeof(T));
		T val2 = (T)Activator.CreateInstance(typeof(T));
		PropertyInfo[] properties = typeof(T).GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			object obj = propertyInfo.GetCustomAttributes(typeof(JsonFieldAttribute), inherit: true).FirstOrDefault();
			JsonFieldAttribute jsonAttr = obj as JsonFieldAttribute;
			if (jsonAttr != null)
			{
				AuditLogChange auditLogChange = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == jsonAttr.FieldName);
				if (auditLogChange != null)
				{
					propertyInfo.SetValue(val, auditLogChange.OldValue?.ToObject(propertyInfo.PropertyType, discord.ApiClient.Serializer));
					propertyInfo.SetValue(val2, auditLogChange.NewValue?.ToObject(propertyInfo.PropertyType, discord.ApiClient.Serializer));
				}
			}
		}
		return (val, val2);
	}
}
