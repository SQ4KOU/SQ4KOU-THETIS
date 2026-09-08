using Microsoft.CodeAnalysis.CodeGen;

namespace Microsoft.CodeAnalysis.Symbols;

internal interface ISynthesizedGlobalMethodSymbol
{
	PrivateImplementationDetails ContainingPrivateImplementationDetailsType { get; }
}
