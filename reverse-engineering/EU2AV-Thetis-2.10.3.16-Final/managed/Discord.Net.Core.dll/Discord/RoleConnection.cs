using System.Collections.Generic;

namespace Discord;

public class RoleConnection
{
	public string PlatformName { get; }

	public string PlatformUsername { get; }

	public IReadOnlyDictionary<string, string> Metadata { get; }

	internal RoleConnection(string platformName, string platformUsername, IReadOnlyDictionary<string, string> metadata)
	{
		PlatformName = platformName;
		PlatformUsername = platformUsername;
		Metadata = metadata;
	}

	public RoleConnectionProperties ToRoleConnectionProperties()
	{
		return new RoleConnectionProperties
		{
			PlatformName = PlatformName,
			PlatformUsername = PlatformUsername,
			Metadata = Metadata.ToDictionary()
		};
	}
}
