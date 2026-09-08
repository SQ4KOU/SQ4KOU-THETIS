using System.Collections.Immutable;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class FunctionPointerParameterSymbol : ParameterSymbol
{
	private readonly FunctionPointerMethodSymbol _containingSymbol;

	public override TypeWithAnnotations TypeWithAnnotations { get; }

	public override RefKind RefKind { get; }

	public override int Ordinal { get; }

	public override Symbol ContainingSymbol => _containingSymbol;

	public override ImmutableArray<CustomModifier> RefCustomModifiers { get; }

	internal override bool HasEnumeratorCancellationAttribute => false;

	internal override ScopedKind DeclaredScope
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/FunctionPointers/FunctionPointerParameterSymbol.cs", 33);
		}
	}

	internal override ScopedKind EffectiveScope
	{
		get
		{
			if (!ParameterHelpers.IsRefScopedByDefault(this))
			{
				return ScopedKind.None;
			}
			return ScopedKind.ScopedRef;
		}
	}

	internal override bool HasUnscopedRefAttribute => false;

	internal override bool UseUpdatedEscapeRules => _containingSymbol.UseUpdatedEscapeRules;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override bool IsDiscard => false;

	public override bool IsParamsArray => false;

	public override bool IsParamsCollection => false;

	public override bool IsImplicitlyDeclared => true;

	internal override MarshalPseudoCustomAttributeData? MarshallingInformation => null;

	internal override bool IsMetadataOptional => false;

	internal override bool IsMetadataIn
	{
		get
		{
			RefKind refKind = RefKind;
			if (refKind - 3 <= RefKind.Ref)
			{
				return true;
			}
			return false;
		}
	}

	internal override bool IsMetadataOut => RefKind == RefKind.Out;

	internal override ConstantValue? ExplicitDefaultConstantValue => null;

	internal override ConstantValue? DefaultValueFromAttributes => null;

	internal override bool IsIDispatchConstant => false;

	internal override bool IsIUnknownConstant => false;

	internal override bool IsCallerFilePath => false;

	internal override bool IsCallerLineNumber => false;

	internal override bool IsCallerMemberName => false;

	internal override int CallerArgumentExpressionParameterIndex => -1;

	internal override FlowAnalysisAnnotations FlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

	internal override ImmutableHashSet<string> NotNullIfParameterNotNull => ImmutableHashSet<string>.Empty;

	internal override ImmutableArray<int> InterpolatedStringHandlerArgumentIndexes => ImmutableArray<int>.Empty;

	internal override bool HasInterpolatedStringHandlerArgumentError => false;

	public FunctionPointerParameterSymbol(TypeWithAnnotations typeWithAnnotations, RefKind refKind, int ordinal, FunctionPointerMethodSymbol containingSymbol, ImmutableArray<CustomModifier> refCustomModifiers)
	{
		TypeWithAnnotations = typeWithAnnotations;
		RefKind = refKind;
		Ordinal = ordinal;
		_containingSymbol = containingSymbol;
		RefCustomModifiers = refCustomModifiers;
	}

	public override bool Equals(Symbol other, TypeCompareKind compareKind)
	{
		if ((object)this == other)
		{
			return true;
		}
		if (!(other is FunctionPointerParameterSymbol other2))
		{
			return false;
		}
		return Equals(other2, compareKind);
	}

	internal bool Equals(FunctionPointerParameterSymbol other, TypeCompareKind compareKind)
	{
		if (other.Ordinal == Ordinal)
		{
			return _containingSymbol.Equals(other._containingSymbol, compareKind);
		}
		return false;
	}

	internal bool MethodEqualityChecks(FunctionPointerParameterSymbol other, TypeCompareKind compareKind)
	{
		if (FunctionPointerTypeSymbol.RefKindEquals(compareKind, RefKind, other.RefKind) && ((compareKind & TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds) != TypeCompareKind.ConsiderEverything || RefCustomModifiers.SequenceEqual(other.RefCustomModifiers)))
		{
			return TypeWithAnnotations.Equals(other.TypeWithAnnotations, compareKind);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(_containingSymbol.GetHashCode(), Ordinal + 1);
	}

	internal int MethodHashCode()
	{
		return Hash.Combine(TypeWithAnnotations.GetHashCode(), ((int)FunctionPointerTypeSymbol.GetRefKindForHashCode(RefKind)).GetHashCode());
	}
}
