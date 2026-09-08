using System;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.Emit;

internal readonly struct EncMappedMethod(IMethodSymbolInternal previousMethod, Func<SyntaxNode, SyntaxNode?>? syntaxMap, Func<SyntaxNode, RuntimeRudeEdit?>? runtimeRudeEdit)
{
	public readonly IMethodSymbolInternal PreviousMethod = previousMethod;

	public readonly Func<SyntaxNode, SyntaxNode?>? SyntaxMap = syntaxMap;

	public readonly Func<SyntaxNode, RuntimeRudeEdit?>? RuntimeRudeEdit = runtimeRudeEdit;
}
