using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.CodeAnalysis.Scripting;

internal static class ScriptingResources
{
	private static ResourceManager s_resourceManager;

	internal static ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new ResourceManager(typeof(ScriptingResources)));

	internal static CultureInfo Culture { get; set; }

	internal static string StackOverflowWhileEvaluating => GetResourceString("StackOverflowWhileEvaluating");

	internal static string CantAssignTo => GetResourceString("CantAssignTo");

	internal static string ExpectedAnAssemblyReference => GetResourceString("ExpectedAnAssemblyReference");

	internal static string DisplayNameOrPathCannotBe => GetResourceString("DisplayNameOrPathCannotBe");

	internal static string AbsolutePathExpected => GetResourceString("AbsolutePathExpected");

	internal static string GlobalsNotAssignable => GetResourceString("GlobalsNotAssignable");

	internal static string StartingStateIncompatible => GetResourceString("StartingStateIncompatible");

	internal static string InvalidAssemblyName => GetResourceString("InvalidAssemblyName");

	internal static string InvalidCharactersInAssemblyName => GetResourceString("InvalidCharactersInAssemblyName");

	internal static string ScriptRequiresGlobalVariables => GetResourceString("ScriptRequiresGlobalVariables");

	internal static string GlobalVariablesWithoutGlobalType => GetResourceString("GlobalVariablesWithoutGlobalType");

	internal static string PlusAdditionalError => GetResourceString("PlusAdditionalError");

	internal static string PlusAdditionalErrors => GetResourceString("PlusAdditionalErrors");

	internal static string AtFileLine => GetResourceString("AtFileLine");

	internal static string CannotSetReadOnlyVariable => GetResourceString("CannotSetReadOnlyVariable");

	internal static string CannotSetConstantVariable => GetResourceString("CannotSetConstantVariable");

	internal static string HelpPrompt => GetResourceString("HelpPrompt");

	internal static string HelpText => GetResourceString("HelpText");

	internal static string AssemblyAlreadyLoaded => GetResourceString("AssemblyAlreadyLoaded");

	internal static string AssemblyAlreadyLoadedNotSigned => GetResourceString("AssemblyAlreadyLoadedNotSigned");

	internal static string CannotSetLanguageSpecificOption => GetResourceString("CannotSetLanguageSpecificOption");

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static string GetResourceString(string resourceKey, string defaultValue = null)
	{
		return ResourceManager.GetString(resourceKey, Culture);
	}
}
