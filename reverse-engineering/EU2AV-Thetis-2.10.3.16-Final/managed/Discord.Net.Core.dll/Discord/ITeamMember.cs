namespace Discord;

public interface ITeamMember
{
	MembershipState MembershipState { get; }

	string[] Permissions { get; }

	ulong TeamId { get; }

	IUser User { get; }

	TeamRole Role { get; }
}
