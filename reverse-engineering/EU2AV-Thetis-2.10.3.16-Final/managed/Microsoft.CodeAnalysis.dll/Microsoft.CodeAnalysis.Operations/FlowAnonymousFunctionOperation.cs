using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class FlowAnonymousFunctionOperation : Operation, IFlowAnonymousFunctionOperation, IOperation
{
	public readonly ControlFlowGraphBuilder.Context Context;

	public readonly IAnonymousFunctionOperation Original;

	public IMethodSymbol Symbol => Original.Symbol;

	internal override int ChildOperationsCount => 0;

	public override OperationKind Kind => OperationKind.FlowAnonymousFunction;

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public FlowAnonymousFunctionOperation(in ControlFlowGraphBuilder.Context context, IAnonymousFunctionOperation original, bool isImplicit)
		: base(null, original.Syntax, isImplicit)
	{
		Context = context;
		Original = original;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		throw ExceptionUtilities.UnexpectedValue((slot, index));
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNext(int previousSlot, int previousIndex)
	{
		return (hasNext: false, nextSlot: int.MinValue, nextIndex: int.MinValue);
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		return (hasNext: false, nextSlot: int.MinValue, nextIndex: int.MinValue);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitFlowAnonymousFunction(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitFlowAnonymousFunction(this, argument);
	}
}
