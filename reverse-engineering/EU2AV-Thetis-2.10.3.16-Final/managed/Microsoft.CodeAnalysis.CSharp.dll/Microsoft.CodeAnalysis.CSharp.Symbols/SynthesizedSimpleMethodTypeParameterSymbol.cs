using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedSimpleMethodTypeParameterSymbol : TypeParameterSymbol
{
	private readonly MethodSymbol _container;

	private readonly int _ordinal;

	private readonly string _name;

	public override string Name => _name;

	public override int Ordinal => _ordinal;

	public override TypeParameterKind TypeParameterKind => TypeParameterKind.Method;

	public override bool HasConstructorConstraint => false;

	public override bool HasReferenceTypeConstraint => false;

	public override bool IsReferenceTypeFromConstraintTypes => false;

	internal override bool? ReferenceTypeConstraintIsNullable => false;

	public override bool HasNotNullConstraint => false;

	internal override bool? IsNotNullable => null;

	public override bool HasValueTypeConstraint => false;

	public override bool AllowsRefLikeType => false;

	public override bool IsValueTypeFromConstraintTypes => false;

	public override bool HasUnmanagedTypeConstraint => false;

	public override VarianceKind Variance => VarianceKind.None;

	public override Symbol ContainingSymbol => _container;

	public override ImmutableArray<Location> Locations
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SynthesizedSimpleMethodTypeParameterSymbol.cs", 98);
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SynthesizedSimpleMethodTypeParameterSymbol.cs", 103);
		}
	}

	public SynthesizedSimpleMethodTypeParameterSymbol(MethodSymbol container, int ordinal, string name)
	{
		_container = container;
		_ordinal = ordinal;
		_name = name;
	}

	internal override void EnsureAllConstraintsAreResolved()
	{
	}

	internal override ImmutableArray<TypeWithAnnotations> GetConstraintTypes(ConsList<TypeParameterSymbol> inProgress)
	{
		return ImmutableArray<TypeWithAnnotations>.Empty;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfaces(ConsList<TypeParameterSymbol> inProgress)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SynthesizedSimpleMethodTypeParameterSymbol.cs", 117);
	}

	internal override NamedTypeSymbol GetEffectiveBaseClass(ConsList<TypeParameterSymbol> inProgress)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SynthesizedSimpleMethodTypeParameterSymbol.cs", 122);
	}

	internal override TypeSymbol GetDeducedBaseType(ConsList<TypeParameterSymbol> inProgress)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/SynthesizedSimpleMethodTypeParameterSymbol.cs", 127);
	}
}
