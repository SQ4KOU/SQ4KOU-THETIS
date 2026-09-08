namespace Microsoft.CodeAnalysis.Symbols;

internal interface IPropertySymbolInternal : ISymbolInternal
{
	IPropertySymbolInternal? PartialImplementationPart { get; }

	IPropertySymbolInternal? PartialDefinitionPart { get; }
}
