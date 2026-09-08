namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedPropertyAccessorValueParameterSymbol : SynthesizedAccessorValueParameterSymbol
{
	public override TypeWithAnnotations TypeWithAnnotations => ((PropertySymbol)((SourcePropertyAccessorSymbol)ContainingSymbol).AssociatedSymbol).TypeWithAnnotations;

	public SynthesizedPropertyAccessorValueParameterSymbol(SourcePropertyAccessorSymbol accessor, int ordinal)
		: base(accessor, ordinal)
	{
	}
}
