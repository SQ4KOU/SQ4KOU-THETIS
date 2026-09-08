using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class TypeParameterInfo
{
	public ImmutableArray<TypeParameterSymbol> LazyTypeParameters;

	public ImmutableArray<ImmutableArray<TypeWithAnnotations>> LazyTypeParameterConstraintTypes;

	public ImmutableArray<TypeParameterConstraintKind> LazyTypeParameterConstraintKinds;

	public static readonly TypeParameterInfo Empty = new TypeParameterInfo
	{
		LazyTypeParameters = ImmutableArray<TypeParameterSymbol>.Empty,
		LazyTypeParameterConstraintTypes = ImmutableArray<ImmutableArray<TypeWithAnnotations>>.Empty,
		LazyTypeParameterConstraintKinds = ImmutableArray<TypeParameterConstraintKind>.Empty
	};
}
