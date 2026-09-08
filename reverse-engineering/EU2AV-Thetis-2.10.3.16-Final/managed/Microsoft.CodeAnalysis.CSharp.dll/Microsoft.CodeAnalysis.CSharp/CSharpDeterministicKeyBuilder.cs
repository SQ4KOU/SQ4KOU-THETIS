using System;
using System.Collections.Generic;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class CSharpDeterministicKeyBuilder : DeterministicKeyBuilder
{
	internal static readonly CSharpDeterministicKeyBuilder Instance = new CSharpDeterministicKeyBuilder();

	private CSharpDeterministicKeyBuilder()
	{
	}

	protected override void WriteCompilationOptionsCore(JsonWriter writer, CompilationOptions options)
	{
		if (!(options is CSharpCompilationOptions cSharpCompilationOptions))
		{
			throw new ArgumentException(null, "options");
		}
		base.WriteCompilationOptionsCore(writer, options);
		writer.Write("unsafe", cSharpCompilationOptions.AllowUnsafe);
		writer.Write("topLevelBinderFlags", cSharpCompilationOptions.TopLevelBinderFlags);
		writer.WriteKey("usings");
		writer.WriteArrayStart();
		foreach (string @using in cSharpCompilationOptions.Usings)
		{
			writer.Write(@using);
		}
		writer.WriteArrayEnd();
	}

	protected override void WriteParseOptionsCore(JsonWriter writer, ParseOptions parseOptions)
	{
		if (!(parseOptions is CSharpParseOptions cSharpParseOptions))
		{
			throw new ArgumentException(null, "parseOptions");
		}
		base.WriteParseOptionsCore(writer, parseOptions);
		writer.Write("languageVersion", cSharpParseOptions.LanguageVersion);
		writer.Write("specifiedLanguageVersion", cSharpParseOptions.SpecifiedLanguageVersion);
		writer.WriteKey("preprocessorSymbols");
		writer.WriteArrayStart();
		foreach (string item in ((IEnumerable<string>)cSharpParseOptions.PreprocessorSymbols).OrderBy((IComparer<string>?)StringComparer.Ordinal))
		{
			writer.Write(item);
		}
		writer.WriteArrayEnd();
	}
}
