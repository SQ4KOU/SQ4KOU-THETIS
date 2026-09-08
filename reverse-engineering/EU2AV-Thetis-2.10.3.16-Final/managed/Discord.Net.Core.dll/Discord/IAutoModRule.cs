using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord;

public interface IAutoModRule : ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	ulong GuildId { get; }

	string Name { get; }

	ulong CreatorId { get; }

	AutoModEventType EventType { get; }

	AutoModTriggerType TriggerType { get; }

	IReadOnlyCollection<string> KeywordFilter { get; }

	IReadOnlyCollection<string> RegexPatterns { get; }

	IReadOnlyCollection<string> AllowList { get; }

	IReadOnlyCollection<KeywordPresetTypes> Presets { get; }

	int? MentionTotalLimit { get; }

	IReadOnlyCollection<AutoModRuleAction> Actions { get; }

	bool Enabled { get; }

	IReadOnlyCollection<ulong> ExemptRoles { get; }

	IReadOnlyCollection<ulong> ExemptChannels { get; }

	bool? MentionRaidProtectionEnabled { get; }

	Task ModifyAsync(Action<AutoModRuleProperties> func, RequestOptions options = null);
}
