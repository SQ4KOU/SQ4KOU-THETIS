using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundSlicePatternReceiverPlaceholder : BoundEarlyValuePlaceholderBase
{
	public sealed override bool IsEquivalentToThisReference => false;

	public new TypeSymbol Type => base.Type;

	public BoundSlicePatternReceiverPlaceholder(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.SlicePatternReceiverPlaceholder, syntax, type, hasErrors)
	{
	}

	public BoundSlicePatternReceiverPlaceholder(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.SlicePatternReceiverPlaceholder, syntax, type)
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitSlicePatternReceiverPlaceholder(this);
	}

	public BoundSlicePatternReceiverPlaceholder Update(TypeSymbol type)
	{
		if (!TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundSlicePatternReceiverPlaceholder boundSlicePatternReceiverPlaceholder = new BoundSlicePatternReceiverPlaceholder(Syntax, type, base.HasErrors);
			boundSlicePatternReceiverPlaceholder.CopyAttributes(this);
			return boundSlicePatternReceiverPlaceholder;
		}
		return this;
	}
}
