using System.Collections.Generic;
using System.Collections.Immutable;

namespace Discord;

public class ApplicationInstallParams
{
	public IReadOnlyCollection<string> Scopes { get; }

	public GuildPermission Permission { get; }

	public ApplicationInstallParams(string[] scopes, GuildPermission permission)
	{
		Preconditions.NotNull(scopes, "scopes");
		for (int i = 0; i < scopes.Length; i++)
		{
			Preconditions.NotNull(scopes[i], "scopes");
		}
		Scopes = ((IEnumerable<string>)scopes).ToImmutableArray();
		Permission = permission;
	}
}
