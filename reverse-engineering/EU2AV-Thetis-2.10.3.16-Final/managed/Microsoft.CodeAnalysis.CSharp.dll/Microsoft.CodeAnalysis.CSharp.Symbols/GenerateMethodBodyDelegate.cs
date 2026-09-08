namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal delegate BoundStatement GenerateMethodBodyDelegate(SyntheticBoundNodeFactory factory, MethodSymbol method, MethodSymbol interfaceMethod);
