using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceNotOverridingMethodTypeParameterSymbol : SourceMethodTypeParameterSymbol
{
	private readonly SourceMethodSymbol _owner;

	public override SourceMethodSymbol Owner => _owner;

	public override bool HasConstructorConstraint => (GetConstraintKinds() & TypeParameterConstraintKind.Constructor) != 0;

	public override bool HasValueTypeConstraint => (GetConstraintKinds() & TypeParameterConstraintKind.AllValueTypeKinds) != 0;

	public override bool AllowsRefLikeType => (GetConstraintKinds() & TypeParameterConstraintKind.AllowByRefLike) != 0;

	public override bool IsValueTypeFromConstraintTypes => (GetConstraintKinds() & TypeParameterConstraintKind.ValueTypeFromConstraintTypes) != 0;

	public override bool HasReferenceTypeConstraint => (GetConstraintKinds() & TypeParameterConstraintKind.ReferenceType) != 0;

	public override bool IsReferenceTypeFromConstraintTypes => (GetConstraintKinds() & TypeParameterConstraintKind.ReferenceTypeFromConstraintTypes) != 0;

	public override bool HasNotNullConstraint => (GetConstraintKinds() & TypeParameterConstraintKind.NotNull) != 0;

	internal override bool? ReferenceTypeConstraintIsNullable => CalculateReferenceTypeConstraintIsNullable(GetConstraintKinds());

	internal override bool? IsNotNullable
	{
		get
		{
			if ((GetConstraintKinds() & TypeParameterConstraintKind.ObliviousNullabilityIfReferenceType) != TypeParameterConstraintKind.None)
			{
				return null;
			}
			return CalculateIsNotNullable();
		}
	}

	public override bool HasUnmanagedTypeConstraint => (GetConstraintKinds() & TypeParameterConstraintKind.Unmanaged) != 0;

	public SourceNotOverridingMethodTypeParameterSymbol(SourceMethodSymbol owner, string name, int ordinal, ImmutableArray<Location> locations, ImmutableArray<SyntaxReference> syntaxRefs)
		: base(name, ordinal, locations, syntaxRefs)
	{
		_owner = owner;
	}

	protected override TypeParameterBounds ResolveBounds(ConsList<TypeParameterSymbol> inProgress, BindingDiagnosticBag diagnostics)
	{
		ImmutableArray<ImmutableArray<TypeWithAnnotations>> typeParameterConstraintTypes = _owner.GetTypeParameterConstraintTypes();
		ImmutableArray<TypeWithAnnotations> constraintTypes = (typeParameterConstraintTypes.IsEmpty ? ImmutableArray<TypeWithAnnotations>.Empty : typeParameterConstraintTypes[Ordinal]);
		if (constraintTypes.IsEmpty && GetConstraintKinds() == TypeParameterConstraintKind.None)
		{
			return null;
		}
		return this.ResolveBounds(ContainingAssembly.CorLibrary, inProgress.Prepend(this), constraintTypes, inherited: false, DeclaringCompilation, diagnostics);
	}

	private TypeParameterConstraintKind GetConstraintKinds()
	{
		ImmutableArray<TypeParameterConstraintKind> typeParameterConstraintKinds = _owner.GetTypeParameterConstraintKinds();
		if (!typeParameterConstraintKinds.IsEmpty)
		{
			return typeParameterConstraintKinds[Ordinal];
		}
		return TypeParameterConstraintKind.None;
	}
}
