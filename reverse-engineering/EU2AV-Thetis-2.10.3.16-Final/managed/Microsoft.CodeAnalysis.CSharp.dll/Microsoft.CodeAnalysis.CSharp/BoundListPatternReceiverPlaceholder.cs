using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundListPatternReceiverPlaceholder : BoundEarlyValuePlaceholderBase
{
	public sealed override bool IsEquivalentToThisReference => false;

	public new TypeSymbol Type => base.Type;

	public BoundListPatternReceiverPlaceholder(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.ListPatternReceiverPlaceholder, syntax, type, hasErrors)
	{
	}

	public BoundListPatternReceiverPlaceholder(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.ListPatternReceiverPlaceholder, syntax, type)
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitListPatternReceiverPlaceholder(this);
	}

	public BoundListPatternReceiverPlaceholder Update(TypeSymbol type)
	{
		if (!TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundListPatternReceiverPlaceholder boundListPatternReceiverPlaceholder = new BoundListPatternReceiverPlaceholder(Syntax, type, base.HasErrors);
			boundListPatternReceiverPlaceholder.CopyAttributes(this);
			return boundListPatternReceiverPlaceholder;
		}
		return this;
	}
}
