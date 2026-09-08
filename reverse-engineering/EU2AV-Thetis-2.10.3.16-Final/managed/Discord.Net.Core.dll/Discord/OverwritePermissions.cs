using System.Collections.Generic;
using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct OverwritePermissions
{
	public static OverwritePermissions InheritAll { get; }

	public ulong AllowValue { get; }

	public ulong DenyValue { get; }

	public PermValue CreateInstantInvite => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.CreateInstantInvite);

	public PermValue ManageChannel => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.ManageChannels);

	public PermValue AddReactions => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.AddReactions);

	public PermValue ViewChannel => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.ViewChannel);

	public PermValue SendMessages => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.SendMessages);

	public PermValue SendTTSMessages => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.SendTTSMessages);

	public PermValue ManageMessages => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.ManageMessages);

	public PermValue EmbedLinks => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.EmbedLinks);

	public PermValue AttachFiles => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.AttachFiles);

	public PermValue ReadMessageHistory => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.ReadMessageHistory);

	public PermValue MentionEveryone => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.MentionEveryone);

	public PermValue UseExternalEmojis => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.UseExternalEmojis);

	public PermValue Connect => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.Connect);

	public PermValue Speak => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.Speak);

	public PermValue MuteMembers => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.MuteMembers);

	public PermValue DeafenMembers => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.DeafenMembers);

	public PermValue MoveMembers => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.MoveMembers);

	public PermValue UseVAD => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.UseVAD);

	public PermValue PrioritySpeaker => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.PrioritySpeaker);

	public PermValue Stream => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.Stream);

	public PermValue ManageRoles => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.ManageRoles);

	public PermValue ManageWebhooks => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.ManageWebhooks);

	public PermValue UseApplicationCommands => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.UseApplicationCommands);

	public PermValue RequestToSpeak => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.RequestToSpeak);

	public PermValue ManageThreads => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.ManageThreads);

	public PermValue CreatePublicThreads => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.CreatePublicThreads);

	public PermValue CreatePrivateThreads => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.CreatePrivateThreads);

	public PermValue UseExternalStickers => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.UseExternalStickers);

	public PermValue SendMessagesInThreads => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.SendMessagesInThreads);

	public PermValue StartEmbeddedActivities => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.StartEmbeddedActivities);

	public PermValue UseSoundboard => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.UseSoundboard);

	public PermValue CreateEvents => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.CreateEvents);

	public PermValue SendVoiceMessages => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.SendVoiceMessages);

	public PermValue UseClydeAI => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.UseClydeAI);

	public PermValue SetVoiceChannelStatus => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.SetVoiceChannelStatus);

	public PermValue SendPolls => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.SendPolls);

	public PermValue UserExternalApps => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.UseExternalApps);

	public PermValue UseExternalSounds => Permissions.GetValue(AllowValue, DenyValue, ChannelPermission.UseExternalSounds);

	private string DebuggerDisplay => "Allow " + string.Join(", ", ToAllowList()) + ", Deny " + string.Join(", ", ToDenyList());

	public static OverwritePermissions AllowAll(IChannel channel)
	{
		return new OverwritePermissions(ChannelPermissions.All(channel).RawValue, 0uL);
	}

	public static OverwritePermissions DenyAll(IChannel channel)
	{
		return new OverwritePermissions(0uL, ChannelPermissions.All(channel).RawValue);
	}

	public OverwritePermissions(ulong allowValue, ulong denyValue)
	{
		AllowValue = allowValue;
		DenyValue = denyValue;
	}

	public OverwritePermissions(string allowValue, string denyValue)
	{
		AllowValue = ulong.Parse(allowValue);
		DenyValue = ulong.Parse(denyValue);
	}

	private OverwritePermissions(ulong allowValue, ulong denyValue, PermValue? createInstantInvite = null, PermValue? manageChannel = null, PermValue? addReactions = null, PermValue? viewChannel = null, PermValue? sendMessages = null, PermValue? sendTTSMessages = null, PermValue? manageMessages = null, PermValue? embedLinks = null, PermValue? attachFiles = null, PermValue? readMessageHistory = null, PermValue? mentionEveryone = null, PermValue? useExternalEmojis = null, PermValue? connect = null, PermValue? speak = null, PermValue? muteMembers = null, PermValue? deafenMembers = null, PermValue? moveMembers = null, PermValue? useVoiceActivation = null, PermValue? manageRoles = null, PermValue? manageWebhooks = null, PermValue? prioritySpeaker = null, PermValue? stream = null, PermValue? useSlashCommands = null, PermValue? useApplicationCommands = null, PermValue? requestToSpeak = null, PermValue? manageThreads = null, PermValue? createPublicThreads = null, PermValue? createPrivateThreads = null, PermValue? usePublicThreads = null, PermValue? usePrivateThreads = null, PermValue? useExternalStickers = null, PermValue? sendMessagesInThreads = null, PermValue? startEmbeddedActivities = null, PermValue? useSoundboard = null, PermValue? createEvents = null, PermValue? sendVoiceMessages = null, PermValue? useClydeAI = null, PermValue? setVoiceChannelStatus = null, PermValue? sendPolls = null, PermValue? useExternalApps = null, PermValue? useExternalSounds = null)
	{
		Permissions.SetValue(ref allowValue, ref denyValue, createInstantInvite, ChannelPermission.CreateInstantInvite);
		Permissions.SetValue(ref allowValue, ref denyValue, manageChannel, ChannelPermission.ManageChannels);
		Permissions.SetValue(ref allowValue, ref denyValue, addReactions, ChannelPermission.AddReactions);
		Permissions.SetValue(ref allowValue, ref denyValue, viewChannel, ChannelPermission.ViewChannel);
		Permissions.SetValue(ref allowValue, ref denyValue, sendMessages, ChannelPermission.SendMessages);
		Permissions.SetValue(ref allowValue, ref denyValue, sendTTSMessages, ChannelPermission.SendTTSMessages);
		Permissions.SetValue(ref allowValue, ref denyValue, manageMessages, ChannelPermission.ManageMessages);
		Permissions.SetValue(ref allowValue, ref denyValue, embedLinks, ChannelPermission.EmbedLinks);
		Permissions.SetValue(ref allowValue, ref denyValue, attachFiles, ChannelPermission.AttachFiles);
		Permissions.SetValue(ref allowValue, ref denyValue, readMessageHistory, ChannelPermission.ReadMessageHistory);
		Permissions.SetValue(ref allowValue, ref denyValue, mentionEveryone, ChannelPermission.MentionEveryone);
		Permissions.SetValue(ref allowValue, ref denyValue, useExternalEmojis, ChannelPermission.UseExternalEmojis);
		Permissions.SetValue(ref allowValue, ref denyValue, connect, ChannelPermission.Connect);
		Permissions.SetValue(ref allowValue, ref denyValue, speak, ChannelPermission.Speak);
		Permissions.SetValue(ref allowValue, ref denyValue, muteMembers, ChannelPermission.MuteMembers);
		Permissions.SetValue(ref allowValue, ref denyValue, deafenMembers, ChannelPermission.DeafenMembers);
		Permissions.SetValue(ref allowValue, ref denyValue, moveMembers, ChannelPermission.MoveMembers);
		Permissions.SetValue(ref allowValue, ref denyValue, useVoiceActivation, ChannelPermission.UseVAD);
		Permissions.SetValue(ref allowValue, ref denyValue, prioritySpeaker, ChannelPermission.PrioritySpeaker);
		Permissions.SetValue(ref allowValue, ref denyValue, stream, ChannelPermission.Stream);
		Permissions.SetValue(ref allowValue, ref denyValue, manageRoles, ChannelPermission.ManageRoles);
		Permissions.SetValue(ref allowValue, ref denyValue, manageWebhooks, ChannelPermission.ManageWebhooks);
		Permissions.SetValue(ref allowValue, ref denyValue, useApplicationCommands, ChannelPermission.UseApplicationCommands);
		Permissions.SetValue(ref allowValue, ref denyValue, requestToSpeak, ChannelPermission.RequestToSpeak);
		Permissions.SetValue(ref allowValue, ref denyValue, manageThreads, ChannelPermission.ManageThreads);
		Permissions.SetValue(ref allowValue, ref denyValue, createPublicThreads, ChannelPermission.CreatePublicThreads);
		Permissions.SetValue(ref allowValue, ref denyValue, createPrivateThreads, ChannelPermission.CreatePrivateThreads);
		Permissions.SetValue(ref allowValue, ref denyValue, useExternalStickers, ChannelPermission.UseExternalStickers);
		Permissions.SetValue(ref allowValue, ref denyValue, sendMessagesInThreads, ChannelPermission.SendMessagesInThreads);
		Permissions.SetValue(ref allowValue, ref denyValue, startEmbeddedActivities, ChannelPermission.StartEmbeddedActivities);
		Permissions.SetValue(ref allowValue, ref denyValue, useSoundboard, ChannelPermission.UseSoundboard);
		Permissions.SetValue(ref allowValue, ref denyValue, createEvents, ChannelPermission.CreateEvents);
		Permissions.SetValue(ref allowValue, ref denyValue, sendVoiceMessages, ChannelPermission.SendVoiceMessages);
		Permissions.SetValue(ref allowValue, ref denyValue, useClydeAI, ChannelPermission.UseClydeAI);
		Permissions.SetValue(ref allowValue, ref denyValue, setVoiceChannelStatus, ChannelPermission.SetVoiceChannelStatus);
		Permissions.SetValue(ref allowValue, ref denyValue, sendPolls, ChannelPermission.SendPolls);
		Permissions.SetValue(ref allowValue, ref denyValue, useExternalApps, ChannelPermission.UseExternalApps);
		Permissions.SetValue(ref allowValue, ref denyValue, useExternalSounds, ChannelPermission.UseExternalSounds);
		AllowValue = allowValue;
		DenyValue = denyValue;
	}

	public OverwritePermissions(PermValue createInstantInvite = PermValue.Inherit, PermValue manageChannel = PermValue.Inherit, PermValue addReactions = PermValue.Inherit, PermValue viewChannel = PermValue.Inherit, PermValue sendMessages = PermValue.Inherit, PermValue sendTTSMessages = PermValue.Inherit, PermValue manageMessages = PermValue.Inherit, PermValue embedLinks = PermValue.Inherit, PermValue attachFiles = PermValue.Inherit, PermValue readMessageHistory = PermValue.Inherit, PermValue mentionEveryone = PermValue.Inherit, PermValue useExternalEmojis = PermValue.Inherit, PermValue connect = PermValue.Inherit, PermValue speak = PermValue.Inherit, PermValue muteMembers = PermValue.Inherit, PermValue deafenMembers = PermValue.Inherit, PermValue moveMembers = PermValue.Inherit, PermValue useVoiceActivation = PermValue.Inherit, PermValue manageRoles = PermValue.Inherit, PermValue manageWebhooks = PermValue.Inherit, PermValue prioritySpeaker = PermValue.Inherit, PermValue stream = PermValue.Inherit, PermValue useSlashCommands = PermValue.Inherit, PermValue useApplicationCommands = PermValue.Inherit, PermValue requestToSpeak = PermValue.Inherit, PermValue manageThreads = PermValue.Inherit, PermValue createPublicThreads = PermValue.Inherit, PermValue createPrivateThreads = PermValue.Inherit, PermValue usePublicThreads = PermValue.Inherit, PermValue usePrivateThreads = PermValue.Inherit, PermValue useExternalStickers = PermValue.Inherit, PermValue sendMessagesInThreads = PermValue.Inherit, PermValue startEmbeddedActivities = PermValue.Inherit, PermValue useSoundboard = PermValue.Inherit, PermValue createEvents = PermValue.Inherit, PermValue sendVoiceMessages = PermValue.Inherit, PermValue useClydeAI = PermValue.Inherit, PermValue setVoiceChannelStatus = PermValue.Inherit, PermValue sendPolls = PermValue.Inherit, PermValue useExternalApps = PermValue.Inherit, PermValue useExternalSounds = PermValue.Inherit)
		: this(0uL, 0uL, createInstantInvite, manageChannel, addReactions, viewChannel, sendMessages, sendTTSMessages, manageMessages, embedLinks, attachFiles, readMessageHistory, mentionEveryone, useExternalEmojis, connect, speak, muteMembers, deafenMembers, moveMembers, useVoiceActivation, manageRoles, manageWebhooks, prioritySpeaker, stream, useSlashCommands, useApplicationCommands, requestToSpeak, manageThreads, createPublicThreads, createPrivateThreads, usePublicThreads, usePrivateThreads, useExternalStickers, sendMessagesInThreads, startEmbeddedActivities, useSoundboard, createEvents, sendVoiceMessages, useClydeAI, setVoiceChannelStatus, sendPolls, useExternalApps, useExternalSounds)
	{
	}

	public OverwritePermissions Modify(PermValue? createInstantInvite = null, PermValue? manageChannel = null, PermValue? addReactions = null, PermValue? viewChannel = null, PermValue? sendMessages = null, PermValue? sendTTSMessages = null, PermValue? manageMessages = null, PermValue? embedLinks = null, PermValue? attachFiles = null, PermValue? readMessageHistory = null, PermValue? mentionEveryone = null, PermValue? useExternalEmojis = null, PermValue? connect = null, PermValue? speak = null, PermValue? muteMembers = null, PermValue? deafenMembers = null, PermValue? moveMembers = null, PermValue? useVoiceActivation = null, PermValue? manageRoles = null, PermValue? manageWebhooks = null, PermValue? prioritySpeaker = null, PermValue? stream = null, PermValue? useSlashCommands = null, PermValue? useApplicationCommands = null, PermValue? requestToSpeak = null, PermValue? manageThreads = null, PermValue? createPublicThreads = null, PermValue? createPrivateThreads = null, PermValue? usePublicThreads = null, PermValue? usePrivateThreads = null, PermValue? useExternalStickers = null, PermValue? sendMessagesInThreads = null, PermValue? startEmbeddedActivities = null, PermValue? useSoundboard = null, PermValue? createEvents = null, PermValue? sendVoiceMessages = null, PermValue? useClydeAI = null, PermValue? setVoiceChannelStatus = null, PermValue? sendPolls = null, PermValue? useExternalApps = null, PermValue? useExternalSounds = null)
	{
		return new OverwritePermissions(AllowValue, DenyValue, createInstantInvite, manageChannel, addReactions, viewChannel, sendMessages, sendTTSMessages, manageMessages, embedLinks, attachFiles, readMessageHistory, mentionEveryone, useExternalEmojis, connect, speak, muteMembers, deafenMembers, moveMembers, useVoiceActivation, manageRoles, manageWebhooks, prioritySpeaker, stream, useSlashCommands, useApplicationCommands, requestToSpeak, manageThreads, createPublicThreads, createPrivateThreads, usePublicThreads, usePrivateThreads, useExternalStickers, sendMessagesInThreads, startEmbeddedActivities, useSoundboard, createEvents, sendVoiceMessages, useClydeAI, setVoiceChannelStatus, sendPolls, useExternalApps, useExternalSounds);
	}

	public List<ChannelPermission> ToAllowList()
	{
		List<ChannelPermission> list = new List<ChannelPermission>();
		for (byte b = 0; b < 53; b++)
		{
			ulong num = (ulong)(1L << (int)b);
			if ((AllowValue & num) != 0L)
			{
				list.Add((ChannelPermission)num);
			}
		}
		return list;
	}

	public List<ChannelPermission> ToDenyList()
	{
		List<ChannelPermission> list = new List<ChannelPermission>();
		for (byte b = 0; b < 53; b++)
		{
			ulong num = (ulong)(1L << (int)b);
			if ((DenyValue & num) != 0L)
			{
				list.Add((ChannelPermission)num);
			}
		}
		return list;
	}

	public override string ToString()
	{
		return $"Allow {AllowValue}, Deny {DenyValue}";
	}
}
