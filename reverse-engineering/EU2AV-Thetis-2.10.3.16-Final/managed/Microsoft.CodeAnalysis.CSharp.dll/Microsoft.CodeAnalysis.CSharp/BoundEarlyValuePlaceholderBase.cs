using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundEarlyValuePlaceholderBase : BoundValuePlaceholderBase
{
	protected BoundEarlyValuePlaceholderBase(BoundKind kind, SyntaxNode syntax, TypeSymbol? type, bool hasErrors)
		: base(kind, syntax, type, hasErrors)
	{
	}

	protected BoundEarlyValuePlaceholderBase(BoundKind kind, SyntaxNode syntax, TypeSymbol? type)
		: base(kind, syntax, type)
	{
	}
}
