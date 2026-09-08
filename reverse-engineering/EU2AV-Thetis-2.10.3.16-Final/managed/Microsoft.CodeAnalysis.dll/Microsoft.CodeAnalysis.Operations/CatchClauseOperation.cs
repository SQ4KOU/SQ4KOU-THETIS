using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class CatchClauseOperation : Operation, ICatchClauseOperation, IOperation
{
	public IOperation? ExceptionDeclarationOrExpression { get; }

	public ITypeSymbol ExceptionType { get; }

	public ImmutableArray<ILocalSymbol> Locals { get; }

	public IOperation? Filter { get; }

	public IBlockOperation Handler { get; }

	internal override int ChildOperationsCount => ((ExceptionDeclarationOrExpression != null) ? 1 : 0) + ((Filter != null) ? 1 : 0) + ((Handler != null) ? 1 : 0);

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.CatchClause;

	internal CatchClauseOperation(IOperation? exceptionDeclarationOrExpression, ITypeSymbol exceptionType, ImmutableArray<ILocalSymbol> locals, IOperation? filter, IBlockOperation handler, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(semanticModel, syntax, isImplicit)
	{
		ExceptionDeclarationOrExpression = Operation.SetParentOperation(exceptionDeclarationOrExpression, this);
		ExceptionType = exceptionType;
		Locals = locals;
		Filter = Operation.SetParentOperation(filter, this);
		Handler = Operation.SetParentOperation(handler, this);
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (ExceptionDeclarationOrExpression != null)
			{
				return ExceptionDeclarationOrExpression;
			}
			break;
		case 1:
			if (Filter != null)
			{
				return Filter;
			}
			break;
		case 2:
			if (Handler != null)
			{
				return Handler;
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
			if (ExceptionDeclarationOrExpression != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (Filter != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
			if (Handler != null)
			{
				return (hasNext: true, nextSlot: 2, nextIndex: 0);
			}
			goto case 2;
		case 2:
		case 3:
			return (hasNext: false, nextSlot: 3, nextIndex: 0);
		default:
			throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
		}
	}

	internal override (bool hasNext, int nextSlot, int nextIndex) MoveNextReversed(int previousSlot, int previousIndex)
	{
		switch (previousSlot)
		{
		case int.MaxValue:
			if (Handler != null)
			{
				return (hasNext: true, nextSlot: 2, nextIndex: 0);
			}
			goto case 2;
		case 2:
			if (Filter != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
			if (ExceptionDeclarationOrExpression != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case -1;
		case -1:
		case 0:
			return (hasNext: false, nextSlot: -1, nextIndex: 0);
		default:
			throw ExceptionUtilities.UnexpectedValue((previousSlot, previousIndex));
		}
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitCatchClause(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitCatchClause(this, argument);
	}
}
