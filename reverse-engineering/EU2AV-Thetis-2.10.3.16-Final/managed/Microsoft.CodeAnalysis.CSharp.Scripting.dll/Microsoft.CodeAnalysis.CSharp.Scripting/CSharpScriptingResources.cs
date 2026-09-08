using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.CodeAnalysis.CSharp.Scripting;

internal static class CSharpScriptingResources
{
	private static ResourceManager s_resourceManager;

	internal static ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new ResourceManager(typeof(CSharpScriptingResources)));

	internal static CultureInfo Culture { get; set; }

	internal static string LogoLine1 => GetResourceString("LogoLine1");

	internal static string LogoLine2 => GetResourceString("LogoLine2");

	internal static string InteractiveHelp => GetResourceString("InteractiveHelp");

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static string GetResourceString(string resourceKey, string defaultValue = null)
	{
		return ResourceManager.GetString(resourceKey, Culture);
	}
}
