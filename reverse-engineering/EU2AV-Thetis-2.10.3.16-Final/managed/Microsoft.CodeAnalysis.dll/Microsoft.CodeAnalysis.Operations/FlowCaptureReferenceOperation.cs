using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class FlowCaptureReferenceOperation : Operation, IFlowCaptureReferenceOperation, IOperation
{
	public CaptureId Id { get; }

	public bool IsInitialization { get; }

	internal override int ChildOperationsCount => 0;

	public override ITypeSymbol? Type { get; }

	internal override ConstantValue? OperationConstantValue { get; }

	public override OperationKind Kind => OperationKind.FlowCaptureReference;

	internal FlowCaptureReferenceOperation(CaptureId id, bool isInitialization, SemanticModel? semanticModel, SyntaxNode syntax, ITypeSymbol? type, ConstantValue? constantValue, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Id = id;
		IsInitialization = isInitialization;
		OperationConstantValue = constantValue;
		Type = type;
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
		visitor.VisitFlowCaptureReference(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitFlowCaptureReference(this, argument);
	}

	public FlowCaptureReferenceOperation(int id, SyntaxNode syntax, ITypeSymbol? type, ConstantValue? constantValue, bool isInitialization = false)
		: this(new CaptureId(id), isInitialization, null, syntax, type, constantValue, isImplicit: true)
	{
	}
}
