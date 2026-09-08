using System;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class RequireTeamAttribute : PreconditionAttribute
{
	public string[] TeamRoles { get; } = Array.Empty<string>();

	public RequireTeamAttribute(params string[] teamRoles)
	{
		TeamRoles = teamRoles ?? TeamRoles;
	}

	public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo command, IServiceProvider services)
	{
		if (context.Client.TokenType == TokenType.Bot)
		{
			IApplication obj = await context.Client.GetApplicationInfoAsync().ConfigureAwait(continueOnCapturedContext: false);
			bool flag = false;
			foreach (ITeamMember member in obj.Team.TeamMembers)
			{
				if (member.User.Id == context.User.Id)
				{
					if (TeamRoles.Length == 0 || TeamRoles.Any((string role) => Enumerable.Contains(member.Permissions, role)))
					{
						flag = true;
					}
					break;
				}
			}
			if (!flag)
			{
				return PreconditionResult.FromError(ErrorMessage ?? ("Command can only be run by a member of the bot's team " + ((TeamRoles.Length == 0) ? "." : "with the specified permissions.")));
			}
			return PreconditionResult.FromSuccess();
		}
		return PreconditionResult.FromError("RequireTeamAttribute is not supported by this TokenType.");
	}
}
