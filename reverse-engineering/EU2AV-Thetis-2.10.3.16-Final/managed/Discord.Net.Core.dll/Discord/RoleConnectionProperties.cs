using System;
using System.Collections.Generic;

namespace Discord;

public class RoleConnectionProperties
{
	private const int MaxPlatformNameLength = 50;

	private const int MaxPlatformUsernameLength = 100;

	private const int MaxMetadataRecords = 100;

	private string _platformName;

	private string _platformUsername;

	private Dictionary<string, string> _metadata;

	public string PlatformName
	{
		get
		{
			return _platformName;
		}
		set
		{
			if (value != null)
			{
				Preconditions.AtMost(value.Length, 50, "PlatformName", $"Platform name length must be less or equal to {50}");
			}
			_platformName = value;
		}
	}

	public string PlatformUsername
	{
		get
		{
			return _platformUsername;
		}
		set
		{
			if (value != null)
			{
				Preconditions.AtMost(value.Length, 100, "PlatformUsername", $"Platform username length must be less or equal to {100}");
			}
			_platformUsername = value;
		}
	}

	public Dictionary<string, string> Metadata
	{
		get
		{
			return _metadata;
		}
		set
		{
			if (value != null)
			{
				Preconditions.AtMost(value.Count, 100, "Metadata", $"Metadata records count must be less or equal to {100}");
			}
			_metadata = value;
		}
	}

	public RoleConnectionProperties WithDate(string key, DateTimeOffset value)
	{
		return AddMetadataRecord(key, value.ToString("O"));
	}

	public RoleConnectionProperties WithBool(string key, bool value)
	{
		return AddMetadataRecord(key, value ? "1" : "0");
	}

	public RoleConnectionProperties WithNumber(string key, int value)
	{
		return AddMetadataRecord(key, value.ToString());
	}

	public RoleConnectionProperties WithNumber(string key, uint value)
	{
		return AddMetadataRecord(key, value.ToString());
	}

	public RoleConnectionProperties WithNumber(string key, long value)
	{
		return AddMetadataRecord(key, value.ToString());
	}

	public RoleConnectionProperties WithNumber(string key, ulong value)
	{
		return AddMetadataRecord(key, value.ToString());
	}

	internal RoleConnectionProperties AddMetadataRecord(string key, string value)
	{
		if (Metadata == null)
		{
			Dictionary<string, string> dictionary = (Metadata = new Dictionary<string, string>());
		}
		if (!Metadata.ContainsKey(key))
		{
			Preconditions.AtMost(Metadata.Count + 1, 100, "Metadata", $"Metadata records count must be less or equal to {100}");
		}
		_metadata[key] = value;
		return this;
	}

	public RoleConnectionProperties(string platformName, string platformUsername, IDictionary<string, string> metadata = null)
	{
		PlatformName = platformName;
		PlatformUsername = platformUsername;
		Metadata = metadata?.ToDictionary() ?? new Dictionary<string, string>();
	}

	public RoleConnectionProperties()
	{
		Metadata = new Dictionary<string, string>();
	}

	public static RoleConnectionProperties FromRoleConnection(RoleConnection roleConnection)
	{
		return new RoleConnectionProperties
		{
			PlatformName = roleConnection.PlatformName,
			PlatformUsername = roleConnection.PlatformUsername,
			Metadata = roleConnection.Metadata?.ToDictionary()
		};
	}
}
