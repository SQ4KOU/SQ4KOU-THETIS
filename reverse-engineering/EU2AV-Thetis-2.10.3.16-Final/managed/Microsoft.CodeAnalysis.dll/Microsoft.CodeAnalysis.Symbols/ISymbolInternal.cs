using System.Collections.Immutable;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Symbols;

internal interface ISymbolInternal
{
	SymbolKind Kind { get; }

	string Name { get; }

	string MetadataName { get; }

	int MetadataToken { get; }

	TypeMemberVisibility MetadataVisibility { get; }

	Compilation DeclaringCompilation { get; }

	ISymbolInternal ContainingSymbol { get; }

	IAssemblySymbolInternal ContainingAssembly { get; }

	IModuleSymbolInternal ContainingModule { get; }

	INamedTypeSymbolInternal ContainingType { get; }

	INamespaceSymbolInternal ContainingNamespace { get; }

	bool IsDefinition { get; }

	ImmutableArray<Location> Locations { get; }

	bool IsImplicitlyDeclared { get; }

	Accessibility DeclaredAccessibility { get; }

	bool IsStatic { get; }

	bool IsVirtual { get; }

	bool IsOverride { get; }

	bool IsAbstract { get; }

	bool IsExtern { get; }

	bool Equals(ISymbolInternal? other, TypeCompareKind compareKind);

	Location GetFirstLocation();

	Location GetFirstLocationOrNone();

	ISymbol GetISymbol();

	IReference GetCciAdapter();

	bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithinSpan, CancellationToken cancellationToken = default(CancellationToken));
}
