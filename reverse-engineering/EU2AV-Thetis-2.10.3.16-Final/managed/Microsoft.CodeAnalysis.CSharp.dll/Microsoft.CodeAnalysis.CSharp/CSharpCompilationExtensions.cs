using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class CSharpCompilationExtensions
{
	internal static bool IsFeatureEnabled(this CSharpCompilation compilation, MessageID feature)
	{
		return compilation.LanguageVersion >= feature.RequiredVersion();
	}

	internal static bool IsFeatureEnabled(this SyntaxNode? syntax, MessageID feature)
	{
		return ((CSharpParseOptions)(syntax?.SyntaxTree.Options))?.IsFeatureEnabled(feature) ?? false;
	}

	internal static bool ShouldEmitNativeIntegerAttributes(this CSharpCompilation compilation, TypeSymbol type)
	{
		if (compilation.ShouldEmitNativeIntegerAttributes())
		{
			return type.ContainsNativeIntegerWrapperType();
		}
		return false;
	}
}
