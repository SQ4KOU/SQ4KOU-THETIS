using System.Collections.Generic;

namespace Discord;

public interface IGuildOnboardingPrompt : ISnowflakeEntity, IEntity<ulong>
{
	IReadOnlyCollection<IGuildOnboardingPromptOption> Options { get; }

	string Title { get; }

	bool IsSingleSelect { get; }

	bool IsRequired { get; }

	bool IsInOnboarding { get; }

	GuildOnboardingPromptType Type { get; }
}
