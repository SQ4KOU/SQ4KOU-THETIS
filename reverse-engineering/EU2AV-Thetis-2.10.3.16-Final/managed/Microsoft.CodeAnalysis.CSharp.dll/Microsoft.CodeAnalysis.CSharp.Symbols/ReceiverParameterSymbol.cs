using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class ReceiverParameterSymbol : RewrittenParameterSymbol
{
	private readonly NamedTypeSymbol _containingType;

	public override Symbol ContainingSymbol => _containingType;

	internal override bool HasEnumeratorCancellationAttribute => _underlyingParameter.HasEnumeratorCancellationAttribute;

	internal override ImmutableArray<int> InterpolatedStringHandlerArgumentIndexes => ImmutableArray<int>.Empty;

	internal override bool HasInterpolatedStringHandlerArgumentError => _underlyingParameter.HasInterpolatedStringHandlerArgumentError;

	public ReceiverParameterSymbol(NamedTypeSymbol containingType, ParameterSymbol originalParameter)
		: base(originalParameter)
	{
		_containingType = containingType;
	}

	internal sealed override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Extensions/ReceiverParameterSymbol.cs", 44);
	}
}
