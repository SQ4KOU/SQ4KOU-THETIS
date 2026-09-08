using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ConstructorBodyOperation : BaseMethodBodyBaseOperation, IConstructorBodyOperation, IMethodBodyBaseOperation, IOperation
{
	public ImmutableArray<ILocalSymbol> Locals { get; }

	public IOperation? Initializer { get; }

	internal override int ChildOperationsCount => ((Initializer != null) ? 1 : 0) + ((base.BlockBody != null) ? 1 : 0) + ((base.ExpressionBody != null) ? 1 : 0);

	public override ITypeSymbol? Type => null;

	internal override ConstantValue? OperationConstantValue => null;

	public override OperationKind Kind => OperationKind.ConstructorBody;

	internal ConstructorBodyOperation(ImmutableArray<ILocalSymbol> locals, IOperation? initializer, IBlockOperation? blockBody, IBlockOperation? expressionBody, SemanticModel? semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(blockBody, expressionBody, semanticModel, syntax, isImplicit)
	{
		Locals = locals;
		Initializer = Operation.SetParentOperation(initializer, this);
	}

	internal override IOperation GetCurrent(int slot, int index)
	{
		switch (slot)
		{
		case 0:
			if (Initializer != null)
			{
				return Initializer;
			}
			break;
		case 1:
			if (base.BlockBody != null)
			{
				return base.BlockBody;
			}
			break;
		case 2:
			if (base.ExpressionBody != null)
			{
				return base.ExpressionBody;
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
			if (Initializer != null)
			{
				return (hasNext: true, nextSlot: 0, nextIndex: 0);
			}
			goto case 0;
		case 0:
			if (base.BlockBody != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
			if (base.ExpressionBody != null)
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
			if (base.ExpressionBody != null)
			{
				return (hasNext: true, nextSlot: 2, nextIndex: 0);
			}
			goto case 2;
		case 2:
			if (base.BlockBody != null)
			{
				return (hasNext: true, nextSlot: 1, nextIndex: 0);
			}
			goto case 1;
		case 1:
			if (Initializer != null)
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
		visitor.VisitConstructorBodyOperation(this);
	}

	public override TResult? Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument) where TResult : default
	{
		return visitor.VisitConstructorBodyOperation(this, argument);
	}
}
