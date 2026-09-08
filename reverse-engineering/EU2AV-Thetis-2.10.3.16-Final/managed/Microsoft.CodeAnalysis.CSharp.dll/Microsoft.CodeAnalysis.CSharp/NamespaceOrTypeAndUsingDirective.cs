using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal readonly struct NamespaceOrTypeAndUsingDirective(NamespaceOrTypeSymbol namespaceOrType, UsingDirectiveSyntax? usingDirective, ImmutableArray<AssemblySymbol> dependencies)
{
	public readonly NamespaceOrTypeSymbol NamespaceOrType = namespaceOrType;

	public readonly SyntaxReference? UsingDirectiveReference = usingDirective?.GetReference();

	public readonly ImmutableArray<AssemblySymbol> Dependencies = dependencies.NullToEmpty();

	public UsingDirectiveSyntax? UsingDirective => (UsingDirectiveSyntax)(UsingDirectiveReference?.GetSyntax());
}
