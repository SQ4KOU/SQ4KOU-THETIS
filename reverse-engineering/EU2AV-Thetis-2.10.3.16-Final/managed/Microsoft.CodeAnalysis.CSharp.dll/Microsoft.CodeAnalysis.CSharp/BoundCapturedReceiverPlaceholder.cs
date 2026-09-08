using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundCapturedReceiverPlaceholder : BoundValuePlaceholderBase
{
	public sealed override bool IsEquivalentToThisReference => false;

	public BoundExpression Receiver { get; }

	public BoundCapturedReceiverPlaceholder(SyntaxNode syntax, BoundExpression receiver, TypeSymbol? type, bool hasErrors = false)
		: base(BoundKind.CapturedReceiverPlaceholder, syntax, type, hasErrors || receiver.HasErrors())
	{
		Receiver = receiver;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitCapturedReceiverPlaceholder(this);
	}

	public BoundCapturedReceiverPlaceholder Update(BoundExpression receiver, TypeSymbol? type)
	{
		if (receiver != Receiver || !TypeSymbol.Equals(type, base.Type, TypeCompareKind.ConsiderEverything))
		{
			BoundCapturedReceiverPlaceholder boundCapturedReceiverPlaceholder = new BoundCapturedReceiverPlaceholder(Syntax, receiver, type, base.HasErrors);
			boundCapturedReceiverPlaceholder.CopyAttributes(this);
			return boundCapturedReceiverPlaceholder;
		}
		return this;
	}
}
