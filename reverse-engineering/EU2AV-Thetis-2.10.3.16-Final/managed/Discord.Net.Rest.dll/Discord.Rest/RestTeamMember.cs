using System;
using Discord.API;

namespace Discord.Rest;

public class RestTeamMember : ITeamMember
{
	public MembershipState MembershipState { get; }

	public string[] Permissions { get; }

	public ulong TeamId { get; }

	public IUser User { get; }

	public TeamRole Role { get; }

	internal RestTeamMember(BaseDiscordClient discord, TeamMember model, Team team)
	{
		MembershipState = model.MembershipState switch
		{
			Discord.API.MembershipState.Invited => MembershipState.Invited, 
			Discord.API.MembershipState.Accepted => MembershipState.Accepted, 
			_ => throw new InvalidOperationException("Invalid membership state"), 
		};
		Permissions = model.Permissions;
		TeamId = model.TeamId;
		User = RestUser.Create(discord, model.User);
		if (team.OwnerUserId == model.User.Id)
		{
			Role = TeamRole.Owner;
			return;
		}
		Role = model.Role switch
		{
			"admin" => TeamRole.Admin, 
			"developer" => TeamRole.Developer, 
			"read_only" => TeamRole.ReadOnly, 
			_ => TeamRole.Owner, 
		};
	}
}
