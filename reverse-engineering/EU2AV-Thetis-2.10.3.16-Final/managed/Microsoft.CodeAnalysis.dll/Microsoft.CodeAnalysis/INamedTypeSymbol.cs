using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis;

public interface INamedTypeSymbol : ITypeSymbol, INamespaceOrTypeSymbol, ISymbol, IEquatable<ISymbol?>
{
	int Arity { get; }

	bool IsGenericType { get; }

	bool IsUnboundGenericType { get; }

	bool IsScriptClass { get; }

	bool IsImplicitClass { get; }

	bool IsComImport { get; }

	bool IsFileLocal { get; }

	IEnumerable<string> MemberNames { get; }

	ImmutableArray<ITypeParameterSymbol> TypeParameters { get; }

	ImmutableArray<ITypeSymbol> TypeArguments { get; }

	ImmutableArray<NullableAnnotation> TypeArgumentNullableAnnotations { get; }

	new INamedTypeSymbol OriginalDefinition { get; }

	IMethodSymbol? DelegateInvokeMethod { get; }

	INamedTypeSymbol? EnumUnderlyingType { get; }

	INamedTypeSymbol ConstructedFrom { get; }

	ImmutableArray<IMethodSymbol> InstanceConstructors { get; }

	ImmutableArray<IMethodSymbol> StaticConstructors { get; }

	ImmutableArray<IMethodSymbol> Constructors { get; }

	ISymbol? AssociatedSymbol { get; }

	bool MightContainExtensionMethods { get; }

	INamedTypeSymbol? TupleUnderlyingType { get; }

	ImmutableArray<IFieldSymbol> TupleElements { get; }

	bool IsSerializable { get; }

	INamedTypeSymbol? NativeIntegerUnderlyingType { get; }

	[MemberNotNullWhen(true, new string[] { "ExtensionGroupingName", "ExtensionMarkerName" })]
	new bool IsExtension
	{
		[MemberNotNullWhen(true, new string[] { "ExtensionGroupingName", "ExtensionMarkerName" })]
		get;
	}

	new IParameterSymbol? ExtensionParameter { get; }

	string? ExtensionGroupingName { get; }

	string? ExtensionMarkerName { get; }

	ImmutableArray<CustomModifier> GetTypeArgumentCustomModifiers(int ordinal);

	INamedTypeSymbol Construct(params ITypeSymbol[] typeArguments);

	INamedTypeSymbol Construct(ImmutableArray<ITypeSymbol> typeArguments, ImmutableArray<NullableAnnotation> typeArgumentNullableAnnotations);

	INamedTypeSymbol ConstructUnboundGenericType();
}
