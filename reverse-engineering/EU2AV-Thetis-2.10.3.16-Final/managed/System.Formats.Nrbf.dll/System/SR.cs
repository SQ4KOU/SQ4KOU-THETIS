using System.Resources;
using FxResources.System.Formats.Nrbf;

namespace System;

internal static class SR
{
	private static readonly bool s_usingResourceKeys = GetUsingResourceKeysSwitchValue();

	private static ResourceManager s_resourceManager;

	internal static ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new ResourceManager(typeof(SR)));

	internal static string Serialization_ArrayContainedNulls => GetResourceString("Serialization_ArrayContainedNulls");

	internal static string Serialization_InvalidValue => GetResourceString("Serialization_InvalidValue");

	internal static string Serialization_UnexpectedNullRecordCount => GetResourceString("Serialization_UnexpectedNullRecordCount");

	internal static string NotSupported_RecordType => GetResourceString("NotSupported_RecordType");

	internal static string Serialization_InvalidReference => GetResourceString("Serialization_InvalidReference");

	internal static string Serialization_InvalidTypeName => GetResourceString("Serialization_InvalidTypeName");

	internal static string Serialization_TypeMismatch => GetResourceString("Serialization_TypeMismatch");

	internal static string Serialization_InvalidTypeOrAssemblyName => GetResourceString("Serialization_InvalidTypeOrAssemblyName");

	internal static string Serialization_DuplicateMemberName => GetResourceString("Serialization_DuplicateMemberName");

	internal static string Argument_NonSeekableStream => GetResourceString("Argument_NonSeekableStream");

	internal static string Serialization_DuplicateSerializationRecordId => GetResourceString("Serialization_DuplicateSerializationRecordId");

	internal static string Serialization_MemberTypeMismatchException => GetResourceString("Serialization_MemberTypeMismatchException");

	internal static string NotSupported_NonZeroOffsets => GetResourceString("NotSupported_NonZeroOffsets");

	internal static string Serialization_InvalidAssemblyName => GetResourceString("Serialization_InvalidAssemblyName");

	internal static string Serialization_InvalidFormat => GetResourceString("Serialization_InvalidFormat");

	internal static string Serialization_SurrogateCharacter => GetResourceString("Serialization_SurrogateCharacter");

	private static bool GetUsingResourceKeysSwitchValue()
	{
		if (!AppContext.TryGetSwitch("System.Resources.UseSystemResourceKeys", out var isEnabled))
		{
			return false;
		}
		return isEnabled;
	}

	internal static bool UsingResourceKeys()
	{
		return s_usingResourceKeys;
	}

	private static string GetResourceString(string resourceKey)
	{
		if (UsingResourceKeys())
		{
			return resourceKey;
		}
		string result = null;
		try
		{
			result = ResourceManager.GetString(resourceKey);
		}
		catch (MissingManifestResourceException)
		{
		}
		return result;
	}

	private static string GetResourceString(string resourceKey, string defaultString)
	{
		string resourceString = GetResourceString(resourceKey);
		if (!(resourceKey == resourceString) && resourceString != null)
		{
			return resourceString;
		}
		return defaultString;
	}

	internal static string Format(string resourceFormat, object p1)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1);
		}
		return string.Format(resourceFormat, p1);
	}

	internal static string Format(string resourceFormat, object p1, object p2)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2);
		}
		return string.Format(resourceFormat, p1, p2);
	}

	internal static string Format(string resourceFormat, object p1, object p2, object p3)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2, p3);
		}
		return string.Format(resourceFormat, p1, p2, p3);
	}

	internal static string Format(string resourceFormat, params object[] args)
	{
		if (args != null)
		{
			if (UsingResourceKeys())
			{
				return resourceFormat + ", " + string.Join(", ", args);
			}
			return string.Format(resourceFormat, args);
		}
		return resourceFormat;
	}

	internal static string Format(IFormatProvider provider, string resourceFormat, object p1)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1);
		}
		return string.Format(provider, resourceFormat, p1);
	}

	internal static string Format(IFormatProvider provider, string resourceFormat, object p1, object p2)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2);
		}
		return string.Format(provider, resourceFormat, p1, p2);
	}

	internal static string Format(IFormatProvider provider, string resourceFormat, object p1, object p2, object p3)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2, p3);
		}
		return string.Format(provider, resourceFormat, p1, p2, p3);
	}

	internal static string Format(IFormatProvider provider, string resourceFormat, params object[] args)
	{
		if (args != null)
		{
			if (UsingResourceKeys())
			{
				return resourceFormat + ", " + string.Join(", ", args);
			}
			return string.Format(provider, resourceFormat, args);
		}
		return resourceFormat;
	}
}
