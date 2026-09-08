using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

public class RestAutoModRule : RestEntity<ulong>, IAutoModRule, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	public DateTimeOffset CreatedAt { get; private set; }

	public ulong GuildId { get; private set; }

	public string Name { get; private set; }

	public ulong CreatorId { get; private set; }

	public AutoModEventType EventType { get; private set; }

	public AutoModTriggerType TriggerType { get; private set; }

	public IReadOnlyCollection<string> KeywordFilter { get; private set; }

	public IReadOnlyCollection<string> RegexPatterns { get; private set; }

	public IReadOnlyCollection<string> AllowList { get; private set; }

	public IReadOnlyCollection<KeywordPresetTypes> Presets { get; private set; }

	public int? MentionTotalLimit { get; private set; }

	public IReadOnlyCollection<AutoModRuleAction> Actions { get; private set; }

	public bool Enabled { get; private set; }

	public IReadOnlyCollection<ulong> ExemptRoles { get; private set; }

	public IReadOnlyCollection<ulong> ExemptChannels { get; private set; }

	public bool? MentionRaidProtectionEnabled { get; private set; }

	internal RestAutoModRule(BaseDiscordClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal static RestAutoModRule Create(BaseDiscordClient discord, AutoModerationRule model)
	{
		RestAutoModRule restAutoModRule = new RestAutoModRule(discord, model.Id);
		restAutoModRule.Update(model);
		return restAutoModRule;
	}

	internal void Update(AutoModerationRule model)
	{
		Name = model.Name;
		CreatorId = model.CreatorId;
		GuildId = model.GuildId;
		EventType = model.EventType;
		TriggerType = model.TriggerType;
		KeywordFilter = ((IEnumerable<string>)model.TriggerMetadata.KeywordFilter.GetValueOrDefault(Array.Empty<string>())).ToImmutableArray();
		Presets = ((IEnumerable<KeywordPresetTypes>)model.TriggerMetadata.Presets.GetValueOrDefault(Array.Empty<KeywordPresetTypes>())).ToImmutableArray();
		RegexPatterns = ((IEnumerable<string>)model.TriggerMetadata.RegexPatterns.GetValueOrDefault(Array.Empty<string>())).ToImmutableArray();
		AllowList = ((IEnumerable<string>)model.TriggerMetadata.AllowList.GetValueOrDefault(Array.Empty<string>())).ToImmutableArray();
		MentionTotalLimit = (model.TriggerMetadata.MentionLimit.IsSpecified ? new int?(model.TriggerMetadata.MentionLimit.Value) : ((int?)null));
		Actions = model.Actions.Select((AutoModAction x) => new AutoModRuleAction(x.Type, x.Metadata.GetValueOrDefault()?.ChannelId.ToNullable(), x.Metadata.GetValueOrDefault()?.DurationSeconds.ToNullable(), (!x.Metadata.IsSpecified) ? null : (x.Metadata.Value.CustomMessage.IsSpecified ? x.Metadata.Value.CustomMessage.Value : null))).ToImmutableArray();
		Enabled = model.Enabled;
		ExemptRoles = ((IEnumerable<ulong>)model.ExemptRoles).ToImmutableArray();
		ExemptChannels = ((IEnumerable<ulong>)model.ExemptChannels).ToImmutableArray();
		MentionRaidProtectionEnabled = model.TriggerMetadata.MentionRaidProtectionEnabled.ToNullable();
	}

	public async Task ModifyAsync(Action<AutoModRuleProperties> func, RequestOptions options = null)
	{
		Update(await GuildHelper.ModifyRuleAsync(base.Discord, this, func, options));
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return GuildHelper.DeleteRuleAsync(base.Discord, this, options);
	}
}
