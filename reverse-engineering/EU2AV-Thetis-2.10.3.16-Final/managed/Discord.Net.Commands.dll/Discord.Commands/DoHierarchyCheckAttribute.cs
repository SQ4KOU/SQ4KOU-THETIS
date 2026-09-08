using System;
using System.Threading.Tasks;

namespace Discord.Commands;

public class DoHierarchyCheckAttribute : ParameterPreconditionAttribute
{
	public string NotAGuildErrorMessage { get; set; } = "This command cannot be used outside of a guild.";

	public string ErrorMessage { get; set; } = "You cannot target anyone who is higher or equal in the hierarchy to you or the bot.";

	public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameterInfo, object value, IServiceProvider services)
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
