using System;

namespace Microsoft.CodeAnalysis;

public interface IFunctionPointerTypeSymbol : ITypeSymbol, INamespaceOrTypeSymbol, ISymbol, IEquatable<ISymbol?>
{
	IMethodSymbol Signature { get; }
}
