using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal readonly struct AliasAndExternAliasDirective(AliasSymbol alias, ExternAliasDirectiveSyntax? externAliasDirective, bool skipInLookup)
{
	public readonly AliasSymbol Alias = alias;

	public readonly SyntaxReference? ExternAliasDirectiveReference = externAliasDirective?.GetReference();

	public readonly bool SkipInLookup = skipInLookup;

	public ExternAliasDirectiveSyntax? ExternAliasDirective => (ExternAliasDirectiveSyntax)(ExternAliasDirectiveReference?.GetSyntax());
}
