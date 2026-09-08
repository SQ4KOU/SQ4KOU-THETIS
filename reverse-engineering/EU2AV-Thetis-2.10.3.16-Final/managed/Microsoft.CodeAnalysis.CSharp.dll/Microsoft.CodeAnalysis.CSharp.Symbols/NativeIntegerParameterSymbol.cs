using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class NativeIntegerParameterSymbol : WrappedParameterSymbol, IReference
{
	private readonly NativeIntegerTypeSymbol _containingType;

	private readonly NativeIntegerMethodSymbol _container;

	public override Symbol ContainingSymbol => _container;

	public override TypeWithAnnotations TypeWithAnnotations => _containingType.SubstituteUnderlyingType(_underlyingParameter.TypeWithAnnotations);

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _underlyingParameter.RefCustomModifiers;

	internal override bool IsCallerLineNumber => _underlyingParameter.IsCallerLineNumber;

	internal override bool IsCallerFilePath => _underlyingParameter.IsCallerFilePath;

	internal override bool IsCallerMemberName => _underlyingParameter.IsCallerMemberName;

	internal override int CallerArgumentExpressionParameterIndex => _underlyingParameter.CallerArgumentExpressionParameterIndex;

	internal override ImmutableArray<int> InterpolatedStringHandlerArgumentIndexes => _underlyingParameter.InterpolatedStringHandlerArgumentIndexes;

	internal override bool HasInterpolatedStringHandlerArgumentError => _underlyingParameter.HasInterpolatedStringHandlerArgumentError;

	internal override bool HasEnumeratorCancellationAttribute => _underlyingParameter.HasEnumeratorCancellationAttribute;

	internal NativeIntegerParameterSymbol(NativeIntegerTypeSymbol containingType, NativeIntegerMethodSymbol container, ParameterSymbol underlyingParameter)
		: base(underlyingParameter)
	{
		_containingType = containingType;
		_container = container;
	}

	public override bool Equals(Symbol? other, TypeCompareKind comparison)
	{
		return NativeIntegerTypeSymbol.EqualsHelper(this, other, comparison, (NativeIntegerParameterSymbol symbol) => symbol._underlyingParameter);
	}

	public override int GetHashCode()
	{
		return _underlyingParameter.GetHashCode();
	}

	internal sealed override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/NativeIntegerTypeSymbol.cs", 474);
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/NativeIntegerTypeSymbol.cs", 480);
	}
}
