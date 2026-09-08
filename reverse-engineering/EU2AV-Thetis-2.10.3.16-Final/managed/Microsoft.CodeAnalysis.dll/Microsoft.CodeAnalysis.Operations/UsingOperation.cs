using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class UsingOperation : Operation, IUsingOperation, IOperation
{
	public IOperation Resources { get; }

	public IOperation Body { get; }

	public ImmutableArray<ILocalSymbol> Locals { get; }

	public bool IsAsynchronous { get; }

	public DisposeOperationInfo DisposeInfo { get; }

	internal override int ChildOperationsCount => ((Resources != null) ? 1 : 0) + ((Body != null) ? 1 : 0);

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.Using;

	internal UsingOperation(IOperation resources, IOperation body, ImmutableArray<ILocalSymbol> locals, bool isAsynchronous, DisposeOperationInfo disposeInfo, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		Resources = Operation.SetParentOperation(resources, this);
		Body = Operation.SetParentOperation(body, this);
		Locals = locals;
		IsAsynchronous = isAsynchronous;
		DisposeInfo = disposeInfo;
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (Resources != null)
			{
				return Resources;
			}
			break;
		case 1:
			if (Body != null)
			{
				return Body;
			}
			break;
		}
		throw ExceptionUtilities.UnexpectedValue((slot, index));
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNext(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case -1:
			if (Resources != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (Body != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
		case 2:
			return (hasNext: false, nextSlot: 2, nextIndex: 0);
		default:
			throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
		}
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		if ((uint)(previousSlot - -1) > 1u)
		{
			if (previousSlot != 1)
			{
				if (previousSlot != int.MaxValue)
				{
					throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
				}
				if (Body != null)
				{
					return (hasNext: true, nextSlot: 1, nextIndex: 0);
				}
			}
			if (Resources != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
		}
		return (hasNext: false, nextSlot: -1, nextIndex: 0);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitUsing(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitUsing(this, argument);
	}
}
