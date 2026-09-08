using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundMethodOrPropertyGroup : BoundExpression
{
	protected override ImmutableArray<BoundNode?> Children => ImmutableArray.Create((BoundNode)ReceiverOpt);

	public new TypeSymbol? Type => base.Type;

	public BoundExpression? ReceiverOpt { get; }

	public override LookupResultKind ResultKind { get; }

	protected BoundMethodOrPropertyGroup(BoundKind kind, SyntaxNode syntax, BoundExpression? receiverOpt, LookupResultKind resultKind, bool hasErrors = false)
		: base(kind, syntax, null, hasErrors)
	{
		ReceiverOpt = receiverOpt;
		ResultKind = resultKind;
	}
}
