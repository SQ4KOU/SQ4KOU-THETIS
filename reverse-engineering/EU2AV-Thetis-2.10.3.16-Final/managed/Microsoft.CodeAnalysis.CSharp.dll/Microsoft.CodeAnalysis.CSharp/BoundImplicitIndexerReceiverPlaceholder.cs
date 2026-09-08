using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundImplicitIndexerReceiverPlaceholder : BoundValuePlaceholderBase
{
	public new TypeSymbol Type => base.Type;

	public override bool IsEquivalentToThisReference { get; }

	public BoundImplicitIndexerReceiverPlaceholder(SyntaxNode syntax, bool isEquivalentToThisReference, TypeSymbol type, bool hasErrors)
		: base(BoundKind.ImplicitIndexerReceiverPlaceholder, syntax, type, hasErrors)
	{
		IsEquivalentToThisReference = isEquivalentToThisReference;
	}

	public BoundImplicitIndexerReceiverPlaceholder(SyntaxNode syntax, bool isEquivalentToThisReference, TypeSymbol type)
		: base(BoundKind.ImplicitIndexerReceiverPlaceholder, syntax, type)
	{
		IsEquivalentToThisReference = isEquivalentToThisReference;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitImplicitIndexerReceiverPlaceholder(this);
	}

	public BoundImplicitIndexerReceiverPlaceholder Update(bool isEquivalentToThisReference, TypeSymbol type)
	{
		if (isEquivalentToThisReference != IsEquivalentToThisReference || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundImplicitIndexerReceiverPlaceholder boundImplicitIndexerReceiverPlaceholder = new BoundImplicitIndexerReceiverPlaceholder(Syntax, isEquivalentToThisReference, type, base.HasErrors);
			boundImplicitIndexerReceiverPlaceholder.CopyAttributes(this);
			return boundImplicitIndexerReceiverPlaceholder;
		}
		return this;
	}
}
