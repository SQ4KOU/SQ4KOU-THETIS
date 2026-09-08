using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketAutoModRule : SocketEntity<ulong>, IAutoModRule, ISnowflakeEntity, IEntity<ulong>, IDeletable
{
	private ulong _creatorId;

	public SocketGuild Guild { get; }

	public string Name { get; private set; }

	public SocketGuildUser Creator { get; private set; }

	public AutoModEventType EventType { get; private set; }

	public AutoModTriggerType TriggerType { get; private set; }

	public IReadOnlyCollection<string> KeywordFilter { get; private set; }

	public IReadOnlyCollection<string> RegexPatterns { get; private set; }

	public IReadOnlyCollection<string> AllowList { get; private set; }

	public IReadOnlyCollection<KeywordPresetTypes> Presets { get; private set; }

	public IReadOnlyCollection<AutoModRuleAction> Actions { get; private set; }

	public int? MentionTotalLimit { get; private set; }

	public bool Enabled { get; private set; }

	public IReadOnlyCollection<SocketRole> ExemptRoles { get; private set; }

	public IReadOnlyCollection<SocketGuildChannel> ExemptChannels { get; private set; }

	public bool? MentionRaidProtectionEnabled { get; private set; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	IReadOnlyCollection<ulong> IAutoModRule.ExemptRoles => ExemptRoles.Select((SocketRole x) => x.Id).ToImmutableArray();

	IReadOnlyCollection<ulong> IAutoModRule.ExemptChannels => ExemptChannels.Select((SocketGuildChannel x) => x.Id).ToImmutableArray();

	ulong IAutoModRule.GuildId => Guild.Id;

	ulong IAutoModRule.CreatorId => _creatorId;

	internal SocketAutoModRule(DiscordSocketClient discord, ulong id, SocketGuild guild)
		: base(discord, id)
	{
		Guild = guild;
	}

	internal static SocketAutoModRule Create(DiscordSocketClient discord, SocketGuild guild, AutoModerationRule model)
	{
		SocketAutoModRule socketAutoModRule = new SocketAutoModRule(discord, model.Id, guild);
		socketAutoModRule.Update(model);
		return socketAutoModRule;
	}

	internal void Update(AutoModerationRule model)
	{
		Name = model.Name;
		_creatorId = model.CreatorId;
		if (Creator == null)
		{
			SocketGuildUser socketGuildUser = (Creator = Guild.GetUser(_creatorId));
		}
		EventType = model.EventType;
		TriggerType = model.TriggerType;
		KeywordFilter = ((IEnumerable<string>)model.TriggerMetadata.KeywordFilter.GetValueOrDefault(Array.Empty<string>())).ToImmutableArray();
		Presets = ((IEnumerable<KeywordPresetTypes>)model.TriggerMetadata.Presets.GetValueOrDefault(Array.Empty<KeywordPresetTypes>())).ToImmutableArray();
		RegexPatterns = ((IEnumerable<string>)model.TriggerMetadata.RegexPatterns.GetValueOrDefault(Array.Empty<string>())).ToImmutableArray();
		AllowList = ((IEnumerable<string>)model.TriggerMetadata.AllowList.GetValueOrDefault(Array.Empty<string>())).ToImmutableArray();
		MentionTotalLimit = (model.TriggerMetadata.MentionLimit.IsSpecified ? new int?(model.TriggerMetadata.MentionLimit.Value) : ((int?)null));
		Actions = model.Actions.Select((AutoModAction x) => new AutoModRuleAction(x.Type, x.Metadata.GetValueOrDefault()?.ChannelId.ToNullable(), x.Metadata.GetValueOrDefault()?.DurationSeconds.ToNullable(), (!x.Metadata.IsSpecified) ? null : (x.Metadata.Value.CustomMessage.IsSpecified ? x.Metadata.Value.CustomMessage.Value : null))).ToImmutableArray();
		Enabled = model.Enabled;
		ExemptRoles = model.ExemptRoles.Select((ulong x) => Guild.GetRole(x)).ToImmutableArray();
		ExemptChannels = model.ExemptChannels.Select((ulong x) => Guild.GetChannel(x)).ToImmutableArray();
		MentionRaidProtectionEnabled = model.TriggerMetadata.MentionRaidProtectionEnabled.ToNullable();
	}

	public async Task ModifyAsync(Action<AutoModRuleProperties> func, RequestOptions options = null)
	{
		AutoModerationRule model = await GuildHelper.ModifyRuleAsync(base.Discord, this, func, options);
		Guild.AddOrUpdateAutoModRule(model);
	}

	public Task DeleteAsync(RequestOptions options = null)
	{
		return GuildHelper.DeleteRuleAsync(base.Discord, this, options);
	}

	internal SocketAutoModRule Clone()
	{
		return MemberwiseClone() as SocketAutoModRule;
	}
}
