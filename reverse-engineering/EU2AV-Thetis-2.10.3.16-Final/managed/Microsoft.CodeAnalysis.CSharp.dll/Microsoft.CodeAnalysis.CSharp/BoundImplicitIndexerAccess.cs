using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundImplicitIndexerAccess : BoundExpression
{
	protected override ImmutableArray<BoundNode?> Children => ImmutableArray.Create((BoundNode)Receiver, (BoundNode)Argument);

	public new TypeSymbol Type => base.Type;

	public BoundExpression Receiver { get; }

	public BoundExpression Argument { get; }

	public BoundExpression LengthOrCountAccess { get; }

	public BoundImplicitIndexerReceiverPlaceholder ReceiverPlaceholder { get; }

	public BoundExpression IndexerOrSliceAccess { get; }

	public ImmutableArray<BoundImplicitIndexerValuePlaceholder> ArgumentPlaceholders { get; }

	internal BoundImplicitIndexerAccess WithLengthOrCountAccess(BoundExpression lengthOrCountAccess)
	{
		return Update(Receiver, Argument, lengthOrCountAccess, ReceiverPlaceholder, IndexerOrSliceAccess, ArgumentPlaceholders, Type);
	}

	public BoundImplicitIndexerAccess(SyntaxNode syntax, BoundExpression receiver, BoundExpression argument, BoundExpression lengthOrCountAccess, BoundImplicitIndexerReceiverPlaceholder receiverPlaceholder, BoundExpression indexerOrSliceAccess, ImmutableArray<BoundImplicitIndexerValuePlaceholder> argumentPlaceholders, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ImplicitIndexerAccess, syntax, type, hasErrors || receiver.HasErrors() || argument.HasErrors() || lengthOrCountAccess.HasErrors() || receiverPlaceholder.HasErrors() || indexerOrSliceAccess.HasErrors() || argumentPlaceholders.HasErrors())
	{
		Receiver = receiver;
		Argument = argument;
		LengthOrCountAccess = lengthOrCountAccess;
		ReceiverPlaceholder = receiverPlaceholder;
		IndexerOrSliceAccess = indexerOrSliceAccess;
		ArgumentPlaceholders = argumentPlaceholders;
	}

	[Conditional("DEBUG")]
	private void Validate()
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitImplicitIndexerAccess(this);
	}

	public BoundImplicitIndexerAccess Update(BoundExpression receiver, BoundExpression argument, BoundExpression lengthOrCountAccess, BoundImplicitIndexerReceiverPlaceholder receiverPlaceholder, BoundExpression indexerOrSliceAccess, ImmutableArray<BoundImplicitIndexerValuePlaceholder> argumentPlaceholders, TypeSymbol type)
	{
		if (receiver != Receiver || argument != Argument || lengthOrCountAccess != LengthOrCountAccess || receiverPlaceholder != ReceiverPlaceholder || indexerOrSliceAccess != IndexerOrSliceAccess || argumentPlaceholders != ArgumentPlaceholders || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundImplicitIndexerAccess boundImplicitIndexerAccess = new BoundImplicitIndexerAccess(Syntax, receiver, argument, lengthOrCountAccess, receiverPlaceholder, indexerOrSliceAccess, argumentPlaceholders, type, base.HasErrors);
			boundImplicitIndexerAccess.CopyAttributes(this);
			return boundImplicitIndexerAccess;
		}
		return this;
	}
}
