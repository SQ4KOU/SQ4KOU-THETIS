using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.CodeAnalysis;

[InternalImplementationOnly]
public interface ISymbol : IEquatable<ISymbol?>
{
	SymbolKind Kind { get; }

	string Language { get; }

	string Name { get; }

	string MetadataName { get; }

	int MetadataToken { get; }

	ISymbol ContainingSymbol { get; }

	IAssemblySymbol ContainingAssembly { get; }

	IModuleSymbol ContainingModule { get; }

	INamedTypeSymbol ContainingType { get; }

	INamespaceSymbol ContainingNamespace { get; }

	bool IsDefinition { get; }

	bool IsStatic { get; }

	bool IsVirtual { get; }

	bool IsOverride { get; }

	bool IsAbstract { get; }

	bool IsSealed { get; }

	bool IsExtern { get; }

	bool IsImplicitlyDeclared { get; }

	bool CanBeReferencedByName { get; }

	ImmutableArray<Location> Locations { get; }

	ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	Accessibility DeclaredAccessibility { get; }

	ISymbol OriginalDefinition { get; }

	bool HasUnsupportedMetadata { get; }

	ImmutableArray<AttributeData> GetAttributes();

	void Accept(SymbolVisitor visitor);

	TResult? Accept<TResult>(SymbolVisitor<TResult> visitor);

	TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument);

	string? GetDocumentationCommentId();

	string? GetDocumentationCommentXml(CultureInfo? preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken));

	string ToDisplayString(SymbolDisplayFormat? format = null);

	ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat? format = null);

	string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null);

	ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null);

	bool Equals([NotNullWhen(true)] ISymbol? other, SymbolEqualityComparer equalityComparer);
}
