using System.Resources;
using FxResources.System.CodeDom;

namespace System;

internal static class SR
{
	private static readonly bool s_usingResourceKeys = GetUsingResourceKeysSwitchValue();

	private static ResourceManager s_resourceManager;

	internal static ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new ResourceManager(typeof(SR)));

	internal static string CodeDomProvider_NotDefined => GetResourceString("CodeDomProvider_NotDefined");

	internal static string NotSupported_CodeDomAPI => GetResourceString("NotSupported_CodeDomAPI");

	internal static string CodeGenOutputWriter => GetResourceString("CodeGenOutputWriter");

	internal static string CodeGenReentrance => GetResourceString("CodeGenReentrance");

	internal static string InvalidElementType => GetResourceString("InvalidElementType");

	internal static string Argument_NullComment => GetResourceString("Argument_NullComment");

	internal static string InvalidPrimitiveType => GetResourceString("InvalidPrimitiveType");

	internal static string InvalidIdentifier => GetResourceString("InvalidIdentifier");

	internal static string ArityDoesntMatch => GetResourceString("ArityDoesntMatch");

	internal static string InvalidNullEmptyArgument => GetResourceString("InvalidNullEmptyArgument");

	internal static string DuplicateFileName => GetResourceString("DuplicateFileName");

	internal static string InvalidTypeName => GetResourceString("InvalidTypeName");

	internal static string InvalidRegion => GetResourceString("InvalidRegion");

	internal static string InvalidPathCharsInChecksum => GetResourceString("InvalidPathCharsInChecksum");

	internal static string ExecTimeout => GetResourceString("ExecTimeout");

	internal static string Provider_does_not_support_options => GetResourceString("Provider_does_not_support_options");

	internal static string InvalidLanguageIdentifier => GetResourceString("InvalidLanguageIdentifier");

	internal static string toStringUnknown => GetResourceString("toStringUnknown");

	internal static string AutoGen_Comment_Line1 => GetResourceString("AutoGen_Comment_Line1");

	internal static string AutoGen_Comment_Line2 => GetResourceString("AutoGen_Comment_Line2");

	internal static string AutoGen_Comment_Line4 => GetResourceString("AutoGen_Comment_Line4");

	internal static string AutoGen_Comment_Line5 => GetResourceString("AutoGen_Comment_Line5");

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
