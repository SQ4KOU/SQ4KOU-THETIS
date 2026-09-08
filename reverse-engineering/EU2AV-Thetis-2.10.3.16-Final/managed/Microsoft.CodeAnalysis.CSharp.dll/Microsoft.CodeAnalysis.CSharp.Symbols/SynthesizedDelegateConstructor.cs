using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedDelegateConstructor : SynthesizedInstanceConstructor
{
	private readonly ImmutableArray<ParameterSymbol> _parameters;

	public override ImmutableArray<ParameterSymbol> Parameters => _parameters;

	public SynthesizedDelegateConstructor(NamedTypeSymbol containingType, TypeSymbol objectType, TypeSymbol intPtrType)
		: base(containingType)
	{
		_parameters = ImmutableArray.Create(SynthesizedParameterSymbol.Create(this, TypeWithAnnotations.Create(objectType), 0, RefKind.None, "object"), SynthesizedParameterSymbol.Create(this, TypeWithAnnotations.Create(intPtrType), 1, RefKind.None, "method"));
	}
}
