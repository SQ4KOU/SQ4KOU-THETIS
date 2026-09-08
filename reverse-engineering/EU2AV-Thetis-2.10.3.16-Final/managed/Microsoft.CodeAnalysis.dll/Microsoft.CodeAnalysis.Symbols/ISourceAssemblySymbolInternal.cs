using System.Reflection;

namespace Microsoft.CodeAnalysis.Symbols;

internal interface ISourceAssemblySymbolInternal : IAssemblySymbolInternal, ISymbolInternal
{
	AssemblyFlags AssemblyFlags { get; }

	string? SignatureKey { get; }

	AssemblyHashAlgorithm HashAlgorithm { get; }

	bool InternalsAreVisible { get; }
}
