namespace Discord;

public class AutoModRuleProperties
{
	public const int MaxKeywordCount = 1000;

	public const int MaxKeywordLength = 60;

	public const int MaxRegexPatternCount = 10;

	public const int MaxRegexPatternLength = 260;

	public const int MaxAllowListCountKeyword = 100;

	public const int MaxAllowListCountKeywordPreset = 1000;

	public const int MaxAllowListEntryLength = 60;

	public const int MaxMentionLimit = 50;

	public const int MaxExemptRoles = 20;

	public const int MaxExemptChannels = 50;

	public const int MaxTimeoutSeconds = 2419200;

	public const int MaxCustomBlockMessageLength = 150;

	public Optional<string> Name { get; set; }

	public Optional<AutoModEventType> EventType { get; set; }

	public Optional<AutoModTriggerType> TriggerType { get; set; }

	public Optional<string[]> KeywordFilter { get; set; }

	public Optional<string[]> RegexPatterns { get; set; }

	public Optional<string[]> AllowList { get; set; }

	public Optional<int> MentionLimit { get; set; }

	public Optional<KeywordPresetTypes[]> Presets { get; set; }

	public Optional<AutoModRuleActionProperties[]> Actions { get; set; }

	public Optional<bool> Enabled { get; set; }

	public Optional<ulong[]> ExemptRoles { get; set; }

	public Optional<ulong[]> ExemptChannels { get; set; }

	public Optional<bool> MentionRaidProtectionEnabled { get; set; }
}
