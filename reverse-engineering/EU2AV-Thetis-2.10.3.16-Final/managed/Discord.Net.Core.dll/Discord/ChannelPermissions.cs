using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct ChannelPermissions
{
	public static readonly ChannelPermissions None = default(ChannelPermissions);

	public static readonly ChannelPermissions Text = new ChannelPermissions(1905439490441041uL);

	public static readonly ChannelPermissions Voice = new ChannelPermissions(2204120106139473uL);

	public static readonly ChannelPermissions Stage = new ChannelPermissions(1917701041159761uL);

	public static readonly ChannelPermissions Category = new ChannelPermissions(111049398812497uL);

	public static readonly ChannelPermissions DM = new ChannelPermissions(37080128uL);

	public static readonly ChannelPermissions Group = new ChannelPermissions(36755456uL);

	public static readonly ChannelPermissions Forum = new ChannelPermissions(5380318429009uL);

	public static readonly ChannelPermissions Media = new ChannelPermissions(5380318429009uL);

	public ulong RawValue { get; }

	public bool CreateInstantInvite => Permissions.GetValue(RawValue, ChannelPermission.CreateInstantInvite);

	public bool ManageChannel => Permissions.GetValue(RawValue, ChannelPermission.ManageChannels);

	public bool AddReactions => Permissions.GetValue(RawValue, ChannelPermission.AddReactions);

	public bool ViewChannel => Permissions.GetValue(RawValue, ChannelPermission.ViewChannel);

	public bool SendMessages => Permissions.GetValue(RawValue, ChannelPermission.SendMessages);

	public bool SendTTSMessages => Permissions.GetValue(RawValue, ChannelPermission.SendTTSMessages);

	public bool ManageMessages => Permissions.GetValue(RawValue, ChannelPermission.ManageMessages);

	public bool EmbedLinks => Permissions.GetValue(RawValue, ChannelPermission.EmbedLinks);

	public bool AttachFiles => Permissions.GetValue(RawValue, ChannelPermission.AttachFiles);

	public bool ReadMessageHistory => Permissions.GetValue(RawValue, ChannelPermission.ReadMessageHistory);

	public bool MentionEveryone => Permissions.GetValue(RawValue, ChannelPermission.MentionEveryone);

	public bool UseExternalEmojis => Permissions.GetValue(RawValue, ChannelPermission.UseExternalEmojis);

	public bool Connect => Permissions.GetValue(RawValue, ChannelPermission.Connect);

	public bool Speak => Permissions.GetValue(RawValue, ChannelPermission.Speak);

	public bool MuteMembers => Permissions.GetValue(RawValue, ChannelPermission.MuteMembers);

	public bool DeafenMembers => Permissions.GetValue(RawValue, ChannelPermission.DeafenMembers);

	public bool MoveMembers => Permissions.GetValue(RawValue, ChannelPermission.MoveMembers);

	public bool UseVAD => Permissions.GetValue(RawValue, ChannelPermission.UseVAD);

	public bool PrioritySpeaker => Permissions.GetValue(RawValue, ChannelPermission.PrioritySpeaker);

	public bool Stream => Permissions.GetValue(RawValue, ChannelPermission.Stream);

	public bool ManageRoles => Permissions.GetValue(RawValue, ChannelPermission.ManageRoles);

	public bool ManageWebhooks => Permissions.GetValue(RawValue, ChannelPermission.ManageWebhooks);

	public bool UseApplicationCommands => Permissions.GetValue(RawValue, ChannelPermission.UseApplicationCommands);

	public bool RequestToSpeak => Permissions.GetValue(RawValue, ChannelPermission.RequestToSpeak);

	public bool ManageThreads => Permissions.GetValue(RawValue, ChannelPermission.ManageThreads);

	public bool CreatePublicThreads => Permissions.GetValue(RawValue, ChannelPermission.CreatePublicThreads);

	public bool CreatePrivateThreads => Permissions.GetValue(RawValue, ChannelPermission.CreatePrivateThreads);

	public bool UseExternalStickers => Permissions.GetValue(RawValue, ChannelPermission.UseExternalStickers);

	public bool SendMessagesInThreads => Permissions.GetValue(RawValue, ChannelPermission.SendMessagesInThreads);

	public bool StartEmbeddedActivities => Permissions.GetValue(RawValue, ChannelPermission.StartEmbeddedActivities);

	public bool UseSoundboard => Permissions.GetValue(RawValue, ChannelPermission.UseSoundboard);

	public bool CreateEvents => Permissions.GetValue(RawValue, ChannelPermission.CreateEvents);

	public bool SendVoiceMessages => Permissions.GetValue(RawValue, ChannelPermission.SendVoiceMessages);

	public bool UseClydeAI => Permissions.GetValue(RawValue, ChannelPermission.UseClydeAI);

	public bool SetVoiceChannelStatus => Permissions.GetValue(RawValue, ChannelPermission.SetVoiceChannelStatus);

	public bool SendPolls => Permissions.GetValue(RawValue, ChannelPermission.SendPolls);

	public bool UserExternalApps => Permissions.GetValue(RawValue, ChannelPermission.UseExternalApps);

	public bool UseExternalSounds => Permissions.GetValue(RawValue, ChannelPermission.UseExternalSounds);

	private string DebuggerDisplay => string.Join(", ", ToList()) ?? "";

	public static ChannelPermissions All(IChannel channel)
	{
		if (!(channel is IStageChannel))
		{
			if (!(channel is IVoiceChannel))
			{
				if (!(channel is ITextChannel))
				{
					if (!(channel is ICategoryChannel))
					{
						if (!(channel is IDMChannel))
						{
							if (!(channel is IGroupChannel))
							{
								if (!(channel is IMediaChannel))
								{
									if (channel is IForumChannel)
									{
										return Forum;
									}
									throw new ArgumentException("Unknown channel type.", "channel");
								}
								return Media;
							}
							return Group;
						}
						return DM;
					}
					return Category;
				}
				return Text;
			}
			return Voice;
		}
		return Stage;
	}

	public ChannelPermissions(ulong rawValue)
	{
		RawValue = rawValue;
	}

	private ChannelPermissions(ulong initialValue, bool? createInstantInvite = null, bool? manageChannel = null, bool? addReactions = null, bool? viewChannel = null, bool? sendMessages = null, bool? sendTTSMessages = null, bool? manageMessages = null, bool? embedLinks = null, bool? attachFiles = null, bool? readMessageHistory = null, bool? mentionEveryone = null, bool? useExternalEmojis = null, bool? connect = null, bool? speak = null, bool? muteMembers = null, bool? deafenMembers = null, bool? moveMembers = null, bool? useVoiceActivation = null, bool? prioritySpeaker = null, bool? stream = null, bool? manageRoles = null, bool? manageWebhooks = null, bool? useApplicationCommands = null, bool? requestToSpeak = null, bool? manageThreads = null, bool? createPublicThreads = null, bool? createPrivateThreads = null, bool? useExternalStickers = null, bool? sendMessagesInThreads = null, bool? startEmbeddedActivities = null, bool? useSoundboard = null, bool? createEvents = null, bool? sendVoiceMessages = null, bool? useClydeAI = null, bool? setVoiceChannelStatus = null, bool? sendPolls = null, bool? useExternalApps = null, bool? useExternalSounds = null)
	{
		ulong rawValue = initialValue;
		Permissions.SetValue(ref rawValue, createInstantInvite, ChannelPermission.CreateInstantInvite);
		Permissions.SetValue(ref rawValue, manageChannel, ChannelPermission.ManageChannels);
		Permissions.SetValue(ref rawValue, addReactions, ChannelPermission.AddReactions);
		Permissions.SetValue(ref rawValue, viewChannel, ChannelPermission.ViewChannel);
		Permissions.SetValue(ref rawValue, sendMessages, ChannelPermission.SendMessages);
		Permissions.SetValue(ref rawValue, sendTTSMessages, ChannelPermission.SendTTSMessages);
		Permissions.SetValue(ref rawValue, manageMessages, ChannelPermission.ManageMessages);
		Permissions.SetValue(ref rawValue, embedLinks, ChannelPermission.EmbedLinks);
		Permissions.SetValue(ref rawValue, attachFiles, ChannelPermission.AttachFiles);
		Permissions.SetValue(ref rawValue, readMessageHistory, ChannelPermission.ReadMessageHistory);
		Permissions.SetValue(ref rawValue, mentionEveryone, ChannelPermission.MentionEveryone);
		Permissions.SetValue(ref rawValue, useExternalEmojis, ChannelPermission.UseExternalEmojis);
		Permissions.SetValue(ref rawValue, connect, ChannelPermission.Connect);
		Permissions.SetValue(ref rawValue, speak, ChannelPermission.Speak);
		Permissions.SetValue(ref rawValue, muteMembers, ChannelPermission.MuteMembers);
		Permissions.SetValue(ref rawValue, deafenMembers, ChannelPermission.DeafenMembers);
		Permissions.SetValue(ref rawValue, moveMembers, ChannelPermission.MoveMembers);
		Permissions.SetValue(ref rawValue, useVoiceActivation, ChannelPermission.UseVAD);
		Permissions.SetValue(ref rawValue, prioritySpeaker, ChannelPermission.PrioritySpeaker);
		Permissions.SetValue(ref rawValue, stream, ChannelPermission.Stream);
		Permissions.SetValue(ref rawValue, manageRoles, ChannelPermission.ManageRoles);
		Permissions.SetValue(ref rawValue, manageWebhooks, ChannelPermission.ManageWebhooks);
		Permissions.SetValue(ref rawValue, useApplicationCommands, ChannelPermission.UseApplicationCommands);
		Permissions.SetValue(ref rawValue, requestToSpeak, ChannelPermission.RequestToSpeak);
		Permissions.SetValue(ref rawValue, manageThreads, ChannelPermission.ManageThreads);
		Permissions.SetValue(ref rawValue, createPublicThreads, ChannelPermission.CreatePublicThreads);
		Permissions.SetValue(ref rawValue, createPrivateThreads, ChannelPermission.CreatePrivateThreads);
		Permissions.SetValue(ref rawValue, useExternalStickers, ChannelPermission.UseExternalStickers);
		Permissions.SetValue(ref rawValue, sendMessagesInThreads, ChannelPermission.SendMessagesInThreads);
		Permissions.SetValue(ref rawValue, startEmbeddedActivities, ChannelPermission.StartEmbeddedActivities);
		Permissions.SetValue(ref rawValue, useSoundboard, ChannelPermission.UseSoundboard);
		Permissions.SetValue(ref rawValue, createEvents, ChannelPermission.CreateEvents);
		Permissions.SetValue(ref rawValue, sendVoiceMessages, ChannelPermission.SendVoiceMessages);
		Permissions.SetValue(ref rawValue, useClydeAI, ChannelPermission.UseClydeAI);
		Permissions.SetValue(ref rawValue, setVoiceChannelStatus, ChannelPermission.SetVoiceChannelStatus);
		Permissions.SetValue(ref rawValue, sendPolls, ChannelPermission.SendPolls);
		Permissions.SetValue(ref rawValue, useExternalApps, ChannelPermission.UseExternalApps);
		Permissions.SetValue(ref rawValue, useExternalSounds, ChannelPermission.UseExternalSounds);
		RawValue = rawValue;
	}

	public ChannelPermissions(bool createInstantInvite = false, bool manageChannel = false, bool addReactions = false, bool viewChannel = false, bool sendMessages = false, bool sendTTSMessages = false, bool manageMessages = false, bool embedLinks = false, bool attachFiles = false, bool readMessageHistory = false, bool mentionEveryone = false, bool useExternalEmojis = false, bool connect = false, bool speak = false, bool muteMembers = false, bool deafenMembers = false, bool moveMembers = false, bool useVoiceActivation = false, bool prioritySpeaker = false, bool stream = false, bool manageRoles = false, bool manageWebhooks = false, bool useApplicationCommands = false, bool requestToSpeak = false, bool manageThreads = false, bool createPublicThreads = false, bool createPrivateThreads = false, bool useExternalStickers = false, bool sendMessagesInThreads = false, bool startEmbeddedActivities = false, bool useSoundboard = false, bool createEvents = false, bool sendVoiceMessages = false, bool useClydeAI = false, bool setVoiceChannelStatus = false, bool sendPolls = false, bool useExternalApps = false, bool useExternalSounds = false)
		: this(0uL, createInstantInvite, manageChannel, addReactions, viewChannel, sendMessages, sendTTSMessages, manageMessages, embedLinks, attachFiles, readMessageHistory, mentionEveryone, useExternalEmojis, connect, speak, muteMembers, deafenMembers, moveMembers, useVoiceActivation, prioritySpeaker, stream, manageRoles, manageWebhooks, useApplicationCommands, requestToSpeak, manageThreads, createPublicThreads, createPrivateThreads, useExternalStickers, sendMessagesInThreads, startEmbeddedActivities, useSoundboard, createEvents, sendVoiceMessages, useClydeAI, setVoiceChannelStatus, sendPolls, useExternalApps, useExternalSounds)
	{
	}

	public ChannelPermissions Modify(bool? createInstantInvite = null, bool? manageChannel = null, bool? addReactions = null, bool? viewChannel = null, bool? sendMessages = null, bool? sendTTSMessages = null, bool? manageMessages = null, bool? embedLinks = null, bool? attachFiles = null, bool? readMessageHistory = null, bool? mentionEveryone = null, bool? useExternalEmojis = null, bool? connect = null, bool? speak = null, bool? muteMembers = null, bool? deafenMembers = null, bool? moveMembers = null, bool? useVoiceActivation = null, bool? prioritySpeaker = null, bool? stream = null, bool? manageRoles = null, bool? manageWebhooks = null, bool? useApplicationCommands = null, bool? requestToSpeak = null, bool? manageThreads = null, bool? createPublicThreads = null, bool? createPrivateThreads = null, bool? useExternalStickers = null, bool? sendMessagesInThreads = null, bool? startEmbeddedActivities = null, bool? useSoundboard = null, bool? createEvents = null, bool? sendVoiceMessages = null, bool? useClydeAI = null, bool? setVoiceChannelStatus = null, bool? sendPolls = null, bool? useExternalApps = null, bool? useExternalSounds = null)
	{
		return new ChannelPermissions(RawValue, createInstantInvite, manageChannel, addReactions, viewChannel, sendMessages, sendTTSMessages, manageMessages, embedLinks, attachFiles, readMessageHistory, mentionEveryone, useExternalEmojis, connect, speak, muteMembers, deafenMembers, moveMembers, useVoiceActivation, prioritySpeaker, stream, manageRoles, manageWebhooks, useApplicationCommands, requestToSpeak, manageThreads, createPublicThreads, createPrivateThreads, useExternalStickers, sendMessagesInThreads, startEmbeddedActivities, useSoundboard, createEvents, sendVoiceMessages, useClydeAI, setVoiceChannelStatus, sendPolls, useExternalApps, useExternalSounds);
	}

	public bool Has(ChannelPermission permission)
	{
		return Permissions.GetValue(RawValue, permission);
	}

	public List<ChannelPermission> ToList()
	{
		List<ChannelPermission> list = new List<ChannelPermission>();
		for (byte b = 0; b < 53; b++)
		{
			ulong num = (ulong)(1L << (int)b);
			if ((RawValue & num) != 0L)
			{
				list.Add((ChannelPermission)num);
			}
		}
		return list;
	}

	public override string ToString()
	{
		return RawValue.ToString();
	}
}
