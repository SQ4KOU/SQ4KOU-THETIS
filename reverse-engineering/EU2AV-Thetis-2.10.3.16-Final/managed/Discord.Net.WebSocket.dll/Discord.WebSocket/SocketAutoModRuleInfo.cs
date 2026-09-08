using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.WebSocket;

public class SocketAutoModRuleInfo
{
	public string Name { get; set; }

	public AutoModEventType? EventType { get; set; }

	public AutoModTriggerType? TriggerType { get; set; }

	public bool? Enabled { get; set; }

	public IReadOnlyCollection<ulong> ExemptRoles { get; set; }

	public IReadOnlyCollection<ulong> ExemptChannels { get; set; }

	public IReadOnlyCollection<string> KeywordFilter { get; }

	public IReadOnlyCollection<string> RegexPatterns { get; }

	public IReadOnlyCollection<string> AllowList { get; }

	public IReadOnlyCollection<KeywordPresetTypes> Presets { get; }

	public int? MentionTotalLimit { get; }

	public IReadOnlyCollection<AutoModRuleAction> Actions { get; private set; }

	internal SocketAutoModRuleInfo(AutoModRuleInfoAuditLogModel model)
	{
		Actions = model.Actions?.Select((AutoModAction x) => new AutoModRuleAction(x.Type, x.Metadata.GetValueOrDefault()?.ChannelId.ToNullable(), x.Metadata.GetValueOrDefault()?.DurationSeconds.ToNullable(), (!x.Metadata.IsSpecified) ? null : (x.Metadata.Value.CustomMessage.IsSpecified ? x.Metadata.Value.CustomMessage.Value : null))).ToImmutableArray();
		KeywordFilter = model.TriggerMetadata?.KeywordFilter.GetValueOrDefault(Array.Empty<string>())?.ToImmutableArray();
		Presets = model.TriggerMetadata?.Presets.GetValueOrDefault(Array.Empty<KeywordPresetTypes>())?.ToImmutableArray();
		RegexPatterns = model.TriggerMetadata?.RegexPatterns.GetValueOrDefault(Array.Empty<string>())?.ToImmutableArray();
		AllowList = model.TriggerMetadata?.AllowList.GetValueOrDefault(Array.Empty<string>())?.ToImmutableArray();
		TriggerMetadata triggerMetadata = model.TriggerMetadata;
		MentionTotalLimit = ((triggerMetadata == null || !triggerMetadata.MentionLimit.IsSpecified) ? ((int?)null) : model.TriggerMetadata?.MentionLimit.Value);
		Name = model.Name;
		Enabled = model.Enabled;
		ExemptRoles = model.ExemptRoles?.ToImmutableArray();
		ExemptChannels = model.ExemptChannels?.ToImmutableArray();
		TriggerType = model.TriggerType;
		EventType = model.EventType;
	}
}
