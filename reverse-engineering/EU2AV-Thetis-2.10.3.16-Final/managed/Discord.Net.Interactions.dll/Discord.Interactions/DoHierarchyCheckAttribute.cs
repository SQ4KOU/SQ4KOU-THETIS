using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

public class DoHierarchyCheckAttribute : ParameterPreconditionAttribute
{
	public string NotAGuildErrorMessage { get; set; } = "This command cannot be used outside of a guild.";

	public override string ErrorMessage => "You cannot target anyone who is higher or equal in the hierarchy to you or the bot.";

	public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, IParameterInfo parameterInfo, object value, IServiceProvider services)
	{
		if (!(context.User is IGuildUser guildUser))
		{
			return PreconditionResult.FromError(NotAGuildErrorMessage);
		}
		int hieararchy = PermissionUtils.GetHieararchy(value);
		bool flag = hieararchy >= guildUser.Hierarchy;
		if (!flag)
		{
			int num = hieararchy;
			flag = num >= (await context.Guild.GetCurrentUserAsync().ConfigureAwait(continueOnCapturedContext: false)).Hierarchy;
		}
		if (flag)
		{
			return PreconditionResult.FromError(ErrorMessage);
		}
		return PreconditionResult.FromSuccess();
	}
}
