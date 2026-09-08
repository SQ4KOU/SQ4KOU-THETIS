using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal static class CommandLineHelpers
{
	public static ImmutableArray<string> GetImports(CommandLineArguments args)
	{
		return args.CompilationOptions.GetImports();
	}

	internal static ScriptOptions RemoveImportsAndReferences(this ScriptOptions options)
	{
		return options.WithReferences(Array.Empty<MetadataReference>()).WithImports();
	}
}
