using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis;

internal readonly struct UnifiedAssembly<TAssemblySymbol>(TAssemblySymbol targetAssembly, AssemblyIdentity originalReference) where TAssemblySymbol : class, IAssemblySymbolInternal
{
	internal readonly AssemblyIdentity OriginalReference = originalReference;

	internal readonly TAssemblySymbol TargetAssembly = targetAssembly;
}
