using System;
using Microsoft.CodeAnalysis.Scripting;

namespace Microsoft.CodeAnalysis.CSharp.Scripting;

public static class ScriptOptionsExtensions
{
	public static ScriptOptions WithLanguageVersion(this ScriptOptions options, LanguageVersion languageVersion)
	{
		CSharpParseOptions cSharpParseOptions2;
		if ((object)options.ParseOptions != null)
		{
			if (!(options.ParseOptions is CSharpParseOptions cSharpParseOptions))
			{
				throw new InvalidOperationException(string.Format(ScriptingResources.CannotSetLanguageSpecificOption, "C#", "LanguageVersion"));
			}
			cSharpParseOptions2 = cSharpParseOptions;
		}
		else
		{
			cSharpParseOptions2 = CSharpScriptCompiler.DefaultParseOptions;
		}
		CSharpParseOptions cSharpParseOptions3 = cSharpParseOptions2;
		return options.WithParseOptions(cSharpParseOptions3.WithLanguageVersion(languageVersion));
	}
}
